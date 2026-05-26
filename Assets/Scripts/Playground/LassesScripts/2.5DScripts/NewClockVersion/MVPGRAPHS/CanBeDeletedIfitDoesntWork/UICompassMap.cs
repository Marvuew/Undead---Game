using UnityEngine;

namespace Assets.Scripts.GameScripts
{
    public class UICompassMap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private RectTransform mapImage;
        [SerializeField] private RectTransform maskArea;

        [Header("World Bounds")]
        [SerializeField] private Transform topLeftWorld;
        [SerializeField] private Transform bottomRightWorld;

        [Header("Fine Tune")]
        [SerializeField] private Vector2 mapOffset;

        private void LateUpdate()
        {
            if (player == null)
                return;

            UpdateMap();
        }

        private void UpdateMap()
        {
            float normalizedX = Mathf.InverseLerp(
                topLeftWorld.position.x,
                bottomRightWorld.position.x,
                player.position.x
            );

            float normalizedY = Mathf.InverseLerp(
                bottomRightWorld.position.y,
                topLeftWorld.position.y,
                player.position.y
            );

            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);

            Vector2 mapSize = mapImage.rect.size;
            Vector2 maskSize = maskArea.rect.size;

            float moveRangeX = (mapSize.x - maskSize.x) * 0.5f;
            float moveRangeY = (mapSize.y - maskSize.y) * 0.5f;

            float x = Mathf.Lerp(moveRangeX, -moveRangeX, normalizedX);
            float y = Mathf.Lerp(moveRangeY, -moveRangeY, normalizedY);

            mapImage.anchoredPosition = new Vector2(x, y) + mapOffset;
        }
    }
}