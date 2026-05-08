using UnityEngine;
using UnityEngine.EventSystems; // Required for UI events

public class CulpritButtonMouseOverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Undead dealtUndead;

    // This triggers when the mouse enters the Image's rect
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (dealtUndead != null)
        {
            SelectionHandler.instance.culpritNameText.text = dealtUndead.undeadType.ToString();
        }
    }

    // This triggers when the mouse leaves the Image's rect
    public void OnPointerExit(PointerEventData eventData)
    {
        SelectionHandler.instance.culpritNameText.text = ""; // Use empty string instead of null
    }
}
