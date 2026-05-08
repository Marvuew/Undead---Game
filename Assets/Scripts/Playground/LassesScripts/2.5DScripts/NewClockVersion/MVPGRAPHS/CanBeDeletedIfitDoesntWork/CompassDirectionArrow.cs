using UnityEngine;

namespace Assets.Scripts.GameScripts
{
    public class CompassDirectionArrow : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private RectTransform arrowImage;

        private void LateUpdate()
        {
            if (player == null || targetPoint == null || arrowImage == null)
                return;

            Vector2 direction = targetPoint.position - player.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Use -90 if your arrow graphic points up by default.
            arrowImage.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        public void SetTarget(Transform newTarget)
        {
            targetPoint = newTarget;
        }
    }
}