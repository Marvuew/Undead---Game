using UnityEngine;
using UnityEngine.UI;

public class BookUIPageController : MonoBehaviour
{
    [Header("Pages (root objects). Order: 0=Closed, 1..n pages")]
    [SerializeField] private GameObject[] pages;

    [Header("Behavior")]
    [Tooltip("If true, pages not active are disabled. If false, all stay active but raycasts are blocked.")]
    [SerializeField] private bool deactivateNonActivePages = false;

    [Tooltip("Remember last page (including closed)")]
    [SerializeField] private bool rememberLastPage = true;

    [SerializeField] private string memoryKey = "NecroBook_LastPage";

    [Header("Raycast Control")]
    [SerializeField] private bool forceDisableRaycastsOnInactivePages = true;
    [SerializeField] private bool forceEnableRaycastsOnActivePage = true;

    private int currentPageIndex = 0;

    private void Awake()
    {
        if (pages == null || pages.Length == 0)
        {
            Debug.LogError("BookUIPageController: No pages assigned.");
            return;
        }

        if (rememberLastPage)
            currentPageIndex = Mathf.Clamp(PlayerPrefs.GetInt(memoryKey, 0), 0, pages.Length - 1);

        ApplyPageState();
    }

    public int CurrentPageIndex => currentPageIndex;

    public void NextPage()
    {
        SetPage(Mathf.Clamp(currentPageIndex + 1, 0, pages.Length - 1));
    }

    public void PrevPage()
    {
        SetPage(Mathf.Clamp(currentPageIndex - 1, 0, pages.Length - 1));
    }

    public void SetPage(int index)
    {
        if (pages == null || pages.Length == 0) return;
        if (index < 0 || index >= pages.Length) return;

        currentPageIndex = index;

        if (rememberLastPage)
        {
            PlayerPrefs.SetInt(memoryKey, currentPageIndex);
            PlayerPrefs.Save();
        }

        ApplyPageState();
    }

    private void ApplyPageState()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            if (page == null) continue;

            bool isActive = (i == currentPageIndex);

            // Visibility
            if (deactivateNonActivePages)
            {
                page.SetActive(isActive);
            }
            else
            {
                // keep them active (your stacked-layer setup)
                page.SetActive(true);
            }

            // Raycast blocking at page level (works even with stacked layers)
            var cg = page.GetComponent<CanvasGroup>();
            if (cg == null) cg = page.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = isActive;
            cg.interactable = isActive;

            // Extra: force raycast targets off/on inside each page (kills click stealing)
            if (forceDisableRaycastsOnInactivePages && !isActive)
                SetAllGraphicRaycastTargets(page, false);

            if (forceEnableRaycastsOnActivePage && isActive)
                SetAllGraphicRaycastTargets(page, true);
        }

        // Refresh highlight scripts on the active page (important when pages stay active)
        var active = pages[currentPageIndex];
        if (active != null)
        {
            foreach (var h in active.GetComponentsInChildren<HighlightableTMP>(true))
                h.Refresh();
        }
    }

    private static void SetAllGraphicRaycastTargets(GameObject root, bool enabled)
    {
        var graphics = root.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
        {
            if (g == null) continue;
            g.raycastTarget = enabled;
        }
    }
}