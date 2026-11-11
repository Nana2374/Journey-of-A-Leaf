using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Quest Item Highlighting")]
    [Tooltip("Words that will automatically be shown in bold red")]
    [SerializeField] private string[] questKeywords = { "berry", "flower", "fish" };
    [SerializeField] private string highlightColor = "#FF0000";

    [Header("Hide while dialogue is active")]
    [Tooltip("e.g. movement buttons, interact prompt - anything that shouldn't be usable mid-conversation")]
    [SerializeField] private GameObject[] hudToHideDuringDialogue;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private Action onComplete;
    private Regex highlightRegex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        dialoguePanel.SetActive(false);
        BuildHighlightRegex();
    }

    // Builds one regex that matches any keyword as a whole word, case-insensitive
    private void BuildHighlightRegex()
    {
        if (questKeywords == null || questKeywords.Length == 0)
        {
            highlightRegex = null;
            return;
        }

        List<string> escaped = new List<string>();
        foreach (string word in questKeywords)
        {
            if (!string.IsNullOrWhiteSpace(word))
                escaped.Add(Regex.Escape(word.Trim()));
        }

        if (escaped.Count == 0)
        {
            highlightRegex = null;
            return;
        }

        // \b = word boundary, so "fish" won't match inside "selfish"
        // s? lets plurals match too: berry -> berries needs its own entry, but fish -> fishs is harmless
        string pattern = @"\b(" + string.Join("|", escaped) + @")\b";
        highlightRegex = new Regex(pattern, RegexOptions.IgnoreCase);
    }

    private string HighlightKeywords(string text)
    {
        if (highlightRegex == null || string.IsNullOrEmpty(text)) return text;

        return highlightRegex.Replace(text,
            $"<b><color={highlightColor}>$1</color></b>");
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
        SetHudVisible(false);
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
        dialogueText.text = HighlightKeywords(line.text);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        SetHudVisible(true);

        Action callback = onComplete;
        currentDialogue = null;
        onComplete = null;

        callback?.Invoke();
    }

    private void SetHudVisible(bool visible)
    {
        if (hudToHideDuringDialogue == null) return;

        foreach (GameObject hud in hudToHideDuringDialogue)
        {
            if (hud != null)
                hud.SetActive(visible);
        }
    }
}