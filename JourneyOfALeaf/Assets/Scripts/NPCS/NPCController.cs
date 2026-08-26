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
    private bool hasMetNPC = false;        // has the player EVER talked to this NPC
    private bool currentStepOffered = false; // has the CURRENT step been formally offered/activated

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
        // First-ever meeting: play a one-time greeting, then offer the first request
        if (!hasMetNPC)
        {
            hasMetNPC = true;

            if (introDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(introDialogue, OfferCurrentStep);
            }
            else
            {
                OfferCurrentStep();
            }
            return;
        }

        // Already met, but the current step hasn't been offered yet
        // (e.g. player just finished the previous request and came back)
        if (!currentStepOffered)
        {
            OfferCurrentStep();
            return;
        }

        // Current step already active - just remind the player what's needed
        if (!IsFullyComplete)
            Debug.Log(DisplayName + ": " + GetObjectiveText());
    }

    // Plays this step's request dialogue (if any), then activates it as a tracked quest
    private void OfferCurrentStep()
    {
        ItemRequestStep step = CurrentStep;
        if (step == null) return; // no more requests from this NPC

        if (step.requestDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(step.requestDialogue, ActivateCurrentStep);
        }
        else
        {
            ActivateCurrentStep();
        }
    }

    private void ActivateCurrentStep()
    {
        currentStepOffered = true;
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
        if (step == null || !currentStepOffered) return false; // not offered yet, or all steps done
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
        currentStepOffered = false; // next request needs a fresh Interact() to activate

        // Remove from the tracker/board immediately - it'll reappear once re-offered
        QuestManager.Instance?.UnregisterQuest(this);

        if (IsFullyComplete)
        {
            Debug.Log(name + " has no more requests. Quest chain complete!");
            // QuestManager.Instance.CompleteQuestChain(this);
        }
        else
        {
            Debug.Log(name + " has another request ready - talk to them again!");
        }
    }
}