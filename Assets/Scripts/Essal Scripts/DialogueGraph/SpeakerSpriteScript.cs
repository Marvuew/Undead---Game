using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerSpriteScript : MonoBehaviour
{

    [Serializable]
    public class SpeakerSpritePair
    {
        public Speakers speaker;
        public Sprite sprite;
    }

    [SerializeField] private List<SpeakerSpritePair> speakerSpriteList;

    public static Dictionary<Speakers, Sprite> speakerSprites;

    private void Awake()
    {
        speakerSprites = new Dictionary<Speakers, Sprite>();

        foreach (var pair in speakerSpriteList)
        {
            speakerSprites[pair.speaker] = pair.sprite;
        }
    }
}
