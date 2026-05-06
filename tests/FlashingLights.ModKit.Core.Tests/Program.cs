using FlashingLights.ModKit.Core;
using UnityEngine;

var tests = new (string Name, Action Body)[]
{
    ("ModKitTypeResolver resolves System.String", TypeResolverResolvesSystemString),
    ("ModKitTypeResolver returns null for missing full name", TypeResolverReturnsNullForMissingName),
    ("ModKitPaths builds config paths", ModKitPathsBuildsConfigPaths),
    ("ModKitPaths rejects invalid file names", ModKitPathsRejectsInvalidFileNames),
    ("ModKitConfig creates default config", ModConfigCreatesDefaultConfig),
    ("ModKitConfig keeps defaults for invalid JSON", ModConfigKeepsDefaultsForInvalidJson),
    ("ModKitConfig detects newer hot reload timestamp", ModConfigDetectsNewerHotReloadTimestamp),
    ("ModKitConfig reads config version metadata", ModKitConfigReadsVersionMetadata),
    ("ModKitConfig saves version metadata only for versioned configs", ModKitConfigSavesVersionMetadata),
    ("ModKitConfig falls back when version metadata is malformed", ModKitConfigFallsBackWhenVersionMetadataMalformed),
    ("SceneQuery exposes name contains API", SceneQueryExposesNameContainsApi),
    ("SceneQuery filters names by fragment", SceneQueryFiltersNamesByFragment),
    ("SceneQuery rejects invalid name filters", SceneQueryRejectsInvalidNameFilters),
    ("PatchGuard resolves and misses methods safely", PatchGuardResolvesMethodsSafely),
    ("PatchGuard patch helpers keep warn before parameters", PatchGuardPatchHelpersKeepWarnBeforeParameters),
    ("ModKitMelonMod exposes sealed Melon lifecycle wrappers", ModKitMelonModExposesLifecycleWrappers),
    ("ModKitMelonMod exposes patch helper hooks", ModKitMelonModExposesPatchHelpers),
    ("ModKitConfigAdapter pulls current config values", ModKitConfigAdapterPullsCurrentConfigValues),
    ("ModKitConfigAdapter exposes display and range metadata", ModKitConfigAdapterExposesDisplayAndRangeMetadata),
    ("ModKitConfigAdapter updates writable config properties", ModKitConfigAdapterUpdatesWritableConfigProperties),
    ("ModKitConfigAdapter notifies after writable config updates", ModKitConfigAdapterNotifiesAfterWritableConfigUpdates),
    ("ModKitConfigAdapter saves and reloads config files", ModKitConfigAdapterSavesAndReloadsConfigFiles),
    ("ModKit UI blocks mouse and keyboard overlay events", ModKitUiBlocksMouseAndKeyboardOverlayEvents),
    ("ModKit UI close keys include Insert and Escape", ModKitUiCloseKeysIncludeInsertAndEscape),
    ("ModKit UI defers cursor restore after close", ModKitUiDefersCursorRestoreAfterClose),
    ("ModKit UI tabs select distinct panes", ModKitUiTabsSelectDistinctPanes),
    ("ModKit UI layout keeps footer inside 1080p", ModKitUiLayoutKeepsFooterInside1080p),
    ("ModKit multiplayer guard blocks online when enabled mods exist", ModKitMultiplayerGuardBlocksOnlineWhenEnabledModsExist),
    ("ModKit multiplayer guard locks enabling mods while in multiplayer", ModKitMultiplayerGuardLocksEnablingModsWhileInMultiplayer),
    ("ModKit multiplayer guard blocks online room operations only", ModKitMultiplayerGuardBlocksOnlineRoomOperationsOnly),
    ("ModKit multiplayer guard redirects after online room detection", ModKitMultiplayerGuardRedirectsAfterOnlineRoomDetection),
    ("ModKit multiplayer guard shows redirect banner after online detection", ModKitMultiplayerGuardShowsRedirectBannerAfterOnlineDetection),
    ("ModKit multiplayer policy treats lobby and joining states as active online", ModKitMultiplayerPolicyTreatsLobbyAndJoiningAsActive),
    ("ModKit multiplayer policy treats disconnected states as inactive", ModKitMultiplayerPolicyTreatsDisconnectedStatesAsInactive),
    ("ModKit runtime policy blocks manifest-invalid enabled mods", ModKitRuntimePolicyBlocksManifestInvalidEnabledMods),
    ("PatchContext TryGet returns active state atomically", PatchContextTryGetReturnsAtomicState),
    ("PatchGuard refuses duplicate patch on same Harmony id", PatchGuardRefusesDuplicatePatch),
    ("ModKitPaths rejects reserved DOS names", ModKitPathsRejectsReservedNames),
    ("ModKitPaths rejects path-traversal attempts", ModKitPathsRejectsPathTraversal),
    ("ModKitManifestValidator rejects self-dependency", ModKitManifestValidatorRejectsSelfDependency),
    ("ModKitManifestValidator detects two-node cycles", ModKitManifestValidatorDetectsTwoNodeCycles),
    ("ModKitManifestValidator detects three-node cycles", ModKitManifestValidatorDetectsThreeNodeCycles),
    ("ModKitLogFormat Quote escapes control characters", ModLogFormatQuoteEscapesControlChars),
    ("ModKitConfig Save returns false on IO failure", ModConfigSaveReturnsFalseOnIoFailure),
    ("ModKitRegistry replaces managed mods by id", ModKitRegistryReplacesManagedModsById),
    ("ModKitRegistry tracks UI owner", ModKitRegistryTracksUiOwner),
    ("ModKitModMetadata carries license, sdk version, and dependencies", ModKitModMetadataCarriesNewFields),
    ("ModKitModMetadata defaults optional metadata fields", ModKitModMetadataDefaultsOptionalFields),
    ("ModKitManifestAttribute round-trips every field", ModKitManifestAttributeRoundTripsEveryField),
    ("ModKitMetadataResolver uses fallbacks when attribute missing", ModKitMetadataResolverUsesFallbacksWhenAttributeMissing),
    ("ModKitMetadataResolver populates from attribute when present", ModKitMetadataResolverPopulatesFromAttribute),
    ("ModKitMetadataResolver merges null attribute fields with fallbacks", ModKitMetadataResolverMergesNullAttributeFields),
    ("ModKitMelonMod metadata picks up manifest attribute fields", ModKitMelonModMetadataUsesManifestAttribute),
    ("ModKitDependencyParser parses bare ids", ModKitDependencyParserParsesBareIds),
    ("ModKitDependencyParser parses minimum version constraints", ModKitDependencyParserParsesMinVersionConstraints),
    ("ModKitDependencyParser parses optional dependency specs", ModKitDependencyParserParsesOptionalSpecs),
    ("ModKitDependencyParser rejects malformed dependency specs", ModKitDependencyParserRejectsMalformedSpecs),
    ("ModKitManifestValidator returns Ok when nothing to check", ModKitManifestValidatorReturnsOkForEmptyConstraints),
    ("ModKitManifestValidator reports SDK version below MinSdkVersion", ModKitManifestValidatorReportsSdkVersionTooLow),
    ("ModKitManifestValidator reports missing dependency", ModKitManifestValidatorReportsMissingDependency),
    ("ModKitManifestValidator reports outdated dependency", ModKitManifestValidatorReportsOutdatedDependency),
    ("ModKitManifestValidator skips missing optional dependency", ModKitManifestValidatorSkipsMissingOptionalDependency),
    ("ModKitManifestValidator reports outdated optional dependency", ModKitManifestValidatorReportsOutdatedOptionalDependency),
    ("ModKitManifestValidator approves satisfied dependency constraints", ModKitManifestValidatorApprovesSatisfiedConstraints),
    ("ModKitManifestValidator matches dependency ids case-insensitively", ModKitManifestValidatorMatchesIdsCaseInsensitively),
    ("ModKitManifestValidator collects multiple issues", ModKitManifestValidatorCollectsMultipleIssues),
    ("ModKitRegistry validates mods on register", ModKitRegistryValidatesOnRegister),
    ("ModKitRegistry returns Ok for unknown mod ids", ModKitRegistryReturnsOkForUnknownIds),
    ("ModKitRegistry revalidates dependents when dependency joins", ModKitRegistryRevalidatesAfterDependencyRegistered),
    ("ModKitRegistry revalidates dependents when dependency unregisters", ModKitRegistryRevalidatesAfterDependencyUnregistered),
    ("ModKitRegistry surfaces self-check issues", ModKitRegistrySurfacesSelfCheckIssues),
    ("ModKit UI About policy formats license and sdk version", ModKitUiAboutPolicyFormatsLicenseAndSdk),
    ("ModKit UI About policy formats dependencies and validation status", ModKitUiAboutPolicyFormatsDependenciesAndValidation),
    ("ModKit UI mod list policy shows groups for multiple categories", ModKitUiModListPolicyShowsGroupsForMultipleCategories),
    ("ModKit UI log viewer policy takes last lines", ModKitUiLogViewerPolicyTakesLastLines),
    ("ModKit UI blocks manifest-invalid config enabled edits", ModKitUiBlocksManifestInvalidConfigEnabledEdits),
    ("SDK release surface excludes gameplay mods", SdkReleaseSurfaceExcludesGameplayMods),
    ("SDK release surface uses Babyhamsta project metadata", SdkReleaseSurfaceUsesBabyhamstaProjectMetadata),
    ("ModKitFileLog Header appends once and preserves prior content", ModFileLogHeaderAppendsOnceAndPreservesPriorContent),
    ("ModKitFileLog Info forwards to console and appends to file", ModFileLogInfoForwardsToConsoleAndAppendsToFile),
    ("ModKitFileLog Info honors LogToConsole and LogToFile flags", ModFileLogInfoHonorsToggles),
    ("ModKitFileLog Warn always emits to warn callback", ModFileLogWarnAlwaysEmits),
    ("ModKitFileLog Debug writes file only", ModKitFileLogDebugWritesFileOnly),
    ("ModKitLogFormat Quote escapes backslash and double-quote", ModLogFormatQuoteEscapes),
    ("ModKitLogFormat KeyValue produces space-prefixed key=value", ModLogFormatKeyValueProducesPair),
    ("ModKitLogFormat Vector3 formats invariantly with three decimals", ModLogFormatVector3InvariantFormatting),
    ("PatchContext starts inactive with null state", PatchContextStartsInactive),
    ("PatchContext stores state and activates on demand", PatchContextStoresStateAndActivates),
    ("PatchContext keeps state per type parameter", PatchContextKeepsStatePerType),
    ("PatchContext Clear resets state and deactivates", PatchContextClearResetsState),
    ("Hotkeys fires callback when input source reports key down", HotkeysFiresCallbackOnKeyDown),
    ("Hotkeys does not fire callback when key not pressed", HotkeysDoesNotFireWhenKeyAbsent),
    ("Hotkeys Unregister removes binding", HotkeysUnregisterRemovesBinding),
    ("Hotkeys rejects duplicate names", HotkeysRejectsDuplicateNames),
    ("Hotkeys swallows callback exceptions to one warn", HotkeysSwallowsCallbackException),
    ("Hotkeys exposes bindings and rebinds by name", HotkeysExposesBindingsAndRebindsByName),
    ("ModKitObjectSnapshot captures selected members", ModKitObjectSnapshotCapturesSelectedMembers),
    ("ModKitObjectSnapshot captures vector-shaped members", ModKitObjectSnapshotCapturesVectorShapedMembers),
    ("ModKitStrings loads overrides and formats values", ModKitStringsLoadsOverridesAndFormatsValues),
    ("ModKitMelonMod exposes verbose logging probe", ModKitMelonModExposesVerboseLoggingProbe)
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
    var type = ModKitTypeResolver.ResolveFullName("System.String");
    AssertNotNull(type, "System.String should resolve.");
    AssertEqual(typeof(string), type, "Resolved type should be System.String.");
}

