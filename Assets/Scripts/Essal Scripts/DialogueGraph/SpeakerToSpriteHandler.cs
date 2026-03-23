using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerToSpriteHandler : MonoBehaviour
{
    public static SpeakerToSpriteHandler instance;

    [Serializable]
    public class SpeakerSpritePair
    {
        public Speakers speaker;
        public Sprite sprite;
    }

    [SerializeField] private List<SpeakerSpritePair> speakerSpriteList;

    public Dictionary<Speakers, Sprite> speakerSprites;

    private void Awake()
    {
        instance = this;

        speakerSprites = new Dictionary<Speakers, Sprite>();

        foreach (var pair in speakerSpriteList)
        {
            // Catches the nullreference exception because Speaker.None doesnt have a sprite
            if (pair.speaker == Speakers.None)
                continue;

            if (pair.sprite == null)
                continue;
            speakerSprites[pair.speaker] = pair.sprite;
        }
    }
}
