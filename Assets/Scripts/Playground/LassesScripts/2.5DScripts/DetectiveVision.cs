using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetectiveVision : MonoBehaviour
{
    public static DetectiveVision Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private KeyCode abilityKey = KeyCode.Q;

    [Header("Ability")]
    [SerializeField] private float activeDuration = 10f;
    [SerializeField] private float cooldownDuration = 10f;
    [SerializeField] private float revealRadius = 3f;

    [Header("UI - Ability Icon")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string readyPrompt = "Press \"Q\" to activate";
    [SerializeField] private string cooldownPrompt = "Recharging...";
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("UI - Red Screen Flash")]
    [SerializeField] private Image screenFlashOverlay;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.35f);
    [SerializeField] private float flashMinAlpha = 0.08f;
    [SerializeField] private float flashMaxAlpha = 0.28f;
    [SerializeField] private float flashSlowSpeed = 1.5f;
    [SerializeField] private float flashFastSpeed = 7f;

    [Header("World Radius Visual")]
    [SerializeField] private Transform revealRadiusVisual;
    [SerializeField] private float visualAlpha = 0.18f;

    private bool abilityActive;
    private float activeTimer;
    private float cooldownTimer;
    private float flashTime;

    public bool AbilityActive => abilityActive;
    public bool IsReady => cooldownTimer <= 0f;
    public float RevealRadius => revealRadius;

    public float ActiveNormalizedRemaining =>
        activeDuration <= 0f ? 0f : Mathf.Clamp01(activeTimer / activeDuration);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
        UpdateFlashOverlay();
        UpdateRadiusVisual();
    }

    private void Update()
    {
        HandleInput();
        UpdateTimers();
        UpdateUI();
        UpdateFlashOverlay();
        UpdateRadiusVisual();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(abilityKey) && IsReady)
            ActivateAbility();
    }

    private void ActivateAbility()
    {
        abilityActive = true;
        activeTimer = activeDuration;
        cooldownTimer = cooldownDuration;
        flashTime = 0f;

        DetectiveVisionRevealable.RefreshAll();
    }

    private void DeactivateAbility()
    {
        abilityActive = false;
        DetectiveVisionRevealable.RefreshAll();
    }

    private void UpdateTimers()
    {
        if (abilityActive)
        {
            activeTimer -= Time.deltaTime;

            if (activeTimer <= 0f)
            {
                activeTimer = 0f;
                DeactivateAbility();
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer < 0f)
                cooldownTimer = 0f;
        }

        if (abilityActive)
        {
            float urgency = 1f - ActiveNormalizedRemaining;
            float flashSpeed = Mathf.Lerp(flashSlowSpeed, flashFastSpeed, urgency);
            flashTime += Time.deltaTime * flashSpeed;
        }
    }

    private void UpdateUI()
    {
        if (abilityIcon != null)
        {
            float iconT = cooldownDuration <= 0f
                ? 1f
                : 1f - Mathf.Clamp01(cooldownTimer / cooldownDuration);

            abilityIcon.color = Color.Lerp(cooldownColor, readyColor, iconT);
        }

        if (promptText != null)
            promptText.text = IsReady ? readyPrompt : cooldownPrompt;
    }

    private void UpdateFlashOverlay()
    {
        if (screenFlashOverlay == null)
            return;

        if (!abilityActive)
        {
            Color hidden = flashColor;
            hidden.a = 0f;
            screenFlashOverlay.color = hidden;
            return;
        }

        float pulse = (Mathf.Sin(flashTime) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(flashMinAlpha, flashMaxAlpha, pulse);

        Color c = flashColor;
        c.a = alpha;
        screenFlashOverlay.color = c;
    }

    private void UpdateRadiusVisual()
    {
        if (revealRadiusVisual == null)
            return;

        revealRadiusVisual.gameObject.SetActive(abilityActive);

        if (!abilityActive)
            return;

        float diameter = revealRadius * 2f;
        revealRadiusVisual.localScale = new Vector3(diameter, diameter, 1f);

        SpriteRenderer sr = revealRadiusVisual.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = visualAlpha;
            sr.color = c;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, revealRadius);
    }
}