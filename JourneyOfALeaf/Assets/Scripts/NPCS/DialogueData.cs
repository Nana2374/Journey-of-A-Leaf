using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DATA ASSET. Create one per conversation via:
// Project window -> Create -> Ant Game -> Dialogue Data
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Ant Game/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class Line
    {
        public string speakerName;
        [TextArea(2, 4)] public string text;
    }

    public Line[] lines;
}
