namespace FlashingLights.ModKit.Core;

public static class PatchContext<TState>
    where TState : class
{
    private static volatile Holder? holder;

    public static TState? State => holder?.State;

    public static bool IsActive => holder?.Active == true;

    public static void Set(TState value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var previous = holder;
        holder = new Holder(value, previous?.Active ?? false);
    }

    public static void SetActive(bool value)
    {
        var previous = holder;
        holder = previous == null
            ? new Holder(null, value)
            : new Holder(previous.State, value);
    }

    public static void Clear()
    {
        holder = null;
    }

    public static bool TryGet(out TState state)
    {
        var snapshot = holder;
        if (snapshot is { Active: true, State: not null })
        {
            state = snapshot.State;
            return true;
        }

        state = null!;
        return false;
    }

    private sealed record Holder(TState? State, bool Active);
}
