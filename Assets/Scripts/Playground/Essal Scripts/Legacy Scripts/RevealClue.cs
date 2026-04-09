using UnityEngine;
using UnityEngine.UI;

public class RevealClue : MonoBehaviour
{
    public float revealRadius = 150f;
    public float fadeSpeed = 5f;

    private Image clueImage;
    private CanvasGroup group;
    private float timer = 0f;
    private Transform lensTransform;

    void Start()
    {
        clueImage = GetComponent<Image>();
        group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        // Find the LensParent (the object moving with the mouse)
        // We look for the parent of our parent (since we are inside the Mask)
        lensTransform = transform.parent.parent;
    }

    void Update()
    {
        if (lensTransform == null) return;

        // Use World Distance for accuracy
        float dist = Vector3.Distance(transform.position, lensTransform.position);

        if (dist < revealRadius)
            timer += Time.deltaTime * fadeSpeed;
        else
            timer -= Time.deltaTime * fadeSpeed;

        timer = Mathf.Clamp01(timer);
        float exponentialT = timer * timer * timer;

        // We keep Alpha at 1 because the MASK handles the "Pixel Cutting"
        group.alpha = 1f;

        // The exponential lerp only affects the COLOR (Dark to White)
        Color hiddenColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        clueImage.color = Color.Lerp(hiddenColor, Color.white, exponentialT);
    }
}