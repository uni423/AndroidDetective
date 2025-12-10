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

    [Header("NPC Spawn Points")]
    [Tooltip("이 방에 배치될 수 있는 NPC들의 스폰 위치들")]
    public Transform[] npcSpawnPoints;
}
