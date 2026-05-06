using UnityEngine;

namespace FlashingLights.ModKit.Core;

public sealed class Hotkeys
{
    private readonly Dictionary<string, Binding> bindings = new(StringComparer.OrdinalIgnoreCase);
    private readonly IHotkeyInputSource input;
    private readonly Action<string> warn;

    public Hotkeys(IHotkeyInputSource? input = null, Action<string>? warn = null)
    {
        this.input = input ?? UnityHotkeyInputSource.Instance;
        this.warn = warn ?? (_ => { });
    }

    public IReadOnlyCollection<string> RegisteredNames => bindings.Keys;

    public IReadOnlyList<HotkeyBinding> Bindings => bindings
        .Select(entry => new HotkeyBinding(entry.Key, entry.Value.Key, entry.Value.DisplayName))
        .ToArray();

    public void Register(string name, KeyCode key, Action callback, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Binding name cannot be empty.", nameof(name));
        }
        ArgumentNullException.ThrowIfNull(callback);

        if (bindings.ContainsKey(name))
        {
            throw new ArgumentException($"Hotkey '{name}' is already registered.", nameof(name));
        }

        bindings[name] = new Binding(key, callback, string.IsNullOrWhiteSpace(displayName) ? name : displayName);
    }

    public void Unregister(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        bindings.Remove(name);
    }

    public void Rebind(string name, KeyCode newKey)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Binding name cannot be empty.", nameof(name));
        }

        if (!bindings.TryGetValue(name, out var binding))
        {
            throw new KeyNotFoundException($"Hotkey '{name}' is not registered.");
        }

        bindings[name] = binding with { Key = newKey };
    }

    public void Update()
    {
        var snapshot = bindings.Count == 0
            ? Array.Empty<KeyValuePair<string, Binding>>()
            : bindings.ToArray();

        foreach (var entry in snapshot)
        {
            if (!input.GetKeyDown(entry.Value.Key))
            {
                continue;
            }

            try
            {
                entry.Value.Callback();
            }
            catch (Exception ex)
            {
                warn($"Hotkey '{entry.Key}' callback failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private readonly record struct Binding(KeyCode Key, Action Callback, string DisplayName);
}
