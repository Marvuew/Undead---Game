using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureManager : MonoBehaviour
{
    public GameObject creaturePagePanel;

    public void ShowCreature(CreatureData data)
    {
        creaturePagePanel.SetActive(true);
    }
}
