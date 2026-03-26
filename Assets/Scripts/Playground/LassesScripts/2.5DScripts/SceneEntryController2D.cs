using System.Collections;
using UnityEngine;

public class SceneEntryController2D : MonoBehaviour
{
    [Header("Optional First Room Reveal")]
    [SerializeField] private RoomCoverFade2D roomToRevealOnEnter;
    [SerializeField] private bool revealRoomOnEnter = true;

    [Header("Auto Walk")]
    [SerializeField] private float autoWalkSpeed = 2.5f;

    [Header("Spawn Offset")]
    [SerializeField] private Vector2 spawnOffset = Vector2.zero;

    private void Start()
    {
        StartCoroutine(HandleSceneEntry());
    }

    private IEnumerator HandleSceneEntry()
    {
        yield return null;

        if (PersistentPlayer2D.Instance == null)
            yield break;

        GameObject player = PersistentPlayer2D.Instance.gameObject;
        PlayerMovement2D movement = player.GetComponent<PlayerMovement2D>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (!TransitionState2D.hasPendingTransition)
        {
            if (revealRoomOnEnter && roomToRevealOnEnter != null)
                roomToRevealOnEnter.FadeOut();

            CameraFollow2D cam = FindObjectOfType<CameraFollow2D>();
            if (cam != null)
                cam.SnapToTarget();

            yield break;
        }

        DoorSpawnPoint2D[] spawnPoints = FindObjectsOfType<DoorSpawnPoint2D>();
        DoorSpawnPoint2D chosenSpawn = null;

        foreach (DoorSpawnPoint2D spawn in spawnPoints)
        {
            if (spawn.spawnPointId == TransitionState2D.spawnPointId)
            {
                chosenSpawn = spawn;
                break;
            }
        }

        if (chosenSpawn != null)
        {
            Vector2 finalSpawnPosition = (Vector2)chosenSpawn.transform.position + spawnOffset;
            player.transform.position = finalSpawnPosition;

            if (rb != null)
                rb.position = finalSpawnPosition;
        }

        CameraFollow2D cameraFollow = FindObjectOfType<CameraFollow2D>();
        if (cameraFollow != null)
            cameraFollow.SnapToTarget();

        if (revealRoomOnEnter && roomToRevealOnEnter != null)
            roomToRevealOnEnter.FadeOut();

        if (movement != null)
            movement.SetMovementEnabled(false);

        if (TransitionState2D.autoWalkDistance > 0f && rb != null)
        {
            Vector2 start = rb.position;
            Vector2 target = start + TransitionState2D.autoWalkDirection * TransitionState2D.autoWalkDistance;

            if (movement != null)
                movement.SetFacingDirection(TransitionState2D.autoWalkDirection);

            while (Vector2.Distance(rb.position, target) > 0.02f)
            {
                Vector2 next = Vector2.MoveTowards(rb.position, target, autoWalkSpeed * Time.deltaTime);
                rb.MovePosition(next);
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }

        if (movement != null)
            movement.SetMovementEnabled(true);

        TransitionState2D.Clear();
    }
}