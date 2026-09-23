using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private readonly HashSet<string> unlockedMapIds = new HashSet<string>();

    public event Action<string> OnMapUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Unlock(string mapId)
    {
        if (string.IsNullOrEmpty(mapId)) return;

        if (unlockedMapIds.Add(mapId))
        {
            Debug.Log($"Map unlocked: {mapId}");
            OnMapUnlocked?.Invoke(mapId);
        }
    }

    public bool IsUnlocked(string mapId)
    {
        // blank ID = always unlocked (for biomes you don't want gated at all)
        return string.IsNullOrEmpty(mapId) || unlockedMapIds.Contains(mapId);
    }
}