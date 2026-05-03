using Assets.Scripts.GameScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HouseIntroController : MonoBehaviour
{
    [Header("Dialogue Graphs")]
    [SerializeField] private RuntimeDialogueGraph wakeUpGraph;
    [SerializeField] private RuntimeDialogueGraph movementNarratorGraph;
    [SerializeField] private RuntimeDialogueGraph interactTutorialGraph;
    [SerializeField] private RuntimeDialogueGraph afterBookNarratorGraph;

    [Header("Intro Start")]
    [SerializeField] private bool startSceneBlack = true;
    [SerializeField] private Color startFadeColor = Color.black;
    [SerializeField] private float blackScreenDuration = 1.5f;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private string startSoundName = "";

    [Header("Random Knocking")]
    [SerializeField] private bool useRandomKnocking = true;
    [SerializeField] private string[] knockSoundNames;
    [SerializeField] private float minKnockDelay = 2f;
    [SerializeField] private float maxKnockDelay = 6f;

    [Header("WASD Tutorial UI")]
    [SerializeField] private GameObject wasdTutorialObject;

    [SerializeField] private Image wImage;
    [SerializeField] private Sprite wNormalSprite;
    [SerializeField] private Sprite wPressedSprite;

    [SerializeField] private Image aImage;
    [SerializeField] private Sprite aNormalSprite;
    [SerializeField] private Sprite aPressedSprite;

    [SerializeField] private Image sImage;
    [SerializeField] private Sprite sNormalSprite;
    [SerializeField] private Sprite sPressedSprite;

    [SerializeField] private Image dImage;
    [SerializeField] private Sprite dNormalSprite;
    [SerializeField] private Sprite dPressedSprite;

    [Header("Interact Tutorial UI")]
    [SerializeField] private GameObject interactTutorialObject;
    [SerializeField] private Image interactImage;
    [SerializeField] private Sprite interactNormalSprite;
    [SerializeField] private Sprite interactPressedSprite;

    [Header("Objects")]
    [SerializeField] private GameObject necrolexiconBook;
    [SerializeField] private GameObject frontDoor;
    [SerializeField] private GameObject bookIcon;

    private bool pressedW;
    private bool pressedA;
    private bool pressedS;
    private bool pressedD;

    private bool wakeUpGraphStarted;
    private bool wakeUpFinished;

    private bool movementNarratorStarted;
    private bool movementNarratorFinished;
    private bool movementInputRecordingStarted;
    private bool movementComplete;

    private bool interactGraphStarted;
    private bool canPickUpBook;

    private bool doorOpened;
    private Coroutine knockRoutine;

    private IEnumerator Start()
    {
        yield return null;

        if (GameProgressState.CompletedHouseIntro)
        {
            GameProgressState.HasNecrolexicon = true;

            if (necrolexiconBook != null)
                necrolexiconBook.SetActive(false);

            if (frontDoor != null)
                frontDoor.SetActive(true);

            if (bookIcon != null)
                bookIcon.SetActive(true);

            if (wasdTutorialObject != null)
                wasdTutorialObject.SetActive(false);

            if (interactTutorialObject != null)
                interactTutorialObject.SetActive(false);

            yield break;
        }

        GameProgressState.HasNecrolexicon = false;

        if (necrolexiconBook != null)
            necrolexiconBook.SetActive(true);

        if (frontDoor != null)
            frontDoor.SetActive(false);

        if (bookIcon != null)
            bookIcon.SetActive(false);

        if (wasdTutorialObject != null)
            wasdTutorialObject.SetActive(false);

        if (interactTutorialObject != null)
            interactTutorialObject.SetActive(false);

        ResetWasdSprites();
        ResetInteractSprite();

        if (startSceneBlack && WorldFade.Instance != null)
            WorldFade.Instance.SetBlackScreen(startFadeColor);

        if (!string.IsNullOrWhiteSpace(startSoundName) && AudioManager.instance != null)
            AudioManager.instance.PlaySFX(startSoundName);

        if (useRandomKnocking && knockSoundNames != null && knockSoundNames.Length > 0)
            knockRoutine = StartCoroutine(RandomKnockingRoutine());

        yield return new WaitForSeconds(blackScreenDuration);

        if (startSceneBlack && WorldFade.Instance != null)
        {
            WorldFade.Instance.StartFadeFromBlack(fadeInDuration, startFadeColor);
            yield return new WaitForSeconds(fadeInDuration);
        }

        wakeUpGraphStarted = StartDialogueSafely(wakeUpGraph, "Wake up dialogue");
    }

    private void Update()
    {
        if (GameProgressState.CompletedHouseIntro)
            return;

        UpdateKeySprites();

        if (wakeUpGraphStarted && !wakeUpFinished)
        {
            if (DialogueGraphManager.instance != null && DialogueGraphManager.instance.isDialogueRunning)
                return;

            wakeUpFinished = true;
            StartMovementNarrator();
            return;
        }

        if (movementNarratorStarted && !movementNarratorFinished)
        {
            if (DialogueGraphManager.instance != null && DialogueGraphManager.instance.isDialogueRunning)
                return;

            movementNarratorFinished = true;
            StartMovementInputRecording();
            return;
        }

        if (DialogueGraphManager.instance != null && DialogueGraphManager.instance.isDialogueRunning)
            return;

        if (!movementComplete)
        {
            HandleMovementInput();
            return;
        }

        if (!interactGraphStarted)
        {
            interactGraphStarted = true;
            canPickUpBook = true;

            if (interactTutorialObject != null)
                interactTutorialObject.SetActive(true);

            StartDialogueSafely(interactTutorialGraph, "Interact tutorial dialogue");
        }
    }

    private void StartMovementNarrator()
    {
        movementNarratorStarted = StartDialogueSafely(movementNarratorGraph, "Movement narrator dialogue");
    }

    private void StartMovementInputRecording()
    {
        movementInputRecordingStarted = true;

        pressedW = false;
        pressedA = false;
        pressedS = false;
        pressedD = false;

        ResetWasdSprites();

        if (wasdTutorialObject != null)
            wasdTutorialObject.SetActive(true);
    }

    private void HandleMovementInput()
    {
        if (!movementInputRecordingStarted || Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.wasPressedThisFrame) pressedW = true;
        if (Keyboard.current.aKey.wasPressedThisFrame) pressedA = true;
        if (Keyboard.current.sKey.wasPressedThisFrame) pressedS = true;
        if (Keyboard.current.dKey.wasPressedThisFrame) pressedD = true;

        if (pressedW && pressedA && pressedS && pressedD)
        {
            movementComplete = true;

            if (wasdTutorialObject != null)
                wasdTutorialObject.SetActive(false);
        }
    }

    private void UpdateKeySprites()
    {
        if (Keyboard.current == null)
            return;

        if (wImage != null)
            wImage.sprite = Keyboard.current.wKey.isPressed ? wPressedSprite : wNormalSprite;

        if (aImage != null)
            aImage.sprite = Keyboard.current.aKey.isPressed ? aPressedSprite : aNormalSprite;

        if (sImage != null)
            sImage.sprite = Keyboard.current.sKey.isPressed ? sPressedSprite : sNormalSprite;

        if (dImage != null)
            dImage.sprite = Keyboard.current.dKey.isPressed ? dPressedSprite : dNormalSprite;

        if (interactImage != null)
            interactImage.sprite = Keyboard.current.eKey.isPressed ? interactPressedSprite : interactNormalSprite;
    }

    public bool CanPickUpBook()
    {
        return canPickUpBook;
    }

    public void BookPickedUp()
    {
        if (!canPickUpBook)
            return;

        GameProgressState.HasNecrolexicon = true;

        if (necrolexiconBook != null)
            necrolexiconBook.SetActive(false);

        if (bookIcon != null)
            bookIcon.SetActive(true);

        if (frontDoor != null)
            frontDoor.SetActive(true);

        if (interactTutorialObject != null)
            interactTutorialObject.SetActive(false);

        StartDialogueSafely(afterBookNarratorGraph, "After book narrator dialogue");

        GameProgressState.CompletedHouseIntro = true;
    }

    public void OnDoorOpened()
    {
        doorOpened = true;

        if (knockRoutine != null)
        {
            StopCoroutine(knockRoutine);
            knockRoutine = null;
        }
    }

    private IEnumerator RandomKnockingRoutine()
    {
        while (!doorOpened)
        {
            float delay = Random.Range(minKnockDelay, maxKnockDelay);
            yield return new WaitForSeconds(delay);

            if (doorOpened)
                yield break;

            if (knockSoundNames == null || knockSoundNames.Length == 0)
                yield break;

            string randomKnockSound = knockSoundNames[Random.Range(0, knockSoundNames.Length)];

            if (!string.IsNullOrWhiteSpace(randomKnockSound) && AudioManager.instance != null)
                AudioManager.instance.PlaySFX(randomKnockSound);
        }
    }

    private void ResetWasdSprites()
    {
        if (wImage != null) wImage.sprite = wNormalSprite;
        if (aImage != null) aImage.sprite = aNormalSprite;
        if (sImage != null) sImage.sprite = sNormalSprite;
        if (dImage != null) dImage.sprite = dNormalSprite;
    }

    private void ResetInteractSprite()
    {
        if (interactImage != null)
            interactImage.sprite = interactNormalSprite;
    }

    private bool StartDialogueSafely(RuntimeDialogueGraph graph, string debugName)
    {
        if (DialogueGraphManager.instance == null)
        {
            Debug.LogWarning(debugName + " could not start because DialogueGraphManager.instance is missing.");
            return false;
        }

        if (graph == null)
        {
            Debug.LogWarning(debugName + " could not start because the dialogue graph is missing.");
            return false;
        }

        if (Player.Instance != null)
            Player.Instance.interacting = true;

        DialogueGraphManager.instance.gameObject.SetActive(true);

        if (DialogueGraphManager.instance.DialoguePanel != null)
            DialogueGraphManager.instance.DialoguePanel.SetActive(true);

        DialogueGraphManager.instance.StartDialogue(graph);

        Debug.Log(debugName + " started.");
        return true;
    }
}