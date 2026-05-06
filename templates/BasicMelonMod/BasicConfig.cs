namespace BasicMelonMod;

public sealed class BasicConfig
{
    public bool Enabled { get; set; } = true;
    public bool EnableHotReload { get; set; } = true;
    public double HotReloadIntervalSeconds { get; set; } = 1;
    public bool LogSceneLoads { get; set; } = true;
    public string DisplayName { get; set; } = "Basic Melon Mod";
}
