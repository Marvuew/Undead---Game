using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NecroLexiconUI : MonoBehaviour
{
    public int cluesPerPage = 4;

    [Header("UI Elements")]
    public GameObject creaturesPage;
    public TMPro.TextMeshProUGUI creaturesText;
    public GameObject casePage;
    public GameObject bookCover;
    public GameObject pagesContainer;
    public Transform leftSideContainer;

    [Header("References")]
    public CreatureManager creatureManager;
    public CasePage casePageScript;
    public CaseData caseData;
    public CaseManagerMathilde caseManager;

    [Header("Prefabs")]
    public GameObject casePanelPrefab;

    [Header("Clue layout - Lasse A")]
    public LayoutSwitcher currentClueLayout;
    public GameObject activeCluePage;
    public List<GameObject> cluePages = new List<GameObject>();
    public GameObject cluePage;
    public GameObject clueTxtPrefab;
    public List<string> tempDescriptions = new List<string>();
    public List<Clue> clues = new List<Clue>();
    public GameObject nextCluePageBtn;
    public GameObject lastCluePageBtn;
    public GameObject cluesHeader;

    public static NecroLexiconUI Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    } //Ensuring singleton pattern

    [System.Serializable]
    public class  PageButton
    {
        public RectTransform button;
        public Vector2 closedPos;
        public Vector2 openPos;
        public Button uiButton;
        public UnityEngine.UI.Image image;
    }

    public List<PageButton> pageButtons;

    private void Start()
    {
        bookCover.SetActive(true);
        pagesContainer.SetActive(false);
        
        foreach (var pb in pageButtons)
        {
            pb.button.anchoredPosition = pb.closedPos;
            pb.button.gameObject.SetActive(false);
        }
    }

    void SetSelectedButton(PageButton selected)
    {
        foreach (var pb in pageButtons)
        {
            if (pb == selected)
            {
                pb.image.color = new Color32(227, 138, 138, 255); // selected farve
            }
            else
            {
                pb.image.color = Color.white; // white
            }
        }
    }

    /*public void OpenCluesPage()
    {
        OpenBook();
        Debug.Log("Clues clicked");
        DisableAllPages();
        cluesText.enabled = true;
        UpdateCluesList();

        SetSelectedButton(pageButtons[0]);
    }*/
    public void OpenCreaturesPage()
    {
        OpenBook();
        Debug.Log("Creatures clicked");
        DisableAllPages();
        creaturesPage.SetActive(true);
        creaturesText.enabled = true;

        SetSelectedButton(pageButtons[1]);
    }

    public void OpenCasePage()
    {
        OpenBook();
        Debug.Log("Case clicked");
        DisableAllPages();
        casePage.SetActive(true);

        SetSelectedButton(pageButtons[2]);
    }

    public void OpenCluePage()
    {
        UpdateCluesList();
        ToggleCluePage(cluePages[0]);
        lastCluePageBtn.SetActive(true);
        nextCluePageBtn.SetActive(true);
        print("Opened first cluepage");
    }

    public void ToggleCluePage(GameObject page)
    {
        OpenBook();
        DisableAllPages(); // This hides everything first
        cluesHeader.SetActive(true);

        // Set only the requested page to active
        page.SetActive(true);
        activeCluePage = page;
        SetSelectedButton(pageButtons[0]);
    }

    public void NextCluePage()
    {
        int currentIndex = cluePages.IndexOf(activeCluePage);

        if (currentIndex + 1 < cluePages.Count)
        {
            ToggleCluePage(cluePages[currentIndex + 1]);
            UpdateNavButtons(); // Refresh visibility
        }
    }

    public void LastCluePage()
    {
        int currentIndex = cluePages.IndexOf(activeCluePage);

        if (currentIndex - 1 >= 0)
        {
            ToggleCluePage(cluePages[currentIndex - 1]);
            UpdateNavButtons(); // Refresh visibility
        }
    }

    // Call this inside OpenCluePage and whenever you flip pages
    public void UpdateNavButtons()
    {
        int currentIndex = cluePages.IndexOf(activeCluePage);

        // Assign these buttons in the inspector
        nextCluePageBtn.SetActive(currentIndex < cluePages.Count - 1);
        lastCluePageBtn.SetActive(currentIndex > 0);
    }

    private void DisableAllPages()
    {
        foreach(var layout in cluePages)
        {
            layout.gameObject.SetActive(false);
        }
        cluesHeader.SetActive(false);
        creaturesPage.SetActive(false);
        creaturesText.enabled = false;
        casePage.SetActive(false);
        creatureManager.OnTabChanged();
        caseManager.OnTabChanged();

    }

    public void OpenBook()
    {
        if (bookCover.activeSelf == true)
        {
            //soundManager.PlayOpenBookSound();
            AudioManager.instance.PlaySFX("OpenBook");
        }
        else
        {
            //soundManager.PlayPageTurnSound();
            AudioManager.instance.PlaySFX("PageTurn1");
        }

        bookCover.SetActive(false);
        pagesContainer.SetActive(true);

        foreach (var pb in pageButtons)
        {
            pb.button.anchoredPosition = pb.openPos;
            pb.button.gameObject.SetActive(true);
        }
    }
    public void ToggleBook(GameObject book) { book.SetActive((book.activeSelf == true) ? false : true); }
    public void CloseBook()
    {
        //soundManager.PlayCloseBookSound();
        AudioManager.instance.PlaySFX("CloseBook");
        DisableAllPages();
        bookCover.SetActive(true);
        pagesContainer.SetActive(false);

        foreach (var pb in pageButtons)
        {
            pb.button.anchoredPosition = pb.closedPos;
            pb.button.gameObject.SetActive(false);
        }

        SetSelectedButton(pageButtons[3]);
    }

    public GameObject CreateNewCluePage()
    {
        // Instantiate the prefab inside your pages container
        GameObject newPage = Instantiate(cluePage, pagesContainer.transform);

        // Ensure it's positioned correctly (RectTransform reset)
        RectTransform rt = newPage.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // Add to your list for navigation (Next/Last page)
        cluePages.Add(newPage);

        return newPage;
    }

    public void UpdateCluesList()
    {
        // 1. Force containers active (Essential for Layout Calculation)
        pagesContainer.SetActive(true);
        if (cluePages.Count > 0) cluePages[0].SetActive(true);

        // 2. Clear all previous clues and extra pages
        ClearClueList();

        // 3. Dictionary Safety (Test data)
        foreach (Clue clue in clues)
        {
            if (!CaseManager.Instance.clueDescriptions.ContainsKey(clue))
            {
                CaseManager.Instance.clueDescriptions.Add(clue, new List<string>(tempDescriptions));
            }
        }

        // 4. Determine which clues to show
        var cluesToDisplay = (CaseManager.Instance.cluesfound.Count == 0) ? clues : CaseManager.Instance.cluesfound.ToList();

        // 5. Initialize the LayoutSwitcher from the first page
        if (cluePages.Count > 0)
        {
            currentClueLayout = cluePages[0].GetComponent<LayoutSwitcher>();

            // Ensure the LayoutSwitcher is reset to start at the Left Page
            currentClueLayout.ResetToLeftPage();
        }
        else
        {
            Debug.LogError("No CluePages found in the list! Assign the first page in the Inspector.");
            return;
        }

        // 6. Spawn the items
        foreach (Clue _clue in cluesToDisplay)
        {
            Debug.Log($"Attempting to spawn clue: {_clue.name}");
            if (CaseManager.Instance.clueDescriptions.TryGetValue(_clue, out List<string> descriptions))
            {
                string fullText = $"<b>{_clue.name}</b>\n";
                foreach (var description in descriptions)
                {
                    fullText += "* " + description + "\n";
                }

                // The Switcher now handles: Left -> Right -> New Page
                currentClueLayout.AddItem(clueTxtPrefab, fullText);
            }
        }

        // 7. Show the first page and update UI
        cluePages[0].SetActive(true);
        Debug.Log("UI Clue List Refreshed Successfully.");
    }

    public void ClearClueList()
    {
        if (cluePages.Count == 0) return;

        // Destroy all spreads except the first one
        for (int i = cluePages.Count - 1; i > 0; i--)
        {
            Destroy(cluePages[i]);
            cluePages.RemoveAt(i);
        }

        // Clear BOTH left and right containers on the first page
        LayoutSwitcher firstSwitcher = cluePages[0].GetComponent<LayoutSwitcher>();
        foreach (Transform child in firstSwitcher.leftPageContainer) Destroy(child.gameObject);
        foreach (Transform child in firstSwitcher.rightPageContainer) Destroy(child.gameObject);
    }

    public void InstantiateCaseOne()
    {
        GameObject newPanel = Instantiate(casePanelPrefab, leftSideContainer);
        newPanel.transform.SetParent(leftSideContainer, false);
        newPanel.GetComponent<CasePage>().Setup(caseData);
        newPanel.SetActive(true);
    }
}
