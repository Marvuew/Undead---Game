using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class ConfrontationManager : MonoBehaviour
{
    public static ConfrontationManager instance;

    public Animator confrontationAnimator;
    public Animator culpritAnimator;

    public SpriteRenderer backGround;
    public Image culpritImage;

    private Culprit culprit;

    private void Awake()
    {
        instance = this;
    }
    public void TransferCulprit(Culprit culprit)
    {
        this.culprit = culprit;
    }

    public IEnumerator Level0Manifestation()
    {
        confrontationAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(confrontationAnimator.GetCurrentAnimatorStateInfo(0).length);
        backGround.sprite = culprit.homeSprite;
        Debug.Log("No Creature appeared???");
        print("Level 0");
    }
    
    public IEnumerator Level1Manifestation()
    {
        confrontationAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(confrontationAnimator.GetCurrentAnimatorStateInfo(0).length);
        HandleCulpritData(culprit);
        yield return new WaitForSeconds(2f); // Should find another way...
        culpritAnimator.SetTrigger("Level1");
        print("Level 1");
    }

    public IEnumerator Level2Manifestation()
    {
        confrontationAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(confrontationAnimator.GetCurrentAnimatorStateInfo(0).length);
        HandleCulpritData(culprit);
        yield return new WaitForSeconds(2f);
        culpritAnimator.SetTrigger("Level2");
        print("Level 2");
    }

    public IEnumerator Level3Manifestation()
    {
        confrontationAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(confrontationAnimator.GetCurrentAnimatorStateInfo(0).length);
        HandleCulpritData(culprit);
        yield return new WaitForSeconds(2f);
        culpritAnimator.SetTrigger("Level3");
        print("Level 3");
    }

    public void HandleCulpritData(Culprit culprit)
    {
        backGround.sprite = culprit.homeSprite;
        culpritImage.sprite = culprit.culpritSprite;
    }
}
