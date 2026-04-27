using UnityEngine;

public static class TransitionState2D
{
    public static bool hasPendingTransition = false;

    public static string spawnPointId = "";
    public static Vector2 autoWalkDirection = Vector2.zero;
    public static float autoWalkDistance = 0f;

    public static bool isSceneEntryInProgress = false;

    public static void SetTransition(string targetSpawnPointId, Vector2 direction, float distance)
    {
        hasPendingTransition = true;
        spawnPointId = targetSpawnPointId;
        autoWalkDirection = direction.normalized;
        autoWalkDistance = distance;
    }

    public static void Clear()
    {
        hasPendingTransition = false;
        spawnPointId = "";
        autoWalkDirection = Vector2.zero;
        autoWalkDistance = 0f;
        isSceneEntryInProgress = false;
    }
}