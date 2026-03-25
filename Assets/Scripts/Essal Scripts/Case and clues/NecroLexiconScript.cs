using UnityEngine;
using UnityEngine.UI;

public class NecroLexiconScript : MonoBehaviour
{
    public Animator necroLexiconAnimator;
    
    public void NextPage()
    {
        necroLexiconAnimator.SetTrigger("NextPage");
    }

    public void LastPage()
    {
        necroLexiconAnimator.SetTrigger("LastPage");
    }
}
