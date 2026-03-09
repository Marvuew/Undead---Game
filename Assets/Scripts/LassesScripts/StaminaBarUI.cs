using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("Assign the stamina panel images in order")]
    [SerializeField] private Image[] staminaSlots;

    [Header("Optional colors")]
    [SerializeField] private Color filledColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Setup")]
    [SerializeField] private int defaultMaxStamina = 10;

    private void Awake()
    {
        GlobalStaminaSystem.InitializeIfNeeded(defaultMaxStamina);
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int current = GlobalStaminaSystem.GetCurrentStamina();
        int max = GlobalStaminaSystem.GetMaxStamina();

        for (int i = 0; i < staminaSlots.Length; i++)
        {
            if (staminaSlots[i] == null)
                continue;

            bool shouldExist = i < max;
            staminaSlots[i].gameObject.SetActive(shouldExist);

            if (!shouldExist)
                continue;

            staminaSlots[i].color = i < current ? filledColor : emptyColor;
        }
    }

    [ContextMenu("Debug Refill")]
    public void DebugRefill()
    {
        GlobalStaminaSystem.RefillToMax();
        Refresh();
    }

    [ContextMenu("Debug Empty")]
    public void DebugEmpty()
    {
        GlobalStaminaSystem.SetCurrentStamina(0);
        Refresh();
    }
}
