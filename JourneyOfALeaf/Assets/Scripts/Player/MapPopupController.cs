using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapPopupController : MonoBehaviour
{
    public static bool IsMapOpen { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private bool pauseGameWhenOpen = true;

    [Header("Hide while map is open")]
    [SerializeField] private GameObject[] hudToHideDuringMap;

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        SetHudVisible(false);
        IsMapOpen = true;

        if (pauseGameWhenOpen)
            Time.timeScale = 0f;
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
        SetHudVisible(true);
        IsMapOpen = false;

        if (pauseGameWhenOpen)
            Time.timeScale = 1f;
    }

    public void ToggleMap()
    {
        if (mapPanel.activeSelf)
            CloseMap();
        else
            OpenMap();
    }

    private void SetHudVisible(bool visible)
    {
        if (hudToHideDuringMap == null) return;

        foreach (GameObject hud in hudToHideDuringMap)
        {
            if (hud != null)
                hud.SetActive(visible);
        }
    }
}