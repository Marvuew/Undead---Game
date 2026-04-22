using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayNavigateSound : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.instance.PlaySFX("NavigateChoice");
    }
}

