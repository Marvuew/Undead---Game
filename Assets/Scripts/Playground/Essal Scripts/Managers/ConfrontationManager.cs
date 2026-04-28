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

    public IEnumerator Manifest(int foundClues, bool rightCulprit, GameObject corkBoard, Undead pickedCulprit, Image selectBackground)
    {
        AnimationManager.instance.BlackFadeAnimation(); // FADE ANIMATION
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false); // WAIT TILL ITS DONE

        selectBackground.enabled = false; // DISABLE THE SELECT BACKGROUND
        corkBoard.SetActive(false); // DISBALE THE CORKBOARD
        undeadBackGround.enabled = true; // ENABLE THE UNDEAD SPECIFIC BACKGROUND


        HandleCulpritData(pickedCulprit); // SET THE CULPRIT DATA, BACKGROUND AND CHARACTER SPRITE...
        AnimationManager.instance.UndeadManifestationAnimation(foundClues, rightCulprit); // PICK THE RIGHT ANIMATION
        yield return new WaitForSeconds(3f); // WAIT A LITTLE

        yield return new WaitUntil(() => Keyboard.current.spaceKey.wasPressedThisFrame); // PROCEED WITH SPACE BAR
        AnimationManager.instance.BlackFadeAnimation(); // THEN CALL FADEANIMATION
        yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);

        ContinueToOutro(pickedCulprit, foundClues, rightCulprit); // CALL THE TRANSITION MEHTOD TO OUTRO
    }

    public void ContinueToOutro(Undead pickedCulprit, int foundClues, bool rightCulprit)
    {
        AnimationManager.instance.StopUndeadAnimation(); // STOPPING THE UNDEAD ANIMATIONS

        StartCoroutine(caseOutroScript.SetupOutro(pickedCulprit, foundClues, rightCulprit, undeadBackGround)); // START THE OUTRO
    }

    public void HandleCulpritData(Undead suspect)
    {
        undeadBackGround.sprite = suspect.homeSprite;
        culpritImage.sprite = suspect.undeadSprite;
    }
}
