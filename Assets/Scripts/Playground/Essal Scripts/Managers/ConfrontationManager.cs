using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ConfrontationScript : MonoBehaviour
{
    public Image undeadBackGround;
    public Image culpritImage;
    public CaseOutroScript caseOutroScript;

    public IEnumerator Manifest(int foundClues, bool rightCulprit, GameObject corkBoard, Suspect pickedCulprit, Image selectBackground)
    {
        //Fade Animation
        AnimationManager.instance.BlackFadeAnimation();
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);

        selectBackground.enabled = false;
        corkBoard.SetActive(false);
        undeadBackGround.enabled = true;

        Debug.Log("HEY");
        //Culprit Animation
        HandleCulpritData(pickedCulprit);
        AnimationManager.instance.UndeadManifestationAnimation(foundClues, rightCulprit);

        //Proceed with mouse click => Fade Animation
        yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);

        AnimationManager.instance.BlackFadeAnimation();
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);
        ContinueToOutro(pickedCulprit, foundClues, rightCulprit);

    }

    public void ContinueToOutro(Suspect pickedCulprit, int foundClues, bool rightCulprit)
    {
        Debug.Log("Contine to Outro");
        //Stop the undead Animations
        AnimationManager.instance.StopUndeadAnimation();

        //Continues on with the Case outro.
        StartCoroutine(caseOutroScript.SetupOutro(pickedCulprit, foundClues, rightCulprit, undeadBackGround));
    }

    public void HandleCulpritData(Suspect culprit)
    {
        undeadBackGround.sprite = culprit.homeSprite;
        culpritImage.sprite = culprit.culpritSprite;
    }
}
