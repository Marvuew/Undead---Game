using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterTMP : MonoBehaviour
{
    [SerializeField] private float charsPerSecond = 40f;

    public IEnumerator TypeRoutine(TMP_Text label, string text)
    {
        label.text = text;
        label.maxVisibleCharacters = 0;
        label.ForceMeshUpdate();

        int total = label.textInfo.characterCount;
        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        for (int i = 0; i <= total; i++)
        {
            label.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }
    }
}