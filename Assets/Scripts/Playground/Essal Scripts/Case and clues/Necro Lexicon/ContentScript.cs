using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ContentScript : MonoBehaviour
{
    private Suspect culprit;

    [Header("Page Content")]
    public Image undeadImage;
    public TextMeshProUGUI undeadName;
    public TextMeshProUGUI undeadHabitat;
    public TextMeshProUGUI undeadDescription;

    public void SetContent(Suspect culprit)
    {
        undeadImage.enabled = true;

        undeadImage.sprite = culprit.culpritSprite;
        undeadName.text = culprit.culpritName;
        undeadHabitat.text = culprit.culpritHabitat;
        undeadDescription.text = culprit.culpritDescription;
    }

    public void Clear()
    {
        undeadImage.enabled = false;
        undeadImage.sprite = null;

        undeadName.text = null;
        undeadHabitat.text = null;
        undeadDescription.text = null;
    }
}
