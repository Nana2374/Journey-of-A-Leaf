using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BiomeTeleporter : MonoBehaviour
{
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