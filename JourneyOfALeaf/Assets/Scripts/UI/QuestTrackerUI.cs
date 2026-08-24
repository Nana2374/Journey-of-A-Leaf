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

        Debug.Log("QuestTrackerUI.Refresh() called. Tracked quest = " +
                  (tracked != null ? tracked.name : "NULL"));

        if (tracked == null)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);
        objectiveText.text = tracked.GetObjectiveText();
    }
}