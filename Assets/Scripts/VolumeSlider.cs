using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    Slider volumeSlider;

    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            float volume = PlayerPrefs.GetFloat("MasterVolume");
            audioMixer.SetFloat("MasterVolume", -80f + Mathf.Pow(volume, 1 / 6f) * 80f);
            volumeSlider.value = volume;
        }
        else
        {
            volumeSlider.value = 1;
            audioMixer.SetFloat("MasterVolume", 0);
        }

        volumeSlider.onValueChanged.AddListener(v =>
        {
            audioMixer.SetFloat("MasterVolume", -80f + Mathf.Pow(v, 1/6f) * 80f);
            PlayerPrefs.SetFloat("MasterVolume", v);
        });
    }
}
