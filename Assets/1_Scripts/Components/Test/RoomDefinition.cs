using UnityEngine;

[CreateAssetMenu(menuName = "ProcGen/RoomDefinition")]
public class RoomDefinition : ScriptableObject
{
    [Header("원본 방 프리팹")]
    public GameObject Prefab;

    [Header("회전 허용(Y축 0/90/180/270°)")]
    public bool AllowRotate90 = true;

    [Header("등장 확률 가중치 (기본 1)")]
    [Range(0f, 5f)] public float Weight = 1f;

    // 허브(거실) 선택 관련 메타(선택)
    [Header("허브 선택 메타(선택)")]
    public string[] Tags;          // 예: "Hub","Living","KitchenAdj"
    public float HubAffinity = 1f; // 허브로 쓰기 좋은 정도(가중치 보정)
}
