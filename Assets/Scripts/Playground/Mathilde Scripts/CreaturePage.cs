using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class CreaturePage : MonoBehaviour
{

    public TMP_Text creatureNameText;
    public Image creatureImage;
    public TMP_Text creatureDescriptionText;


    public void Setup(CreatureData data)
    {
        creatureNameText.text = data.creatureName;
        creatureImage.sprite = data.image;
        creatureDescriptionText.text = data.description;
        gameObject.SetActive(true);
    }


}
