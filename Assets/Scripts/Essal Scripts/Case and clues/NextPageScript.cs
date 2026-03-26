using UnityEngine;
using UnityEngine.EventSystems;

public class NextPageScript : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        NecroLexiconScript.instance.NextPage();
    }
}
