using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.MaterialUpgrader;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager instance;

    public Animator fadeAnimator;
    public Animator UndeadAnimator;

    public bool fadeHappening;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    } 

    public void BlackFadeAnimation()
    {
        Debug.Log("Fade");
        fadeHappening = true;
        fadeAnimator.SetTrigger("FadeOut");

        //AnimationManager.instance.BlackFadeAnimation();
        //yield return new WaitUntil(() => AnimationManager.instance.fadeHappening == false);
    }

    public void UndeadManifestationAnimation(int foundclues, bool rightCulprit)
    {
        Debug.Log("Animating");
        if (foundclues == 0 || !rightCulprit)
        {
            Debug.Log("Nothing is happening?");
        }
        else if (foundclues == 1)
        {
            Debug.Log("1");
            UndeadAnimator.SetTrigger("Level1"); 
        }
        else if (foundclues == 2)
        {
            Debug.Log("2");
            UndeadAnimator.SetTrigger("Level2");
        }
        else if (foundclues >= 3)
        {
            Debug.Log("3");
            UndeadAnimator.SetTrigger("Level3");
        }
    }

    public void StopUndeadAnimation()
    {
        UndeadAnimator.SetTrigger("Stop");
    }

    public IEnumerator TypeWriterEffect(string fullText, TMP_Text textUI, float speed)
    {
        textUI.text = "";

        foreach (char letter in fullText)
        {
            textUI.text += letter;
            yield return new WaitForSeconds(speed);
        }
    }
}
    

