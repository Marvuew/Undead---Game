using TMPro;
using UnityEngine;

public class WorldSpeechPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private bool followTarget = true;

    private Transform target;
    private float timer;

    public void Show(string message, Transform followTransform)
    {
        target = followTransform;
        timer = lifetime;

        if (textLabel != null)
            textLabel.text = message;

        UpdatePosition();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (followTarget)
            UpdatePosition();

        if (timer <= 0f)
            Destroy(gameObject);
    }

    private void UpdatePosition()
    {
        if (target == null)
            return;

        transform.position = target.position + worldOffset;
    }
}