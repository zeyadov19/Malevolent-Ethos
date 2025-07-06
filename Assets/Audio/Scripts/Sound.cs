using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string            name;
    public AudioClip         clip;
    public AudioMixerGroup   mixer;

    [Range(0f, 1f)]
    public float    volume = 1f;
    [Range(-3f, 3f)]
    public float    pitch  = 1f;
    public bool     loop   = false;

    [Header("3D Spatial Settings")]
    [Range(0f, 1f)]
    public float    spatialBlend = 1f;         // fully 3D by default
    [Range(0f, 5f)]
    public float    dopplerLevel = 1f;
    public float    minDistance   = 1f;
    public float    maxDistance   = 500f;
    [HideInInspector ]public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    [HideInInspector]
    public AudioSource source;                 // manager’s source (for Play)
}
