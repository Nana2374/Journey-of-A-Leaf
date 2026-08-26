using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private Action onComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    // Called by NPCInteraction when the player taps "Click to interact"
    public void StartDialogue(DialogueData dialogue, Action onComplete)
    {
        if (dialogue == null || dialogue.lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        this.onComplete = onComplete;

        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    // Hook this to a full-panel "tap to continue" button
    public void AdvanceDialogue()
    {
        if (currentDialogue == null) return;

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void ShowCurrentLine()
    {
        DialogueData.Line line = currentDialogue.lines[currentLineIndex];
        speakerNameText.text = line.speakerName;
        dialogueText.text = line.text;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        Action callback = onComplete;
        currentDialogue = null;
        onComplete = null;

        callback?.Invoke();
    }
}