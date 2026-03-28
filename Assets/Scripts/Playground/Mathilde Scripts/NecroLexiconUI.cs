using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NecroLexiconUI : MonoBehaviour
{
    public GameObject cluesPage;
    public GameObject creaturesPage;
    public GameObject casePage;
    public GameObject bookCover;
    public GameObject pagesContainer;

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


}
