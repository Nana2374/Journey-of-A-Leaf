using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectsDatabaseSO : ScriptableObject
{
    public List<ObjectData> objectsData;
}

[Serializable]
public class ObjectData
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;
    //Vector2Int.one means the smallest item is 1x1 sized, can change based on size of furniture later

    [field: SerializeField]
    public GameObject Prefab { get; private set; }
    //Ref to prefabs in the assets folder: Add all furniture into prefabs
    //Make sure all furniture have pivot point set to bottom left corner since grid is set to that

}
