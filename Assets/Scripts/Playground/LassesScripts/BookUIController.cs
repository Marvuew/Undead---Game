using UnityEngine;
using UnityEngine.InputSystem;

public class BookUIController : MonoBehaviour
{
    [Header("All pages (only ONE will be active at a time)")]
    [Tooltip("Drag pages in any order. Include the closed cover page too.")]
    [SerializeField] private GameObject[] pages;

    [Header("Key to toggle book")]
    [SerializeField] private Key toggleKey = Key.E;

    [Header("Chapter targets (drag the page UI objects here)")]
    [SerializeField] private GameObject vampireChapterPage;
    [SerializeField] private GameObject ghastChapterPage;
    [SerializeField] private GameObject faeChapterPage;

    [Header("Mutual exclusion")]
    [Tooltip("Drag your MapToggleUI script object here.")]
    [SerializeField] private MapToggleUI mapToggle;

    private int currentPageIndex = 0; // remembers exact page, including cover
    private bool bookVisible = false;

    public bool IsBookOpen => bookVisible;

    private void Start()
    {
        // Start hidden, but remember currentPageIndex (default 0)
        HideAllPages();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (!bookVisible)
            {
                // If opening book, force-close map
                if (mapToggle != null) mapToggle.CloseMap();

                ShowPageByIndex(currentPageIndex); // reopen where we left off
            }
            else
            {
                HideAllPages();
            }
        }
    }

    // --------------------
    // Core show/hide logic
    // --------------------

    private void HideAllPages()
    {
        if (pages == null) return;

        foreach (var page in pages)
            if (page != null) page.SetActive(false);

        bookVisible = false;
        // IMPORTANT: do NOT reset currentPageIndex
    }

    private void ShowOnly(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("BookUIController: Tried to show a null page.");
            return;
        }

        // If opening a book page, force-close map
        if (mapToggle != null) mapToggle.CloseMap();

        // Deactivate all known pages first
        if (pages != null)
        {
            foreach (var page in pages)
                if (page != null) page.SetActive(false);
        }

        // Activate requested page
        target.SetActive(true);

        bookVisible = true;

        // If this target exists inside pages[], remember its index
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] == target)
                {
                    currentPageIndex = i;
                    return;
                }
            }
        }

        Debug.LogWarning("BookUIController: Target page is not in pages[] so reopen position can't be remembered.");
    }

    private void ShowPageByIndex(int index)
    {
        if (pages == null || pages.Length == 0) return;

        index = Mathf.Clamp(index, 0, pages.Length - 1);

        for (int i = 0; i < pages.Length; i++)
            if (pages[i] != null) pages[i].SetActive(i == index);

        currentPageIndex = index;
        bookVisible = true;
    }

    // --------------------
    // Next/Prev (your buttons)
    // --------------------

    public void NextPage()
    {
        if (!bookVisible) return;
        ShowPageByIndex(currentPageIndex + 1);
    }

    public void PrevPage()
    {
        if (!bookVisible) return;
        ShowPageByIndex(currentPageIndex - 1);
    }

    // --------------------
    // Chapter buttons
    // --------------------

    public void GoToVampireChapter() => ShowOnly(vampireChapterPage);
    public void GoToGhastChapter()   => ShowOnly(ghastChapterPage);
    public void GoToFaeChapter()     => ShowOnly(faeChapterPage);

    // Generic: lets you use one function for any button and drag a page in
    public void GoToPage(GameObject page) => ShowOnly(page);

    // --------------------
    // Called by Map to force close book
    // --------------------
    public void ForceCloseBook()
    {
        HideAllPages();
    }
}