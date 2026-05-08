using UnityEngine;

public static class TransitionState2D
{
    public static string TargetSceneName;
    public static SpawnPointID SpawnPointID;

    public static bool HasTransition;

    public static Vector2 autoWalkDirection;
    public static float autoWalkDistance;

    public static bool hasPendingTransition => HasTransition;
    public static string spawnPointId => SpawnPointID.ToString();

    public static Vector2 AutoWalkDirection => autoWalkDirection;
    public static float AutoWalkDistance => autoWalkDistance;

    public static void SetTransition(
        string targetSceneName,
        SpawnPointID spawnPointID,
        Vector2 walkDirection,
        float walkDistance
    )
    {
        TargetSceneName = targetSceneName;
        SpawnPointID = spawnPointID;
        autoWalkDirection = walkDirection;
        autoWalkDistance = walkDistance;
        HasTransition = true;
    }

    public static void Clear()
    {
        TargetSceneName = "";
        SpawnPointID = SpawnPointID.None;
        autoWalkDirection = Vector2.zero;
        autoWalkDistance = 0f;
        HasTransition = false;
    }
}