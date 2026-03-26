using UnityEngine;

public class AnimationManager : MonoBehaviour
{
   public static AnimationManager instance;

    public Animator fadeAnimator;
    public Animator UndeadAnimator;

    public bool fadeHappening;

    private void Awake()
    {
        instance = this;
    }

    public void BlackFadeAnimation()
    {
        Debug.Log("Fade");
        fadeHappening = true;
        fadeAnimator.SetTrigger("FadeOut");
    }

    public void UndeadManifestationAnimation(int foundclues, bool rightCulprit)
    {
        foundclues = CaseManager.instance.foundClues.Count;

        if (foundclues == 0 | !rightCulprit)
        {
            Debug.Log("Nothing is happening?");
        }
        else if (foundclues == 1)
        {
            UndeadAnimator.SetTrigger("Level1");
        }
        else if (foundclues == 2)
        {
            UndeadAnimator.SetTrigger("Level2");
        }
        else if (foundclues == 3)
        {
            UndeadAnimator.SetTrigger("Level3");
        }
    }
}
