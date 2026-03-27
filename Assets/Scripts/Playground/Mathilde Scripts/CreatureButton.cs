using UnityEngine;
using UnityEngine.EventSystems;

public class CreatureButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject highlightCircle;
    public GameObject creaturePanelPrefab;
    public Transform pagesContainer;

    public CreatureData creatureData;

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightCircle.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightCircle.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CLICKED");
        
        GameObject newPanel = Instantiate(creaturePanelPrefab, pagesContainer);
        newPanel.transform.SetAsLastSibling();
        
        RectTransform rect = newPanel.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        newPanel.GetComponent<CreaturePage>().Setup(creatureData);
        newPanel.SetActive(true);

        Debug.Log(newPanel);
        Debug.Log("Parent: " + newPanel.transform.parent);
    }
}
