using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [SerializeField] private GameObject promptButtonObject;
    [SerializeField] private Button promptButton;

    // Tracks which NPC currently "owns" the visible prompt,
    // so one NPC walking away doesn't hide another NPC's prompt.
    private Action currentCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        promptButtonObject.SetActive(false);
    }

    public void Show(Action onInteract)
    {
        currentCallback = onInteract;
        promptButtonObject.SetActive(true);

        promptButton.onClick.RemoveAllListeners();
        promptButton.onClick.AddListener(() => currentCallback?.Invoke());
    }

    public void Hide()
    {
        currentCallback = null;
        promptButtonObject.SetActive(false);
    }

    // Only hides if THIS caller is the one currently showing the prompt
    public void HideIfOwnedBy(Action callback)
    {
        if (currentCallback == callback)
            Hide();
    }
}