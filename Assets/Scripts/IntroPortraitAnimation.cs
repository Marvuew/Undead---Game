using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class IntroPortraitAnimation : MonoBehaviour
{
    float speed = 100f; // Pixels per second

    float startPos;
    float endPos;
    float portraitHeight;
    float totalLoopHeight;
    private int totalIcons;

    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>(); 
        portraitHeight = rect.rect.height; // Calculate the height

        int totalIcons = transform.parent.childCount;
        totalLoopHeight = rect.rect.height * totalIcons;
    }

    void Update()
    {
        Vector3 pos = rect.anchoredPosition;
        pos.y += speed * Time.deltaTime;

        if (pos.y > (rect.rect.height * 2))
        {
            pos.y -= totalLoopHeight;
        }

        rect.anchoredPosition = pos;
    }
}
