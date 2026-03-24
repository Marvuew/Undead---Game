using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomRevealTrigger2D : MonoBehaviour
{
    public RoomCoverFade2D roomCover;
    public string playerTag = "Player";
    public bool revealOnlyOnce = true;

    private bool hasRevealed = false;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (roomCover == null)
            return;

        if (revealOnlyOnce && hasRevealed)
            return;

        roomCover.FadeOut();
        hasRevealed = true;
    }
}

