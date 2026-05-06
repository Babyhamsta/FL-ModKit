namespace FlashingLights.ModKit.Core;

public static class ModKitMultiplayerPolicy
{
    public static string OnlineBlockedMessage => ModKitStrings.Get("online.blocked");
    public static string ToggleBlockedMessage => ModKitStrings.Get("online.toggleBlocked");

    private static readonly HashSet<string> InactiveClientStates = new(StringComparer.Ordinal)
    {
        "Uninitialized",
        "PeerCreated",
        "Disconnected",
        "Disconnecting",
        "ConnectingToNameServer"
    };

    public static bool ShouldBlockOnline(bool hasEnabledMods)
    {
        return hasEnabledMods;
    }

    public static bool CanEnableMods(bool isInOnlineMultiplayer)
    {
        return !isInOnlineMultiplayer;
    }

    public static bool IsActiveOnlineClientState(string? clientStateName)
    {
        if (string.IsNullOrWhiteSpace(clientStateName))
        {
            return false;
        }

        return !InactiveClientStates.Contains(clientStateName);
    }

    public static bool ShouldRedirectFromOnlineSession(bool hasEnabledMods, bool isInOnlineMultiplayer)
    {
        return hasEnabledMods && isInOnlineMultiplayer;
    }

    public static bool ShouldBlockPhotonOperation(
        bool hasEnabledMods,
        bool isOfflineMode,
        bool isOnlineRoomOperationReady,
        string methodName,
        string? roomName = null,
        int? maxPlayers = null)
    {
        if (!hasEnabledMods || isOfflineMode)
        {
            return false;
        }

        if (IsOnlineJoinOperation(methodName))
        {
            return true;
        }

        if (string.Equals(methodName, "JoinOrCreateRoom", StringComparison.Ordinal))
        {
            return true;
        }

        if (!string.Equals(methodName, "CreateRoom", StringComparison.Ordinal))
        {
            return false;
        }

        if (IsKnownLocalCreateRoom(roomName, maxPlayers))
        {
            return false;
        }

        return isOnlineRoomOperationReady || IsLikelyOnlineCreateRoom(roomName, maxPlayers);
    }

    public static bool ShouldShowOnlineBlockedUi(bool hasEnabledMods, bool isMainMenuScene, bool recentlyBlockedOnlineSession)
    {
        return hasEnabledMods && (isMainMenuScene || recentlyBlockedOnlineSession);
    }

    public static bool IsOnlineJoinOperation(string methodName)
    {
        return string.Equals(methodName, "JoinRoom", StringComparison.Ordinal)
            || string.Equals(methodName, "JoinRandomRoom", StringComparison.Ordinal)
            || string.Equals(methodName, "ReJoinRoom", StringComparison.Ordinal)
            || string.Equals(methodName, "ReconnectAndRejoin", StringComparison.Ordinal);
    }

    private static bool IsKnownLocalCreateRoom(string? roomName, int? maxPlayers)
    {
        if (maxPlayers.HasValue && maxPlayers.Value <= 1)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(roomName))
        {
            return false;
        }

        return roomName.StartsWith("Singleplayer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(roomName, "PD_Room", StringComparison.Ordinal)
            || string.Equals(roomName, "FD_Room", StringComparison.Ordinal)
            || string.Equals(roomName, "EMS_Room", StringComparison.Ordinal)
            || string.Equals(roomName, "GLP_Room", StringComparison.Ordinal);
    }

    private static bool IsLikelyOnlineCreateRoom(string? roomName, int? maxPlayers)
    {
        if (!string.IsNullOrWhiteSpace(roomName)
            && roomName.Contains("(id:", StringComparison.OrdinalIgnoreCase)
            && !roomName.Contains("(id:)", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return maxPlayers.HasValue && maxPlayers.Value > 1;
    }

}
