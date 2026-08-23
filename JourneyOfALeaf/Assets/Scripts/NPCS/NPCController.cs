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

    private int itemsDelivered = 0;

    // ==========================================
    // Called by AntPickupController before giving
    // ==========================================
    public bool CanAccept(LeafItem item)
    {
        if (request == null) return false;
        if (itemsDelivered >= request.quantityNeeded) return false;
        return item.Data == request.requiredItem;
    }

    // ==========================================
    // Called by AntPickupController when handing over
    // ==========================================
    public void ReceiveItem(LeafItem item)
    {
        item.RemoveFromLeaf();

        Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
        Vector3 dropPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        item.transform.position = dropPosition;

        Destroy(item.gameObject, disappearDelay);

        itemsDelivered++;

        if (itemsDelivered >= request.quantityNeeded)
        {
            Debug.Log(request.acceptDialogue);
            // MapManager.Instance.Unlock(request.unlocksMapId);
            // QuestManager.Instance.CompleteQuest(this);
        }
        else
        {
            Debug.Log($"{itemsDelivered}/{request.quantityNeeded} delivered to {name}.");
        }
    }
}
