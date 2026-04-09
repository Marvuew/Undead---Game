using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class RevealClueSprite : MonoBehaviour
{
    [Header("Settings")]
    public float fadeDuration = 3f;

    private SpriteRenderer _sprite;
    private float timer = 0f;
    private Color hiddenColor;
    private Color startColour;
    bool revealed;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();

        hiddenColor = _sprite.color;
    }

    private void OnMouseEnter()
    {
        if (!revealed)
        {
            timer = 0f;
            startColour = _sprite.color;
        }
    }

    private void OnMouseOver()
    {
        if (!revealed)
        {
            if (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;
                t = t * t;

                _sprite.color = Color.Lerp(startColour, Color.white, t);

                // Lock the sprite color
                if (_sprite.color == Color.white)
                {
                    Debug.Log("revealed");
                    StopAllCoroutines();
                    _sprite.maskInteraction = SpriteMaskInteraction.None;
                    revealed = true;
                }
            }
        }   
    }

    private void OnMouseExit()
    {
        if (!revealed)
        {
            timer = 0f;
            startColour = _sprite.color;
            StopAllCoroutines();
            StartCoroutine(LerpToHiddenColour());
        }
    }

    IEnumerator LerpToHiddenColour()
    {
        Debug.Log("Starting Coroutine");
        while (_sprite.color != hiddenColor)
        {
            _sprite.color = Color.Lerp(_sprite.color, hiddenColor, Time.deltaTime);
            yield return null;
        }
        _sprite.color = hiddenColor;
    }

}
