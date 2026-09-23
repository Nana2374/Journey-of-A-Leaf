using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BiomeLockVisual : MonoBehaviour
{
    [SerializeField] private string sceneName; // same string used in the OnClick, e.g. "GoldenGrove"
    [SerializeField] private GameObject lockedOverlay; // optional padlock icon/greyed sprite
    [SerializeField] private Button button;

    private void OnEnable()
    {
        Refresh();
        if (MapManager.Instance != null)
            MapManager.Instance.OnMapUnlocked += HandleUnlocked;
    }

    private void OnDisable()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.OnMapUnlocked -= HandleUnlocked;
    }

    private void HandleUnlocked(string id)
    {
        if (id == sceneName) Refresh();
    }

    private void Refresh()
    {
        bool unlocked = MapManager.Instance == null || MapManager.Instance.IsUnlocked(sceneName);
        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
        if (button != null) button.interactable = unlocked;
    }
}