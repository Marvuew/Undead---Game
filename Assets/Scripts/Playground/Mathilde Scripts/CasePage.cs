using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CasePage : MonoBehaviour
{
    public TMP_Text caseNameText;
    public Image caseImage;
    public TMP_Text caseDescriptionText;

    public void Setup(CaseData data)
    {
        caseNameText.text = data.caseName;
        caseImage.sprite = data.caseImage;
        caseDescriptionText.text = data.caseDescription;
        gameObject.SetActive(true);
    }
}
