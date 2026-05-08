using Assets.Scripts.GameScripts;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{

    [SerializeField] private List<Sound> sounds;
    public static AudioManager instance;

    public AudioClip[] pageTurnSounds;

    [Header("Music")]
    public AudioClip[] Songs;
    public int songChangeWaitTime = 30;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = s.PlayOnAwake;
        }

    }

    public void Start()
    {
        PlayMusic("AmbientDay"); // START THE AMBIENCE
        StartCoroutine(SongController()); // START THE RANDOM SONG LOOP
    }

    public void PlayMusic(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);

        if (s == null)
        {
            print("Sound: " + name + " not found");
            return;
        }

        s.source.Play();
    }

    public void StopMusic(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);

        if (s == null)
        {
            print("Sound: " + name + " not found");
            return;
        }
        s.source.Stop();
    }

    public void PlaySFX(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);

        if (s == null)
        {
            print("Sound: " + name + " not found");
            return;
        }

        s.source.Play();
    }

    public void StopSFX(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);

        if (s == null)
        {
            print("Sound: " + name + " not found");
            return;
        }

        s.source.Stop();
    }

    public void AddSound(AudioClip clip)
    {
        Sound s = new Sound()
        {
            clip = clip,
            name = clip.name,
            volume = 1f,
            loop = false,
            pitch = 1f,
            PlayOnAwake = false,
            source = gameObject.AddComponent<AudioSource>()
        };

        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.loop = s.loop;
        s.source.pitch = s.pitch;
        s.source.playOnAwake = s.PlayOnAwake;
        sounds.Add(s);
    }

    public IEnumerator WalkingLoop()
    {
        while (true)
        {
            Sound walkSound = sounds.Find(sound => sound.name == "Walk");

            walkSound.source.pitch = UnityEngine.Random.Range((float)0.8, 1.2f);
            PlaySFX(walkSound.name);

            float delay = 0.35f + UnityEngine.Random.Range((float)-0.05f, 0.08f);
            yield return new WaitForSeconds(delay);
        }  
    }

    public IEnumerator QueueClueFoundSound()
    {
        yield return null;
        yield return new WaitUntil(() => Player.Instance.interacting == false);
        PlaySFX("ClueFound");
    }

    public void PlayPageTurnSound()
    {
        
        if (pageTurnSounds != null && pageTurnSounds.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, pageTurnSounds.Length);
            PlaySFX(pageTurnSounds[index].name);
        }
    }

    public IEnumerator SongController()
    {
        foreach(var song in Songs)
        {
            Debug.Log(song.name);
        }
        int lastIndex = -1; // INIT LAST INDEX TO -1 TO PREVENT ACCIDENTAL INFINITE LOOP
        while (true)
        {
            if (Songs.Length == 0) yield break;
            int index; // 
            do { index = UnityEngine.Random.Range(0, Songs.Length); } // TRY TO FIND AN INDEX AS LONG AS IT ISNT THE SAME AS THE LAST ONE
            while (index == lastIndex && Songs.Length > 1);
            var song = Songs[index]; // MAKE A VARIABLE FOR THE SONG
            lastIndex = index; // UPDATE THE LAST INDEX
            PlayMusic(song.name); // PLAY THE SONG
            yield return new WaitForSeconds(sounds.Find(s => s.name == song.name).clip.length); // WAIT FOR SONG TO FINISH
            int randomWaitTime = UnityEngine.Random.Range(songChangeWaitTime - 15, songChangeWaitTime + 15); // MAKE THE WAITTIME IN BETWEEN SONGS A BIT RANDOM
            yield return new WaitForSeconds(randomWaitTime); // WAIT A RANDOM AMOUNT OF TIME TILL NEXT SONG
        }
    }
}
