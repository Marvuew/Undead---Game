using UnityEngine;
using UnityEngine.EventSystems;

public class CreatureButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject highlightCircle;
    public GameObject creaturePanelPrefab;
    public Transform pagesContainer;
    public Transform canvas;

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
        newPanel.transform.SetParent(pagesContainer, false);

        RectTransform rect = newPanel.GetComponent<RectTransform>();
        // Anchor til højre side (midt på Y)
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);

        // Pivot skal også være i højre side
        rect.pivot = new Vector2(1f, 0.5f);

        // Flyt lidt ind fra kanten (valgfrit)
        rect.anchoredPosition = new Vector2(-50f, 0f);




        newPanel.GetComponent<CreaturePage>().Setup(creatureData);
        newPanel.SetActive(true);

        Debug.Log(newPanel);
        Debug.Log("Parent: " + newPanel.transform.parent);

        Debug.Log(rect.anchoredPosition);
        Debug.Log(rect.anchorMin + " / " + rect.anchorMax);
    }
}
