using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public CanvasGroup UIComponents;

    private void Awake()
    {
       instance = this;  
    }

   public void TransparentUI()
   {
        UIComponents.alpha = 0;
        UIComponents.interactable = false;
        UIComponents.blocksRaycasts = false;
   }

    public void VisibleUI()
    {
        UIComponents.alpha = 1;
        UIComponents.interactable = true;
        UIComponents.blocksRaycasts = true;
    }

    public void DisableButton(GameObject target)
    {
        foreach (Button btn in target.GetComponentsInChildren<Button>())
            btn.interactable = false;

        foreach (Image img in target.GetComponentsInChildren<Image>())
            img.enabled = false;

        foreach (Text txt in target.GetComponentsInChildren<Text>())
            txt.enabled = false;

        foreach (TMP_Text tmp in target.GetComponentsInChildren<TMP_Text>())
            tmp.enabled = false;
    }

    public void EnableButton(GameObject target)
    {
        foreach (Button btn in target.GetComponentsInChildren<Button>())
            btn.interactable = true;

        foreach (Image img in target.GetComponentsInChildren<Image>())
            img.enabled = true;

        foreach (Text txt in target.GetComponentsInChildren<Text>())
            txt.enabled = true;

        foreach (TMP_Text tmp in target.GetComponentsInChildren<TMP_Text>())
            tmp.enabled = true;
    }
} 
