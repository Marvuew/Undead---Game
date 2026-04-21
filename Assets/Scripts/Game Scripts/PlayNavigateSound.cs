using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayNavigateSound : MonoBehaviour, ISelectHandler
{
    public ScrollRect scroll;


    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.instance.PlaySFX("NavigateChoice");

        RectTransform content = scroll.content;
        RectTransform item = (RectTransform)transform;

        // Get index of this item in the content
        int index = item.GetSiblingIndex();

        float itemHeight = item.rect.height;
        float spacing = content.GetComponent<VerticalLayoutGroup>().spacing;

        float stepSize = itemHeight + spacing;

        float viewportHeight = scroll.viewport.rect.height;
        float contentHeight = content.rect.height;

        // Calculate target Y so the selected item moves toward bottom
        float targetY = index * stepSize;

        // Clamp so we don't overscroll
        float maxY = contentHeight - viewportHeight;
        targetY = Mathf.Clamp(targetY, 0, maxY);

        // Apply
        content.anchoredPosition = new Vector2(0, targetY);
    }
}

