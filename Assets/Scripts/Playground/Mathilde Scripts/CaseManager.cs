using System.Collections.Generic;
using UnityEngine;

public class CaseManagerMathilde : MonoBehaviour
{
    public GameObject casePagePanel;
    public GameObject caseCurrentPage;

    public List<CaseData> caseList;

    public void ShowCase(CaseData data)
    {
        casePagePanel.SetActive(true);

        caseCurrentPage.GetComponent<CasePage>().Setup(data);
    }

    public void OnTabChanged()
    {
        if (caseCurrentPage != null)
        {
            //Destroy(caseCurrentPage); // WHY DESTROY IT?
            caseCurrentPage.SetActive(false);
            caseCurrentPage = null;
        }
    }
}
