using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestBoardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private Button selectButton;

    public void Setup(NPCController quest, Action<NPCController> onSelected)
    {
        titleText.text = quest.QuestTitle;
        objectiveText.text = quest.GetObjectiveText();

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelected?.Invoke(quest));
    }
}