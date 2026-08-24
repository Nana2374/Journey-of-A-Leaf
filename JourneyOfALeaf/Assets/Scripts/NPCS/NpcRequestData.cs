using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A single ask within an NPC's request chain (e.g. "3 Berries")
[System.Serializable]
public class ItemRequestStep
{
    [Header("What's needed")]
    public ItemData requiredItem;
    public int quantityNeeded = 1;

    [Header("Dialogue / Feedback")]
    [TextArea] public string acceptDialogue;   // shown when THIS step completes
    [TextArea] public string rejectDialogue;   // shown when wrong item given during this step

    [Header("Reward (optional, per step)")]
    public string unlocksMapId; // leave blank if only the FINAL step should unlock something
}

// DATA ASSET. Create one per NPC via:
// Project window -> Create -> Ant Game -> NPC Request
[CreateAssetMenu(fileName = "NewNPCRequest", menuName = "Ant Game/NPC Request")]
public class NPCRequestData : ScriptableObject
{
    [Header("Requests, in order")]
    [Tooltip("Drag elements up/down in this list to change the order the NPC asks for them.")]
    public List<ItemRequestStep> steps = new List<ItemRequestStep>();
}