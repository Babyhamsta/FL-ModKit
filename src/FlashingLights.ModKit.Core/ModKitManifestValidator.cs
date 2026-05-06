namespace FlashingLights.ModKit.Core;

public static class ModKitManifestValidator
{
    public static ModKitManifestValidationResult Validate(
        ModKitModMetadata metadata,
        string currentSdkVersion,
        IReadOnlyList<ModKitModMetadata> registeredMods)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(currentSdkVersion);
        ArgumentNullException.ThrowIfNull(registeredMods);

        var issues = new List<string>();

        ValidateSdkVersion(metadata, currentSdkVersion, issues);
        ValidateDependencies(metadata, registeredMods, issues);
        ValidateDependencyCycles(metadata, registeredMods, issues);

        return issues.Count == 0
            ? ModKitManifestValidationResult.Ok
            : ModKitManifestValidationResult.Failed(issues);
    }

    private static void ValidateSdkVersion(
        ModKitModMetadata metadata,
        string currentSdkVersion,
        List<string> issues)
    {
        if (string.IsNullOrWhiteSpace(metadata.MinSdkVersion))
        {
            return;
        }

        if (!Version.TryParse(metadata.MinSdkVersion, out var required))
        {
            issues.Add($"Mod '{metadata.Id}' declares an invalid MinSdkVersion '{metadata.MinSdkVersion}'.");
            return;
        }

        if (!Version.TryParse(currentSdkVersion, out var current))
        {
            issues.Add($"Current SDK version '{currentSdkVersion}' is not parseable for comparison.");
            return;
        }

        if (current < required)
        {
            issues.Add($"Mod '{metadata.Id}' requires SDK >= {required}, current SDK is {current}.");
        }
    }

    private static void ValidateDependencies(
        ModKitModMetadata metadata,
        IReadOnlyList<ModKitModMetadata> registeredMods,
        List<string> issues)
    {
        foreach (var spec in metadata.Dependencies)
        {
            if (!ModKitDependencyParser.TryParse(spec, out var dependency, out var error))
            {
                issues.Add(error ?? $"Mod '{metadata.Id}' has an unparseable dependency '{spec}'.");
                continue;
            }

            if (string.Equals(dependency!.Id, metadata.Id, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add($"Mod '{metadata.Id}' cannot depend on itself.");
                continue;
            }

            ModKitModMetadata? match = null;
            foreach (var candidate in registeredMods)
            {
                if (string.Equals(candidate.Id, metadata.Id, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.Equals(candidate.Id, dependency.Id, StringComparison.OrdinalIgnoreCase))
                {
                    match = candidate;
                    break;
                }
            }

            if (match == null)
            {
                if (dependency.IsOptional)
                {
                    continue;
                }

                issues.Add($"Mod '{metadata.Id}' is missing dependency '{dependency!.Id}'.");
                continue;
            }

            if (dependency!.MinVersion == null)
            {
                continue;
            }

            if (!Version.TryParse(match.Version, out var matchVersion))
            {
                issues.Add($"Mod '{metadata.Id}' dependency '{dependency.Id}' has an unparseable version '{match.Version}'.");
                continue;
            }

            if (matchVersion < dependency.MinVersion)
            {
                issues.Add($"Mod '{metadata.Id}' requires '{dependency.Id}' >= {dependency.MinVersion}, found {matchVersion}.");
            }
        }
    }

    private static void ValidateDependencyCycles(
        ModKitModMetadata metadata,
        IReadOnlyList<ModKitModMetadata> registeredMods,
        List<string> issues)
    {
        var graph = BuildDependencyGraph(registeredMods);
        if (!graph.ContainsKey(metadata.Id))
        {
            graph[metadata.Id] = DependencyIds(metadata, registeredMods).ToArray();
        }

        var path = new List<string>();
        if (FindCycle(metadata.Id, metadata.Id, graph, path, new HashSet<string>(StringComparer.OrdinalIgnoreCase)))
        {
            issues.Add($"Mod '{metadata.Id}' is in a dependency cycle: {string.Join(" → ", path)}");
        }
    }

    private static Dictionary<string, IReadOnlyList<string>> BuildDependencyGraph(IReadOnlyList<ModKitModMetadata> registeredMods)
    {
        var graph = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var mod in registeredMods)
        {
            graph[mod.Id] = DependencyIds(mod, registeredMods).ToArray();
        }

        return graph;
    }

    private static IEnumerable<string> DependencyIds(ModKitModMetadata mod, IReadOnlyList<ModKitModMetadata> registeredMods)
    {
        foreach (var spec in mod.Dependencies)
        {
            if (!ModKitDependencyParser.TryParse(spec, out var dependency, out _)
                || dependency == null
                || string.Equals(dependency.Id, mod.Id, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!registeredMods.Any(candidate => string.Equals(candidate.Id, dependency.Id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return dependency.Id;
        }
    }

    private static bool FindCycle(
        string rootId,
        string currentId,
        Dictionary<string, IReadOnlyList<string>> graph,
        List<string> path,
        HashSet<string> visited)
    {
        path.Add(currentId);
        if (!graph.TryGetValue(currentId, out var dependencies))
        {
            path.RemoveAt(path.Count - 1);
            return false;
        }

        foreach (var dependencyId in dependencies)
        {
            if (string.Equals(dependencyId, rootId, StringComparison.OrdinalIgnoreCase))
            {
                path.Add(rootId);
                return true;
            }

            if (!visited.Add(dependencyId))
            {
                continue;
            }

            if (FindCycle(rootId, dependencyId, graph, path, visited))
            {
                return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }
}
