using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.DeviceSimulation;

public class WebcamConfig : MonoBehaviour
{
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
        //this.gameObject.SetActive(false);
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
