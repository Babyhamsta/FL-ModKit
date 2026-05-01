namespace FlashingLights.ModKit.Core;

public static class GameTypes
{
    public const string OmniAiVehicleController = "Il2CppOmniVehicleAi.AIVehicleController";
    public const string OmniPathProgressTracker = "Il2CppOmniVehicleAi.PathProgressTracker";
    public const string EvpVehicleController = "Il2CppEVP.VehicleController";
    public const string SimpleCarController = "Il2Cpp.CarController";
    public const string SetCarWaypointAction = "Il2CppHutongGames.PlayMaker.Actions.SetCarWaypoint";
    public const string CarTypesEnum = "Il2CppJinxterGames.Scripts.FlashingLights.CarTypes";

    public static readonly string[] AllKnownTypeNames =
    [
        OmniAiVehicleController,
        OmniPathProgressTracker,
        EvpVehicleController,
        SimpleCarController,
        SetCarWaypointAction,
        CarTypesEnum
    ];
}
