using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlashingLights.ModKit.Core;

public static class ModKitMultiplayerGuard
{
    private const string MainMenuSceneName = "MainMenu2";

    private static readonly HashSet<string> PhotonOnlineRoomMethodNames = new(StringComparer.Ordinal)
    {
        "ReconnectAndRejoin",
        "CreateRoom",
        "JoinRoom",
        "JoinOrCreateRoom",
        "JoinRandomRoom",
        "ReJoinRoom"
    };

    private static Type? cachedPhotonNetworkType;
    private static bool searchedPhotonNetworkType;
    private static PropertyInfo? cachedClientStateProperty;
    private static bool searchedClientStateProperty;
    private static bool patchesInstalled;
    private static bool mainMenuRedirectQueued;
    private static bool forceMainMenuReloadQueued;
    private static float blockedAttemptBannerUntil;
    private static bool lastDetectedOnlineState;
    private static string? lastObservedClientState;
    private static GUIStyle? bannerStyle;
    private static Texture2D? bannerTexture;

    public static bool IsInOnlineMultiplayer { get; private set; }

    public static bool IsOnlineBlockedInCurrentScene { get; private set; }

    public static void InstallPatches(HarmonyLib.Harmony harmony, Action<string>? warn = null)
    {
        ArgumentNullException.ThrowIfNull(harmony);
        if (patchesInstalled)
        {
            return;
        }

        var photonType = ResolvePhotonNetworkType(warn);
        if (photonType == null)
        {
            warn?.Invoke("ModKit multiplayer guard could not resolve PhotonNetwork; online room guard is not installed.");
            return;
        }

        var prefix = typeof(ModKitMultiplayerGuard).GetMethod(
            nameof(BlockPhotonRoomOperationPrefix),
            BindingFlags.NonPublic | BindingFlags.Static);
        if (prefix == null)
        {
            warn?.Invoke("ModKit multiplayer guard room operation prefix method was not found.");
            return;
        }

        var patched = 0;
        foreach (var method in photonType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.ReturnType != typeof(bool) || !PhotonOnlineRoomMethodNames.Contains(method.Name))
            {
                continue;
            }

            try
            {
                harmony.Patch(method, prefix: new HarmonyMethod(prefix));
                patched++;
            }
            catch (Exception ex)
            {
                warn?.Invoke($"ModKit multiplayer guard could not patch {photonType.FullName}.{method.Name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        patched += PatchPlayMakerCreateRoomAction(harmony, "Il2Cpp.HutongGames.PlayMaker.Actions.PhotonNetworkCreateRoom", warn) ? 1 : 0;
        patched += PatchPlayMakerCreateRoomAction(harmony, "HutongGames.PlayMaker.Actions.PhotonNetworkCreateRoom", warn) ? 1 : 0;
        patched += PatchPlayMakerCreateRoomAction(harmony, "Il2Cpp.HutongGames.PlayMaker.Actions.PhotonNetworkCreateRoomAdvanced", warn) ? 1 : 0;
        patched += PatchPlayMakerCreateRoomAction(harmony, "HutongGames.PlayMaker.Actions.PhotonNetworkCreateRoomAdvanced", warn) ? 1 : 0;

        patchesInstalled = patched > 0;
        if (!patchesInstalled)
        {
            warn?.Invoke($"ModKit multiplayer guard found {photonType.FullName}, but no online room entry points were patched.");
        }
    }

    public static void Update(Action<string>? warn = null, Action<string>? info = null)
    {
        var detected = DetectOnlineMultiplayer();
        if (detected != lastDetectedOnlineState)
        {
            info?.Invoke($"ModKit multiplayer guard: in online MP = {detected} (clientState={lastObservedClientState ?? "<unknown>"})");
            lastDetectedOnlineState = detected;
        }
        IsInOnlineMultiplayer = detected;

        if (!ModKitMultiplayerPolicy.ShouldBlockOnline(ModKitRegistry.HasEnabledMods))
        {
            mainMenuRedirectQueued = false;
            forceMainMenuReloadQueued = false;
            IsOnlineBlockedInCurrentScene = false;
            return;
        }

        if (ModKitMultiplayerPolicy.ShouldRedirectFromOnlineSession(ModKitRegistry.HasEnabledMods, IsInOnlineMultiplayer))
        {
            blockedAttemptBannerUntil = Time.unscaledTime + 6f;
            IsOnlineBlockedInCurrentScene = true;
            QueueMainMenuRedirect();
        }

        if (mainMenuRedirectQueued)
        {
            var forceReloadMainMenu = forceMainMenuReloadQueued;
            mainMenuRedirectQueued = false;
            forceMainMenuReloadQueued = false;
            DisconnectAndReturnToMainMenu(warn, forceReloadMainMenu);
        }

        IsOnlineBlockedInCurrentScene = Time.unscaledTime <= blockedAttemptBannerUntil;
    }

    public static void OnGUI()
    {
        var shouldShowBlockedUi = ModKitMultiplayerPolicy.ShouldShowOnlineBlockedUi(
            ModKitRegistry.HasEnabledMods,
            IsMainMenuScene(SafeActiveSceneName()),
            Time.unscaledTime <= blockedAttemptBannerUntil);

        if (!shouldShowBlockedUi)
        {
            return;
        }

        EnsureBannerStyle();
        var previousDepth = GUI.depth;
        try
        {
            GUI.depth = -9999;
            var width = Math.Min(760, Screen.width - 32);
            var rect = new Rect((Screen.width - width) / 2f, 14, width, 38);
            GUI.Box(rect, ModKitMultiplayerPolicy.OnlineBlockedMessage, bannerStyle!);
        }
        finally
        {
            GUI.depth = previousDepth;
        }
    }

    public static bool CanEnableMods()
    {
        return ModKitMultiplayerPolicy.CanEnableMods(IsInOnlineMultiplayer);
    }

    private static bool BlockPhotonRoomOperationPrefix(ref bool __result, MethodBase __originalMethod, object[] __args)
    {
        var state = ReadPhotonGuardState();
        var roomName = GetRoomName(__args);
        var maxPlayers = GetRoomMaxPlayers(__args);
        var blocked = ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(
            ModKitRegistry.HasEnabledMods,
            state.OfflineMode,
            state.OnlineRoomOperationReady,
            __originalMethod.Name,
            roomName,
            maxPlayers);
        if (!blocked)
        {
            return true;
        }

        __result = false;
        blockedAttemptBannerUntil = Time.unscaledTime + 6f;
        IsOnlineBlockedInCurrentScene = true;
        QueueMainMenuRedirect(forceReloadMainMenu: true);
        return false;
    }

    private static bool BlockPlayMakerCreateRoomPrefix(object __instance, MethodBase __originalMethod)
    {
        var state = ReadPhotonGuardState();
        var roomName = GetPlayMakerRoomName(__instance);
        var maxPlayers = GetPlayMakerMaxPlayers(__instance);
        var blocked = ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(
            ModKitRegistry.HasEnabledMods,
            state.OfflineMode,
            state.OnlineRoomOperationReady,
            "CreateRoom",
            roomName,
            maxPlayers);
        if (!blocked)
        {
            return true;
        }

        TryFinishPlayMakerAction(__instance);
        blockedAttemptBannerUntil = Time.unscaledTime + 6f;
        IsOnlineBlockedInCurrentScene = true;
        QueueMainMenuRedirect(forceReloadMainMenu: true);
        return false;
    }

    private static void QueueMainMenuRedirect(bool forceReloadMainMenu = false)
    {
        mainMenuRedirectQueued = true;
        forceMainMenuReloadQueued |= forceReloadMainMenu;
    }

    private static void DisconnectAndReturnToMainMenu(Action<string>? warn, bool forceReloadMainMenu)
    {
        TryDisconnectPhoton(warn);

        var sceneName = SafeActiveSceneName();
        if (!forceReloadMainMenu && string.Equals(sceneName, MainMenuSceneName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
        }
        catch (Exception ex)
        {
            warn?.Invoke($"ModKit multiplayer guard could not return to {MainMenuSceneName}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void TryDisconnectPhoton(Action<string>? warn)
    {
        var photonType = ResolvePhotonNetworkType(warn);
        if (photonType == null)
        {
            return;
        }

        try
        {
            photonType.GetMethod("Disconnect", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)
                ?.Invoke(null, null);
        }
        catch (Exception ex)
        {
            warn?.Invoke($"ModKit multiplayer guard could not disconnect Photon: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool DetectOnlineMultiplayer()
    {
        var photonType = ResolvePhotonNetworkType();
        if (photonType == null)
        {
            return false;
        }

        var offlineMode = IsPhotonOfflineMode(photonType);
        if (offlineMode)
        {
            lastObservedClientState = "OfflineMode";
            return false;
        }

        var clientStateName = ReadClientStateName(photonType);
        lastObservedClientState = clientStateName;
        if (ModKitMultiplayerPolicy.IsActiveOnlineClientState(clientStateName))
        {
            return true;
        }

        if (ReadStaticBool(photonType, "InRoom", "inRoom", "PCCJFAELHCI") == true)
        {
            return true;
        }

        return ReadStaticObject(photonType, "CurrentRoom", "currentRoom", "room", "LJMJGGLKBKG") != null;
    }

    private static string? ReadClientStateName(Type photonType)
    {
        var property = GetClientStateProperty(photonType);
        if (property == null)
        {
            return null;
        }

        try
        {
            return property.GetValue(null)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static PropertyInfo? GetClientStateProperty(Type photonType)
    {
        if (searchedClientStateProperty)
        {
            return cachedClientStateProperty;
        }

        foreach (var property in photonType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            var propertyType = property.PropertyType;
            if (!propertyType.IsEnum)
            {
                continue;
            }

            var name = propertyType.Name;
            if (string.Equals(name, "ClientState", StringComparison.Ordinal)
                || name.EndsWith("ClientState", StringComparison.Ordinal))
            {
                cachedClientStateProperty = property;
                break;
            }
        }

        searchedClientStateProperty = true;
        return cachedClientStateProperty;
    }

    private static Type? ResolvePhotonNetworkType()
    {
        return ResolvePhotonNetworkType(warn: null);
    }

    private static Type? ResolvePhotonNetworkType(Action<string>? warn)
    {
        if (cachedPhotonNetworkType != null)
        {
            return cachedPhotonNetworkType;
        }

        var resolved = ModKitTypeResolver.ResolveFullName("Il2Cpp.PhotonNetwork", warn: null)
            ?? ModKitTypeResolver.ResolveFullName("PhotonNetwork", warn: null)
            ?? ModKitTypeResolver.ResolveFullName("Photon.Pun.PhotonNetwork", warn: null);

        if (resolved != null)
        {
            cachedPhotonNetworkType = resolved;
            searchedPhotonNetworkType = true;
            return resolved;
        }

        if (!searchedPhotonNetworkType)
        {
            warn?.Invoke("ModKit multiplayer guard could not resolve PhotonNetwork; will retry next tick.");
            searchedPhotonNetworkType = true;
        }
        return null;
    }

    private static bool IsPhotonOfflineMode()
    {
        var photonType = ResolvePhotonNetworkType();
        return photonType != null && IsPhotonOfflineMode(photonType);
    }

    private static bool IsPhotonOfflineMode(Type photonType)
    {
        return ReadStaticBool(photonType, "OfflineMode", "offlineMode", "LOIKDEMAJNI") == true;
    }

    private static PhotonGuardState ReadPhotonGuardState()
    {
        var photonType = ResolvePhotonNetworkType();
        if (photonType == null)
        {
            return new PhotonGuardState(offlineMode: false, onlineRoomOperationReady: false);
        }

        var offlineMode = IsPhotonOfflineMode(photonType);
        if (offlineMode)
        {
            return new PhotonGuardState(offlineMode: true, onlineRoomOperationReady: false);
        }

        var connectedAndReady = ReadStaticBool(photonType, "IsConnectedAndReady", "FOCDIDKBIOA");
        var serverConnection = ReadStaticObject(photonType, "Server", "ServerConnection", "IJCAHBJJKFJ")?.ToString();
        var ready = connectedAndReady == true
            && (serverConnection == null || string.Equals(serverConnection, "MasterServer", StringComparison.Ordinal));

        return new PhotonGuardState(offlineMode: false, ready);
    }

    private static bool PatchPlayMakerCreateRoomAction(
        HarmonyLib.Harmony harmony,
        string typeName,
        Action<string>? warn)
    {
        var type = ModKitTypeResolver.ResolveFullName(typeName);
        if (type == null)
        {
            return false;
        }

        var method = type.GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance);
        var prefix = typeof(ModKitMultiplayerGuard).GetMethod(
            nameof(BlockPlayMakerCreateRoomPrefix),
            BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null || prefix == null)
        {
            warn?.Invoke($"ModKit multiplayer guard could not resolve PlayMaker create-room hook for {typeName}.");
            return false;
        }

        try
        {
            harmony.Patch(method, prefix: new HarmonyMethod(prefix));
            return true;
        }
        catch (Exception ex)
        {
            warn?.Invoke($"ModKit multiplayer guard could not patch {type.FullName}.OnEnter: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static bool? ReadStaticBool(Type type, params string[] names)
    {
        foreach (var name in names)
        {
            try
            {
                var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (property?.PropertyType == typeof(bool))
                {
                    return (bool?)property.GetValue(null);
                }

                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (field?.FieldType == typeof(bool))
                {
                    return (bool?)field.GetValue(null);
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    private static object? ReadStaticObject(Type type, params string[] names)
    {
        foreach (var name in names)
        {
            try
            {
                var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var propertyValue = property?.GetValue(null);
                if (propertyValue != null)
                {
                    return propertyValue;
                }

                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var fieldValue = field?.GetValue(null);
                if (fieldValue != null)
                {
                    return fieldValue;
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    private static object? ReadInstanceObject(object? instance, params string[] names)
    {
        if (instance == null)
        {
            return null;
        }

        var type = instance.GetType();
        foreach (var name in names)
        {
            try
            {
                var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var propertyValue = property?.GetValue(instance);
                if (propertyValue != null)
                {
                    return propertyValue;
                }

                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var fieldValue = field?.GetValue(instance);
                if (fieldValue != null)
                {
                    return fieldValue;
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    private static string? GetRoomName(object[] args)
    {
        if (args.Length == 0)
        {
            return null;
        }

        return Convert.ToString(args[0]);
    }

    private static int? GetRoomMaxPlayers(object[] args)
    {
        if (args.Length <= 1)
        {
            return null;
        }

        return ToNullableInt(ReadInstanceObject(args[1], "MaxPlayers", "DNJFINGNIML"));
    }

    private static string? GetPlayMakerRoomName(object instance)
    {
        return ReadFsmValueText(ReadInstanceObject(instance, "roomName"));
    }

    private static int? GetPlayMakerMaxPlayers(object instance)
    {
        return ToNullableInt(ReadInstanceObject(ReadInstanceObject(instance, "maxNumberOfPLayers"), "Value"));
    }

    private static string? ReadFsmValueText(object? fsmValue)
    {
        var value = ReadInstanceObject(fsmValue, "Value");
        return value == null ? null : Convert.ToString(value);
    }

    private static int? ToNullableInt(object? value)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return null;
        }
    }

    private static void TryFinishPlayMakerAction(object instance)
    {
        try
        {
            instance.GetType().GetMethod("Finish", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(instance, null);
        }
        catch
        {
        }
    }

    private static string SafeActiveSceneName()
    {
        try
        {
            return SceneManager.GetActiveScene().name;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool IsMainMenuScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        return string.Equals(sceneName, MainMenuSceneName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(sceneName, "MainMenu", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sceneName, "Start", StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureBannerStyle()
    {
        if (bannerTexture == null)
        {
            bannerTexture = Texture(new Color(0.34f, 0.12f, 0.1f, 0.95f));
        }

        bannerStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal = { background = bannerTexture, textColor = new Color(1f, 0.92f, 0.86f, 1f) },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(14, 14, 6, 6),
            wordWrap = true
        };
    }

    private static Texture2D Texture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private readonly struct PhotonGuardState
    {
        public PhotonGuardState(bool offlineMode, bool onlineRoomOperationReady)
        {
            OfflineMode = offlineMode;
            OnlineRoomOperationReady = onlineRoomOperationReady;
        }

        public bool OfflineMode { get; }

        public bool OnlineRoomOperationReady { get; }
    }
}
