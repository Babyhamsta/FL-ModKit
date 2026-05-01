using FlashingLights.ModKit.Core;

var tests = new (string Name, Action Body)[]
{
    ("TypeResolver resolves System.String", TypeResolverResolvesSystemString),
    ("TypeResolver returns null for missing full name", TypeResolverReturnsNullForMissingName),
    ("ModConfig creates default config", ModConfigCreatesDefaultConfig),
    ("ModConfig keeps defaults for invalid JSON", ModConfigKeepsDefaultsForInvalidJson),
    ("PatchGuard resolves and misses methods safely", PatchGuardResolvesMethodsSafely)
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

static void TypeResolverResolvesSystemString()
{
    var type = TypeResolver.ResolveFullName("System.String");
    AssertNotNull(type, "System.String should resolve.");
    AssertEqual(typeof(string), type, "Resolved type should be System.String.");
}

static void TypeResolverReturnsNullForMissingName()
{
    var warnings = new List<string>();
    var type = TypeResolver.ResolveFullName("Missing.Namespace.TypeName", warnings.Add);
    AssertNull(type, "Missing type should return null.");
    AssertTrue(warnings.Count == 1, "Missing type should emit one warning.");
}

static void ModConfigCreatesDefaultConfig()
{
    var tempRoot = NewTempRoot();
    try
    {
        var defaults = new TestConfig { Enabled = true, Count = 7 };
        var config = ModConfig.LoadOrCreate("unit-test", defaults, tempRoot);
        var path = ModConfig.GetConfigPath("unit-test", tempRoot);

        AssertTrue(File.Exists(path), "Config file should be created.");
        AssertTrue(config.Enabled, "Default Enabled should be true.");
        AssertEqual(7, config.Count, "Default Count should be preserved.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModConfigKeepsDefaultsForInvalidJson()
{
    var tempRoot = NewTempRoot();
    try
    {
        var path = ModConfig.GetConfigPath("invalid-json", tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ bad json");

        var warnings = new List<string>();
        var defaults = new TestConfig { Enabled = false, Count = 3 };
        var config = ModConfig.LoadOrCreate("invalid-json", defaults, tempRoot, warn: warnings.Add);

        AssertTrue(!config.Enabled, "Invalid JSON should return default Enabled.");
        AssertEqual(3, config.Count, "Invalid JSON should return default Count.");
        AssertTrue(warnings.Count == 1, "Invalid JSON should emit one warning.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void PatchGuardResolvesMethodsSafely()
{
    var warnings = new List<string>();

    var toString = PatchGuard.ResolveMethod(typeof(string), nameof(ToString), warn: warnings.Add);
    AssertNotNull(toString, "string.ToString should resolve.");

    var missing = PatchGuard.ResolveMethod(typeof(string), "NoSuchMethodForModKitTest", warn: warnings.Add);
    AssertNull(missing, "Missing method should return null.");
    AssertTrue(warnings.Count == 1, "Missing method should emit one warning.");
}

static string NewTempRoot()
{
    return Path.Combine(Path.GetTempPath(), "flashing-lights-modkit-tests", Guid.NewGuid().ToString("N"));
}

static void DeleteTempRoot(string tempRoot)
{
    if (Directory.Exists(tempRoot))
    {
        Directory.Delete(tempRoot, recursive: true);
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertNull(object? value, string message)
{
    if (value != null)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertNotNull(object? value, string message)
{
    if (value == null)
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

sealed class TestConfig
{
    public bool Enabled { get; set; }
    public int Count { get; set; }
}
