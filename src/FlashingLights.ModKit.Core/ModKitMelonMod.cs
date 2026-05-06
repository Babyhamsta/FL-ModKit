using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace FlashingLights.ModKit.Core;

public abstract class ModKitMelonMod<TConfig> : MelonMod, IModKitManagedMod
    where TConfig : class, new()
{
    private DateTimeOffset nextConfigReloadCheckAt = DateTimeOffset.MinValue;
    private ModKitModMetadata? metadata;
    private bool runtimeActive;

    protected abstract string ModId { get; }

    protected TConfig Config { get; private set; } = new();

    public ModKitModMetadata Metadata => metadata ??= CreateMetadata();

    public bool IsEnabled { get; private set; } = true;

    public IModKitConfigAdapter? ConfigAdapter { get; private set; }

    protected string ConfigPath { get; private set; } = string.Empty;

    string IModKitManagedMod.ConfigPath => ConfigPath;

    protected ModKitFileLog? FileLog { get; set; }

    protected DateTimeOffset? LoadedConfigWriteTime { get; private set; }

    protected virtual bool EnableConfigHotReload => false;

    protected virtual TimeSpan ConfigReloadInterval => TimeSpan.FromSeconds(1);

    protected virtual string DisplayName => GetType().Name;

    protected virtual string ModVersion => "0.1.0";

    protected virtual string ModAuthor => "Babyhamsta";

    protected virtual string? GitHubUrl => ModKitModMetadata.DefaultGitHubUrl;

    protected virtual string? Changelog => null;

    protected virtual bool RegisterWithModKitUi => true;

    public sealed override void OnInitializeMelon()
    {
        ConfigPath = ModKitConfig.GetConfigPath(ModId);
        Config = LoadConfig();
        ConfigAdapter = new ModKitConfigAdapter<TConfig>(ModId, () => Config, ApplyConfig, onChanged: ApplyLiveConfigEdit, warn: LogWarning);
        LoadedConfigWriteTime = GetFileWriteTime(ConfigPath);
        IsEnabled = ReadEnabledFlag(Config) ?? true;

        var validation = ModKitManifestValidationResult.Ok;
        if (RegisterWithModKitUi)
        {
            ModKitRegistry.Register(this);
            validation = ModKitRegistry.GetValidation(Metadata.Id);
        }

        var loggedIssues = new HashSet<string>(StringComparer.Ordinal);
        foreach (var issue in validation.Issues)
        {
            loggedIssues.Add(issue);
            LogWarning($"Manifest validation: {issue}");
        }

        ModKitMultiplayerGuard.InstallPatches(HarmonyInstance, LogWarning);
        SdkInfo.LogOnce(LogInfo);
        OnModKitInitialized();
        if (RegisterWithModKitUi)
        {
            validation = ModKitRegistry.RefreshSelfCheck(Metadata.Id);
            foreach (var issue in validation.Issues)
            {
                if (loggedIssues.Add(issue))
                {
                    LogWarning($"Manifest validation: {issue}");
                }
            }
        }

        if (IsEnabled && !validation.IsValid)
        {
            LogWarning($"Mod '{Metadata.Id}' is enabled but blocked by manifest validation; OnModKitEnabled skipped.");
        }

        SyncRuntimeState();
    }

    public sealed override void OnUpdate()
    {
        if (ModKitRegistry.IsUiOwner(ModId))
        {
            ModKitMultiplayerGuard.Update(LogWarning, LogInfo);
            ModKitUiHost.Update();
        }

        TryReloadConfig();
        SyncRuntimeState();
        if (runtimeActive)
        {
            Hotkeys?.Update();
            OnModKitUpdate();
        }
    }

    public sealed override void OnGUI()
    {
        if (ModKitRegistry.IsUiOwner(ModId))
        {
            ModKitMultiplayerGuard.OnGUI();
            ModKitUiHost.OnGUI();
        }

        SyncRuntimeState();
        if (runtimeActive)
        {
            OnModKitGui();
        }
    }

    public sealed override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        SyncRuntimeState();
        if (runtimeActive)
        {
            OnModKitSceneWasLoaded(buildIndex, sceneName);
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (IsEnabled == enabled)
        {
            SyncRuntimeState();
            return;
        }

        if (enabled && !ModKitMultiplayerGuard.CanEnableMods())
        {
            return;
        }

        if (enabled && ModKitRegistry.IsBlockedByManifest(Metadata.Id))
        {
            LogWarning($"Cannot enable '{Metadata.Id}': blocked by manifest validation.");
            return;
        }

        IsEnabled = enabled;
        WriteEnabledFlag(Config, enabled);
        ConfigAdapter?.Save();
        SyncRuntimeState();
    }

    protected virtual void OnModKitInitialized()
    {
    }

    protected virtual void OnModKitUpdate()
    {
    }

    protected virtual void OnModKitGui()
    {
    }

    protected virtual void OnModKitSceneWasLoaded(int buildIndex, string sceneName)
    {
    }

    protected virtual void OnModKitEnabled()
    {
    }

    protected virtual void OnModKitDisabled()
    {
    }

    protected virtual void OnConfigReloaded(TConfig previousConfig, TConfig currentConfig)
    {
    }

    protected virtual void OnConfigApplied(TConfig currentConfig)
    {
    }

    protected virtual TConfig MigrateConfig(int loadedVersion, TConfig loaded)
    {
        return loaded;
    }

    protected virtual ModKitManifestValidationResult? RunSelfCheck()
    {
        return null;
    }

    protected virtual Hotkeys? Hotkeys => null;

    protected void LogInfo(string message)
    {
        LoggerInstance.Msg(message);
    }

    protected void LogWarning(string message)
    {
        LoggerInstance.Warning(message);
    }

    protected void LogDebug(string message)
    {
        FileLog?.Debug(message);
        if (IsVerboseLoggingEnabled(Config))
        {
            LoggerInstance.Msg(message);
        }
    }

    protected Type? ResolveType(string fullName)
    {
        return ModKitTypeResolver.ResolveFullName(fullName, LogWarning);
    }

    protected bool PatchPostfix(string targetTypeName, string methodName, MethodInfo postfix, Type[]? parameters = null)
    {
        return PatchGuard.PatchPostfix(
            HarmonyInstance,
            ResolveType(targetTypeName),
            methodName,
            postfix,
            LogWarning,
            parameters);
    }

    protected bool PatchPrefix(string targetTypeName, string methodName, MethodInfo prefix, Type[]? parameters = null)
    {
        return PatchGuard.PatchPrefix(
            HarmonyInstance,
            ResolveType(targetTypeName),
            methodName,
            prefix,
            LogWarning,
            parameters);
    }

    protected TConfig ReloadConfig()
    {
        var previousEnabled = IsEnabled;
        Config = LoadConfig();
        LoadedConfigWriteTime = GetFileWriteTime(ConfigPath);
        SyncEnabledFromConfig(previousEnabled);
        OnConfigApplied(Config);
        SyncRuntimeState();
        return Config;
    }

    private TConfig LoadConfig()
    {
        var result = ModKitConfig.LoadOrCreateWithVersion(ModId, new TConfig(), info: LogInfo, warn: LogWarning);
        return result.LoadedVersion == result.TargetVersion
            ? result.Config
            : MigrateConfig(result.LoadedVersion, result.Config);
    }

    private void TryReloadConfig()
    {
        if (!EnableConfigHotReload || string.IsNullOrWhiteSpace(ConfigPath))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var interval = ConfigReloadInterval <= TimeSpan.Zero
            ? TimeSpan.FromSeconds(1)
            : ConfigReloadInterval;
        if (now < nextConfigReloadCheckAt)
        {
            return;
        }

        nextConfigReloadCheckAt = now.Add(interval);

        var preLoadWriteTime = GetFileWriteTime(ConfigPath);
        if (!ModKitConfig.ShouldReload(LoadedConfigWriteTime, preLoadWriteTime))
        {
            return;
        }

        var previous = Config;
        var previousEnabled = IsEnabled;
        Config = LoadConfig();
        LoadedConfigWriteTime = preLoadWriteTime;

        // If the file was rewritten while we were reading it, post-load mtime advances
        // and the next tick will pick up the newer content. Detect mismatch eagerly
        // so a single late write isn't lost on filesystems with coarse mtime resolution.
        var postLoadWriteTime = GetFileWriteTime(ConfigPath);
        if (postLoadWriteTime.HasValue && preLoadWriteTime.HasValue && postLoadWriteTime > preLoadWriteTime)
        {
            nextConfigReloadCheckAt = DateTimeOffset.UtcNow;
        }

        SyncEnabledFromConfig(previousEnabled);
        OnConfigApplied(Config);
        OnConfigReloaded(previous, Config);
        SyncRuntimeState();
    }

    private ModKitModMetadata CreateMetadata()
    {
        return ModKitMetadataResolver.Resolve(
            modType: GetType(),
            fallbackId: ModId,
            fallbackDisplayName: DisplayName,
            fallbackVersion: ModVersion,
            fallbackAuthor: ModAuthor,
            fallbackGitHubUrl: GitHubUrl,
            fallbackChangelog: Changelog);
    }

    private void ApplyConfig(TConfig config)
    {
        var previous = Config;
        var previousEnabled = IsEnabled;
        Config = config;
        SyncEnabledFromConfig(previousEnabled);
        OnConfigApplied(Config);
        OnConfigReloaded(previous, Config);
        SyncRuntimeState();
    }

    private void ApplyLiveConfigEdit(TConfig config)
    {
        var previousEnabled = IsEnabled;
        SyncEnabledFromConfig(previousEnabled);
        OnConfigApplied(config);
        SyncRuntimeState();
    }

    private void SyncEnabledFromConfig(bool previousEnabled)
    {
        var nextEnabled = ReadEnabledFlag(Config) ?? IsEnabled;
        if (nextEnabled && !previousEnabled && !ModKitMultiplayerGuard.CanEnableMods())
        {
            nextEnabled = false;
            WriteEnabledFlag(Config, enabled: false);
        }

        IsEnabled = nextEnabled;
    }

    private void SyncRuntimeState()
    {
        var shouldRun = ModKitRuntimePolicy.ShouldRun(IsEnabled, ModKitRegistry.IsBlockedByManifest(Metadata.Id));
        if (runtimeActive == shouldRun)
        {
            return;
        }

        runtimeActive = shouldRun;
        if (runtimeActive)
        {
            OnModKitEnabled();
        }
        else
        {
            OnModKitDisabled();
        }
    }

    private static bool? ReadEnabledFlag(TConfig config)
    {
        var property = typeof(TConfig).GetProperty("Enabled");
        if (property == null)
        {
            return null;
        }

        var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (underlying != typeof(bool))
        {
            return null;
        }

        return (bool?)property.GetValue(config);
    }

    private static void WriteEnabledFlag(TConfig config, bool enabled)
    {
        var property = typeof(TConfig).GetProperty("Enabled");
        if (property == null || property.SetMethod == null)
        {
            return;
        }

        var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (underlying != typeof(bool))
        {
            return;
        }

        property.SetValue(config, enabled);
    }

    private static DateTimeOffset? GetFileWriteTime(string path)
    {
        try
        {
            return File.Exists(path)
                ? new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero)
                : null;
        }
        catch
        {
            return null;
        }
    }

    internal static bool IsVerboseLoggingEnabled(TConfig config)
    {
        var property = typeof(TConfig).GetProperty("VerboseLogging");
        if (property == null)
        {
            return false;
        }

        var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        return underlying == typeof(bool) && property.GetValue(config) is true;
    }

    IReadOnlyList<HotkeyBinding>? IModKitHotkeyOwner.GetHotkeyBindings()
    {
        return Hotkeys?.Bindings;
    }

    bool IModKitHotkeyOwner.TryRebindHotkey(string name, KeyCode newKey)
    {
        if (Hotkeys == null)
        {
            return false;
        }

        Hotkeys.Rebind(name, newKey);
        return true;
    }

    ModKitManifestValidationResult? IModKitManagedMod.SelfCheck()
    {
        return RunSelfCheck();
    }
}
