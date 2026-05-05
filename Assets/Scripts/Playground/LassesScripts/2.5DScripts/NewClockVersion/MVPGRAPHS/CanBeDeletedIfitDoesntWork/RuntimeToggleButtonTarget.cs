using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RuntimeToggleButtonTarget : MonoBehaviour
{
    [Header("Target To Toggle")]
    [SerializeField] private GameObject target;
    [SerializeField] private string targetName = "Necro Lexicon";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveListener(ToggleTarget);
        button.onClick.AddListener(ToggleTarget);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(ToggleTarget);
    }

    private void ToggleTarget()
    {
        if (target == null)
            target = GameObject.Find(targetName);

        if (target == null)
        {
            Debug.LogWarning("RuntimeToggleButtonTarget: Could not find target named " + targetName);
            return;
        }

        target.SetActive(!target.activeSelf);
    }
}