using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Master Sound Library")]
    public Sound[] sounds;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Create a persistent AudioSource for each Sound (used by Play/Stop)
        foreach (var s in sounds)
        {
            var src = gameObject.AddComponent<AudioSource>();
            s.source = src;

            src.clip                 = s.clip;
            src.outputAudioMixerGroup = s.mixer;
            src.volume               = s.volume;
            src.pitch                = s.pitch;
            src.loop                 = s.loop;

            // Apply spatial settings even on manager source
            src.spatialBlend = s.spatialBlend;
            src.dopplerLevel = s.dopplerLevel;
            src.minDistance  = s.minDistance;
            src.maxDistance  = s.maxDistance;
            src.rolloffMode  = s.rolloffMode;
        }
    }

    /// <summary>
    /// Play a sound on the manager itself (global/looped SFX).
    /// </summary>
    public void Play(string soundName)
    {
        var s = Array.Find(sounds, x => x.name == soundName);
        if (s == null) return;
        s.source.Play();
    }

    /// <summary>
    /// Stop a looping or playing sound on the manager.
    /// </summary>
    public void Stop(string soundName)
    {
        var s = Array.Find(sounds, x => x.name == soundName);
        if (s == null) return;
        s.source.Stop();
    }

    /// <summary>
    /// Play a one‐shot or looping sound on the given emitter GameObject.
    /// Returns the created AudioSource for further control (e.g. fading).
    /// </summary>
    public AudioSource PlayAt(string soundName, GameObject emitter)
    {
        var s = Array.Find(sounds, x => x.name == soundName);
        if (s == null || emitter == null)
            return null;

        var src = emitter.AddComponent<AudioSource>();
        src.clip                 = s.clip;
        src.outputAudioMixerGroup = s.mixer;
        src.volume               = s.volume;
        src.pitch                = s.pitch;
        src.loop                 = s.loop;

        // Copy spatial settings
        src.spatialBlend = s.spatialBlend;
        src.dopplerLevel = s.dopplerLevel;
        src.minDistance  = s.minDistance;
        src.maxDistance  = s.maxDistance;
        src.rolloffMode  = s.rolloffMode;

        src.Play();
        if (!s.loop)
            Destroy(src, s.clip.length / Mathf.Abs(s.pitch));

        return src;
    }

    /// <summary>
    /// Stop all instances of this clip playing on the given emitter.
    /// </summary>
    public void StopAt(string soundName, GameObject emitter)
    {
        var s = Array.Find(sounds, x => x.name == soundName);
        if (s == null || emitter == null) return;

        foreach (var src in emitter.GetComponents<AudioSource>())
        {
            if (src.clip == s.clip)
                Destroy(src);
        }
    }

    /// <summary>
    /// Gradually lowers the source’s volume to zero over fadeTime seconds,
    /// then stops it and optionally destroys the AudioSource component.
    /// </summary>
    public IEnumerator FadeOut(AudioSource source, float fadeTime, bool destroyWhenDone = false)
    {
        if (source == null) yield break;

        float startVol = source.volume;
        float timer    = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, timer / fadeTime);
            yield return null;
        }

        source.Stop();
        source.volume = startVol;

        if (destroyWhenDone)
            Destroy(source);
    }
}