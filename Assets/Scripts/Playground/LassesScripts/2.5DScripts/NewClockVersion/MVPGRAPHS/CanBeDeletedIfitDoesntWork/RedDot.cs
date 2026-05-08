using UnityEngine;

namespace Assets.Scripts.GameScripts
{
    public class CompassTargetDot : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private RectTransform dot;
        [SerializeField] private RectTransform maskArea;

        [Header("Settings")]
        [SerializeField] private float edgePadding = 8f;

        private void LateUpdate()
        {
            if (minimapCamera == null || targetPoint == null || dot == null || maskArea == null)
                return;

            UpdateDotPosition();
        }

        private void UpdateDotPosition()
        {
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(targetPoint.position);

            float x = (viewportPos.x - 0.5f) * maskArea.rect.width;
            float y = (viewportPos.y - 0.5f) * maskArea.rect.height;

            Vector2 dotPosition = new Vector2(x, y);

            float radius = Mathf.Min(maskArea.rect.width, maskArea.rect.height) * 0.5f;
            radius -= edgePadding;

            if (dotPosition.magnitude > radius)
            {
                dotPosition = dotPosition.normalized * radius;
            }

            dot.anchoredPosition = dotPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            targetPoint = newTarget;
        }
    }
}