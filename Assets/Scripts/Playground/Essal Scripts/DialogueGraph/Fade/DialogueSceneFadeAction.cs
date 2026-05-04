using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Actions/New Scene Fade Action")]
public class DialogueSceneFadeAction : DialogueAction
{
    public SceneFadeSettings fadeSettings;

    [Header("Input Blocking")]
    public bool blockSpaceDuringFade = true;

    public override void DoAction()
    {
        if (WorldFade.Instance == null || fadeSettings == null)
            return;

        WorldFade.Instance.StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        if (blockSpaceDuringFade)
            DialogueInputBlocker.BlockSpaceAdvance = true;

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

        if (blockSpaceDuringFade)
            DialogueInputBlocker.BlockSpaceAdvance = false;
    }
}