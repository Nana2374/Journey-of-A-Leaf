using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Always unlocked from the start (e.g. starting area, hub)")]
    [SerializeField] private List<string> defaultUnlockedMapIds = new List<string>();

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
        DontDestroyOnLoad(gameObject);

        foreach (string id in defaultUnlockedMapIds)
        {
            if (!string.IsNullOrEmpty(id))
                unlockedMapIds.Add(id);
        }
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
        return string.IsNullOrEmpty(mapId) || unlockedMapIds.Contains(mapId);
    }
}