static void TypeResolverReturnsNullForMissingName()
{
    var warnings = new List<string>();
    var type = ModKitTypeResolver.ResolveFullName("Missing.Namespace.TypeName", warnings.Add);
    AssertNull(type, "Missing type should return null.");
    AssertTrue(warnings.Count == 1, "Missing type should emit one warning.");
}

static void ModKitPathsBuildsConfigPaths()
{
    var root = Path.Combine(Path.GetTempPath(), "fl-modkit-paths");
    var configPath = ModKitPaths.ConfigPath("example-mod", root);
    var logPath = ModKitPaths.LogPath("example.log", root);

    AssertEqual(Path.Combine(root, "example-mod.json"), configPath, "Config path should use mod id JSON file.");
    AssertEqual(Path.Combine(root, "example.log"), logPath, "Log path should use requested file name.");
}

static void ModKitPathsRejectsInvalidFileNames()
{
    var invalidName = "bad:name";

    if (Path.GetInvalidFileNameChars().Contains(':'))
    {
        AssertThrows<ArgumentException>(
            () => ModKitPaths.ConfigPath(invalidName, Path.GetTempPath()),
            "Invalid config file names should throw.");
    }

    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath(" ", Path.GetTempPath()),
        "Blank config file names should throw.");
}

static void ModConfigCreatesDefaultConfig()
{
    var tempRoot = NewTempRoot();
    try
    {
        var defaults = new TestConfig { Enabled = true, Count = 7 };
        var config = ModKitConfig.LoadOrCreate("unit-test", defaults, tempRoot);
        var path = ModKitConfig.GetConfigPath("unit-test", tempRoot);

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
        var path = ModKitConfig.GetConfigPath("invalid-json", tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ bad json");

        var warnings = new List<string>();
        var defaults = new TestConfig { Enabled = false, Count = 3 };
        var config = ModKitConfig.LoadOrCreate("invalid-json", defaults, tempRoot, warn: warnings.Add);

        AssertTrue(!config.Enabled, "Invalid JSON should return default Enabled.");
        AssertEqual(3, config.Count, "Invalid JSON should return default Count.");
        AssertTrue(warnings.Count == 1, "Invalid JSON should emit one warning.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModConfigDetectsNewerHotReloadTimestamp()
{
    var loadedAt = new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero);

    AssertTrue(
        ModKitConfig.ShouldReload(loadedAt, loadedAt.AddSeconds(1)),
        "Newer config write timestamp should trigger reload.");

    AssertTrue(
        !ModKitConfig.ShouldReload(loadedAt, loadedAt),
        "Same config write timestamp should not trigger reload.");

    AssertTrue(
        !ModKitConfig.ShouldReload(loadedAt, null),
        "Missing config write timestamp should not trigger reload.");
}

static void ModKitConfigReadsVersionMetadata()
{
    var tempRoot = NewTempRoot();
    try
    {
        var path = ModKitConfig.GetConfigPath("versioned-load", tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, """
        {
          "__configVersion": 1,
          "Enabled": true,
          "Count": 11
        }
        """);

        var result = ModKitConfig.LoadOrCreateWithVersion("versioned-load", new VersionedConfig(), tempRoot);

        AssertEqual(1, result.LoadedVersion, "Loaded version should come from __configVersion.");
        AssertEqual(2, result.TargetVersion, "Target version should come from attribute.");
        AssertTrue(result.Config.Enabled, "Loaded config values should deserialize normally.");
        AssertEqual(11, result.Config.Count, "Loaded Count should deserialize normally.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModKitConfigSavesVersionMetadata()
{
    var tempRoot = NewTempRoot();
    try
    {
        ModKitConfig.Save("versioned-save", new VersionedConfig { Enabled = true, Count = 4 }, tempRoot);
        ModKitConfig.Save("plain-save", new TestConfig { Enabled = true, Count = 4 }, tempRoot);

        var versionedJson = File.ReadAllText(ModKitConfig.GetConfigPath("versioned-save", tempRoot));
        var plainJson = File.ReadAllText(ModKitConfig.GetConfigPath("plain-save", tempRoot));

        AssertTrue(versionedJson.Contains("\"__configVersion\": 2", StringComparison.Ordinal), "Versioned config should persist __configVersion.");
        AssertTrue(!plainJson.Contains("__configVersion", StringComparison.Ordinal), "Unversioned config should not persist __configVersion.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModKitConfigFallsBackWhenVersionMetadataMalformed()
{
    var tempRoot = NewTempRoot();
    try
    {
        var path = ModKitConfig.GetConfigPath("bad-version-load", tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, """
        {
          "__configVersion": "bad",
          "Enabled": true,
          "Count": 11
        }
        """);
        var warnings = new List<string>();

        var result = ModKitConfig.LoadOrCreateWithVersion(
            "bad-version-load",
            new VersionedConfig { Enabled = false, Count = 3 },
            tempRoot,
            warn: warnings.Add);

        AssertTrue(!result.Config.Enabled, "Malformed version metadata should fall back to default config.");
        AssertEqual(3, result.Config.Count, "Malformed version metadata should keep default config values.");
        AssertEqual(2, result.LoadedVersion, "Fallback result should report the target version.");
        AssertTrue(warnings.Count == 1, "Malformed version metadata should emit one warning.");
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

static void ModKitMelonModExposesLifecycleWrappers()
{
    var type = typeof(ModKitMelonMod<TestConfig>);
    var initialize = type.GetMethod(nameof(ModKitMelonMod<TestConfig>.OnInitializeMelon));
    var update = type.GetMethod(nameof(ModKitMelonMod<TestConfig>.OnUpdate));
    var sceneLoaded = type.GetMethod(nameof(ModKitMelonMod<TestConfig>.OnSceneWasLoaded));
    var sceneHook = type.GetMethod(
        "OnModKitSceneWasLoaded",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

    AssertNotNull(initialize, "Base mod should expose OnInitializeMelon.");
    AssertNotNull(update, "Base mod should expose OnUpdate.");
    AssertNotNull(sceneLoaded, "Base mod should expose OnSceneWasLoaded.");
    AssertNotNull(sceneHook, "Base mod should expose OnModKitSceneWasLoaded.");
    AssertTrue(initialize!.IsFinal, "OnInitializeMelon should be sealed.");
    AssertTrue(update!.IsFinal, "OnUpdate should be sealed.");
    AssertTrue(sceneLoaded!.IsFinal, "OnSceneWasLoaded should be sealed.");
}

static void ModKitMelonModExposesPatchHelpers()
{
    var type = typeof(ModKitMelonMod<TestConfig>);
    var postfix = type.GetMethod(
        "PatchPostfix",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    var prefix = type.GetMethod(
        "PatchPrefix",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

    AssertNotNull(postfix, "Base mod should expose protected postfix helper.");
    AssertNotNull(prefix, "Base mod should expose protected prefix helper.");
}

static void ModKitConfigAdapterPullsCurrentConfigValues()
{
    var config = new UiTestConfig
    {
        Enabled = true,
        Count = 6,
        Distance = 12.5,
        Name = "Traffic Diagnostics",
        Mode = UiTestMode.Advanced,
        Tags = ["traffic", "wp"]
    };
    var adapter = new ModKitConfigAdapter<UiTestConfig>("ui-test", () => config, value => config = value, NewTempRoot());

    var properties = adapter.GetProperties();

    AssertEqual("true", FindProperty(properties, nameof(UiTestConfig.Enabled)).ValueText, "Bool property should be pulled from live config.");
    AssertEqual("6", FindProperty(properties, nameof(UiTestConfig.Count)).ValueText, "Integer property should be pulled from live config.");
    AssertEqual("12.5", FindProperty(properties, nameof(UiTestConfig.Distance)).ValueText, "Double property should be invariant.");
    AssertEqual("Traffic Diagnostics", FindProperty(properties, nameof(UiTestConfig.Name)).ValueText, "String property should be pulled from live config.");
    AssertEqual("Advanced", FindProperty(properties, nameof(UiTestConfig.Mode)).ValueText, "Enum property should use enum name.");
    AssertEqual("traffic\nwp", FindProperty(properties, nameof(UiTestConfig.Tags)).ValueText, "String arrays should be newline-delimited for editing.");
}

static void ModKitConfigAdapterExposesDisplayAndRangeMetadata()
{
    var config = new UiTestConfig { Distance = 350, Preset = "Far" };
    var adapter = new ModKitConfigAdapter<UiTestConfig>("ui-test", () => config, value => config = value, NewTempRoot());

    var distance = FindProperty(adapter.GetProperties(), nameof(UiTestConfig.Distance));
    var preset = FindProperty(adapter.GetProperties(), nameof(UiTestConfig.Preset));

    AssertEqual("Traffic Distance (m)", distance.DisplayName, "Display attribute should override generated config labels.");
    AssertEqual(150d, distance.Minimum, "Range attribute should expose minimum slider value.");
    AssertEqual(500d, distance.Maximum, "Range attribute should expose maximum slider value.");
    AssertEqual(50d, distance.Step, "Range attribute should expose slider step.");
    AssertEqual(3, preset.Options.Count, "Options attribute should expose dropdown choices.");
    AssertEqual("Far", preset.ValueText, "String option properties should keep their current value.");
    AssertEqual("Maximum", preset.Options[2], "Dropdown choices should preserve declaration order.");
}

static void ModKitConfigAdapterUpdatesWritableConfigProperties()
{
    var config = new UiTestConfig();
    var adapter = new ModKitConfigAdapter<UiTestConfig>("ui-test", () => config, value => config = value, NewTempRoot());

    AssertTrue(adapter.TrySetProperty(nameof(UiTestConfig.Enabled), "true", out var enabledError), enabledError);
    AssertTrue(adapter.TrySetProperty(nameof(UiTestConfig.Count), "42", out var countError), countError);
    AssertTrue(adapter.TrySetProperty(nameof(UiTestConfig.Distance), "7.25", out var distanceError), distanceError);
    AssertTrue(adapter.TrySetProperty(nameof(UiTestConfig.Name), "Updated", out var nameError), nameError);
    AssertTrue(adapter.TrySetProperty(nameof(UiTestConfig.Mode), "Advanced", out var modeError), modeError);
    AssertTrue(adapter.TrySetProperty(nameof(UiTestConfig.Tags), "one\ntwo", out var tagsError), tagsError);

    AssertTrue(config.Enabled, "Bool property should update.");
    AssertEqual(42, config.Count, "Integer property should update.");
    AssertEqual(7.25, config.Distance, "Double property should update.");
    AssertEqual("Updated", config.Name, "String property should update.");
    AssertEqual(UiTestMode.Advanced, config.Mode, "Enum property should update.");
    AssertEqual("one", config.Tags[0], "First array item should update.");
    AssertEqual("two", config.Tags[1], "Second array item should update.");

    AssertTrue(!adapter.TrySetProperty(nameof(UiTestConfig.Count), "not-number", out var invalidError), "Invalid numbers should fail.");
    AssertTrue(invalidError.Contains(nameof(UiTestConfig.Count), StringComparison.Ordinal), "Error should name the invalid property.");
}

static void ModKitConfigAdapterNotifiesAfterWritableConfigUpdates()
{
    var notifications = 0;
    var config = new UiTestConfig();
    var adapter = new ModKitConfigAdapter<UiTestConfig>(
        "ui-test",
        () => config,
        value => config = value,
        NewTempRoot(),
        onChanged: _ => notifications++);

    AssertTrue(adapter.TrySetProperty(nameof(UiTestConfig.Enabled), "true", out var error), error);

    AssertEqual(1, notifications, "Writable property update should notify the owning mod.");
}

static void ModKitConfigAdapterSavesAndReloadsConfigFiles()
{
    var tempRoot = NewTempRoot();
    try
    {
        var config = new UiTestConfig { Enabled = true, Count = 11, Name = "Saved" };
        var adapter = new ModKitConfigAdapter<UiTestConfig>("ui-test", () => config, value => config = value, tempRoot);

        adapter.Save();
        config = new UiTestConfig();
        adapter.Reload();

        AssertTrue(config.Enabled, "Reload should apply saved bool property.");
        AssertEqual(11, config.Count, "Reload should apply saved integer property.");
        AssertEqual("Saved", config.Name, "Reload should apply saved string property.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModKitUiBlocksMouseAndKeyboardOverlayEvents()
{
    AssertTrue(ModKitUiInputPolicy.ShouldBlockEvent(EventType.MouseDown), "Mouse down should be blocked by the overlay.");
    AssertTrue(ModKitUiInputPolicy.ShouldBlockEvent(EventType.MouseUp), "Mouse up should be blocked by the overlay.");
    AssertTrue(ModKitUiInputPolicy.ShouldBlockEvent(EventType.MouseDrag), "Mouse drag should be blocked by the overlay.");
    AssertTrue(ModKitUiInputPolicy.ShouldBlockEvent(EventType.ScrollWheel), "Scroll wheel should be blocked by the overlay.");
    AssertTrue(ModKitUiInputPolicy.ShouldBlockEvent(EventType.KeyDown), "Key down should be blocked by the overlay.");
    AssertTrue(ModKitUiInputPolicy.ShouldBlockEvent(EventType.KeyUp), "Key up should be blocked by the overlay.");
    AssertTrue(!ModKitUiInputPolicy.ShouldBlockEvent(EventType.Layout), "Layout should not be consumed.");
    AssertTrue(!ModKitUiInputPolicy.ShouldBlockEvent(EventType.Repaint), "Repaint should not be consumed.");
}

static void ModKitUiCloseKeysIncludeInsertAndEscape()
{
    AssertTrue(
        ModKitUiInputPolicy.ShouldCloseOverlay(EventType.KeyDown, KeyCode.Insert),
        "Insert key down should close the overlay from IMGUI events.");

    AssertTrue(
        ModKitUiInputPolicy.ShouldCloseOverlay(EventType.KeyDown, KeyCode.Escape),
        "Escape key down should close the overlay from IMGUI events.");

    AssertTrue(
        !ModKitUiInputPolicy.ShouldCloseOverlay(EventType.KeyUp, KeyCode.Insert),
        "Key up should not close the overlay.");
}

static void ModKitUiDefersCursorRestoreAfterClose()
{
    AssertTrue(
        ModKitUiInputPolicy.DeferredCursorRestoreFrames >= 2,
        "Cursor restore should be re-applied for multiple frames after closing the overlay.");
}

static void ModKitUiTabsSelectDistinctPanes()
{
    AssertEqual(
        ModKitUiPane.Mods | ModKitUiPane.SelectedMod,
        ModKitUiLayoutPolicy.GetVisiblePanes(ModKitUiTab.Mods),
        "Mods tab should show mod list and selected mod details.");

    AssertEqual(
        ModKitUiPane.Mods | ModKitUiPane.Config,
        ModKitUiLayoutPolicy.GetVisiblePanes(ModKitUiTab.Config),
        "Config tab should show mod list and selected mod config.");

    AssertEqual(
        ModKitUiPane.Logs,
        ModKitUiLayoutPolicy.GetVisiblePanes(ModKitUiTab.Logs),
        "Logs tab should show logs only.");

    AssertEqual(
        ModKitUiPane.About,
        ModKitUiLayoutPolicy.GetVisiblePanes(ModKitUiTab.About),
        "About tab should show about only.");
}

static void ModKitUiLayoutKeepsFooterInside1080p()
{
    var bodyHeight = ModKitUiLayoutPolicy.CalculateBodyHeight(1080);
    var totalHeight = ModKitUiLayoutPolicy.HeaderHeight + ModKitUiLayoutPolicy.VerticalGap + bodyHeight + ModKitUiLayoutPolicy.FooterHeight;

    AssertTrue(totalHeight <= 1080, "Header, body, gap, and footer should fit within a 1080p screen.");
}

static void ModKitMultiplayerGuardBlocksOnlineWhenEnabledModsExist()
{
    AssertTrue(
        ModKitMultiplayerPolicy.ShouldBlockOnline(hasEnabledMods: true),
        "Online play should be blocked when any SDK-managed mod is enabled.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldBlockOnline(hasEnabledMods: false),
        "Online play should remain available when all SDK-managed mods are disabled.");

    AssertTrue(
        ModKitMultiplayerPolicy.OnlineBlockedMessage.Contains("mods", StringComparison.OrdinalIgnoreCase),
        "Blocked online message should explain the mod safety reason.");
}

static void ModKitMultiplayerGuardLocksEnablingModsWhileInMultiplayer()
{
    AssertTrue(
        !ModKitMultiplayerPolicy.CanEnableMods(isInOnlineMultiplayer: true),
        "SDK-managed mods should not be enabled while connected to online multiplayer.");

    AssertTrue(
        ModKitMultiplayerPolicy.CanEnableMods(isInOnlineMultiplayer: false),
        "SDK-managed mods should be enableable again outside online multiplayer.");
}

static void ModKitMultiplayerGuardBlocksOnlineRoomOperationsOnly()
{
    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: true, isOfflineMode: false, isOnlineRoomOperationReady: true, "ConnectUsingSettings"),
        "Photon ConnectUsingSettings should not be blocked because single player can use Photon setup before offline mode is active.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(
            hasEnabledMods: true,
            isOfflineMode: false,
            isOnlineRoomOperationReady: false,
            "CreateRoom",
            "Singleplayer (id:)",
            maxPlayers: 1),
        "Singleplayer Photon CreateRoom should remain available even when IL2CPP state reflection is incomplete.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(
            hasEnabledMods: true,
            isOfflineMode: false,
            isOnlineRoomOperationReady: false,
            "CreateRoom",
            "PD_Room",
            maxPlayers: 10),
        "Known local voice-room CreateRoom should remain available.");

    AssertTrue(
        ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(
            hasEnabledMods: true,
            isOfflineMode: false,
            isOnlineRoomOperationReady: false,
            "CreateRoom",
            "Hamsta Game (id:2715)",
            maxPlayers: 10),
        "Online Photon CreateRoom should be blocked when managed mods are enabled.");

    AssertTrue(
        ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: true, isOfflineMode: false, isOnlineRoomOperationReady: false, "JoinOrCreateRoom"),
        "Online Photon JoinOrCreateRoom should be blocked when managed mods are enabled.");

    AssertTrue(
        ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: true, isOfflineMode: false, isOnlineRoomOperationReady: false, "JoinRoom"),
        "Online Photon JoinRoom should be blocked when managed mods are enabled.");

    AssertTrue(
        ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: true, isOfflineMode: false, isOnlineRoomOperationReady: false, "JoinRandomRoom"),
        "Online Photon JoinRandomRoom should be blocked when managed mods are enabled.");

    AssertTrue(
        ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: true, isOfflineMode: false, isOnlineRoomOperationReady: false, "ReJoinRoom"),
        "Online Photon ReJoinRoom should be blocked when managed mods are enabled.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: true, isOfflineMode: true, isOnlineRoomOperationReady: true, "CreateRoom"),
        "Offline Photon CreateRoom should remain available for single player.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: true, isOfflineMode: true, isOnlineRoomOperationReady: true, "JoinRoom"),
        "Offline Photon JoinRoom should remain available for single player.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldBlockPhotonOperation(hasEnabledMods: false, isOfflineMode: false, isOnlineRoomOperationReady: true, "CreateRoom"),
        "Photon room operations should remain available when managed mods are disabled.");
}

static void ModKitMultiplayerGuardRedirectsAfterOnlineRoomDetection()
{
    AssertTrue(
        ModKitMultiplayerPolicy.ShouldRedirectFromOnlineSession(hasEnabledMods: true, isInOnlineMultiplayer: true),
        "Online multiplayer sessions should redirect when managed mods are enabled.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldRedirectFromOnlineSession(hasEnabledMods: true, isInOnlineMultiplayer: false),
        "Single player should not redirect just because managed mods are enabled.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldRedirectFromOnlineSession(hasEnabledMods: false, isInOnlineMultiplayer: true),
        "Online multiplayer should not redirect when no managed mods are enabled.");
}

static void ModKitMultiplayerGuardShowsRedirectBannerAfterOnlineDetection()
{
    AssertTrue(
        ModKitMultiplayerPolicy.ShouldShowOnlineBlockedUi(hasEnabledMods: true, isMainMenuScene: true, recentlyBlockedOnlineSession: false),
        "Main menu should show the online block banner whenever managed mods are enabled.");

    AssertTrue(
        ModKitMultiplayerPolicy.ShouldShowOnlineBlockedUi(hasEnabledMods: true, isMainMenuScene: false, recentlyBlockedOnlineSession: true),
        "Online multiplayer detection should show online disabled UI even outside the initial menu.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldShowOnlineBlockedUi(hasEnabledMods: true, isMainMenuScene: false, recentlyBlockedOnlineSession: false),
        "Non-menu scenes should not show the online block banner unless online multiplayer was detected.");

    AssertTrue(
        !ModKitMultiplayerPolicy.ShouldShowOnlineBlockedUi(hasEnabledMods: false, isMainMenuScene: true, recentlyBlockedOnlineSession: true),
        "Online disabled UI should not show when managed mods are disabled.");
}

static void PatchContextTryGetReturnsAtomicState()
{
    PatchContext<PatchStateA>.Clear();
    AssertTrue(!PatchContext<PatchStateA>.TryGet(out _), "TryGet should be false when context is cleared.");

    PatchContext<PatchStateA>.Set(new PatchStateA { Value = "atomic" });
    AssertTrue(!PatchContext<PatchStateA>.TryGet(out _), "TryGet should be false after Set without SetActive.");

    PatchContext<PatchStateA>.SetActive(true);
    AssertTrue(PatchContext<PatchStateA>.TryGet(out var state), "TryGet should be true once both Set and SetActive ran.");
    AssertEqual("atomic", state.Value, "TryGet should return the stored state.");

    PatchContext<PatchStateA>.Clear();
    AssertTrue(!PatchContext<PatchStateA>.TryGet(out _), "TryGet should be false after Clear.");
}

static void PatchGuardRefusesDuplicatePatch()
{
    PatchGuard.ClearForTests();
    var harmony = new HarmonyLib.Harmony("flashinglights.modkit.tests.patchguard.dedupe");
    var warnings = new List<string>();
    var postfix = typeof(PatchGuardSignatureTargets).GetMethod(nameof(PatchGuardSignatureTargets.Postfix))!;

    var first = PatchGuard.PatchPostfix(harmony, typeof(string), nameof(ToString), postfix, warnings.Add, parameters: Type.EmptyTypes);
    var second = PatchGuard.PatchPostfix(harmony, typeof(string), nameof(ToString), postfix, warnings.Add, parameters: Type.EmptyTypes);

    AssertTrue(first, $"First patch should succeed. Warnings: {string.Join(", ", warnings)}");
    AssertTrue(!second, "Second patch on the same Harmony id and target should be refused.");
    AssertTrue(warnings.Any(w => w.Contains("already installed", StringComparison.Ordinal)), "Duplicate patch should produce a warning.");

    harmony.UnpatchSelf();
    PatchGuard.ClearForTests();
}

static void ModKitPathsRejectsReservedNames()
{
    var root = Path.Combine(Path.GetTempPath(), "fl-modkit-paths-reserved");

    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath("CON", root),
        "Reserved DOS name CON should throw.");
    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath("nul", root),
        "Reserved DOS name nul should throw.");
    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath("LPT3", root),
        "Reserved DOS name LPT3 should throw.");
}

static void ModKitPathsRejectsPathTraversal()
{
    var root = Path.Combine(Path.GetTempPath(), "fl-modkit-paths-traversal");

    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath("..", root),
        "Bare .. should throw (leading/trailing dot).");
    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath(".hidden", root),
        "Leading dot should be rejected.");
    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath("trailing.", root),
        "Trailing dot should be rejected.");
    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath("with/slash", root),
        "Forward slash should be rejected.");
    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath("with\\backslash", root),
        "Backslash should be rejected.");
    AssertThrows<ArgumentException>(
        () => ModKitPaths.ConfigPath(" leading-space", root),
        "Leading whitespace should be rejected.");
}

static void ModKitManifestValidatorRejectsSelfDependency()
{
    var metadata = new ModKitModMetadata("self.mod", "Self", "1.0.0", "Tests")
    {
        Dependencies = new[] { "self.mod>=1.0.0" }
    };
    var registered = new[] { metadata };

    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", registered);

    AssertTrue(!result.IsValid, "Self-dependency should fail validation.");
    AssertTrue(
        result.Issues.Any(i => i.Contains("cannot depend on itself", StringComparison.Ordinal)),
        $"Issue should explain self-dep. Issues: {string.Join(", ", result.Issues)}");
}

static void ModKitManifestValidatorDetectsTwoNodeCycles()
{
    var subject = new ModKitModMetadata("mod.a", "A", "1.0.0", "Tests")
    {
        Dependencies = new[] { "mod.b" }
    };
    var registered = new[]
    {
        subject,
        new ModKitModMetadata("mod.b", "B", "1.0.0", "Tests")
        {
            Dependencies = new[] { "mod.a" }
        }
    };

    var result = ModKitManifestValidator.Validate(subject, "1.0.0", registered);

    AssertTrue(!result.IsValid, "Two-node dependency cycle should fail validation.");
    AssertTrue(result.Issues.Any(issue => issue.Contains("mod.a", StringComparison.OrdinalIgnoreCase) && issue.Contains("mod.b", StringComparison.OrdinalIgnoreCase)), "Cycle issue should name the cycle participants.");
}

static void ModKitManifestValidatorDetectsThreeNodeCycles()
{
    var subject = new ModKitModMetadata("mod.a", "A", "1.0.0", "Tests")
    {
        Dependencies = new[] { "mod.b" }
    };
    var registered = new[]
    {
        subject,
        new ModKitModMetadata("mod.b", "B", "1.0.0", "Tests")
        {
            Dependencies = new[] { "mod.c" }
        },
        new ModKitModMetadata("mod.c", "C", "1.0.0", "Tests")
        {
            Dependencies = new[] { "mod.a" }
        }
    };

    var result = ModKitManifestValidator.Validate(subject, "1.0.0", registered);

    AssertTrue(!result.IsValid, "Three-node dependency cycle should fail validation.");
    AssertTrue(result.Issues.Any(issue => issue.Contains("dependency cycle", StringComparison.OrdinalIgnoreCase)), "Validation should report a dependency cycle.");
}

static void ModLogFormatQuoteEscapesControlChars()
{
    AssertEqual("\"line1\\nline2\"", ModKitLogFormat.Quote("line1\nline2"), "Newline should be escaped.");
    AssertEqual("\"col1\\tcol2\"", ModKitLogFormat.Quote("col1\tcol2"), "Tab should be escaped.");
    AssertEqual("\"a\\rb\"", ModKitLogFormat.Quote("a\rb"), "Carriage return should be escaped.");
    AssertEqual("\"x\\u0001y\"", ModKitLogFormat.Quote("xy"), "Generic control chars should be hex-escaped.");
}

static void ModConfigSaveReturnsFalseOnIoFailure()
{
    var tempRoot = NewTempRoot();
    try
    {
        Directory.CreateDirectory(tempRoot);
        var ok = ModKitConfig.Save("save-ok", new TestConfig { Enabled = true, Count = 1 }, tempRoot);
        AssertTrue(ok, "Successful save should return true.");

        var path = ModKitConfig.GetConfigPath("save-locked", tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var locker = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        var warnings = new List<string>();
        var failed = ModKitConfig.Save("save-locked", new TestConfig(), tempRoot, warnings.Add);
        AssertTrue(!failed, "Save should return false when the file is locked.");
        AssertTrue(warnings.Count >= 1, "Save failure should warn.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModKitMultiplayerPolicyTreatsLobbyAndJoiningAsActive()
{
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("Authenticating"), "Authenticating should count as active online presence.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("ConnectingToMasterserver"), "ConnectingToMasterserver should count as active.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("ConnectedToMaster"), "ConnectedToMaster should count as active.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("JoiningLobby"), "JoiningLobby should count as active.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("JoinedLobby"), "JoinedLobby should count as active.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("ConnectingToGameserver"), "ConnectingToGameserver should count as active.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("Joining"), "Joining should count as active.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("Joined"), "Joined should count as active.");
    AssertTrue(ModKitMultiplayerPolicy.IsActiveOnlineClientState("Leaving"), "Leaving should count as active until fully disconnected.");
}

static void ModKitMultiplayerPolicyTreatsDisconnectedStatesAsInactive()
{
    AssertTrue(!ModKitMultiplayerPolicy.IsActiveOnlineClientState(null), "Null state should not count as active.");
    AssertTrue(!ModKitMultiplayerPolicy.IsActiveOnlineClientState(string.Empty), "Empty state should not count as active.");
    AssertTrue(!ModKitMultiplayerPolicy.IsActiveOnlineClientState("Uninitialized"), "Uninitialized should not count as active.");
    AssertTrue(!ModKitMultiplayerPolicy.IsActiveOnlineClientState("PeerCreated"), "PeerCreated should not count as active.");
    AssertTrue(!ModKitMultiplayerPolicy.IsActiveOnlineClientState("Disconnected"), "Disconnected should not count as active.");
    AssertTrue(!ModKitMultiplayerPolicy.IsActiveOnlineClientState("Disconnecting"), "Disconnecting should not count as active.");
    AssertTrue(!ModKitMultiplayerPolicy.IsActiveOnlineClientState("ConnectingToNameServer"), "ConnectingToNameServer is the initial-handshake step and should not count.");
}

static void ModKitRegistryReplacesManagedModsById()
{
    ModKitRegistry.ClearForTests();
    var first = new TestManagedMod("same-id", "First");
    var second = new TestManagedMod("same-id", "Second");

    ModKitRegistry.Register(first);
    ModKitRegistry.Register(second);

    AssertEqual(1, ModKitRegistry.ManagedMods.Count, "Registering the same id should replace the existing entry.");
    AssertEqual("Second", ModKitRegistry.ManagedMods[0].Metadata.DisplayName, "Latest registration should win.");
}

static void ModKitRegistryTracksUiOwner()
{
    ModKitRegistry.ClearForTests();
    var first = new TestManagedMod("first", "First");
    var second = new TestManagedMod("second", "Second");

    ModKitRegistry.Register(first);
    ModKitRegistry.Register(second);

    AssertTrue(ModKitRegistry.IsUiOwner("first"), "First registered managed mod should own the UI host.");
    ModKitRegistry.Unregister("first");
    AssertTrue(ModKitRegistry.IsUiOwner("second"), "Next managed mod should become UI owner after owner unregisters.");
}

static void ModKitModMetadataCarriesNewFields()
{
    var metadata = new ModKitModMetadata(
        Id: "test.mod",
        DisplayName: "Test Mod",
        Version: "1.2.3",
        Author: "Tester",
        GitHubUrl: "https://example.com/repo",
        Changelog: "Initial release.",
        License: "MIT",
        MinSdkVersion: "0.2.0",
        Category: "Gameplay")
    {
        Dependencies = new[] { "other.mod>=1.0.0", "third.mod" },
        Tags = new[] { "traffic", "debug" }
    };

    AssertEqual("MIT", metadata.License, "License should be carried.");
    AssertEqual("0.2.0", metadata.MinSdkVersion, "MinSdkVersion should be carried.");
    AssertEqual(2, metadata.Dependencies.Count, "Dependencies should be carried.");
    AssertEqual("other.mod>=1.0.0", metadata.Dependencies[0], "Dependency entry should round-trip.");
    AssertEqual("third.mod", metadata.Dependencies[1], "Bare dependency entry should round-trip.");
    AssertEqual("Gameplay", metadata.Category, "Category should be carried.");
    AssertEqual(2, metadata.Tags.Count, "Tags should be carried.");
}

static void ModKitModMetadataDefaultsOptionalFields()
{
    var metadata = new ModKitModMetadata("test.mod", "Test Mod", "1.0.0", "Tester");

    AssertNull(metadata.GitHubUrl, "GitHubUrl should default to null.");
    AssertNull(metadata.Changelog, "Changelog should default to null.");
    AssertNull(metadata.License, "License should default to null.");
    AssertNull(metadata.MinSdkVersion, "MinSdkVersion should default to null.");
    AssertNull(metadata.Category, "Category should default to null.");
    AssertNotNull(metadata.Dependencies, "Dependencies should never be null.");
    AssertEqual(0, metadata.Dependencies.Count, "Dependencies should default to empty.");
    AssertNotNull(metadata.Tags, "Tags should never be null.");
    AssertEqual(0, metadata.Tags.Count, "Tags should default to empty.");
}

static void ModKitManifestAttributeRoundTripsEveryField()
{
    var attr = typeof(ManifestAttributeTarget).GetCustomAttributes(typeof(ModKitManifestAttribute), inherit: false)
        .Cast<ModKitManifestAttribute>()
        .Single();

    AssertEqual("manifest.target", attr.Id, "Id should round-trip.");
    AssertEqual("Manifest Target", attr.DisplayName, "DisplayName should round-trip.");
    AssertEqual("3.4.5", attr.Version, "Version should round-trip.");
    AssertEqual("Tester", attr.Author, "Author should round-trip.");
    AssertEqual("https://example.com/repo", attr.GitHubUrl, "GitHubUrl should round-trip.");
    AssertEqual("Initial release.", attr.Changelog, "Changelog should round-trip.");
    AssertEqual("MIT", attr.License, "License should round-trip.");
    AssertEqual("0.2.0", attr.MinSdkVersion, "MinSdkVersion should round-trip.");
    AssertNotNull(attr.Dependencies, "Dependencies should not be null when set.");
    AssertEqual(2, attr.Dependencies!.Length, "Dependencies should preserve length.");
    AssertEqual("first.mod>=1.0.0", attr.Dependencies[0], "First dependency should round-trip.");
    AssertEqual("second.mod", attr.Dependencies[1], "Second dependency should round-trip.");
    AssertEqual("Diagnostics", attr.Category, "Category should round-trip.");
    AssertNotNull(attr.Tags, "Tags should not be null when set.");
    AssertEqual(2, attr.Tags!.Length, "Tags should preserve length.");
}

static void ModKitMetadataResolverUsesFallbacksWhenAttributeMissing()
{
    var metadata = ModKitMetadataResolver.Resolve(
        modType: typeof(MetadataResolverFallbackTarget),
        fallbackId: "fallback.id",
        fallbackDisplayName: "Fallback Display",
        fallbackVersion: "9.9.9",
        fallbackAuthor: "Fallback Author",
        fallbackGitHubUrl: "https://example.com/fallback",
        fallbackChangelog: "fallback changelog");

    AssertEqual("fallback.id", metadata.Id, "Id should come from fallback.");
    AssertEqual("Fallback Display", metadata.DisplayName, "DisplayName should come from fallback.");
    AssertEqual("9.9.9", metadata.Version, "Version should come from fallback.");
    AssertEqual("Fallback Author", metadata.Author, "Author should come from fallback.");
    AssertEqual("https://example.com/fallback", metadata.GitHubUrl, "GitHubUrl should come from fallback.");
    AssertEqual("fallback changelog", metadata.Changelog, "Changelog should come from fallback.");
    AssertNull(metadata.License, "License should be null when no attribute and no fallback.");
    AssertNull(metadata.MinSdkVersion, "MinSdkVersion should be null when no attribute and no fallback.");
    AssertEqual(0, metadata.Dependencies.Count, "Dependencies should be empty when no attribute.");
}

static void ModKitMetadataResolverPopulatesFromAttribute()
{
    var metadata = ModKitMetadataResolver.Resolve(
        modType: typeof(ManifestAttributeTarget),
        fallbackId: "fallback.id",
        fallbackDisplayName: "Fallback Display",
        fallbackVersion: "9.9.9",
        fallbackAuthor: "Fallback Author",
        fallbackGitHubUrl: "https://example.com/fallback",
        fallbackChangelog: "fallback changelog");

    AssertEqual("manifest.target", metadata.Id, "Id should come from attribute.");
    AssertEqual("Manifest Target", metadata.DisplayName, "DisplayName should come from attribute.");
    AssertEqual("3.4.5", metadata.Version, "Version should come from attribute.");
    AssertEqual("Tester", metadata.Author, "Author should come from attribute.");
    AssertEqual("https://example.com/repo", metadata.GitHubUrl, "GitHubUrl should come from attribute.");
    AssertEqual("Initial release.", metadata.Changelog, "Changelog should come from attribute.");
    AssertEqual("MIT", metadata.License, "License should come from attribute.");
    AssertEqual("0.2.0", metadata.MinSdkVersion, "MinSdkVersion should come from attribute.");
    AssertEqual(2, metadata.Dependencies.Count, "Dependencies should come from attribute.");
    AssertEqual("first.mod>=1.0.0", metadata.Dependencies[0], "Dependency entries should round-trip through resolver.");
    AssertEqual("Diagnostics", metadata.Category, "Category should come from attribute.");
    AssertEqual(2, metadata.Tags.Count, "Tags should come from attribute.");
}

static void ModKitRegistryValidatesOnRegister()
{
    ModKitRegistry.ClearForTests();
    var blocked = new TestManagedMod(new ModKitModMetadata(
        Id: "blocked.mod",
        DisplayName: "Blocked",
        Version: "1.0.0",
        Author: "Tests",
        MinSdkVersion: "99.0.0"));

    ModKitRegistry.Register(blocked);

    var validation = ModKitRegistry.GetValidation("blocked.mod");
    AssertTrue(!validation.IsValid, $"Blocked mod should fail validation. Issues: {string.Join(", ", validation.Issues)}");
    AssertTrue(validation.Issues.Count >= 1, "Validation result should explain failure.");
    AssertTrue(ModKitRegistry.IsBlockedByManifest("blocked.mod"), "Blocked mod should be flagged.");
}

static void ModKitRegistryReturnsOkForUnknownIds()
{
    ModKitRegistry.ClearForTests();

    AssertTrue(ModKitRegistry.GetValidation("unknown.mod").IsValid, "Unknown id should report Ok.");
    AssertTrue(!ModKitRegistry.IsBlockedByManifest("unknown.mod"), "Unknown id should not be blocked.");
}

static void ModKitRegistryRevalidatesAfterDependencyRegistered()
{
    ModKitRegistry.ClearForTests();
    var dependent = new TestManagedMod(new ModKitModMetadata(
        Id: "dependent.mod",
        DisplayName: "Dependent",
        Version: "1.0.0",
        Author: "Tests")
    {
        Dependencies = new[] { "needed.mod>=1.0.0" }
    });

    ModKitRegistry.Register(dependent);
    AssertTrue(!ModKitRegistry.GetValidation("dependent.mod").IsValid, "Dependent should fail before dependency registers.");

    var dependency = new TestManagedMod(new ModKitModMetadata(
        Id: "needed.mod",
        DisplayName: "Needed",
        Version: "1.5.0",
        Author: "Tests"));
    ModKitRegistry.Register(dependency);

    AssertTrue(
        ModKitRegistry.GetValidation("dependent.mod").IsValid,
        $"Dependent should re-validate to Ok after dependency joins. Issues: {string.Join(", ", ModKitRegistry.GetValidation("dependent.mod").Issues)}");
}

static void ModKitRegistryRevalidatesAfterDependencyUnregistered()
{
    ModKitRegistry.ClearForTests();
    var dependency = new TestManagedMod(new ModKitModMetadata(
        Id: "needed.mod",
        DisplayName: "Needed",
        Version: "1.0.0",
        Author: "Tests"));
    var dependent = new TestManagedMod(new ModKitModMetadata(
        Id: "dependent.mod",
        DisplayName: "Dependent",
        Version: "1.0.0",
        Author: "Tests")
    {
        Dependencies = new[] { "needed.mod>=1.0.0" }
    });

    ModKitRegistry.Register(dependency);
    ModKitRegistry.Register(dependent);
    AssertTrue(ModKitRegistry.GetValidation("dependent.mod").IsValid, "Dependent should be valid initially.");

    ModKitRegistry.Unregister("needed.mod");

    AssertTrue(!ModKitRegistry.GetValidation("dependent.mod").IsValid, "Dependent should re-validate after dependency leaves.");
}

static void ModKitRegistrySurfacesSelfCheckIssues()
{
    ModKitRegistry.ClearForTests();
    try
    {
        var mod = new SelfCheckingManagedMod();
        ModKitRegistry.Register(mod);

        var result = ModKitRegistry.RefreshSelfCheck(mod.Metadata.Id);

        AssertTrue(!result.IsValid, "Self-check issues should mark validation invalid.");
        AssertEqual("Runtime probe failed.", result.Issues[0], "Self-check issue should be surfaced.");
    }
    finally
    {
        ModKitRegistry.ClearForTests();
    }
}

static void ModKitUiModListPolicyShowsGroupsForMultipleCategories()
{
    var mods = new IModKitManagedMod[]
    {
        new TestManagedMod(new ModKitModMetadata("traffic.mod", "Traffic", "1.0.0", "Tests", Category: "Gameplay")),
        new TestManagedMod(new ModKitModMetadata("debug.mod", "Debug", "1.0.0", "Tests", Category: "Diagnostics"))
    };

    AssertTrue(ModKitUiModListPolicy.ShouldShowCategoryHeaders(mods), "Two distinct categories should show group headers.");
    AssertEqual(1, ModKitUiModListPolicy.FilterMods(mods, "traffic").Count, "Filter should match display name and id.");
}

static void ModKitUiLogViewerPolicyTakesLastLines()
{
    var lines = Enumerable.Range(1, 5).Select(i => $"line-{i}");

    var tail = ModKitUiLogViewerPolicy.TakeLastLines(lines, 3);

    AssertEqual("line-3\nline-4\nline-5", tail, "Log viewer should keep only the requested tail lines.");
}

static void ModKitUiBlocksManifestInvalidConfigEnabledEdits()
{
    ModKitRegistry.ClearForTests();
    try
    {
        var blocked = new TestManagedMod(new ModKitModMetadata(
            "blocked.config-toggle",
            "Blocked Config Toggle",
            "1.0.0",
            "Tests")
        {
            Dependencies = new[] { "missing.mod" }
        });
        var valid = new TestManagedMod("valid.config-toggle", "Valid Config Toggle");

        ModKitRegistry.Register(blocked);
        ModKitRegistry.Register(valid);

        AssertTrue(ModKitRegistry.IsBlockedByManifest(blocked.Metadata.Id), "Blocked fixture should fail manifest validation.");
        AssertTrue(
            ModKitUiHost.ShouldBlockEnabledConfigEdit(blocked, requestedEnabled: true),
            "Config Enabled=true edit should be blocked when manifest validation failed.");
        AssertTrue(
            !ModKitUiHost.ShouldBlockEnabledConfigEdit(blocked, requestedEnabled: false),
            "Config Enabled=false edit should remain allowed for blocked mods.");
        AssertTrue(
            !ModKitUiHost.ShouldBlockEnabledConfigEdit(valid, requestedEnabled: true),
            "Valid mods should be allowed to persist Enabled=true.");
    }
    finally
    {
        ModKitRegistry.ClearForTests();
    }
}

static void SdkReleaseSurfaceExcludesGameplayMods()
{
    var repoRoot = FindRepoRoot();
    var forbiddenTerms = new[] { "DlcLogger", "DLC Logger", "TrafficEnhancer", "Traffic Enhancer", "FlashingLightsMods" };
    var hits = new List<string>();

    foreach (var file in EnumerateSdkReleaseSurfaceFiles(repoRoot))
    {
        var text = File.ReadAllText(file);
        foreach (var term in forbiddenTerms)
        {
            if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                hits.Add($"{Path.GetRelativePath(repoRoot, file)} contains '{term}'");
            }
        }

        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(text, @"https://github\.com/[^\s\)""']+"))
        {
            var url = match.Value.TrimEnd('.', ',', ';');
            if (!string.Equals(url, ModKitModMetadata.DefaultGitHubUrl, StringComparison.Ordinal))
            {
                hits.Add($"{Path.GetRelativePath(repoRoot, file)} contains non-project GitHub URL '{url}'");
            }
        }
    }

    AssertTrue(
        hits.Count == 0,
        "SDK release-facing files should not reference maintainer gameplay mods: " + string.Join("; ", hits));
}

static void SdkReleaseSurfaceUsesBabyhamstaProjectMetadata()
{
    var repoRoot = FindRepoRoot();
    var forbiddenTerms = new[] { "YourName", "Your Name", "yourname", "https://github.com/yourname", "https://github.com/example", "FlashingLightsModKit-v", "Flashing Lights ModKit" };
    var hits = new List<string>();

    foreach (var file in EnumerateSdkReleaseSurfaceFiles(repoRoot))
    {
        var text = File.ReadAllText(file);
        foreach (var term in forbiddenTerms)
        {
            if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                hits.Add($"{Path.GetRelativePath(repoRoot, file)} contains '{term}'");
            }
        }
    }

    AssertTrue(
        hits.Count == 0,
        "SDK release-facing files should use Babyhamsta project metadata, not placeholder author/GitHub values: " + string.Join("; ", hits));
}

static void HotkeysFiresCallbackOnKeyDown()
{
    var input = new FakeInputSource();
    var hotkeys = new Hotkeys(input);
    var fired = 0;
    hotkeys.Register("toggle", KeyCode.F8, () => fired++);

    input.Pressed.Add(KeyCode.F8);
    hotkeys.Update();
    AssertEqual(1, fired, "Callback should fire when key reports down.");

    input.Pressed.Clear();
    hotkeys.Update();
    AssertEqual(1, fired, "Callback should not re-fire when key is no longer down.");
}

static void HotkeysDoesNotFireWhenKeyAbsent()
{
    var input = new FakeInputSource();
    var hotkeys = new Hotkeys(input);
    var fired = 0;
    hotkeys.Register("never", KeyCode.F8, () => fired++);

    input.Pressed.Add(KeyCode.F9);
    hotkeys.Update();

    AssertEqual(0, fired, "Different key should not invoke the binding.");
}

static void HotkeysUnregisterRemovesBinding()
{
    var input = new FakeInputSource();
    var hotkeys = new Hotkeys(input);
    var fired = 0;
    hotkeys.Register("toggle", KeyCode.F8, () => fired++);

    hotkeys.Unregister("toggle");
    input.Pressed.Add(KeyCode.F8);
    hotkeys.Update();

    AssertEqual(0, fired, "Unregistered binding should not fire.");
}

static void HotkeysRejectsDuplicateNames()
{
    var hotkeys = new Hotkeys(new FakeInputSource());
    hotkeys.Register("dup", KeyCode.F1, () => { });

    AssertThrows<ArgumentException>(
        () => hotkeys.Register("dup", KeyCode.F2, () => { }),
        "Registering a duplicate name should throw.");
}

static void HotkeysSwallowsCallbackException()
{
    var input = new FakeInputSource();
    var warns = new List<string>();
    var hotkeys = new Hotkeys(input, warn: warns.Add);
    hotkeys.Register("boom", KeyCode.F8, () => throw new InvalidOperationException("oops"));

    input.Pressed.Add(KeyCode.F8);
    hotkeys.Update();

    AssertEqual(1, warns.Count, "Callback failure should produce one warning.");
    AssertTrue(warns[0].Contains("oops", StringComparison.Ordinal), "Warning should include the inner message.");
}

static void HotkeysExposesBindingsAndRebindsByName()
{
    var input = new FakeInputSource();
    var fired = 0;
    var hotkeys = new Hotkeys(input);
    hotkeys.Register("toggle-overlay", KeyCode.F8, () => fired++, "Toggle Overlay");

    var binding = hotkeys.Bindings.Single();
    AssertEqual("toggle-overlay", binding.Name, "Binding name should be exposed.");
    AssertEqual(KeyCode.F8, binding.Key, "Binding key should be exposed.");
    AssertEqual("Toggle Overlay", binding.DisplayName, "Binding display name should be exposed.");

    hotkeys.Rebind("toggle-overlay", KeyCode.F9);
    input.Pressed.Add(KeyCode.F8);
    hotkeys.Update();
    input.Pressed.Clear();
    input.Pressed.Add(KeyCode.F9);
    hotkeys.Update();

    AssertEqual(1, fired, "Only rebound key should trigger callback.");
}

static void ModKitObjectSnapshotCapturesSelectedMembers()
{
    var snapshot = ModKitObjectSnapshot.Capture(new SnapshotPoco { Name = "Engine", Count = 3 }, "Name", "Count");

    AssertTrue(snapshot.Contains("Name=\"Engine\"", StringComparison.Ordinal), "Snapshot should include requested string field.");
    AssertTrue(snapshot.Contains("Count=\"3\"", StringComparison.Ordinal), "Snapshot should include requested integer property.");
}

static void ModKitObjectSnapshotCapturesVectorShapedMembers()
{
    var snapshot = ModKitObjectSnapshot.Capture(new SnapshotVectorHolder { Position = new SnapshotVector(1f, 2.5f, 3.125f) }, "Position");

    AssertTrue(snapshot.Contains("Position=\"(1,2.5,3.125)\"", StringComparison.Ordinal), "Snapshot should format vector-shaped values through primitive vector formatter.");
    AssertEqual("target=\"null\"", ModKitObjectSnapshot.Capture(null), "Null target should return sentinel.");
}

static void ModKitStringsLoadsOverridesAndFormatsValues()
{
    var tempRoot = NewTempRoot();
    try
    {
        Directory.CreateDirectory(tempRoot);
        File.WriteAllText(Path.Combine(tempRoot, "strings.json"), """
        {
          "online.blocked": "No online with mods.",
          "ui.modBlocked": "{0} cannot run."
        }
        """);

        ModKitStrings.Reload(tempRoot);

        AssertEqual("No online with mods.", ModKitStrings.Get("online.blocked"), "Override file should replace default string.");
        AssertEqual("Mods cannot be enabled while connected to online multiplayer. Return to the main menu first.", ModKitStrings.Get("online.toggleBlocked"), "Missing override should fall back to registered default.");
        AssertEqual("Traffic cannot run.", ModKitStrings.Get("ui.modBlocked", "Traffic"), "Format args should interpolate through overrides.");
    }
    finally
    {
        ModKitStrings.Reload();
        DeleteTempRoot(tempRoot);
    }
}

static void ModKitMelonModExposesVerboseLoggingProbe()
{
    AssertTrue(ModKitMelonMod<VerboseConfig>.IsVerboseLoggingEnabled(new VerboseConfig { VerboseLogging = true }), "VerboseLogging=true should enable debug console routing.");
    AssertTrue(!ModKitMelonMod<TestConfig>.IsVerboseLoggingEnabled(new TestConfig()), "Missing VerboseLogging property should disable debug console routing.");
}

static void ModKitRuntimePolicyBlocksManifestInvalidEnabledMods()
{
    AssertTrue(ModKitRuntimePolicy.ShouldRun(isEnabled: true, blockedByManifest: false), "Enabled valid mods should run.");
    AssertTrue(!ModKitRuntimePolicy.ShouldRun(isEnabled: true, blockedByManifest: true), "Enabled blocked mods should not run.");
    AssertTrue(!ModKitRuntimePolicy.ShouldRun(isEnabled: false, blockedByManifest: false), "Disabled mods should not run.");
}

static void PatchContextStartsInactive()
{
    PatchContext<PatchStateA>.Clear();
    AssertTrue(!PatchContext<PatchStateA>.IsActive, "Default PatchContext should be inactive.");
    AssertNull(PatchContext<PatchStateA>.State, "Default state should be null.");
}

static void PatchContextStoresStateAndActivates()
{
    PatchContext<PatchStateA>.Clear();
    PatchContext<PatchStateA>.Set(new PatchStateA { Value = "hi" });
    AssertTrue(!PatchContext<PatchStateA>.IsActive, "Set alone should not activate the context.");

    PatchContext<PatchStateA>.SetActive(true);
    AssertTrue(PatchContext<PatchStateA>.IsActive, "After Set + Activate IsActive should be true.");
    AssertNotNull(PatchContext<PatchStateA>.State, "State should be set.");
    AssertEqual("hi", PatchContext<PatchStateA>.State!.Value, "State should round-trip.");

    PatchContext<PatchStateA>.SetActive(false);
    AssertTrue(!PatchContext<PatchStateA>.IsActive, "Deactivate should make IsActive false even when state is present.");
}

static void PatchContextKeepsStatePerType()
{
    PatchContext<PatchStateA>.Clear();
    PatchContext<PatchStateB>.Clear();

    PatchContext<PatchStateA>.Set(new PatchStateA { Value = "A" });
    PatchContext<PatchStateB>.Set(new PatchStateB { Counter = 7 });

    AssertEqual("A", PatchContext<PatchStateA>.State!.Value, "Type A should hold its own state.");
    AssertEqual(7, PatchContext<PatchStateB>.State!.Counter, "Type B should hold its own state independently.");
}

static void PatchContextClearResetsState()
{
    PatchContext<PatchStateA>.Set(new PatchStateA { Value = "set" });
    PatchContext<PatchStateA>.SetActive(true);
    PatchContext<PatchStateA>.Clear();

    AssertNull(PatchContext<PatchStateA>.State, "Clear should null state.");
    AssertTrue(!PatchContext<PatchStateA>.IsActive, "Clear should deactivate.");
}

static void ModLogFormatQuoteEscapes()
{
    AssertEqual("\"plain\"", ModKitLogFormat.Quote("plain"), "Plain string should be wrapped in double quotes.");
    AssertEqual("\"\"", ModKitLogFormat.Quote(null), "Null should produce empty quoted string.");
    AssertEqual("\"with \\\"inner\\\"\"", ModKitLogFormat.Quote("with \"inner\""), "Inner double quotes should be escaped.");
    AssertEqual("\"path\\\\sub\"", ModKitLogFormat.Quote("path\\sub"), "Backslash should be escaped.");
}

static void ModLogFormatKeyValueProducesPair()
{
    AssertEqual("count=\"42\"", ModKitLogFormat.KeyValue("count", 42), "Integer values should round-trip via invariant culture.");
    AssertEqual("name=\"Alice\"", ModKitLogFormat.KeyValue("name", "Alice"), "String values should round-trip quoted.");
    AssertEqual("flag=\"True\"", ModKitLogFormat.KeyValue("flag", true), "Booleans should serialize as their text form.");
    AssertEqual("missing=\"\"", ModKitLogFormat.KeyValue("missing", null), "Null values should produce empty quoted value.");
}

static void ModLogFormatVector3InvariantFormatting()
{
    AssertEqual("(1,2.5,3.142)", ModKitLogFormat.Vector3(1f, 2.5f, 3.1415927f), "Vector3 should format with up to three decimals invariantly.");
}

static void ModFileLogHeaderAppendsOnceAndPreservesPriorContent()
{
    var tempRoot = NewTempRoot();
    try
    {
        Directory.CreateDirectory(tempRoot);
        var log = new ModKitFileLog("file-log-test", info: _ => { }, warn: _ => { }, baseDirectory: tempRoot);

        log.Info("first event");
        log.Header("# fresh header");
        log.Header("# duplicate header");
        log.Info("second event");

        var content = File.ReadAllText(log.FilePath);
        AssertTrue(content.Contains("first event", StringComparison.Ordinal), "Header should not destroy prior content.");
        AssertTrue(content.Contains("# fresh header", StringComparison.Ordinal), "First Header should be written.");
        AssertTrue(!content.Contains("# duplicate header", StringComparison.Ordinal), "Subsequent Header calls should be ignored.");
        AssertTrue(content.Contains("second event", StringComparison.Ordinal), "Lines after Header should still append.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModFileLogInfoForwardsToConsoleAndAppendsToFile()
{
    var tempRoot = NewTempRoot();
    try
    {
        Directory.CreateDirectory(tempRoot);
        var infoMessages = new List<string>();
        var log = new ModKitFileLog("file-log-info", info: infoMessages.Add, warn: _ => { }, baseDirectory: tempRoot);

        log.Info("first event");
        log.Info("second event");

        AssertEqual(2, infoMessages.Count, "Info should forward to console callback.");
        AssertEqual("first event", infoMessages[0], "Console callback should receive raw message.");

        var content = File.ReadAllText(log.FilePath);
        AssertTrue(content.Contains("first event", StringComparison.Ordinal), "File should contain first event.");
        AssertTrue(content.Contains("second event", StringComparison.Ordinal), "File should contain second event.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModFileLogInfoHonorsToggles()
{
    var tempRoot = NewTempRoot();
    try
    {
        Directory.CreateDirectory(tempRoot);
        var infoMessages = new List<string>();
        var log = new ModKitFileLog("file-log-toggle", info: infoMessages.Add, warn: _ => { }, baseDirectory: tempRoot)
        {
            LogToConsole = false,
            LogToFile = false
        };

        log.Info("muted");

        AssertEqual(0, infoMessages.Count, "Console callback should not fire when LogToConsole is false.");
        AssertTrue(!File.Exists(log.FilePath), "File should not be created when LogToFile is false.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModFileLogWarnAlwaysEmits()
{
    var tempRoot = NewTempRoot();
    try
    {
        Directory.CreateDirectory(tempRoot);
        var warns = new List<string>();
        var log = new ModKitFileLog("file-log-warn", info: _ => { }, warn: warns.Add, baseDirectory: tempRoot)
        {
            LogToConsole = false,
            LogToFile = false
        };

        log.Warn("careful");

        AssertEqual(1, warns.Count, "Warn callback should always fire even when toggles are off.");
        AssertEqual("careful", warns[0], "Warn callback should receive raw message.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModKitFileLogDebugWritesFileOnly()
{
    var tempRoot = NewTempRoot();
    try
    {
        Directory.CreateDirectory(tempRoot);
        var infoMessages = new List<string>();
        using var log = new ModKitFileLog("file-log-debug", info: infoMessages.Add, warn: _ => { }, baseDirectory: tempRoot)
        {
            LogToConsole = true,
            LogToFile = true
        };

        log.Debug("hidden detail");

        AssertEqual(0, infoMessages.Count, "Debug should not write to console through ModKitFileLog.");
        AssertTrue(File.ReadAllText(log.FilePath).Contains("DEBUG hidden detail", StringComparison.Ordinal), "Debug should append to file.");
    }
    finally
    {
        DeleteTempRoot(tempRoot);
    }
}

static void ModKitUiAboutPolicyFormatsLicenseAndSdk()
{
    AssertEqual("MIT", ModKitUiAboutPolicy.FormatLicense("MIT"), "License should pass through.");
    AssertEqual(ModKitUiAboutPolicy.LicenseUnspecified, ModKitUiAboutPolicy.FormatLicense(null), "Null license should display as unspecified.");
    AssertEqual(ModKitUiAboutPolicy.LicenseUnspecified, ModKitUiAboutPolicy.FormatLicense("   "), "Blank license should display as unspecified.");

    AssertEqual(">= 0.2.0", ModKitUiAboutPolicy.FormatMinSdkVersion("0.2.0"), "Min SDK version should display with operator.");
    AssertEqual(ModKitUiAboutPolicy.MinSdkAny, ModKitUiAboutPolicy.FormatMinSdkVersion(null), "Null min SDK should display as any.");
}

static void ModKitUiAboutPolicyFormatsDependenciesAndValidation()
{
    AssertEqual(ModKitUiAboutPolicy.DependenciesNone, ModKitUiAboutPolicy.FormatDependencies(Array.Empty<string>()), "Empty dependencies should display as none.");
    AssertEqual("a, b", ModKitUiAboutPolicy.FormatDependencies(new[] { "a", "b" }), "Multiple dependencies should join with comma.");

    AssertEqual(ModKitUiAboutPolicy.ValidationOk, ModKitUiAboutPolicy.FormatValidationStatus(ModKitManifestValidationResult.Ok), "Ok validation should show OK.");
    AssertEqual("Blocked (1 issue)", ModKitUiAboutPolicy.FormatValidationStatus(ModKitManifestValidationResult.Failed(new[] { "x" })), "Single issue should be singular.");
    AssertEqual("Blocked (2 issues)", ModKitUiAboutPolicy.FormatValidationStatus(ModKitManifestValidationResult.Failed(new[] { "x", "y" })), "Multiple issues should be plural.");
    AssertEqual("My Mod is blocked by manifest validation.", ModKitUiAboutPolicy.FormatToggleBlockedMessage("My Mod"), "Toggle blocked message should name the mod.");
}

static void ModKitDependencyParserParsesBareIds()
{
    AssertTrue(
        ModKitDependencyParser.TryParse("first.mod", out var dep, out var error),
        $"Bare id should parse. Error: {error}");
    AssertNotNull(dep, "Parsed dependency should not be null.");
    AssertEqual("first.mod", dep!.Id, "Bare id should round-trip.");
    AssertNull(dep.MinVersion, "Bare id should not declare a minimum version.");
}

static void ModKitDependencyParserParsesMinVersionConstraints()
{
    AssertTrue(
        ModKitDependencyParser.TryParse("flashinglights.othermod>=1.2.3", out var dep, out var error),
        $"Min-version constraint should parse. Error: {error}");
    AssertNotNull(dep, "Parsed dependency should not be null.");
    AssertEqual("flashinglights.othermod", dep!.Id, "Constraint id should round-trip.");
    AssertNotNull(dep.MinVersion, "Min version should be parsed.");
    AssertEqual(new Version(1, 2, 3), dep.MinVersion, "Min version should round-trip.");

    AssertTrue(
        ModKitDependencyParser.TryParse("  spaced.mod  >=  0.5  ", out var spaced, out var spacedError),
        $"Whitespace should be tolerated. Error: {spacedError}");
    AssertEqual("spaced.mod", spaced!.Id, "Whitespace should be trimmed from id.");
    AssertEqual(new Version(0, 5), spaced.MinVersion, "Whitespace should be trimmed from version.");
}

static void ModKitDependencyParserParsesOptionalSpecs()
{
    AssertTrue(ModKitDependencyParser.TryParse("soft.mod?", out var bare, out var bareError), bareError ?? "Bare optional dependency should parse.");
    AssertEqual("soft.mod", bare!.Id, "Optional bare dependency should trim trailing marker.");
    AssertNull(bare.MinVersion, "Optional bare dependency should not require a version.");
    AssertTrue(bare.IsOptional, "Optional bare dependency should be marked optional.");

    AssertTrue(ModKitDependencyParser.TryParse("soft.mod>=1.2.3?", out var versioned, out var versionedError), versionedError ?? "Versioned optional dependency should parse.");
    AssertEqual(new Version(1, 2, 3), versioned!.MinVersion, "Versioned optional dependency should keep minimum version.");
    AssertTrue(versioned.IsOptional, "Versioned optional dependency should be marked optional.");
}

static void ModKitDependencyParserRejectsMalformedSpecs()
{
    AssertTrue(
        !ModKitDependencyParser.TryParse("", out _, out var emptyError),
        "Empty spec should be rejected.");
    AssertNotNull(emptyError, "Empty spec should report an error message.");

    AssertTrue(
        !ModKitDependencyParser.TryParse(">=1.0.0", out _, out var missingIdError),
        "Missing id before '>=' should be rejected.");
    AssertNotNull(missingIdError, "Missing id should report an error message.");

    AssertTrue(
        !ModKitDependencyParser.TryParse("ok.mod>=", out _, out var missingVersionError),
        "Missing version after '>=' should be rejected.");
    AssertNotNull(missingVersionError, "Missing version should report an error message.");

    AssertTrue(
        !ModKitDependencyParser.TryParse("ok.mod>=not-a-version", out _, out var badVersionError),
        "Unparseable version should be rejected.");
    AssertNotNull(badVersionError, "Bad version should report an error message.");

    AssertTrue(
        !ModKitDependencyParser.TryParse("bad id with spaces", out _, out var badIdError),
        "Id with whitespace should be rejected.");
    AssertNotNull(badIdError, "Bad id should report an error message.");
}

static void ModKitManifestValidatorReturnsOkForEmptyConstraints()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author");
    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", Array.Empty<ModKitModMetadata>());

    AssertTrue(result.IsValid, "No constraints should validate as Ok.");
    AssertEqual(0, result.Issues.Count, "Ok result should have no issues.");
}

static void ModKitManifestValidatorReportsSdkVersionTooLow()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author", MinSdkVersion: "0.3.0");
    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", Array.Empty<ModKitModMetadata>());

    AssertTrue(!result.IsValid, "Lower SDK version should fail validation.");
    AssertEqual(1, result.Issues.Count, "SDK version mismatch should produce one issue.");
    AssertTrue(
        result.Issues[0].Contains("0.3.0", StringComparison.Ordinal) && result.Issues[0].Contains("0.2.0", StringComparison.Ordinal),
        "SDK version issue should mention both required and current versions.");
}

static void ModKitManifestValidatorReportsMissingDependency()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author")
    {
        Dependencies = new[] { "missing.mod>=1.0.0" }
    };
    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", Array.Empty<ModKitModMetadata>());

    AssertTrue(!result.IsValid, "Missing dependency should fail validation.");
    AssertEqual(1, result.Issues.Count, "Missing dependency should produce one issue.");
    AssertTrue(
        result.Issues[0].Contains("missing.mod", StringComparison.Ordinal),
        "Missing dependency issue should name the dependency.");
}

static void ModKitManifestValidatorReportsOutdatedDependency()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author")
    {
        Dependencies = new[] { "older.mod>=2.0.0" }
    };
    var registered = new[]
    {
        new ModKitModMetadata("older.mod", "Older", "1.5.0", "Author")
    };
    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", registered);

    AssertTrue(!result.IsValid, "Outdated dependency should fail validation.");
    AssertEqual(1, result.Issues.Count, "Outdated dependency should produce one issue.");
    AssertTrue(
        result.Issues[0].Contains("older.mod", StringComparison.Ordinal) && result.Issues[0].Contains("2.0.0", StringComparison.Ordinal),
        "Outdated dependency issue should name dependency and required version.");
}

static void ModKitManifestValidatorSkipsMissingOptionalDependency()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author")
    {
        Dependencies = new[] { "missing.mod?" }
    };

    var result = ModKitManifestValidator.Validate(metadata, "1.0.0", Array.Empty<ModKitModMetadata>());

    AssertTrue(result.IsValid, "Missing optional dependency should not block validation.");
}

static void ModKitManifestValidatorReportsOutdatedOptionalDependency()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author")
    {
        Dependencies = new[] { "soft.mod>=2.0.0?" }
    };
    var registered = new[]
    {
        new ModKitModMetadata("soft.mod", "Soft", "1.0.0", "Tests")
    };

    var result = ModKitManifestValidator.Validate(metadata, "1.0.0", registered);

    AssertTrue(!result.IsValid, "Present optional dependency should still honor minimum version.");
}

static void ModKitManifestValidatorApprovesSatisfiedConstraints()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author", MinSdkVersion: "0.2.0")
    {
        Dependencies = new[] { "ready.mod>=1.0.0", "bare.mod" }
    };
    var registered = new[]
    {
        new ModKitModMetadata("ready.mod", "Ready", "1.0.0", "Author"),
        new ModKitModMetadata("bare.mod", "Bare", "0.5.0", "Author")
    };
    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", registered);

    AssertTrue(result.IsValid, "Satisfied constraints should validate as Ok.");
    AssertEqual(0, result.Issues.Count, "Ok result should have no issues.");
}

static void ModKitManifestValidatorMatchesIdsCaseInsensitively()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author")
    {
        Dependencies = new[] { "MIXED.case>=1.0.0" }
    };
    var registered = new[]
    {
        new ModKitModMetadata("mixed.CASE", "Mixed", "1.0.0", "Author")
    };
    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", registered);

    AssertTrue(result.IsValid, $"Case-insensitive id match should validate. Issues: {string.Join(", ", result.Issues)}");
}

static void ModKitManifestValidatorCollectsMultipleIssues()
{
    var metadata = new ModKitModMetadata("subject.mod", "Subject", "1.0.0", "Author", MinSdkVersion: "0.5.0")
    {
        Dependencies = new[] { "missing.mod", "outdated.mod>=2.0.0" }
    };
    var registered = new[]
    {
        new ModKitModMetadata("outdated.mod", "Outdated", "1.0.0", "Author")
    };
    var result = ModKitManifestValidator.Validate(metadata, "0.2.0", registered);

    AssertTrue(!result.IsValid, "Multiple violations should fail validation.");
    AssertEqual(3, result.Issues.Count, "Should report SDK + missing + outdated as three issues.");
}

static void ModKitMelonModMetadataUsesManifestAttribute()
{
    var mod = new ManifestWiredMod();

    AssertEqual("wired.test", mod.Metadata.Id, "Attribute Id should win over override fallback when set.");
    AssertEqual("Wired Test", mod.Metadata.DisplayName, "Attribute DisplayName should populate metadata.");
    AssertEqual("9.9.9", mod.Metadata.Version, "Attribute Version should populate metadata.");
    AssertEqual("Wire Tester", mod.Metadata.Author, "Attribute Author should populate metadata.");
    AssertEqual("Apache-2.0", mod.Metadata.License, "Attribute License should populate metadata.");
    AssertEqual("0.2.0", mod.Metadata.MinSdkVersion, "Attribute MinSdkVersion should populate metadata.");
    AssertEqual(1, mod.Metadata.Dependencies.Count, "Attribute Dependencies should populate metadata.");
    AssertEqual("first.mod>=1.0.0", mod.Metadata.Dependencies[0], "Attribute Dependencies should round-trip into metadata.");
}

static void ModKitMetadataResolverMergesNullAttributeFields()
{
    var metadata = ModKitMetadataResolver.Resolve(
        modType: typeof(MetadataResolverPartialTarget),
        fallbackId: "fallback.id",
        fallbackDisplayName: "Fallback Display",
        fallbackVersion: "9.9.9",
        fallbackAuthor: "Fallback Author",
        fallbackGitHubUrl: "https://example.com/fallback",
        fallbackChangelog: "fallback changelog");

    AssertEqual("partial.id", metadata.Id, "Id should come from attribute when set.");
    AssertEqual("Fallback Display", metadata.DisplayName, "DisplayName should fall back when attribute leaves it null.");
    AssertEqual("9.9.9", metadata.Version, "Version should fall back when attribute leaves it null.");
    AssertEqual("Partial Author", metadata.Author, "Author should come from attribute when set.");
    AssertEqual("https://example.com/fallback", metadata.GitHubUrl, "GitHubUrl should fall back when attribute leaves it null.");
    AssertNull(metadata.License, "License should remain null when neither attribute nor fallback set.");
    AssertEqual(0, metadata.Dependencies.Count, "Dependencies should default to empty when attribute omits them.");
}

static string NewTempRoot()
{
    return Path.Combine(Path.GetTempPath(), "flashing-lights-modkit-tests", Guid.NewGuid().ToString("N"));
}

static string FindRepoRoot()
{
    var current = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (current != null)
    {
        if (File.Exists(Path.Combine(current.FullName, "FlashingLightsModKit.sln")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    throw new InvalidOperationException("Could not find FlashingLightsModKit.sln from current directory.");
}

static IEnumerable<string> EnumerateSdkReleaseSurfaceFiles(string repoRoot)
{
    foreach (var relativePath in new[] { "README.md", "CHANGELOG.md", "FlashingLightsModKit.sln", "Directory.Build.props" })
    {
        var path = Path.Combine(repoRoot, relativePath);
        if (File.Exists(path))
        {
            yield return path;
        }
    }

    foreach (var relativeRoot in new[] { "docs", "templates", "tools", ".github", "src" })
    {
        var root = Path.Combine(repoRoot, relativeRoot);
        if (!Directory.Exists(root))
        {
            continue;
        }

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            if (IsIgnoredReleaseSurfacePath(file)
                || !IsReleaseSurfaceFile(file))
            {
                continue;
            }

            yield return file;
        }
    }
}

static bool IsIgnoredReleaseSurfacePath(string path)
{
    var normalized = path.Replace('\\', '/');
    return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/artifacts/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/.git/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/.vs/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/.superpowers/", StringComparison.OrdinalIgnoreCase);
}

static bool IsReleaseSurfaceFile(string path)
{
    var extension = Path.GetExtension(path);
    return string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".cs", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".props", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".targets", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".sln", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".ps1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".yml", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".yaml", StringComparison.OrdinalIgnoreCase);
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

static ModKitConfigProperty FindProperty(IReadOnlyList<ModKitConfigProperty> properties, string name)
{
    foreach (var property in properties)
    {
        if (property.Name == name)
        {
            return property;
        }
    }

    throw new InvalidOperationException($"Property not found: {name}");
}

sealed class TestConfig
{
    public bool Enabled { get; set; }
    public int Count { get; set; }
}

[ModKitConfigVersion(2)]
sealed class VersionedConfig
{
    public bool Enabled { get; set; }
    public int Count { get; set; }
}

sealed class VerboseConfig
{
    public bool VerboseLogging { get; set; }
}

sealed class UiTestConfig
{
    public bool Enabled { get; set; }
    public int Count { get; set; }
    [ModKitConfigDisplay("Traffic Distance (m)")]
    [ModKitConfigRange(150, 500, 50)]
    public double Distance { get; set; }
    [ModKitConfigOptions("Normal", "Far", "Maximum")]
    public string Preset { get; set; } = "Normal";
    public string Name { get; set; } = string.Empty;
    public UiTestMode Mode { get; set; }
    public string[] Tags { get; set; } = [];
    public string ReadOnly => "read-only";
}

enum UiTestMode
{
    Basic,
    Advanced
}

class TestManagedMod : IModKitManagedMod
{
    public TestManagedMod(string id, string displayName)
        : this(new ModKitModMetadata(id, displayName, "0.1.0", "Tests"))
    {
    }

    public TestManagedMod(ModKitModMetadata metadata)
    {
        Metadata = metadata;
    }

    public ModKitModMetadata Metadata { get; }
    public bool IsEnabled { get; private set; } = true;
    public string ConfigPath => string.Empty;
    public IModKitConfigAdapter? ConfigAdapter => null;

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
    }

    public virtual ModKitManifestValidationResult? SelfCheck()
    {
        return null;
    }
}

sealed class SelfCheckingManagedMod : TestManagedMod
{
    public SelfCheckingManagedMod()
        : base(new ModKitModMetadata("self-check.mod", "Self Check", "1.0.0", "Tests"))
    {
    }

    public override ModKitManifestValidationResult? SelfCheck() =>
        ModKitManifestValidationResult.Failed(new[] { "Runtime probe failed." });
}

sealed class SnapshotPoco
{
    public string Name = string.Empty;
    public int Count { get; set; }
}

readonly record struct SnapshotVector(float x, float y, float z);

sealed class SnapshotVectorHolder
{
    public SnapshotVector Position { get; set; }
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

[ModKitManifest(
    Id = "manifest.target",
    DisplayName = "Manifest Target",
    Version = "3.4.5",
    Author = "Tester",
    GitHubUrl = "https://example.com/repo",
    Changelog = "Initial release.",
    License = "MIT",
    MinSdkVersion = "0.2.0",
    Dependencies = new[] { "first.mod>=1.0.0", "second.mod" },
    Category = "Diagnostics",
    Tags = new[] { "probe", "ui" })]
sealed class ManifestAttributeTarget
{
}

sealed class MetadataResolverFallbackTarget
{
}

[ModKitManifest(Id = "partial.id", Author = "Partial Author")]
sealed class MetadataResolverPartialTarget
{
}

sealed class FakeInputSource : IHotkeyInputSource
{
    public HashSet<KeyCode> Pressed { get; } = new();

    public bool GetKeyDown(KeyCode key) => Pressed.Contains(key);
}

sealed class PatchStateA
{
    public string? Value { get; set; }
}

sealed class PatchStateB
{
    public int Counter { get; set; }
}

[ModKitManifest(
    Id = "wired.test",
    DisplayName = "Wired Test",
    Version = "9.9.9",
    Author = "Wire Tester",
    License = "Apache-2.0",
    MinSdkVersion = "0.2.0",
    Dependencies = new[] { "first.mod>=1.0.0" })]
sealed class ManifestWiredMod : ModKitMelonMod<TestConfig>
{
    protected override string ModId => "fallback.id";
    protected override bool RegisterWithModKitUi => false;
}
