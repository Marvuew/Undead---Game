using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CreaturePage : MonoBehaviour
{

    public TMP_Text creatureNameText;
    public Image creatureImage;
    public TMP_Text creatureDescriptionText;


    public void Setup(Undead data)
    {
        creatureNameText.text = data.undeadType.ToString();
        creatureImage.sprite = data.cardSprite;
        creatureDescriptionText.text = data.description;
        gameObject.SetActive(true);
    }


}
