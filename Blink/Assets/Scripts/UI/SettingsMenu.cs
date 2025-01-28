using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioMixer audioMixer;
    public float volume;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volume = PlayerPrefs.GetFloat("Volume", 1f);
        audioMixer.SetFloat("MasterVolume", GetDecibelVolume(volume));

        // Slider Setting
        volumeSlider.value = volume;

        this.gameObject.SetActive(false);
    }

    public void SetVolume()
    {
        volume = volumeSlider.value;

        // Set the volume in the mixer
        audioMixer.SetFloat("MasterVolume", GetDecibelVolume(volume));

        // Save the volume setting
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    private float GetDecibelVolume(float volume)
    {
        // Convert linear volume (0-1) to logarithmic decibel scale (-80 to 0)
        float decibelVolume = Mathf.Log10(volume) * 20;
        if (volume <= 0) decibelVolume = -80; // Mute at 0

        return decibelVolume;
    }
}
