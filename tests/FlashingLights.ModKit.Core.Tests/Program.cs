using FlashingLights.ModKit.Core;

var tests = new (string Name, Action Body)[]
{
    ("TypeResolver resolves System.String", TypeResolverResolvesSystemString),
    ("TypeResolver returns null for missing full name", TypeResolverReturnsNullForMissingName),
    ("ModConfig creates default config", ModConfigCreatesDefaultConfig),
    ("ModConfig keeps defaults for invalid JSON", ModConfigKeepsDefaultsForInvalidJson),
    ("SceneQuery exposes name contains API", SceneQueryExposesNameContainsApi),
    ("SceneQuery filters names by fragment", SceneQueryFiltersNamesByFragment),
    ("SceneQuery rejects invalid name filters", SceneQueryRejectsInvalidNameFilters),
    ("PatchGuard resolves and misses methods safely", PatchGuardResolvesMethodsSafely),
    ("PatchGuard patch helpers keep warn before parameters", PatchGuardPatchHelpersKeepWarnBeforeParameters)
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

static void SceneQueryExposesNameContainsApi()
{
    var method = typeof(SceneQuery).GetMethod(
        nameof(SceneQuery.FindObjectNamesContaining),
        new[] { typeof(string), typeof(bool), typeof(int), typeof(Action<string>) });

    AssertNotNull(method, "SceneQuery should expose FindObjectNamesContaining with warn as fourth argument.");
}

static void SceneQueryFiltersNamesByFragment()
{
    var names = new[] { "Police Cruiser", "Fire Engine", "Unmarked police van", "Ambulance" };

    var matches = SceneQuery.FilterObjectNamesContaining(names, "POLICE", maxNames: 1);

    AssertEqual(1, matches.Count, "Matches should honor max names.");
    AssertEqual("Police Cruiser", matches[0], "Name filtering should be case-insensitive and stable.");
}

static void SceneQueryRejectsInvalidNameFilters()
{
    AssertThrows<ArgumentException>(
        () => SceneQuery.FilterObjectNamesContaining(new[] { "Car" }, " "),
        "Blank name fragment should throw.");

    AssertThrows<ArgumentOutOfRangeException>(
        () => SceneQuery.FilterObjectNamesContaining(new[] { "Car" }, "car", maxNames: -1),
        "Negative max names should throw.");
}

static void PatchGuardPatchHelpersKeepWarnBeforeParameters()
{
    var harmony = new HarmonyLib.Harmony("flashinglights.modkit.tests.patchguard.signature");
    var warnings = new List<string>();
    var postfix = typeof(PatchGuardSignatureTargets).GetMethod(nameof(PatchGuardSignatureTargets.Postfix))!;
    var prefix = typeof(PatchGuardSignatureTargets).GetMethod(nameof(PatchGuardSignatureTargets.Prefix))!;
    var parameters = new[] { typeof(int) };

    var missingPostfix = PatchGuard.PatchPostfix(
        harmony,
        typeof(PatchGuardSignatureTargets),
        "MissingForSignatureTest",
        postfix,
        warnings.Add,
        parameters: parameters);
    var missingPrefix = PatchGuard.PatchPrefix(
        harmony,
        typeof(PatchGuardSignatureTargets),
        "MissingForSignatureTest",
        prefix,
        warnings.Add,
        parameters: parameters);

    AssertTrue(!missingPostfix, "Missing postfix target should return false.");
    AssertTrue(!missingPrefix, "Missing prefix target should return false.");
    AssertEqual(2, warnings.Count, "Missing patch targets should emit warnings.");
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

static void AssertThrows<TException>(Action action, string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"{message} Expected {typeof(TException).Name}, got {ex.GetType().Name}.");
    }

    throw new InvalidOperationException($"{message} Expected {typeof(TException).Name}.");
}

sealed class TestConfig
{
    public bool Enabled { get; set; }
    public int Count { get; set; }
}

static class PatchGuardSignatureTargets
{
    public static void Prefix()
    {
    }

    public static void Postfix()
    {
    }
}
