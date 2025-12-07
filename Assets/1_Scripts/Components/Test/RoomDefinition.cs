using UnityEngine;

[CreateAssetMenu(menuName = "ProcGen/RoomDefinition")]
public class RoomDefinition : ScriptableObject
{
    [Header("원본 방 프리팹")]
    public GameObject Prefab;

    [Header("JSON / 맵 메타데이터")]
    public string Id;          // 예: "roomLobby", "room_TypeA1"
    public string DisplayName; // 예: "로비", "1번 방"
    public string Description; // 방 내부 설명
}
