using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureManager : MonoBehaviour
{
    public GameObject creaturePagePanel;
    public GameObject currentPage;

    public void ShowCreature(CreatureData data)
    {
        creaturePagePanel.SetActive(true);
    }


    public void OnTabChanged()
    {
        if (currentPage != null)
        {
            Destroy(currentPage);
            currentPage = null;
        }
    }

}
