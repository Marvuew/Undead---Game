using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueLockedZone2D : MonoBehaviour
{
    [Header("Requirement")]
    [SerializeField] private string requiredConversationId = "";
    [SerializeField] private string requiredCharacterName = "the rookie";

    [Header("Message")]
    [TextArea]
    [SerializeField] private string blockedMessageTemplate = "I probably should talk to {name} before I go there.";
    [SerializeField] private float messageCooldown = 1.25f;

    [Header("References")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private WorldSpeechPopup speechPopupPrefab;

    [Header("Optional")]
    [SerializeField] private bool disableSelfWhenUnlocked = false;

    private float cooldownTimer;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        RefreshBlockedState();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        RefreshBlockedState();
    }

    private void RefreshBlockedState()
    {
        bool unlocked = IsUnlocked();

        if (blockingCollider != null)
            blockingCollider.enabled = !unlocked;

        if (disableSelfWhenUnlocked && unlocked)
            enabled = false;
    }

    private bool IsUnlocked()
    {
        return MessageChainDialogue.HasCompletedConversation(requiredConversationId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (IsUnlocked())
            return;

        if (cooldownTimer > 0f)
            return;

        ShowBlockedMessage(other.transform);
        cooldownTimer = messageCooldown;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (IsUnlocked())
            return;

        if (cooldownTimer > 0f)
            return;

        ShowBlockedMessage(other.transform);
        cooldownTimer = messageCooldown;
    }

    private void ShowBlockedMessage(Transform playerTransform)
    {
        if (speechPopupPrefab == null)
            return;

        string message = blockedMessageTemplate.Replace("{name}", requiredCharacterName);
        WorldSpeechPopup popup = Instantiate(speechPopupPrefab);
        popup.Show(message, playerTransform);
    }
}