using UnityEngine;
using UnityEngine.UI;

public class LayoutSwitcher : MonoBehaviour
{
    [Header("Containers")]
    public RectTransform leftPageContainer;  // Assign the Left Page object (with Vertical Layout Group)
    public RectTransform rightPageContainer; // Assign the Right Page object (with Vertical Layout Group)

    private RectTransform currentTarget; // The one we are currently filling

    private void Awake()
    {
        // Start by filling the left side
        currentTarget = leftPageContainer;
    }

    public void AddItem(GameObject prefab, string textContent)
    {
        // 1. Instantiate into the current target (Left or Right)
        GameObject newItem = Instantiate(prefab, currentTarget);
        newItem.GetComponent<TMPro.TextMeshProUGUI>().text = textContent;

        // 2. Force layout to calculate
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(currentTarget);

        // 3. Check for overflow
        if (LayoutUtility.GetPreferredHeight(currentTarget) > currentTarget.rect.height)
        {
            // IF LEFT OVERFLOWED -> Move to Right
            if (currentTarget == leftPageContainer)
            {
                Debug.Log("Left Page Full, moving item to Right Page.");
                newItem.transform.SetParent(rightPageContainer, false);
                currentTarget = rightPageContainer;

                // Re-check if it fits in the right page too (highly unlikely for 1 item, but good practice)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rightPageContainer);
            }
            // IF RIGHT OVERFLOWED -> Create a brand new CluePage (Spread)
            else if (currentTarget == rightPageContainer)
            {
                Debug.Log("Right Page Full, spawning new Book Spread.");

                // Call UI Manager to create a new double-page prefab
                GameObject newSpread = NecroLexiconUI.Instance.CreateNewCluePage();
                LayoutSwitcher newSwitcher = newSpread.GetComponent<LayoutSwitcher>();

                // Move the item that caused the overflow to the NEW spread's Left Page
                newItem.transform.SetParent(newSwitcher.leftPageContainer, false);

                // Tell the UI Manager that this new switcher is now the global "Current"
                NecroLexiconUI.Instance.currentClueLayout = newSwitcher;
            }
        }
    }

    // Inside LayoutSwitcher.cs
    public void ResetToLeftPage()
    {
        currentTarget = leftPageContainer;
        // Optional: Ensure containers are active
        leftPageContainer.gameObject.SetActive(true);
        rightPageContainer.gameObject.SetActive(true);
    }
}