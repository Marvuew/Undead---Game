using System.Collections;
using UnityEngine;

public class CutsceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool applyRotation = false;
    [SerializeField] private bool waitOneFrameBeforeSnap = true;

    private IEnumerator Start()
    {
        if (!TransitionState2D.hasPendingTransition)
            yield break;

        if (waitOneFrameBeforeSnap)
            yield return null;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning("CutsceneSpawnPoint: No player with tag '" + playerTag + "' found.");
            yield break;
        }

        player.transform.position = transform.position;

        if (applyRotation)
            player.transform.rotation = transform.rotation;
    }
}