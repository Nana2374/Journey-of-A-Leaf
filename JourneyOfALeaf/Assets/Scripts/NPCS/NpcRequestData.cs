using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Another DATA ASSET. Create one per NPC via:
// Project window -> Create -> Ant Game -> NPC Request
[CreateAssetMenu(fileName = "NewNPCRequest", menuName = "Ant Game/NPC Request")]
public class NPCRequestData : ScriptableObject
{
    [Header("What this NPC wants")]
    public ItemData requiredItem;
    public int quantityNeeded = 1;

    [Header("Dialogue / Feedback")]
    [TextArea] public string acceptDialogue;
    [TextArea] public string rejectDialogue;

    [Header("Reward")]
    public string unlocksMapId; // swap for a MapData/QuestData reference later
}
