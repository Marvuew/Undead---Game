using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class NecroLexiconUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject cluesPage;
    public TMPro.TextMeshProUGUI cluesText; 
    public GameObject creaturesPage;
    public TMPro.TextMeshProUGUI creaturesText;
    public GameObject casePage;
    public GameObject bookCover;
    public GameObject pagesContainer;
    public Transform leftSideContainer;
    [SerializeField] Transform cluesContainer;
    [SerializeField] GameObject clueTxtPrefab;

    [Header("References")]
    public CreatureManager creatureManager;
    public CasePage casePageScript;
    public CaseData caseData;
    public CaseManagerMathilde caseManager;

    [Header("Prefabs")]
    public GameObject casePanelPrefab;

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

    public void OpenCluesPage()
    {
        OpenBook();
        Debug.Log("Clues clicked");
        DisableAllPages();
        cluesPage.SetActive(true);
        cluesText.enabled = true;

        SetSelectedButton(pageButtons[0]);
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

    private void DisableAllPages()
    {
        cluesPage.SetActive(false);
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
    public void UpdateCluesList()
    {
        ClearClueList();

        foreach (Clue _clue in CaseManager.Instance.cluesfound) // LOOP THROUGH ALL CLUES
        {
            //Check if we actually have descriptions for this clue
            if (CaseManager.Instance.clueDescriptions.TryGetValue(_clue, out List<string> descriptions))
            {
                //Instantiate one text object per clue (or per description, depending on your UI design)
                GameObject clueObject = Instantiate(clueTxtPrefab, cluesContainer);
                TextMeshProUGUI textComp = clueObject.GetComponent<TextMeshProUGUI>();

                //Combine the descriptions into one block of text for that clue
                textComp.text = $"<b>{_clue.name}</b>\n"; // Add Clue Name as Header
                foreach (var description in descriptions)
                {
                    textComp.text += "* " + description + "\n";
                }
            }
        }
        Debug.Log("UI Clue List Refreshed");
    }

    public void ClearClueList()
    {
        foreach(Transform child in cluesContainer)
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
