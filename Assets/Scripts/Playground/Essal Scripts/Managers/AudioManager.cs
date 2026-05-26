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

    [SerializeField] private List<Sound> piano;

    [Header("Music")]
    public List<Sound> Songs;
    public int songChangeWaitTime = 30;

    public Sound currentSong;

    private Coroutine loopingTracks;

    public bool loopedTrackPlaying = false;
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


        foreach (Sound s in piano)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = s.PlayOnAwake;

        }

        SceneManager.sceneLoaded += instance.OnSceneLoaded;
    }

    public void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneNames.MainMenu.ToString())
        {
            loopingTracks = StartCoroutine(LoopingTracks());
            PlayMusic("AmbientDay");
        }
        else
        {
            return;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!loopedTrackPlaying)
        {
            StartCoroutine(LoopingTracks());
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        s.source.volume = 0.2f; // Diry quick fix for píano not being too loud

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

    public void PlaySFX(string name, float volume = 1f)
    {
        Sound s = sounds.Find(sound => sound.name == name);

        if (s == null)
        {
            print("Sound: " + name + " not found");
            return;
        }

        s.source.PlayOneShot(s.clip, volume);
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

    public bool CheckSound(string name)
    {
        return sounds.Contains(sounds.Find(sound => sound.name == name));
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
        if (pageTurnSounds != null && pageTurnSounds.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, pageTurnSounds.Count);
            pageTurnSounds[index].source.Play();
        }
    }

    public IEnumerator LoopingTracks()
    {
        loopedTrackPlaying = true;
        Debug.Log("playing looped tracks");
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

    public void StopLoopingTracks()
    {
        StopCoroutine(loopingTracks);
        foreach(var song in Songs)
        {
            song.source.Stop();
            Debug.Log("Stopping: " + song.name);
        }
        loopedTrackPlaying = false;
        Debug.Log("Looping Tracks Stopped");
    }

    /*public IEnumerator StopLoopingTracksForSong(string songName)
    {
        StopLoopingTracks();
        StopAllPlayingSounds();
        Debug.Log("Looping Tracks Stopped");
        Sound s = sounds.Find(s => s.name == songName);
        if (s == null)
        {
            Debug.LogWarning("Could find the piano song");
        }
        else
        {
            Debug.Log("song is " + s);
        }
        s.source.volume = 0.2f; // Diry quick fix for píano not being too loud
        Debug.Log("Waiting for song to finish: " + songName);
        yield return new WaitForSeconds();
        loopingTracks = StartCoroutine(LoopingTracks());
        Debug.Log("Music Started Again");
    }*/
}
