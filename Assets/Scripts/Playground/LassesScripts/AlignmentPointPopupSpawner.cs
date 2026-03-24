using UnityEngine;

public class AlignmentPointPopupSpawner : MonoBehaviour
{
    [Header("Prefab + Parent")]
    [SerializeField] private AlignmentPointPopupUI popupPrefab;

    [Tooltip("If not assigned, script will search for a RectTransform named 'PopupLayer' (including inactive).")]
    [SerializeField] private RectTransform popupLayer;

    [Header("Positioning")]
    [SerializeField] private Vector2 baseAnchoredPosition = new Vector2(0f, -260f);
    [SerializeField] private Vector2 randomScatter = new Vector2(50f, 12f);
    [SerializeField] private float stackStepY = 18f;

    [Header("Colors")]
    [SerializeField] private Color humansColor = new Color(0.3f, 0.9f, 0.5f, 1f);
    [SerializeField] private Color undeadColor = new Color(0.4f, 0.6f, 1f, 1f);

    [Header("Text Labels")]
    [SerializeField] private string humansLabel = "Humans";
    [SerializeField] private string undeadLabel = "Undead";

    private int stackCount;
    private float lastSpawnTime;

    private void OnEnable()
    {
        // When scene reloads, references can be missing -> try to recover
        EnsurePopupLayer();
    }

    public void ShowBoth(float humansDelta, float undeadDelta)
    {
        if (!Mathf.Approximately(humansDelta, 0f)) ShowHumans(humansDelta);
        if (!Mathf.Approximately(undeadDelta, 0f)) ShowUndead(undeadDelta);
    }

    public void ShowHumans(float amount)
    {
        if (Mathf.Approximately(amount, 0f)) return;
        string sign = amount > 0f ? "+" : "-";
        Spawn($"{sign}{FormatAbs(amount)} {humansLabel}", humansColor);
    }

    public void ShowUndead(float amount)
    {
        if (Mathf.Approximately(amount, 0f)) return;
        string sign = amount > 0f ? "+" : "-";
        Spawn($"{sign}{FormatAbs(amount)} {undeadLabel}", undeadColor);
    }

    private void Spawn(string text, Color color)
    {
        // Critical: re-check every time, because your book canvas/layer may be inactive until opened
        EnsurePopupLayer();

        if (popupPrefab == null || popupLayer == null)
        {
            Debug.LogWarning($"{nameof(AlignmentPointPopupSpawner)} on {name}: Missing popupPrefab or popupLayer.");
            return;
        }

        // stack logic
        float now = Time.unscaledTime;
        if (now - lastSpawnTime > 0.35f) stackCount = 0;
        lastSpawnTime = now;

        Vector2 pos = baseAnchoredPosition;
        pos += new Vector2(Random.Range(-randomScatter.x, randomScatter.x),
                           Random.Range(-randomScatter.y, randomScatter.y));
        pos += new Vector2(0f, stackCount * stackStepY);
        stackCount++;

        var instance = Instantiate(popupPrefab, popupLayer);
        instance.Init(text, color, pos);
    }

    private void EnsurePopupLayer()
    {
        if (popupLayer != null) return;

        // Find a RectTransform named "PopupLayer" even if inactive
        var allRects = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var rt in allRects)
        {
            if (rt != null && rt.name == "PopupLayer")
            {
                popupLayer = rt;
                return;
            }
        }
    }

    private string FormatAbs(float value)
    {
        float a = Mathf.Abs(value);

        if (Mathf.Abs(a - Mathf.Round(a)) < 0.0001f)
            return Mathf.Round(a).ToString("0");

        return a.ToString("0.##");
    }
}