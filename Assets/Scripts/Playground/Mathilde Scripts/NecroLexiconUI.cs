using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;

public class NecroLexiconUI : MonoBehaviour
{
    public GameObject cluesPage;
    public GameObject creaturesPage;
    public GameObject casePage;
    public GameObject bookCover;
    public GameObject pagesContainer;

    public CreatureManager creatureManager;
    public CasePage casePageScript;
    public CaseData caseData;
    public CaseManager caseManager;

    public Transform leftSideContainer;
    public GameObject casePanelPrefab;

    [System.Serializable]
    public class  PageButton
    {
        public RectTransform button;
        public Vector2 closedPos;
        public Vector2 openPos;
    }

    public List<PageButton> pageButtons;


    private void Start()
    {
        bookCover.SetActive(true);
        pagesContainer.SetActive(false);

        foreach (var pb in pageButtons)
        {
            pb.button.anchoredPosition = pb.closedPos;
        }
    }

    public void OpenCluesPage()
    {
        OpenBook();
        Debug.Log("Clues clicked");
        DisableAllPages();
        cluesPage.SetActive(true);
    }

    public void OpenCreaturesPage()
    {
        OpenBook();
        Debug.Log("Creatures clicked");
        DisableAllPages();
        creaturesPage.SetActive(true);
    }

    public void OpenCasePage()
    {
        OpenBook();
        Debug.Log("Case clicked");
        DisableAllPages();
        casePage.SetActive(true);

    }

    private void DisableAllPages()
    {
        cluesPage.SetActive(false);
        creaturesPage.SetActive(false);
        casePage.SetActive(false);
        creatureManager.OnTabChanged();
        caseManager.OnTabChanged();

    }

    public void OpenBook()
    {
        bookCover.SetActive(false);
        pagesContainer.SetActive(true);

        foreach (var pb in pageButtons)
        {
            pb.button.anchoredPosition = pb.openPos;
        }
    }

    public void CloseBook()
    {
        DisableAllPages();
        bookCover.SetActive(true);
        pagesContainer.SetActive(false);

        foreach (var pb in pageButtons)
        {
            pb.button.anchoredPosition = pb.closedPos;
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
