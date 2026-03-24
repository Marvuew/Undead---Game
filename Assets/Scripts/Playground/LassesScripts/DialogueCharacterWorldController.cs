using System.Collections;
using UnityEngine;

public class DialogueCharacterWorldController : MonoBehaviour
{
    public enum MCEntryMode
    {
        WithDialoguePanel,     // MC enters automatically when a matching dialogue opens
        OnlyWhenTriggered      // MC only enters if Dialogue explicitly triggers it
    }

    public enum EnemyEntryMode
    {
        OnlyWhenTriggered,     // Enemy only enters if Dialogue explicitly triggers it
        WithDialoguePanel      // Enemy enters automatically when a matching dialogue opens
    }

    [Header("Entry modes")]
    [SerializeField] private MCEntryMode mcEntryMode = MCEntryMode.WithDialoguePanel;
    [SerializeField] private EnemyEntryMode enemyEntryMode = EnemyEntryMode.OnlyWhenTriggered;

    [Header("Auto-enter dialogue IDs")]
    [Tooltip("MC auto-enters only for these conversation IDs. Leave empty = never auto-enter.")]
    [SerializeField] private string[] mcAutoEnterDialogueIds;

    [Tooltip("Enemy auto-enters only for these conversation IDs. Leave empty = never auto-enter.")]
    [SerializeField] private string[] enemyAutoEnterDialogueIds;

    [Header("Characters")]
    [SerializeField] private Transform mc;
    [SerializeField] private Transform enemy;

    [Header("Dialogue UI (optional fallback close detection)")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Sprite sets (optional)")]
    [SerializeField] private DialogueSpriteSet mcSprites;
    [SerializeField] private DialogueSpriteSet enemySprites;

    [Header("Positions")]
    [SerializeField] private Vector3 mcOffscreenLeft = new Vector3(-12f, -1.53f, -2.74f);
    [SerializeField] private Vector3 mcOnscreen = new Vector3(-6.43f, -1.53f, -2.74f);

    [SerializeField] private Vector3 enemyOffscreenRight = new Vector3(12f, -1.53f, -2.74f);
    [SerializeField] private Vector3 enemyOnscreen = new Vector3(-4.2f, -1.53f, -2.74f);

    [Header("Timing")]
    [SerializeField] private float slideSeconds = 0.25f;
    [SerializeField] private float enemyHoldSeconds = 0.5f;

    private Coroutine mcRoutine;
    private Coroutine enemyRoutine;

    private bool dialogueWasActive;
    private string activeDialogueId = "";

    private void Awake()
    {
        if (mcSprites == null && mc != null)
            mcSprites = mc.GetComponentInChildren<DialogueSpriteSet>();

        if (enemySprites == null && enemy != null)
            enemySprites = enemy.GetComponentInChildren<DialogueSpriteSet>();

        ResetCharactersToHiddenState();
    }

    private void Update()
    {
        if (dialoguePanel == null)
            return;

        bool dialogueActive = dialoguePanel.activeInHierarchy;

        // Fallback cleanup if the panel is closed externally.
        if (!dialogueActive && dialogueWasActive)
            NotifyDialogueClosed();

        dialogueWasActive = dialogueActive;
    }

    // -------- Called by MessageChainDialogue --------

    public void NotifyDialogueOpened(string dialogueId)
    {
        activeDialogueId = dialogueId ?? "";

        if (mcEntryMode == MCEntryMode.WithDialoguePanel &&
            ShouldAutoEnter(mcAutoEnterDialogueIds, activeDialogueId))
        {
            EnterMC();
        }

        if (enemyEntryMode == EnemyEntryMode.WithDialoguePanel &&
            ShouldAutoEnter(enemyAutoEnterDialogueIds, activeDialogueId))
        {
            EnterEnemy();
        }

        if (dialoguePanel != null)
            dialogueWasActive = dialoguePanel.activeInHierarchy;
    }

    public void NotifyDialogueClosed()
    {
        activeDialogueId = "";

        ExitMC();
        ExitEnemyImmediate();

        if (dialoguePanel != null)
            dialogueWasActive = dialoguePanel.activeInHierarchy;
    }

    // -------- Public controls (call from MessageChainDialogue) --------

    public void EnterMC()
    {
        if (mc == null) return;
        StartMCRoutine(Move(mc, mcOnscreen, slideSeconds));
    }

    public void ExitMC()
    {
        if (mc == null) return;
        StartMCRoutine(Move(mc, mcOffscreenLeft, slideSeconds));
    }

    public void EnterEnemy()
    {
        if (enemy == null) return;

        enemy.gameObject.SetActive(true);
        StartEnemyRoutine(Move(enemy, enemyOnscreen, slideSeconds));
    }

    public void ExitEnemy()
    {
        if (enemy == null) return;
        StartEnemyRoutine(ExitEnemyRoutine());
    }

    public void ReactAgainstHumans()
    {
        if (enemy == null) return;
        StartEnemyRoutine(EnemyReactionRoutine());
    }

    public void SetMCExpression(string id)
    {
        if (mcSprites != null)
            mcSprites.SetSprite(id);
    }

    public void SetEnemyExpression(string id)
    {
        if (enemySprites != null)
            enemySprites.SetSprite(id);
    }

    // -------- Internals --------

    private bool ShouldAutoEnter(string[] allowedIds, string dialogueId)
    {
        if (string.IsNullOrWhiteSpace(dialogueId))
            return false;

        if (allowedIds == null || allowedIds.Length == 0)
            return false;

        string trimmedDialogueId = dialogueId.Trim();

        for (int i = 0; i < allowedIds.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(allowedIds[i]))
                continue;

            if (allowedIds[i].Trim() == trimmedDialogueId)
                return true;
        }

        return false;
    }

