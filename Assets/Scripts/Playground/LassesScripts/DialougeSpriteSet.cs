using System;
using UnityEngine;

public class DialogueSpriteSet : MonoBehaviour
{
    [Serializable]
    public class SpriteEntry
    {
        public string id;          // e.g. "neutral", "angry", "smile"
        public Sprite sprite;
    }

    [Header("Renderer")]
    [SerializeField] private SpriteRenderer renderer2D;

    [Header("Sprites")]
    [SerializeField] private SpriteEntry[] sprites;

    [Header("Default")]
    [SerializeField] private string defaultId = "neutral";

    private void Awake()
    {
        if (renderer2D == null)
            renderer2D = GetComponentInChildren<SpriteRenderer>();

        if (renderer2D == null)
        {
            Debug.LogError($"DialogueSpriteSet on {name}: Missing SpriteRenderer reference.");
            enabled = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(defaultId))
            SetSprite(defaultId);
    }

    public void SetSprite(string id)
    {
        if (renderer2D == null) return;

        if (string.IsNullOrWhiteSpace(id))
            return;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i].id == id && sprites[i].sprite != null)
            {
                renderer2D.sprite = sprites[i].sprite;
                return;
            }
        }

        Debug.LogWarning($"DialogueSpriteSet on {name}: No sprite found for id '{id}'.");
    }
}