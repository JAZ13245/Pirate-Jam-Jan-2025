using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Settings Values")]
    public float volume;
    public float sensitvity;
    public int toggleCrouch;
    [Header("Components")]
    public Slider volumeSlider;
    public AudioMixer audioMixer;
    public Slider sensitvitySlider;
    public Toggle toggleCrouchBox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volume = PlayerPrefs.GetFloat("Volume", 1f);
        sensitvity = PlayerPrefs.GetFloat("Sensitvity", 0.1f);
        toggleCrouch = PlayerPrefs.GetInt("Toggle_Crouch", 0);
        audioMixer.SetFloat("MasterVolume", GetDecibelVolume(volume));

        sensitvitySlider.minValue = 0f;
        sensitvitySlider.maxValue = 1f;


        // Setting Defualt
        volumeSlider.value = volume;
        sensitvitySlider.value = sensitvity;
        toggleCrouchBox.isOn = toggleCrouch == 1;

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

    public void SetSensitvity()
    {
        sensitvity = sensitvitySlider.value;

        // Save the Sensitvity setting
        PlayerPrefs.SetFloat("Sensitvity", sensitvity);
        PlayerPrefs.Save();
    }

    public void SetToggleCrouch()
    {
        toggleCrouch = toggleCrouchBox.isOn ? 1 : 0;

        PlayerPrefs.SetInt("Toggle_Crouch", toggleCrouch);
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
