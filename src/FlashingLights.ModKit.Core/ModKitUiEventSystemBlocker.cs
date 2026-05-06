using System.Reflection;
using UnityEngine;

namespace FlashingLights.ModKit.Core;

internal static class ModKitUiEventSystemBlocker
{
    private static readonly Dictionary<int, EventSystemState> BlockedEventSystems = [];
    private static Type? cachedEventSystemType;
    private static bool searchedEventSystemType;

    public static void SetBlocked(bool blocked, Action<string>? warn = null)
    {
        if (blocked)
        {
            BlockCurrent(warn);
        }
        else
        {
            RestoreAll(warn);
        }
    }

    private static void BlockCurrent(Action<string>? warn)
    {
        try
        {
            var eventSystemType = ResolveEventSystemType();
            var current = eventSystemType?.GetProperty("current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as Behaviour;
            if (current == null)
            {
                return;
            }

            var id = current.GetInstanceID();
            if (!BlockedEventSystems.ContainsKey(id))
            {
                BlockedEventSystems[id] = new EventSystemState(current, current.enabled);
            }

            if (current.enabled)
            {
                current.enabled = false;
            }
        }
        catch (Exception ex)
        {
            warn?.Invoke($"ModKit UI EventSystem block failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static Type? ResolveEventSystemType()
    {
        if (searchedEventSystemType)
        {
            return cachedEventSystemType;
        }

        cachedEventSystemType = ModKitTypeResolver.ResolveFullName("UnityEngine.EventSystems.EventSystem");
        searchedEventSystemType = true;
        return cachedEventSystemType;
    }

    private static void RestoreAll(Action<string>? warn)
    {
        if (BlockedEventSystems.Count == 0)
        {
            return;
        }

        try
        {
            foreach (var state in BlockedEventSystems.Values)
            {
                if (state.EventSystem != null)
                {
                    state.EventSystem.enabled = state.WasEnabled;
                }
            }
        }
        catch (Exception ex)
        {
            warn?.Invoke($"ModKit UI EventSystem restore failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            BlockedEventSystems.Clear();
        }
    }

    private sealed class EventSystemState
    {
        public EventSystemState(Behaviour eventSystem, bool wasEnabled)
        {
            EventSystem = eventSystem;
            WasEnabled = wasEnabled;
        }

        public Behaviour EventSystem { get; }
        public bool WasEnabled { get; }
    }
}
