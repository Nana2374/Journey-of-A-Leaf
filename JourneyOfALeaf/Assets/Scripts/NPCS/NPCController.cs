using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private NPCRequestData request;

    [Header("Item Drop Settings")]
    [SerializeField] private float dropRadius = 1.5f;
    [SerializeField] private float disappearDelay = 0.75f;

    private int currentStepIndex = 0;
    private int itemsDeliveredThisStep = 0;

    public bool IsFullyComplete =>
        request == null || currentStepIndex >= request.steps.Count;

    private ItemRequestStep CurrentStep =>
        (request != null && currentStepIndex < request.steps.Count)
            ? request.steps[currentStepIndex]
            : null;

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
        }
        else
        {
            Debug.Log(name + " now wants: " + CurrentStep.requiredItem.itemName);
        }
    }
}