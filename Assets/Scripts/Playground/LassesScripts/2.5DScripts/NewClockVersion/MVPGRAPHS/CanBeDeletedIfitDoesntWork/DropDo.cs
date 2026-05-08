using UnityEngine;
using TMPro;

namespace Assets.Scripts.GameScripts
{
    public class DropdownListPositionFix : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Vector2 wantedPosition = new Vector2(67f, -56f);

        private void Awake()
        {
            if (dropdown == null)
                dropdown = GetComponent<TMP_Dropdown>();

            dropdown.onValueChanged.AddListener(_ => HideList());
        }

        private void LateUpdate()
        {
            Transform list = transform.Find("Dropdown List");

            if (list == null)
                return;

            RectTransform rect = list.GetComponent<RectTransform>();

            if (rect == null)
                return;

            rect.anchoredPosition = wantedPosition;
        }

        private void HideList()
        {
            Transform list = transform.Find("Dropdown List");

            if (list != null)
                list.gameObject.SetActive(false);
        }
    }
}
