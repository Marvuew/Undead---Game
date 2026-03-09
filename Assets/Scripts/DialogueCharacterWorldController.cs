using System.Collections;
using UnityEngine;

public class DialogueCharacterWorldController : MonoBehaviour
{
    public enum MCEntryMode
    {
        WithDialoguePanel,     // MC enters when dialogue panel opens
        OnlyWhenTriggered      // MC only enters if Dialogue triggers it
    }

    public enum EnemyEntryMode
    {
        OnlyWhenTriggered,     // Enemy only enters when Dialogue triggers it
        WithDialoguePanel      // (optional) enemy also enters with panel
    }

    [Header("Entry modes")]
    [SerializeField] private MCEntryMode mcEntryMode = MCEntryMode.WithDialoguePanel;
    [SerializeField] private EnemyEntryMode enemyEntryMode = EnemyEntryMode.OnlyWhenTriggered;

    [Header("Characters")]
    [SerializeField] private Transform mc;
    [SerializeField] private Transform enemy;

    [Header("Dialogue UI (only needed for 'WithDialoguePanel' modes)")]
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

    private Coroutine routine;
    private bool dialogueWasActive;

    private void Awake()
    {
        if (mcSprites == null && mc != null) mcSprites = mc.GetComponentInChildren<DialogueSpriteSet>();
        if (enemySprites == null && enemy != null) enemySprites = enemy.GetComponentInChildren<DialogueSpriteSet>();

        if (mc != null) mc.position = mcOffscreenLeft;

        if (enemy != null)
        {
            enemy.position = enemyOffscreenRight;
            enemy.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (dialoguePanel == null) return;

        bool dialogueActive = dialoguePanel.activeInHierarchy;

        // Panel opened
        if (dialogueActive && !dialogueWasActive)
        {
            if (mcEntryMode == MCEntryMode.WithDialoguePanel)
                EnterMC();

            if (enemyEntryMode == EnemyEntryMode.WithDialoguePanel)
                EnterEnemy();
        }

        // Panel closed
        if (!dialogueActive && dialogueWasActive)
        {
            // Always hide on panel close (keeps your scene clean)
            ExitMC();
            ExitEnemyImmediate();
        }

        dialogueWasActive = dialogueActive;
    }

    // -------- Public controls (call from MessageChainDialogue) --------

    public void EnterMC()  => StartRoutine(Move(mc, mcOnscreen, slideSeconds));
    public void ExitMC()   => StartRoutine(Move(mc, mcOffscreenLeft, slideSeconds));

    public void EnterEnemy()
    {
        if (enemy == null) return;
        enemy.gameObject.SetActive(true);
        StartRoutine(Move(enemy, enemyOnscreen, slideSeconds));
    }

    public void ExitEnemy()
    {
        if (enemy == null) return;
        StartRoutine(ExitEnemyRoutine());
    }

    public void ReactAgainstHumans()
    {
        StartRoutine(EnemyReactionRoutine());
    }

    public void SetMCExpression(string id)
    {
        if (mcSprites != null) mcSprites.SetSprite(id);
    }

    public void SetEnemyExpression(string id)
    {
        if (enemySprites != null) enemySprites.SetSprite(id);
    }

    // -------- Internals --------

    private void ExitEnemyImmediate()
    {
        if (enemy == null) return;
        enemy.position = enemyOffscreenRight;
        enemy.gameObject.SetActive(false);
    }

    private void StartRoutine(IEnumerator next)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(next);
    }

    private IEnumerator EnemyReactionRoutine()
    {
        if (enemy == null) yield break;

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
    }

    private IEnumerator ExitEnemyRoutine()
    {
        if (enemy == null) yield break;

        yield return Move(enemy, enemyOffscreenRight, slideSeconds);
        enemy.gameObject.SetActive(false);
    }

    private IEnumerator Move(Transform tr, Vector3 target, float seconds)
    {
        if (tr == null) yield break;

        Vector3 start = tr.position;

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / seconds);
            p = p * p * (3f - 2f * p);
            tr.position = Vector3.Lerp(start, target, p);
            yield return null;
        }

        tr.position = target;
    }
}