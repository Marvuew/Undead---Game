using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Dialogue/Actions/New Scene Fade Action")]
public class DialogueSceneFadeAction : DialogueAction
{
    public SceneFadeSettings fadeSettings;

    [Header("Input Blocking")]
    public bool blockKeyboardDuringFade = true;

    public override void DoAction()
    {
        if (WorldFade.Instance == null || fadeSettings == null)
            return;

        WorldFade.Instance.StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        Keyboard keyboard = Keyboard.current;
        bool disabledKeyboard = false;

        if (blockKeyboardDuringFade && keyboard != null && keyboard.enabled)
        {
            InputSystem.DisableDevice(keyboard);
            disabledKeyboard = true;
        }

        WorldFade.Instance.StartScreenFade(
            fadeSettings.fadeDuration,
            fadeSettings.stayBlackDuration,
            fadeSettings.fadeColor
        );

        float totalFadeTime =
            fadeSettings.fadeDuration +
            fadeSettings.stayBlackDuration +
            fadeSettings.fadeDuration;

        yield return new WaitForSeconds(totalFadeTime);

        if (disabledKeyboard && keyboard != null)
        {
            InputSystem.EnableDevice(keyboard);
        }
    }
}