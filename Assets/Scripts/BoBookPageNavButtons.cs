using UnityEngine;
using UnityEngine.UI;

public class BookPageNavButtons : MonoBehaviour
{
    [Header("Controller (optional)")]
    [Tooltip("Drag your existing BookUIController (ButtonController.cs) here if auto-find fails.")]
    [SerializeField] private BookUIController controller;

    [Header("Buttons on THIS page")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    private void Awake()
    {
        // 1) If you dragged it in, use it.
        if (controller == null)
        {
            // 2) Try parent chain (works if pages are children)
            controller = GetComponentInParent<BookUIController>();
        }

        if (controller == null)
        {
#if UNITY_2023_1_OR_NEWER
            // 3) Newer Unity
            controller = Object.FindFirstObjectByType<BookUIController>();
#else
            // 4) Older Unity
            controller = Object.FindObjectOfType<BookUIController>();
#endif
        }

        if (controller == null)
        {
            Debug.LogError($"BookPageNavButtons on '{name}': Couldn't find BookUIController. Drag it into the 'controller' field.");
            return;
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(controller.NextPage);

        if (prevButton != null)
            prevButton.onClick.AddListener(controller.PrevPage);
    }
}