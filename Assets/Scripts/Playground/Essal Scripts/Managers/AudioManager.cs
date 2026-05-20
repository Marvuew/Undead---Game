using Assets.Scripts.GameScripts;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    [SerializeField] private List<Sound> sounds;
    public static AudioManager instance;

    public List<Sound> pageTurnSounds;

    [Header("Music")]
    public List<Sound> Songs;
    public int songChangeWaitTime = 30;

    [HideInInspector]
    public Sound currentSong;
    private void Awake()
    {
        Debug.Log("AudioManager Awake called");
        if (instance == null)
        {
            instance = this;
            Debug.Log("AudioManager instance set");
        }
        else
        {
            Debug.LogWarning("Duplicate AudioManager destroyed");
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

        foreach (Sound s in Songs)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = s.PlayOnAwake;
        }

        foreach (Sound s in pageTurnSounds)
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
        if (SceneManager.GetActiveScene().name != SceneNames.MainMenu.ToString())
        {
            StartCoroutine(SongController());
            PlayMusic("AmbientDay", 1f);
        }
        else
        {
            return;
        }
    }

    public void PlayMusic(string name, float volume)
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
    
    public void StopAllPlayingSounds()
    {
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
    }

    public void PlaySFX(string name, float volume)
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
            PlaySFX(walkSound.name, walkSound.volume);

            float delay = 0.35f + UnityEngine.Random.Range((float)-0.05f, 0.08f);
            yield return new WaitForSeconds(delay);
        }  
    }

    public IEnumerator QueueClueFoundSound()
    {
        yield return null;
        yield return new WaitUntil(() => Player.Instance.interacting == false);
        PlaySFX("ClueFound", 1f);
    }

    public void PlayPageTurnSound()
    {
        
        if (pageTurnSounds != null && pageTurnSounds.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, pageTurnSounds.Count);
            pageTurnSounds[index].source.Play();
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
            if (Songs.Count == 0) yield break;
            int index; // 
            do { index = UnityEngine.Random.Range(0, Songs.Count); } // TRY TO FIND AN INDEX AS LONG AS IT ISNT THE SAME AS THE LAST ONE
            while (index == lastIndex && Songs.Count > 1);
            var song = Songs[index]; // MAKE A VARIABLE FOR THE SONG
            lastIndex = index; // UPDATE THE LAST INDEX
            song.source.Play(); // PLAY THE SONG
            currentSong = Songs.Find(s => s.name == song.name); // UPDATE THE CURRENT SONG
            yield return new WaitForSeconds(currentSong.clip.length); // WAIT FOR SONG TO FINISH
            int randomWaitTime = UnityEngine.Random.Range(songChangeWaitTime - 15, songChangeWaitTime + 15); // MAKE THE WAITTIME IN BETWEEN SONGS A BIT RANDOM
            yield return new WaitForSeconds(randomWaitTime); // WAIT A RANDOM AMOUNT OF TIME TILL NEXT SONG
        }
    }
}
