using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NecroLexiconScript : MonoBehaviour
{
    public static NecroLexiconScript instance;
    void Awake()
    {
        instance = this;
    }

    public List<Culprit> CulpritList = new List<Culprit>();

    public ContentScript leftContent;
    public ContentScript rightContent;

    public GameObject book;

    private int pageNumber = 0;

    private void Start()
    {
        leftContent.SetContent(CulpritList[pageNumber]);
        rightContent.SetContent(CulpritList[pageNumber + 1]);
    }

    public void LastPage()
    {
        if (pageNumber - 2 < 0)
        {
            Debug.LogWarning("Out of bound left");
            return;
        }

        pageNumber -= 2;

        leftContent.SetContent(CulpritList[pageNumber]);

        if (pageNumber + 1 < CulpritList.Count)
            rightContent.SetContent(CulpritList[pageNumber + 1]);
        else
            rightContent.Clear(); // optional (if you have this)
    }

    public void NextPage()
    {
        if (pageNumber + 2 >= CulpritList.Count)
        {
            Debug.LogWarning("Out of bound right");
            return;
        }

        pageNumber += 2;

        leftContent.SetContent(CulpritList[pageNumber]);

        if (pageNumber + 1 < CulpritList.Count)
            rightContent.SetContent(CulpritList[pageNumber + 1]);
        else
            rightContent.Clear(); // optional
    }

    public void ToggleNecroLexicon()
    {
        book.gameObject.SetActive(!book.gameObject.activeSelf);
    }
}
