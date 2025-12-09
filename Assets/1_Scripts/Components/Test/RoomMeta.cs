using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ProcGen/Room Meta")]
public class RoomMeta : MonoBehaviour
{
    [Header("JSON / 맵 메타데이터")]
    public string Id;
    public string DisplayName;
    
    [TextArea]
    public string Description;

    [Header("Clue Spawn Points")]
    public Transform[] floorSpawnPoints; // 바닥 단서용
    public Transform[] wallSpawnPoints;  // 벽 단서용
}
