using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayoutSwitcher : MonoBehaviour
{
    public RectTransform containerRect; // The object with the Vertical Layout Group
    public VerticalLayoutGroup layoutGroup;
    public Transform secondaryContainer; // Where to send overflow items
    public bool isSecondaryContainer = false;

    public void AddItem(GameObject prefab, string textContent)
    {
        // Instantiate directly to the container
        GameObject newItem = Instantiate(prefab, containerRect);

        // FORCE UI RESET - This is the most common reason for "invisible" spawns
        RectTransform rt = newItem.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;

        newItem.GetComponent<TMPro.TextMeshProUGUI>().text = textContent;

        // Force the container to realize it has a new child before we check height
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        if (LayoutUtility.GetPreferredHeight(containerRect) > containerRect.rect.height)
        {
            // If we are already on a secondary/overflow page, we need a NEW page from the Manager
            // Otherwise, move it to the defined secondary container
            if (isSecondaryContainer || secondaryContainer == null)
            {
                // Create the page via the UI manager
                GameObject newPageObj = NecroLexiconUI.Instance.CreateNewCluePage();

                // Update the UI Manager's "current" pointer so the NEXT clue 
                // knows to go to this new page immediately.
                NecroLexiconUI.Instance.currentClueLayout = newPageObj.GetComponent<LayoutSwitcher>();

                // Move the item to the new page
                newItem.transform.SetParent(newPageObj.GetComponent<LayoutSwitcher>().containerRect, false);
            }
            else
            {
                // Move to the pre-defined secondary column (if using a 2-column spread)
                newItem.transform.SetParent(secondaryContainer, false);
                secondaryContainer.GetComponent<LayoutSwitcher>().isSecondaryContainer = true;
            }
        }
    }
}
