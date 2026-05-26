using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Assets.Scripts.GameScripts;
using UnityEngine.SceneManagement;

public class ConfrontationHandler : MonoBehaviour
{
    public OutroHandler caseOutroScript;
    public GameObject EndCreditUI;

    public IEnumerator Confrontation(int foundClues, bool rightCulprit, GameObject corkBoard, Undead pickedCulprit)
    {
        Debug.Log("Confrontation started! Starting fade animation...");

        corkBoard.SetActive(false); // DISBALE THE CORKBOARD


        SceneNames scene;
        Vector3 undeadPos;

        if (pickedCulprit.undeadInteractable != null) // Check because of SCOPE! I didnt want to make an interactable for all undead...
        {
            scene = pickedCulprit.undeadInteractable.homeScene; // Gets the scene from the interactable of the culprit
            undeadPos = pickedCulprit.undeadInteractable.position; // Gets the position from the interactable of the culprit
        }
        else
        {
            scene = SceneNames.OpenWorld;
            undeadPos = new Vector3(-2.34f, 15.25f, 0f); // HARD CODED BEHIND THE DHAMPIRS HOUSE. JUST BECAUSE....
        }

        WorldFade.Instance.StartSceneTransition(scene.ToString(), 2f, Color.black, undeadPos); // Transistions to the scene of the culprit.
        yield return new WaitForSeconds(3f);
        var undeadRuntimeInteractable = CaseManager.Instance.activeInteractables.Find(undead => undead.interactableType == InteractableType.Culprit); // Find the runtime interactable of the culprit. Only visible if it is the right culprit guess...
        if (undeadRuntimeInteractable != null)
        {
            Debug.Log("Undead Interactable found");
            undeadRuntimeInteractable.gameObject.SetActive(rightCulprit); // Activate the culprit
        }
        else
        {
            Debug.Log("Undead Interactable not found");
        }
        var clock = FindAnyObjectByType<HalfClockHand>();
        // MAKE THE CLOCK TURN TO NIGHT TIME!! LASSE LUND
        yield return new WaitUntil(() => !WorldFade.Instance.isSceneTransitioning2); // Waits until the transition is done.
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == scene.ToString()); // Waits until the scene is actually loaded, just to be sure.
        AudioManager.instance.StopLoopingTracks();
        // Handle Audio
        if (rightCulprit)
        {
            AudioManager.instance.PlaySFX("Dur");
        }
        else
        {
            AudioManager.instance.PlaySFX("Mol");
        }
        if (rightCulprit && foundClues >= 3)
        {
            undeadRuntimeInteractable.GetComponent<RuntimeInteractable>().startInteraction(); // Start the interaction of the culprit, which will trigger the manifestation and then the outro.
            Debug.Log("You guessed the right culprit! The manifestation will now happen, and then you will proceed to the outro.");    
            yield return new WaitUntil(() => DialogueGraphManager.instance.isDialogueRunning == false); // Wait until the dialogue is done, which means the manifestation is done as well, since the manifestation is part of the dialogue.
            
            StartCoroutine(ContinueToOutro(pickedCulprit, foundClues, rightCulprit));
        }
        else if (rightCulprit && foundClues < 3)
        {
            switch (foundClues)
            {
                case 0:
                    DialogueGraphManager.instance.StartDialogue(CaseManager.Instance.Level0Manifestiation);
                    break;
                case 1:
                    DialogueGraphManager.instance.StartDialogue(CaseManager.Instance.Level1Manifestiation);
                    break;
                case 2:
                    DialogueGraphManager.instance.StartDialogue(CaseManager.Instance.Level2Manifestiation);
                    break;
                default:
                    DialogueGraphManager.instance.StartDialogue(CaseManager.Instance.Level0Manifestiation);
                    break;
            }
            yield return new WaitUntil(() => DialogueGraphManager.instance.isDialogueRunning == false); // Wait until the dialogue is done, which means the manifestation is done as well, since the manifestation is part of the dialogue.
            Debug.Log("You guessed the right culprit! No manifestation will happen, but you will still proceed to the outro");
            StartCoroutine(ContinueToOutro(pickedCulprit, foundClues, rightCulprit));
        }
        else
        {
            DialogueGraphManager.instance.StartDialogue(CaseManager.Instance.Level0Manifestiation);
            yield return new WaitUntil(() => DialogueGraphManager.instance.isDialogueRunning == false); // Wait until the dialogue is done, which means the manifestation is done as well, since the manifestation is part of the dialogue.
            Debug.Log("You guessed the wrong culprit! No manifestation will happen, but you will still proceed to the outro.");
            StartCoroutine(ContinueToOutro(pickedCulprit, foundClues, rightCulprit));
        }
    }

    public IEnumerator ContinueToOutro(Undead pickedCulprit, int foundClues, bool rightCulprit)
    {
        //StartCoroutine(caseOutroScript.SetUpOutro(pickedCulprit, foundClues, rightCulprit)); // START THE OUTRO
        WorldFade.Instance.StartScreenFadeWithToggleGameObject(2f, 1f, Color.black, EndCreditUI);
        yield return new WaitForSeconds(2 + 1 + 2f);
        StartCoroutine(caseOutroScript.EndCreditsPan());
    }

    
}
