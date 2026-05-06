using FlashingLights.ModKit.Core;

var tests = new (string Name, Action Body)[]
{
    ("Config round-trips through ModKitConfig", ConfigRoundTrips),
    ("Manifest validation reports missing dependency", ManifestValidationReportsMissingDependency),
    ("Manifest attribute round-trips metadata", ManifestAttributeRoundTripsMetadata)
};

var failed = 0;
foreach (var test in tests)
{
    try
    {
        test.Body();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {test.Name}: {ex.GetType().Name}: {ex.Message}");
    }
}

return failed == 0 ? 0 : 1;

static void ConfigRoundTrips()
{
    var root = Path.Combine(Path.GetTempPath(), "fl-modkit-template-tests", Guid.NewGuid().ToString("N"));
    try
    {
        var config = new SampleConfig { Enabled = true, SpawnDistance = 275 };
        AssertTrue(ModKitConfig.Save("sample-mod", config, root), "Config save should succeed.");

        var loaded = ModKitConfig.LoadOrCreate("sample-mod", new SampleConfig(), root);

        AssertTrue(loaded.Enabled, "Enabled should round-trip.");
        AssertEqual(275, loaded.SpawnDistance, "SpawnDistance should round-trip.");
    }
    finally
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}

static void ManifestValidationReportsMissingDependency()
{
    var metadata = new ModKitModMetadata("sample.mod", "Sample Mod", "1.0.0", "Babyhamsta")
    {
        Dependencies = new[] { "missing.mod>=1.0.0" }
    };

    var result = ModKitManifestValidator.Validate(metadata, "1.0.0", Array.Empty<ModKitModMetadata>());

    AssertTrue(!result.IsValid, "Missing dependency should fail validation.");
    AssertTrue(result.Issues[0].Contains("missing.mod", StringComparison.Ordinal), "Issue should name missing dependency.");
}

static void ManifestAttributeRoundTripsMetadata()
{
    var metadata = typeof(SampleManifestTarget)
        .GetCustomAttributes(typeof(ModKitManifestAttribute), inherit: false)
        .Cast<ModKitManifestAttribute>()
        .Single();

    AssertEqual("sample.manifest", metadata.Id, "Manifest Id should round-trip.");
    AssertEqual("Sample Manifest", metadata.DisplayName, "Manifest DisplayName should round-trip.");
    AssertEqual("Examples", metadata.Category, "Manifest Category should round-trip.");
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
    }
}

sealed class SampleConfig
{
    public bool Enabled { get; set; }
    public int SpawnDistance { get; set; } = 250;
}

[ModKitManifest(
    Id = "sample.manifest",
    DisplayName = "Sample Manifest",
    Version = "1.0.0",
    Author = "Babyhamsta",
    Category = "Examples")]
sealed class SampleManifestTarget
{
}
