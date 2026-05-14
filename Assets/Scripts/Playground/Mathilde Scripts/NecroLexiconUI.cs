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
    public TMPro.TextMeshProUGUI cluesText; 
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
        ToggleCluePage(cluePages[0]);
        print("Opened first cluepage");
    }

    public void ToggleCluePage(GameObject page)
    {
        OpenBook();
        DisableAllPages(); // This hides everything first

        // Set only the requested page to active
        page.SetActive(true);
        activeCluePage = page;
        SetSelectedButton(pageButtons[0]);
        UpdateCluesList();
    }

    public void NextCluePage()
    {
        if (cluePages.IndexOf(activeCluePage) + 1 > cluePages.Count - 1)
        {
            Debug.LogWarning("There are no more pages");
            return;
        }
        else
        {
            ToggleCluePage(cluePages[cluePages.IndexOf(activeCluePage) + 1]);
        }
    }

    public void LastCluePage()
    {
        if (cluePages.IndexOf(activeCluePage) - 1 < 0)
        {
            Debug.LogWarning("There are no more pages");
            return;
        }
        else
        {
            ToggleCluePage(cluePages[cluePages.IndexOf(activeCluePage) - 1]);
        }
    }

    private void DisableAllPages()
    {
        foreach(var layout in cluePages)
        {
            layout.gameObject.SetActive(false);
        }

        cluesText.enabled = false;
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
        // 1. Force the containers active so Unity can calculate UI heights
        pagesContainer.SetActive(true);
        if (cluePages.Count > 0) cluePages[0].SetActive(true);

        // 2. Wipe the old data safely
        ClearClueList();

        // 3. Ensure the test dictionary is populated (Prevents TryGetValue from failing)
        foreach (Clue clue in clues)
        {
            if (!CaseManager.Instance.clueDescriptions.ContainsKey(clue))
            {
                // Note: If tempDescriptions is empty in inspector, nothing shows below the name
                CaseManager.Instance.clueDescriptions.Add(clue, new List<string>(tempDescriptions));
            }
        }

        // 4. Determine data source
        var cluesToDisplay = (CaseManager.Instance.cluesfound.Count == 0) ? clues : CaseManager.Instance.cluesfound.ToList();

        // 5. Initialize the layout pointer
        currentClueLayout = cluePages[0].GetComponent<LayoutSwitcher>();
        currentClueLayout.containerRect.gameObject.SetActive(true);

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

                // This call triggers the overflow logic in your LayoutSwitcher script
                currentClueLayout.AddItem(clueTxtPrefab, fullText);
            }
        }

        // 7. Reset the view to the first page
        ToggleCluePage(cluePages[0]);
        Debug.Log("UI Clue List Refreshed Successfully.");
    }

    public void ClearClueList()
    {
        // Safety check: if the list is empty, we have a bigger setup problem
        if (cluePages.Count == 0) return;

        // 1. Destroy all spawned overflow pages (Index 1 and onwards)
        for (int i = cluePages.Count - 1; i > 0; i--)
        {
            GameObject extraPage = cluePages[i];
            cluePages.RemoveAt(i);
            Destroy(extraPage);
        }

        // 2. Clear the text items out of the original Page 0
        // We access the containerRect specifically to avoid destroying the layout group itself
        Transform container = cluePages[0].GetComponent<LayoutSwitcher>().containerRect;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    public void InstantiateCaseOne()
    {
        GameObject newPanel = Instantiate(casePanelPrefab, leftSideContainer);
        newPanel.transform.SetParent(leftSideContainer, false);
        newPanel.GetComponent<CasePage>().Setup(caseData);
        newPanel.SetActive(true);
    }
}
