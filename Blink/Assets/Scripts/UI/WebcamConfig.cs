using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;

public class WebcamConfig : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown menu;
    [SerializeField] private UnityEngine.UI.Toggle toggleFace;
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameManager.Instance;

        SetUpDropMenu(WebCamTexture.devices);
        //this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

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
        foreach(WebCamDevice device in devices)
        {
            options.Add(device.name);
        }

        menu.AddOptions(options);
    }
}
