using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq.Expressions;
public class CulpritSelectionScript : MonoBehaviour
{
    public Transform corkBoardContainer;
    public Button culpritButtonPrefab;

    public Sprite homeSprite;

    public SpriteRenderer backGround;

    public GameObject corkBoard;

    public IEnumerator CreateCorkBoard(List<Culprit> culprits)
    {
        ClearCulprits();
        Debug.Log("Creating Corkboard");
        AnimationManager.instance.BlackFadeAnimation();
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);
        print("YEAH!");
        backGround.sprite = homeSprite;
        foreach (var culprit in culprits)
        {
            Button button = Instantiate(culpritButtonPrefab, corkBoardContainer);
            button.GetComponent<Image>().sprite = culprit.culpritSprite;
            button.onClick.AddListener(() => HandleCulpritGuess(culprit));
        }
        corkBoard.SetActive(true);
        yield return null;
    }
    private void ClearCulprits()
    {
        foreach (Transform child in corkBoardContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void HandleCulpritGuess(Culprit culprit)
    {
        Debug.Log("Handling CUlprit guess");
        bool isCulprit = CaseManager.instance.currentCase.culprit == culprit;
        StartCoroutine(ConfrontationManager.instance.Manifest(CaseManager.instance.foundClues.Count, isCulprit, corkBoard, culprit));
    }

    #region Legacy Code

    /*public void CalculateConfrontation(int foundClues, Culprit culprit)
    {
        ConfrontationManager.instance.TransferCulprit(culprit);

        if (foundClues == 1)
        {
            StartCoroutine(ConfrontationManager.instance.Level1Manifestation());
        }
        else if (foundClues == 2)
        {
            StartCoroutine(ConfrontationManager.instance.Level2Manifestation());
        }
        else if (foundClues == 3)
        {
            StartCoroutine(ConfrontationManager.instance.Level3Manifestation());
        }
    }*/

    /*public void HandleGuess(Culprit culprit)
{
    print($"You Found {CaseManager.instance.foundClues.Count} out of {CaseManager.instance.currentCase.clues.Count}");

    if (CaseManager.instance.foundClues.Count == 0)
    {
        Debug.LogWarning("This is not serious, you cant be serious");
    }
    if (CaseManager.instance.currentCase.culprit == culprit)
    {
        CalculateConfrontation(CaseManager.instance.foundClues.Count, CaseManager.instance.currentCase.culprit);
    }
    else
    {
        StartCoroutine(ConfrontationManager.instance.Level0Manifestation());
    }
}*/
    #endregion

}
