using UnityEngine;

public static class GameProgressState
{
    public static bool HasNecrolexicon = false;
    public static bool CompletedHouseIntro = false;
    public static bool ForceSkippedHouseIntro = false;

    public static string CurrentHouseDayId = "Day1";
    public static string ReturnSpawnPointId = "BedSpawnpoint";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnPlay()
    {
        HasNecrolexicon = false;
        CompletedHouseIntro = false;
        ForceSkippedHouseIntro = false;
        CurrentHouseDayId = "Day1";
        ReturnSpawnPointId = "BedSpawnpoint";
    }
}