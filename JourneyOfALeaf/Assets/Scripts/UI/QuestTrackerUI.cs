using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text objectiveText;

    private void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnTrackedQuestChanged += Refresh;
            QuestManager.Instance.OnQuestProgressUpdated += Refresh;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnTrackedQuestChanged -= Refresh;
            QuestManager.Instance.OnQuestProgressUpdated -= Refresh;
        }
    }

    private void Refresh()
    {
        NPCController tracked = QuestManager.Instance != null ? QuestManager.Instance.TrackedQuest : null;

        if (tracked == null)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);
        objectiveText.text = tracked.GetObjectiveText();
    }

    // Called by other UI (e.g. QuestBoardUI) to re-check visibility
    // without assuming whether a quest is actually tracked.
    public void RefreshVisibility()
    {
        Refresh();
    }

    // Called by other UI to forcibly hide the panel (e.g. while the quest board is open),
    // regardless of whether a quest is currently tracked.
    // This hides the PANEL only - this script's own GameObject stays active/subscribed.
    public void ForceHide()
    {
        panel.SetActive(false);
    }
}