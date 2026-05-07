using System.Collections;
using Assets.Scripts.GameScripts;
using UnityEngine;

public class SceneEntryController2D : MonoBehaviour
{
    [Header("Optional First Room Reveal")]
    [SerializeField] private RoomCoverFade2D roomToRevealOnEnter;

    [SerializeField] private bool revealRoomOnEnter = true;

    [Header("Spawn Offset")]
    [SerializeField] private Vector2 spawnOffset = Vector2.zero;

    [Header("Spawn Timing")]
    [SerializeField] private int framesToWaitBeforeSpawn = 5;

    [Header("Default Spawn")]
    [SerializeField] private string defaultSpawnPointId = "BedSpawnpoint";

    private void Start()
    {
        StartCoroutine(HandleSceneEntry());
    }

    private IEnumerator HandleSceneEntry()
    {
        for (int i = 0; i < framesToWaitBeforeSpawn; i++)
            yield return null;

        Player player = Player.Instance;

        if (player == null)
        {
            Debug.LogWarning("SceneEntryController2D: Player.Instance missing.");
            yield break;
        }

        // NORMAL DOOR TRANSITIONS
        if (TransitionState2D.HasTransition)
        {
            DoorSpawnPoint2D chosenSpawn = FindMatchingDoorSpawnPoint();

            if (chosenSpawn != null)
            {
                Vector2 finalSpawnPosition =
                    (Vector2)chosenSpawn.transform.position + spawnOffset;

                player.SetPosition(finalSpawnPosition);
                player.StopMovement();

                Debug.Log("Spawned from transition at: " + chosenSpawn.spawnPointID);
            }
            else
            {
                Debug.LogWarning("No matching DoorSpawnPoint2D found.");
            }

            TransitionState2D.Clear();
        }
        else
        {
            // FIRST GAME LOAD / MAIN MENU START
            SpawnPoint2D[] spawnPoints =
                Object.FindObjectsByType<SpawnPoint2D>(FindObjectsSortMode.None);

            foreach (SpawnPoint2D spawnPoint in spawnPoints)
            {
                if (spawnPoint.spawnPointId == defaultSpawnPointId)
                {
                    Vector2 finalSpawnPosition =
                        (Vector2)spawnPoint.transform.position + spawnOffset;

                    player.SetPosition(finalSpawnPosition);
                    player.StopMovement();

                    Debug.Log("Spawned at default spawn: " + spawnPoint.spawnPointId);
                    break;
                }
            }
        }

        SnapCamera();
        RevealRoom();
    }

    private DoorSpawnPoint2D FindMatchingDoorSpawnPoint()
    {
        DoorSpawnPoint2D[] spawnPoints =
            Object.FindObjectsByType<DoorSpawnPoint2D>(FindObjectsSortMode.None);

        foreach (DoorSpawnPoint2D spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnPointID == TransitionState2D.SpawnPointID)
                return spawnPoint;
        }

        return null;
    }

    private void RevealRoom()
    {
        if (revealRoomOnEnter && roomToRevealOnEnter != null)
            roomToRevealOnEnter.FadeOut();
    }

    private void SnapCamera()
    {
        CameraFollow2D cam =
            Object.FindAnyObjectByType<CameraFollow2D>();

        if (cam != null)
            cam.SnapToTarget();
    }
}