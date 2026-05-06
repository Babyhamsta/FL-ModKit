using System.Globalization;
using System.Text.Json;

namespace FlashingLights.ModKit.Core;

public static class ModKitStrings
{
    private const string FileName = "strings.json";

    private static readonly Dictionary<string, string> Defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["online.blocked"] = "Online play is disabled while ModKit mods are enabled. Disable all mods to play online.",
        ["online.toggleBlocked"] = "Mods cannot be enabled while connected to online multiplayer. Return to the main menu first.",
        ["ui.insertCloses"] = "Insert closes ModKit.",
        ["ui.opened"] = "ModKit opened. Insert or Esc closes it.",
        ["ui.modBlocked"] = "{0} is blocked by manifest validation.",
        ["ui.configPulled"] = "Config pulled from selected mod.",
        ["ui.configSaved"] = "Config saved.",
        ["ui.logPathCopied"] = "Log path copied.",
        ["ui.configPathCopied"] = "Config path copied.",
        ["ui.githubUrlCopied"] = "GitHub URL copied.",
        ["ui.hotkeyCanceled"] = "Hotkey rebind canceled.",
        ["ui.hotkeyModMissing"] = "Selected mod was not found for hotkey rebind.",
        ["ui.title"] = "FL-ModKit",
        ["ui.subtitle"] = "Core {0} | Babyhamsta/FL-ModKit",
        ["ui.tab.mods"] = "Mods",
        ["ui.tab.config"] = "Config",
        ["ui.tab.logs"] = "Logs",
        ["ui.tab.about"] = "About",
        ["ui.loadedMods"] = "Loaded Mods ({0} managed)",
        ["ui.noManagedMods"] = "No SDK-managed mods registered.",
        ["ui.noFilterMatches"] = "No mods match the filter.",
        ["ui.selectManagedMod"] = "Select a managed mod.",
        ["ui.noConfigAdapter"] = "This mod did not expose a config adapter.",
        ["ui.configTitle"] = "{0} Config",
        ["ui.reloadFromFile"] = "Reload From File",
        ["ui.configReloaded"] = "{0} config reloaded.",
        ["ui.resetDefaults"] = "Reset Defaults",
        ["ui.confirmReset"] = "Confirm Reset?",
        ["ui.configReset"] = "{0} config reset.",
        ["ui.resetPrompt"] = "Click Confirm Reset within 5 seconds to reset {0}.",
        ["ui.saveConfig"] = "Save Config",
        ["ui.readOnly"] = "read",
        ["ui.enabled"] = "enabled",
        ["ui.disabled"] = "disabled",
        ["ui.array"] = "array",
        ["ui.option"] = "option",
        ["ui.hotkeys"] = "Hotkeys",
        ["ui.pressKey"] = "Press key...",
        ["ui.hotkeyPrompt"] = "Press a key to rebind {0}. Esc cancels.",
        ["ui.hotkeyRebound"] = "{0} hotkey rebound to {1}.",
        ["ui.hotkeyNoRebind"] = "{0} does not expose rebindable hotkeys.",
        ["ui.selectedMod"] = "Selected Mod",
        ["ui.info.id"] = "ID",
        ["ui.info.name"] = "Name",
        ["ui.info.version"] = "Version",
        ["ui.info.author"] = "Author",
        ["ui.info.license"] = "License",
        ["ui.info.minSdk"] = "Min SDK",
        ["ui.info.category"] = "Category",
        ["ui.info.tags"] = "Tags",
        ["ui.info.dependencies"] = "Dependencies",
        ["ui.info.status"] = "Status",
        ["ui.info.state"] = "State",
        ["ui.info.sdk"] = "SDK",
        ["ui.info.github"] = "GitHub",
        ["ui.info.managedMods"] = "Managed Mods",
        ["ui.none"] = "None",
        ["ui.openConfig"] = "Open Config",
        ["ui.rerunSelfCheck"] = "Re-run Self Check",
        ["ui.selfCheckOk"] = "{0} self check OK.",
        ["ui.selfCheckIssues"] = "{0} self check found {1} issue(s).",
        ["ui.logs"] = "Logs",
        ["ui.view"] = "View",
        ["ui.copyPath"] = "Copy Path",
        ["ui.noLogs"] = "No ModKit log files found yet.",
        ["ui.logViewer"] = "Log Viewer",
        ["ui.selectLog"] = "Select a log file.",
        ["ui.refresh"] = "Refresh",
        ["ui.logLoaded"] = "Loaded {0}.",
        ["ui.logReadFailed"] = "Could not read log: {0}",
        ["ui.copyGithubUrl"] = "Copy GitHub URL",
        ["ui.copyConfigPath"] = "Copy Config Path",
        ["ui.statusToggled"] = "{0} {1}.",
        ["ui.switchOn"] = "ON",
        ["ui.switchOff"] = "OFF",
        ["ui.uncategorized"] = "Uncategorized",
        ["ui.unspecified"] = "Unspecified",
        ["ui.any"] = "any",
        ["ui.validationOk"] = "OK",
        ["ui.validationBlocked"] = "Blocked ({0} {1})",
        ["ui.issue"] = "issue",
        ["ui.issues"] = "issues"
    };

    private static readonly object Gate = new();
    private static Dictionary<string, string> overrides = new(StringComparer.OrdinalIgnoreCase);
    private static string? loadedBaseDirectory;

    public static string Get(string key, params object?[] args)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("String key cannot be empty.", nameof(key));
        }

        EnsureLoaded();
        string format;
        lock (Gate)
        {
            if (!overrides.TryGetValue(key, out format!)
                && !Defaults.TryGetValue(key, out format!))
            {
                format = key;
            }
        }

        return args.Length == 0
            ? format
            : string.Format(CultureInfo.InvariantCulture, format, args);
    }

    public static void Reload(string? baseDirectory = null)
    {
        lock (Gate)
        {
            loadedBaseDirectory = baseDirectory;
            overrides = LoadOverrides(baseDirectory);
        }
    }

    private static void EnsureLoaded()
    {
        lock (Gate)
        {
            if (overrides.Count == 0 && loadedBaseDirectory == null)
            {
                overrides = LoadOverrides(baseDirectory: null);
                loadedBaseDirectory = string.Empty;
            }
        }
    }

    private static Dictionary<string, string> LoadOverrides(string? baseDirectory)
    {
        var path = Path.Combine(ModKitPaths.UserDataRoot(baseDirectory), FileName);
        try
        {
            if (!File.Exists(path))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            return loaded == null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(loaded, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
