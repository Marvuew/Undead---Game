using UnityEngine;

[ExecuteInEditMode]
public class PositionBaker : MonoBehaviour
{
    public InteractableScriptableObject data;

    [ContextMenu("Save Current Position to SO")]
    public void Bake()
    {
        if (data == null) return;

        // FORCE Z to 0 to prevent Transparency Sort Mode from adding offsets
        Vector3 flatPosition = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = flatPosition;

        data.RecordPosition(flatPosition);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            data.sortingLayerName = sr.sortingLayerName;
            data.orderInLayer = sr.sortingOrder;
        }
    }
    private void OnValidate()
    {
        Debug.Log("Validating");
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (data == null) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Set the sprite from the SO so you can see what you're placing
            sr.sprite = data.interactableSprite;

            // Set sorting so it appears at the correct depth immediately
            sr.sortingLayerName = data.sortingLayerName;
            sr.sortingOrder = data.orderInLayer;
        }
    }


}
