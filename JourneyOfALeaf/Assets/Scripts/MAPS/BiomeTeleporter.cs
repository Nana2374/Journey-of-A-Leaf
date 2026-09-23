using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BiomeTeleporter : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Scene to load when this biome is selected")]
    [SerializeField] private string sceneName;

    [Tooltip("Must match an unlocksMapId from a quest step. Leave blank if this biome is always unlocked.")]
    [SerializeField] private string mapId;

    [Header("Locked Visuals (optional)")]
    [SerializeField] private GameObject lockedOverlay;   // e.g. a padlock icon/greyed background
    [SerializeField] private Button button;              // the clickable button on this icon

    private void OnEnable()
    {
        RefreshLockState();
        if (MapManager.Instance != null)
            MapManager.Instance.OnMapUnlocked += HandleMapUnlocked;
    }

    private void OnDisable()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.OnMapUnlocked -= HandleMapUnlocked;
    }

    private void HandleMapUnlocked(string unlockedId)
    {
        if (unlockedId == mapId)
            RefreshLockState();
    }

    private void RefreshLockState()
    {
        bool unlocked = MapManager.Instance == null || MapManager.Instance.IsUnlocked(mapId);

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!unlocked);

        if (button != null)
            button.interactable = unlocked;
    }

    public void TeleportTo(string sceneName)
    {
        if (MapManager.Instance != null && !MapManager.Instance.IsUnlocked(sceneName))
        {
            Debug.Log($"{sceneName} is locked - complete the required quest first.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}