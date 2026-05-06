namespace FlashingLights.ModKit.Core;

internal static class ModKitDependencyParser
{
    private const string MinVersionOperator = ">=";

    public static bool TryParse(string spec, out ModKitDependency? dependency, out string? error)
    {
        dependency = null;
        error = null;

        if (string.IsNullOrWhiteSpace(spec))
        {
            error = "Dependency spec cannot be empty.";
            return false;
        }

        var trimmed = spec.Trim();
        var isOptional = trimmed.EndsWith("?", StringComparison.Ordinal);
        if (isOptional)
        {
            trimmed = trimmed[..^1].TrimEnd();
            if (trimmed.Length == 0)
            {
                error = "Dependency spec cannot be empty.";
                return false;
            }
        }

        var operatorIndex = trimmed.IndexOf(MinVersionOperator, StringComparison.Ordinal);

        if (operatorIndex < 0)
        {
            var bareId = trimmed;
            if (!IsValidId(bareId))
            {
                error = $"Dependency id '{bareId}' contains invalid characters.";
                return false;
            }
            dependency = new ModKitDependency(bareId, MinVersion: null, isOptional);
            return true;
        }

        var id = trimmed[..operatorIndex].Trim();
        var versionPart = trimmed[(operatorIndex + MinVersionOperator.Length)..].Trim();

        if (id.Length == 0)
        {
            error = $"Dependency '{trimmed}' is missing an id before '{MinVersionOperator}'.";
            return false;
        }

        if (!IsValidId(id))
        {
            error = $"Dependency id '{id}' contains invalid characters.";
            return false;
        }

        if (versionPart.Length == 0)
        {
            error = $"Dependency '{id}' is missing a version after '{MinVersionOperator}'.";
            return false;
        }

        if (!Version.TryParse(versionPart, out var version))
        {
            error = $"Dependency '{id}' has an unparseable version '{versionPart}'.";
            return false;
        }

        dependency = new ModKitDependency(id, version, isOptional);
        return true;
    }

    private static bool IsValidId(string id)
    {
        foreach (var c in id)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_')
            {
                return false;
            }
        }
        return id.Length > 0;
    }
}
