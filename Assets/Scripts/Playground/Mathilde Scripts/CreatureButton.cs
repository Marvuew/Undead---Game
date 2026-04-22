using UnityEngine;
using UnityEngine.EventSystems;

public class CreatureButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject highlightCircle;
    public GameObject creaturePanelPrefab;
    public Transform pageContainer;
    public Transform leftSideContainer;
    public Transform rightSideContainer;
    public Transform canvas;

    public CreatureManager creatureManager;

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

        creatureManager.OnTabChanged();

        GameObject newPanel = Instantiate(creaturePanelPrefab, rightSideContainer);
        newPanel.transform.SetParent(rightSideContainer, false);

        RectTransform rect = newPanel.GetComponent<RectTransform>();

        newPanel.GetComponent<CreaturePage>().Setup(creatureData);
        newPanel.SetActive(true);

        creatureManager.currentPage = newPanel;
    }

}
