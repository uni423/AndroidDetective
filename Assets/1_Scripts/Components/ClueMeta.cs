using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ClueData
{
    public string id;
    public string name;
    public string description;
    public string location;
}
public enum ClueSpawnType { Floor, Wall }

[DisallowMultipleComponent]
[AddComponentMenu("MysteryGame/Clue Meta")]
public class ClueMeta : MonoBehaviour
{
    [Header("Clue Metadata")]
    public string clueId;
    public string clueName;

    [TextArea(2, 5)]
    public string description;

    [Header("Spawn Type")]
    public ClueSpawnType spawnType = ClueSpawnType.Floor;

    public string location;

    public bool isFind = false;

    public ClueData ToData()
    {
        return new ClueData
        {
            id = string.IsNullOrEmpty(clueId) ? gameObject.name : clueId,
            name = string.IsNullOrEmpty(clueName) ? gameObject.name : clueName,
            description = description ?? string.Empty,
            location = location ?? string.Empty
        };
    }


}
