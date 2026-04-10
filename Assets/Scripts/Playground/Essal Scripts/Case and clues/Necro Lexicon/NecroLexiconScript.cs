using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class NecroLexiconScript : MonoBehaviour
{
    public static NecroLexiconScript instance;
    void Awake()
    {
        instance = this;
    }

    [SerializeField] List<Suspect> culpritList;

    public ContentScript leftContent;
    public ContentScript rightContent;

    public GameObject book;

    private int pageNumber = 0;

    private void Start()
    {
        leftContent.SetContent(culpritList[pageNumber]);
        rightContent.SetContent(culpritList[pageNumber++]);
    }
    public void LastPage()
    {
        if (pageNumber - 2 < 0)
        {
            Debug.LogWarning("Out of bound left");
            return;
        }

        pageNumber -= 2;

        leftContent.SetContent(culpritList[pageNumber]);

        if (pageNumber + 1 < culpritList.Count)
            rightContent.SetContent(culpritList[pageNumber + 1]);
        else
            rightContent.Clear(); // optional (if you have this)
    }

    public void NextPage()
    {
        if (pageNumber + 2 >= culpritList.Count)
        {
            Debug.LogWarning("Out of bound right");
            return;
        }

        pageNumber += 2;

        leftContent.SetContent(culpritList[pageNumber]);

        if (pageNumber + 1 < culpritList.Count)
            rightContent.SetContent(culpritList[pageNumber + 1]);
        else
            rightContent.Clear(); // optional
    }

    public void ToggleNecroLexicon()
    {
        book.gameObject.SetActive(!book.gameObject.activeSelf);
    }
    public void UpdateClueList(Clue clue)
    {
        //Spawn Clues here
    }
}
