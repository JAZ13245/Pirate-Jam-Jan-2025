using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.DeviceSimulation;
using UnityEngine.SceneManagement;

public class WebcamConfig : MonoBehaviour
{
    public GameObject alternateBlinkSettings;
    [SerializeField] private TMP_Dropdown menu;
    [SerializeField] private UnityEngine.UI.Toggle toggleFace;
    
    private GameManager gameManager;
    private int deviceCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameManager.Instance;
        deviceCount = WebCamTexture.devices.Length;
        SetUpDropMenu(WebCamTexture.devices);
        // Hide Menu if not Main Menu
        if(SceneManager.GetActiveScene().name != "MainMenu")
        {
            this.gameObject.SetActive(false);
        }

        toggleFace.isOn = PlayerPrefs.GetInt("Toggle_Face", 0) == 1;
        alternateBlinkSettings.SetActive(!toggleFace.isOn);
    }

    // Update is called once per frame
    void Update()
    {
        if(deviceCount != WebCamTexture.devices.Length)
        {
            DeviceChange();
        }

    }

    public void GetDropdownValue()
    {
        gameManager.SetWebCamDevice(menu.value.ToString());
    }

    public void GetToggleValue()
    {
        gameManager.EnableWebCam(toggleFace.isOn);
        alternateBlinkSettings.SetActive(!toggleFace.isOn);
        PlayerPrefs.SetInt("Toggle_Face", toggleFace.isOn ? 1 : 0);
    }

    private void SetUpDropMenu(WebCamDevice[] devices)
    {
        List<string> options = new List<string>();
        if(devices.Length == 0)
        {
            options.Add("No WebCam Found");
            return;
        }
        foreach(WebCamDevice device in devices)
        {
            options.Add(device.name);
        }

        menu.AddOptions(options);
    }

    private void DeviceChange()
    {
        menu.options.Clear();
        SetUpDropMenu(WebCamTexture.devices);
        toggleFace.isOn = false;
    }
}
