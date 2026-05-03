using UnityEngine;

public static class GameProgressState
{
    public static bool HasNecrolexicon = false;
    public static bool CompletedHouseIntro = false;
    public static string ReturnSpawnPointId = "BedSpawnpoint";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnPlay()
    {
        HasNecrolexicon = false;
        CompletedHouseIntro = false;
        ReturnSpawnPointId = "BedSpawnpoint";
    }
}