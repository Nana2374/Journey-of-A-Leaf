using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        GameObject spawnPoint = GameObject.Find("SpawnPoint");

        if (spawnPoint != null)
        {
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;

            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("No SpawnPoint found in this scene!");
        }
    }
}