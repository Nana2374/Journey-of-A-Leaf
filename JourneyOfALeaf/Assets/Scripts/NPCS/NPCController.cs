using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private NPCRequestData request;
    [SerializeField] private string npcDisplayName; // leave blank to use the GameObject name
    [SerializeField] private DialogueData introDialogue; // played the first time the player interacts

    [Header("Item Drop Settings")]
    [SerializeField] private float dropRadius = 1.5f;
    [SerializeField] private float disappearDelay = 0.75f;

    private int currentStepIndex = 0;
    private int itemsDeliveredThisStep = 0;
    private bool hasBeenTalkedTo = false;

    public string DisplayName => string.IsNullOrEmpty(npcDisplayName) ? name : npcDisplayName;
    public string QuestTitle => request != null ? request.questTitle : "";

    public bool IsFullyComplete =>
        request == null || currentStepIndex >= request.steps.Count;

    private ItemRequestStep CurrentStep =>
        (request != null && currentStepIndex < request.steps.Count)
            ? request.steps[currentStepIndex]
            : null;

    // ==========================================
    // Called by AntPickupController (tap on NPC) OR NPCInteraction (proximity button)
    // ==========================================
    public void Interact()
    {
        if (!hasBeenTalkedTo)
        {
            hasBeenTalkedTo = true;

            if (introDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(introDialogue, OnIntroDialogueFinished);
            }
            else
            {
                OnIntroDialogueFinished();
            }
        }
        else
        {
            // Already met - repeat current objective as a reminder, if the quest is still active
            if (!IsFullyComplete)
                Debug.Log(DisplayName + ": " + GetObjectiveText());
        }
    }

    private void OnIntroDialogueFinished()
    {
        if (request == null || request.steps.Count == 0)
            return; // this NPC has nothing to offer

        QuestManager.Instance?.RegisterQuest(this);
    }

    // Human-readable line for the HUD tracker / quest board, e.g. "Bring 2/3 Berries to Meghill"
    public string GetObjectiveText()
    {
        ItemRequestStep step = CurrentStep;
        if (step == null) return "All requests fulfilled!";

        string itemName = step.requiredItem != null ? step.requiredItem.itemName : "???";
        return $"Bring {itemsDeliveredThisStep}/{step.quantityNeeded} {itemName} to {DisplayName}";
    }

    // ==========================================
    // Called by AntPickupController before giving
    // ==========================================
    public bool CanAccept(LeafItem item)
    {
        ItemRequestStep step = CurrentStep;
        if (step == null) return false; // no active step / all steps done
        return item.Data == step.requiredItem;
    }

    // ==========================================
    // Called by AntPickupController when handing over
    // ==========================================
    public void ReceiveItem(LeafItem item)
    {
        ItemRequestStep step = CurrentStep;
        if (step == null) return;

        item.RemoveFromLeaf();

        Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
        Vector3 dropPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        item.transform.position = dropPosition;

        Destroy(item.gameObject, disappearDelay);

        itemsDeliveredThisStep++;

        if (itemsDeliveredThisStep >= step.quantityNeeded)
        {
            Debug.Log(step.acceptDialogue);

            if (!string.IsNullOrEmpty(step.unlocksMapId))
            {
                // MapManager.Instance.Unlock(step.unlocksMapId);
            }

            AdvanceToNextStep();
        }
        else
        {
            Debug.Log($"{itemsDeliveredThisStep}/{step.quantityNeeded} delivered to {name}.");
            QuestManager.Instance?.NotifyProgressUpdated();
        }
    }

    private void AdvanceToNextStep()
    {
        currentStepIndex++;
        itemsDeliveredThisStep = 0;

        if (IsFullyComplete)
        {
            Debug.Log(name + " has no more requests. Quest chain complete!");
            // QuestManager.Instance.CompleteQuestChain(this);
            QuestManager.Instance?.UnregisterQuest(this);
        }
        else
        {
            Debug.Log(name + " now wants: " + CurrentStep.requiredItem.itemName);
            QuestManager.Instance?.NotifyProgressUpdated();
        }
    }
}