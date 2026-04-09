using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class isWillingToTalkManager : MonoBehaviour
{
    public static isWillingToTalkManager instance;
    private void Awake()
    {
        instance = this;

        foreach (var speaker in speakerList)
        {
            isSpeakerWillingToTalk.Add(speaker, true);
        }
    }

    public Dictionary<DialogueSpeaker, bool> isSpeakerWillingToTalk = new();
    public List<DialogueSpeaker> speakerList;

    public void HandleVendetta(DialogueSpeaker speaker)
    {
        if (isSpeakerWillingToTalk.ContainsKey(speaker))
        {
            isSpeakerWillingToTalk[speaker] = !isSpeakerWillingToTalk[speaker];
        }
        else
        {
            Debug.LogWarning("Speaker doesnt exist in the dicitonary");
        }
    }
}

