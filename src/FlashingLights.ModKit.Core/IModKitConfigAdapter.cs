namespace FlashingLights.ModKit.Core;

public interface IModKitConfigAdapter
{
    string ModId { get; }
    string ConfigPath { get; }
    IReadOnlyList<ModKitConfigProperty> GetProperties();
    bool TrySetProperty(string propertyName, string valueText, out string error);
    void Save();
    void Reload();
    void ResetDefaults();
}
