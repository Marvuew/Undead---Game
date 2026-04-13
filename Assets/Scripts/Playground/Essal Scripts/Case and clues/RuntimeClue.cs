using UnityEngine;
using UnityEngine.EventSystems;
public class RuntimeClue : MonoBehaviour, IPointerClickHandler
{
    public Clue clue;

    public void OnMouseDown()
    {
        //CaseManager.instance.AddClue(clue);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //CaseManager.instance.AddClue(clue);
        Destroy(gameObject);
    }
}
