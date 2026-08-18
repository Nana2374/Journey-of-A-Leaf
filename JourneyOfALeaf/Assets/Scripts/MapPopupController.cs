using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPopupController : MonoBehaviour
{
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private bool pauseGameWhenOpen = true;

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        if (pauseGameWhenOpen)
            Time.timeScale = 0f;
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
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
}
