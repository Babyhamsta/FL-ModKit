namespace FlashingLights.ModKit.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ModKitConfigVersionAttribute : Attribute
{
    public ModKitConfigVersionAttribute(int version)
    {
        if (version < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Config version cannot be negative.");
        }

        Version = version;
    }

    public int Version { get; }
}
