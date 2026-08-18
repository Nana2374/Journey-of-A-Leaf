using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BiomeTeleporter : MonoBehaviour
{
    public void TeleportTo(string sceneName)
    {
        Time.timeScale = 1f; // make sure game isn't still paused from the map
        SceneManager.LoadScene(sceneName);
    }
}