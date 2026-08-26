using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBoardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject boardPanel;      // the whole board, hidden by default
    [SerializeField] private Transform entryContainer;   // parent with a Layout Group for the list
    [SerializeField] private QuestBoardEntryUI entryPrefab;
    [SerializeField] private QuestTrackerUI trackerUI;   // optional: the small HUD tracker, hidden while board is open

    private void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnActiveQuestsChanged += RefreshList;
            QuestManager.Instance.OnQuestProgressUpdated += RefreshList;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnActiveQuestsChanged -= RefreshList;
            QuestManager.Instance.OnQuestProgressUpdated -= RefreshList;
        }
    }

    // Hook this to your quest board button's OnClick
    public void ToggleBoard()
    {
        bool willShow = !boardPanel.activeSelf;
        boardPanel.SetActive(willShow);

        if (trackerUI != null)
        {
            if (willShow)
            {
                // Board is opening - force the tracker's panel hidden (controller stays active/subscribed)
                trackerUI.ForceHide();
            }
            else
            {
                // Board is closing - let the tracker decide for itself whether it has anything to show
                trackerUI.RefreshVisibility();
            }
        }

        if (willShow) RefreshList();
    }

    private void RefreshList()
    {
        if (!boardPanel.activeSelf) return; // no need to rebuild while nobody's looking

        // Clear old entries
        foreach (Transform child in entryContainer)
            Destroy(child.gameObject);

        if (QuestManager.Instance == null) return;

        foreach (NPCController quest in QuestManager.Instance.ActiveQuests)
        {
            QuestBoardEntryUI entry = Instantiate(entryPrefab, entryContainer);
            entry.Setup(quest, OnQuestSelected);
        }
    }

    private void OnQuestSelected(NPCController quest)
    {
        QuestManager.Instance.SetTrackedQuest(quest);
        boardPanel.SetActive(false); // remove this line if the board should stay open after picking

        if (trackerUI != null)
            trackerUI.RefreshVisibility();
    }
}