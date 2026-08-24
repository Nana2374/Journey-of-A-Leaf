using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBoardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject boardPanel;      // the whole board, hidden by default
    [SerializeField] private Transform entryContainer;   // parent with a Layout Group for the list
    [SerializeField] private QuestBoardEntryUI entryPrefab;
    [SerializeField] private GameObject trackerPanel;    // optional: the small HUD tracker panel, hidden while board is open

    private void Start()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnActiveQuestsChanged += RefreshList;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnActiveQuestsChanged -= RefreshList;
    }

    // Hook this to your quest board button's OnClick
    public void ToggleBoard()
    {
        bool willShow = !boardPanel.activeSelf;
        boardPanel.SetActive(willShow);

        // Hide the small HUD tracker while the full board is open, to avoid overlap
        if (trackerPanel != null)
            trackerPanel.SetActive(!willShow);

        if (willShow) RefreshList();
    }

    private void RefreshList()
    {
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

        if (trackerPanel != null)
            trackerPanel.SetActive(true);
    }
}