    private void ResetCharactersToHiddenState()
    {
        if (mc != null)
            mc.position = mcOffscreenLeft;

        if (enemy != null)
        {
            enemy.position = enemyOffscreenRight;
            enemy.gameObject.SetActive(false);
        }
    }

    private void ExitEnemyImmediate()
    {
        if (enemy == null) return;

        StopEnemyRoutine();
        enemy.position = enemyOffscreenRight;
        enemy.gameObject.SetActive(false);
    }

    private void StartMCRoutine(IEnumerator next)
    {
        StopMCRoutine();
        mcRoutine = StartCoroutine(next);
    }

    private void StartEnemyRoutine(IEnumerator next)
    {
        StopEnemyRoutine();
        enemyRoutine = StartCoroutine(next);
    }

    private void StopMCRoutine()
    {
        if (mcRoutine != null)
        {
            StopCoroutine(mcRoutine);
            mcRoutine = null;
        }
    }

    private void StopEnemyRoutine()
    {
        if (enemyRoutine != null)
        {
            StopCoroutine(enemyRoutine);
            enemyRoutine = null;
        }
    }

    private IEnumerator EnemyReactionRoutine()
    {
        if (enemy == null)
            yield break;

        enemy.gameObject.SetActive(true);

        yield return Move(enemy, enemyOnscreen, slideSeconds);

        float t = 0f;
        while (t < enemyHoldSeconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return Move(enemy, enemyOffscreenRight, slideSeconds);

        enemy.gameObject.SetActive(false);
        enemyRoutine = null;
    }

    private IEnumerator ExitEnemyRoutine()
    {
        if (enemy == null)
            yield break;

        yield return Move(enemy, enemyOffscreenRight, slideSeconds);

        enemy.gameObject.SetActive(false);
        enemyRoutine = null;
    }

    private IEnumerator Move(Transform tr, Vector3 target, float seconds)
    {
        if (tr == null)
            yield break;

        Vector3 start = tr.position;
        float safeSeconds = Mathf.Max(0.0001f, seconds);

        float t = 0f;
        while (t < safeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / safeSeconds);
            p = p * p * (3f - 2f * p); // smoothstep
            tr.position = Vector3.Lerp(start, target, p);
            yield return null;
        }

        tr.position = target;
    }
}