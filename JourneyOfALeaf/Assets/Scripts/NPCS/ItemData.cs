using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a DATA ASSET, not a component you attach to a GameObject.
// Right-click in your Project window -> Create -> Ant Game -> Item Data
// to make one asset per item type (e.g. "Berry_Data", "Twig_Data").
[CreateAssetMenu(fileName = "NewItem", menuName = "Ant Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;

    [Header("Visuals")]
    public GameObject prefab;  // The actual 3D item (with LeafItem attached)
    public Sprite icon;        // For 2D UI use: inventory icons, quest log, "Drop Berry" button, etc.

    [TextArea] public string description;
}