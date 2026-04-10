using UnityEngine;

public class CaseManagerMathilde : MonoBehaviour
{
    public GameObject casePagePanel;
    public GameObject caseCurrentPage;

    public void ShowCase(CaseData data)
    {
        casePagePanel.SetActive(true);
    }

    public void OnTabChanged()
    {
        if (caseCurrentPage != null)
        {
            Destroy(caseCurrentPage);
            caseCurrentPage = null;
        }
    }
}
