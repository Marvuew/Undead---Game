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
        DisableAllPages();
        cluesHeader.SetActive(true);

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
            UpdateNavButtons();
        }
    }

    public void LastCluePage()
    {
        int currentIndex = cluePages.IndexOf(activeCluePage);

        if (currentIndex - 1 >= 0)
        {
            ToggleCluePage(cluePages[currentIndex - 1]);
            UpdateNavButtons();
        }
    }

    public void UpdateNavButtons()
    {
        int currentIndex = cluePages.IndexOf(activeCluePage);

        bool isAnyPageActive = false;

        foreach (GameObject page in cluePages)
        {
            if (page != null && page.activeSelf)
            {
                isAnyPageActive = true;
                break; 
            }
        }

        if (isAnyPageActive)
        {
            nextCluePageBtn.SetActive(currentIndex < cluePages.Count - 1);
            lastCluePageBtn.SetActive(currentIndex > 0);
        }
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
            AudioManager.instance.PlayPageTurnSound();
        }
        casePage.SetActive(true);  // Set the casepage as the first page

        SetSelectedButton(pageButtons[2]);

        bookCover.SetActive(false);
        pagesContainer.SetActive(true);
        creaturesText.enabled = false;

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
        GameObject newPage = Instantiate(cluePage, pagesContainer.transform);

        RectTransform rt = newPage.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        cluePages.Add(newPage);

        return newPage;
    }


    public void UpdateCluesList()
    {
        pagesContainer.SetActive(true);
        if (cluePages.Count > 0) cluePages[0].SetActive(true);

        ClearClueList();

        foreach (Clue clue in clues)
        {
            if (!CaseManager.Instance.clueDescriptions.ContainsKey(clue))
            {
                CaseManager.Instance.clueDescriptions.Add(clue, new List<string>(tempDescriptions));
            }
        }

        var cluesToDisplay = (CaseManager.Instance.cluesfound.Count == 0) ? clues : CaseManager.Instance.cluesfound.ToList();

        if (cluePages.Count > 0)
        {
            currentClueLayout = cluePages[0].GetComponent<LayoutSwitcher>();

            currentClueLayout.ResetToLeftPage();
        }
        else
        {
            Debug.LogError("No CluePages found in the list! Assign the first page in the Inspector.");
            return;
        }

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

                currentClueLayout.AddItem(clueTxtPrefab, fullText);
            }
        }

        cluePages[0].SetActive(true);
        Debug.Log("UI Clue List Refreshed Successfully.");
    }

    public void ClearClueList()
    {
        if (cluePages.Count == 0) return;

        for (int i = cluePages.Count - 1; i > 0; i--)
        {
            Destroy(cluePages[i]);
            cluePages.RemoveAt(i);
        }

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
