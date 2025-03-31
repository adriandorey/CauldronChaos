using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GraphicsManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private List<Resolution> _resolutionsList = new();

    private int _currentResolutionIndex;
    private bool _isFullScreen = true;

    [SerializeField] private GameObject abovePlayerUI;
    [SerializeField] private Toggle abovePlayerUIToggle;
    
[SerializeField] private SaveManager saveManager;

    // Start is called before the first frame update
    private void Start()
    {
        abovePlayerUIToggle.isOn = true;

        foreach (var t in Screen.resolutions)
        {
            _resolutionsList.Add(t);
        }
        resolutionDropdown.ClearOptions();

        _resolutionsList.Sort((a, b) =>
        {
            if (a.width != b.width)
                return b.width.CompareTo(a.width);
            else
                return b.height.CompareTo(a.height);
        });

        var options = new List<string>();
        for (var i = 0; i < _resolutionsList.Count; i++)
        {
            var resolutionOption = _resolutionsList[i].width + "x" + _resolutionsList[i].height + " ";
            options.Add(resolutionOption);
            if (_resolutionsList[i].width == Screen.width && _resolutionsList[i].height == Screen.height)
            {
                _currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // Ensure we have a valid index
        if (_resolutionsList.Count > 0)
        {
            resolutionDropdown.value = _currentResolutionIndex = Mathf.Clamp(_currentResolutionIndex, 0, _resolutionsList.Count - 1);
            resolutionDropdown.RefreshShownValue();
            SetResolution(_currentResolutionIndex);
        }
        else
        {
            Debug.LogError("No valid resolutions available to set.");
        }
    }


    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= _resolutionsList.Count)
        {
            Debug.LogError($"Invalid resolution index: {resolutionIndex}");
            return;
        }

        var resolution = _resolutionsList[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, _isFullScreen);
    }


    public void SetWindowed(bool value)
    {
        _isFullScreen = value;
        Screen.fullScreen = value;
    }

    public void ToggleAbovePlayerUI()
    {
        abovePlayerUI.SetActive(abovePlayerUIToggle.isOn);
    }

    internal void SetAbovePlayerUi(bool value)
    {
        abovePlayerUI.SetActive(value);
        abovePlayerUIToggle.isOn = value;
    }

    internal bool GetAbovePlayerUi()
    {
        return abovePlayerUIToggle.isOn;
    }
}