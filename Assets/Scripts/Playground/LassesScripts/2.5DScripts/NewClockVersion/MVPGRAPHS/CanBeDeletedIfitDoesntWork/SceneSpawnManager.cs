using Assets.Scripts.GameScripts;
using UnityEngine;

public class SceneSpawnManager2D : MonoBehaviour
{
    private void Start()
    {
        if (!TransitionState2D.HasTransition)
            return;

        DoorSpawnPoint2D[] spawnPoints = FindObjectsOfType<DoorSpawnPoint2D>();

        foreach (DoorSpawnPoint2D spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnPointID == TransitionState2D.SpawnPointID)
            {
                if (Player.Instance != null)
                    Player.Instance.SetPosition(spawnPoint.transform.position);

                TransitionState2D.Clear();
                return;
            }
        }

        Debug.LogWarning("No matching DoorSpawnPoint2D found for: " + TransitionState2D.SpawnPointID);
    }
}