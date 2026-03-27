using UnityEngine;
using UnityEngine.EventSystems;

public class LastPageScript : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        NecroLexiconScript.instance.LastPage();
    }
}
