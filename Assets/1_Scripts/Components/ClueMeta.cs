using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ClueData
{
    public string id;
    public string name;
    public string description;
}

[System.Serializable]
public class ClueListWrapper
{
    public List<ClueData> clues = new List<ClueData>();
}

[DisallowMultipleComponent]
[AddComponentMenu("MysteryGame/Clue Meta")]
public class ClueMeta : MonoBehaviour
{
    [Header("Clue Metadata")]
    public string clueId;        // 비워두면 GameObject 이름 사용
    public string clueName;      // 비워두면 GameObject 이름 사용

    [TextArea(2, 5)]
    public string description;

    public ClueData ToData()
    {
        return new ClueData
        {
            id = string.IsNullOrEmpty(clueId) ? gameObject.name : clueId,
            name = string.IsNullOrEmpty(clueName) ? gameObject.name : clueName,
            description = description ?? string.Empty
        };
    }
}
