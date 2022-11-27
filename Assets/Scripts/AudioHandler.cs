using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioHandler : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] List<AudioClip> clips;
    [SerializeField] List<AudioSource> sources;
    [SerializeField] AudioMixerGroup ReverbGroup;
    [SerializeField] AudioMixerGroup Master;

    public delegate void AudioEventHandler(string name, float pitch = 1, bool reverb = false);

    private void Awake()
    {
        PlayerController.OnPlaySound += PlaySound;
        Floater.OnPlaySound += PlaySound;
        Checkpoint.OnPlaySound += PlaySound;
    }

    private void PlaySound(string name, float pitch, bool reverb)
    {
        AudioSource source = sources.Find(s => !s.isPlaying);
        if (source != null)
        {
            source.pitch = pitch < 0.5f ? 0.5f : pitch;
            source.outputAudioMixerGroup = reverb ? ReverbGroup : Master;
            AudioClip clip = clips.Find(c => c.name == name);
            if (clip != null) 
                source.PlayOneShot(clip);
        }
    }
}
