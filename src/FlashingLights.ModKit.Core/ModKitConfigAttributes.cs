namespace FlashingLights.ModKit.Core;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ModKitConfigDisplayAttribute : Attribute
{
    public ModKitConfigDisplayAttribute(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        }

        DisplayName = displayName;
    }

    public string DisplayName { get; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ModKitConfigRangeAttribute : Attribute
{
    public ModKitConfigRangeAttribute(double minimum, double maximum, double step = 0)
    {
        if (maximum <= minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum must be greater than minimum.");
        }

        if (step < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(step), "Step cannot be negative.");
        }

        Minimum = minimum;
        Maximum = maximum;
        Step = step;
    }

    public double Minimum { get; }
    public double Maximum { get; }
    public double Step { get; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ModKitConfigOptionsAttribute : Attribute
{
    public ModKitConfigOptionsAttribute(params string[] options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Length == 0)
        {
            throw new ArgumentException("At least one option is required.", nameof(options));
        }

        if (options.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Options cannot be empty.", nameof(options));
        }

        Options = options;
    }

    public IReadOnlyList<string> Options { get; }
}
