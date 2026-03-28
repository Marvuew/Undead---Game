using UnityEngine;
using UnityEngine.EventSystems;

public class CaseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject highlightCircle;
    public GameObject casePagePanel;
    public Transform pageContainer;
    public Transform leftSideContainer;
    public Transform rightSideContainer;
    public Transform canvas;

    public CaseManager caseManager;
    public CaseData caseData;

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
        Debug.Log("CASE CLICKED");

        caseManager.OnTabChanged();

        GameObject newPanel = Instantiate(casePagePanel, rightSideContainer);
        newPanel.transform.SetParent(rightSideContainer, false);

        RectTransform rect = newPanel.GetComponent<RectTransform>();

        newPanel.GetComponent<CasePage>().Setup(caseData);
        newPanel.SetActive(true);

        caseManager.caseCurrentPage = newPanel;
    }

}
