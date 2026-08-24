using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // Subscribe to these from your UI scripts
    public event Action OnActiveQuestsChanged;   // a quest was added or removed
    public event Action OnTrackedQuestChanged;   // the pinned/HUD quest changed
    public event Action OnQuestProgressUpdated;  // an item was delivered / step advanced

    private readonly List<NPCController> activeQuests = new List<NPCController>();
    public IReadOnlyList<NPCController> ActiveQuests => activeQuests;
    public NPCController TrackedQuest { get; private set; }

    private void Awake()
    {
        Debug.Log("QuestManager Awake() ran!");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Called by NPCController when it has requests to give
    public void RegisterQuest(NPCController npc)
    {
        if (activeQuests.Contains(npc)) return;

        activeQuests.Add(npc);
        OnActiveQuestsChanged?.Invoke();

        // Auto-track the first quest the player picks up if nothing else is tracked
        if (TrackedQuest == null)
            SetTrackedQuest(npc);
    }

    // Called by NPCController once all its steps are fulfilled
    public void UnregisterQuest(NPCController npc)
    {
        if (!activeQuests.Remove(npc)) return;

        OnActiveQuestsChanged?.Invoke();

        if (TrackedQuest == npc)
        {
            NPCController next = activeQuests.Count > 0 ? activeQuests[0] : null;
            SetTrackedQuest(next);
        }
    }

    // Called by the quest board when the player picks which quest to track
    public void SetTrackedQuest(NPCController npc)
    {
        if (npc != null && !activeQuests.Contains(npc)) return;

        TrackedQuest = npc;
        OnTrackedQuestChanged?.Invoke();
    }

    // Called by NPCController whenever progress changes but the tracked quest itself hasn't switched
    public void NotifyProgressUpdated()
    {
        OnQuestProgressUpdated?.Invoke();
    }
}