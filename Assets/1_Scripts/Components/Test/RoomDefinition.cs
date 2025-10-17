using UnityEngine;

[CreateAssetMenu(menuName = "ProcGen/RoomDefinition")]
public class RoomDefinition : ScriptableObject
{
    [Header("원본 방 프리팹")]
    public GameObject Prefab;

    [Header("회전 허용")]
    public bool AllowRotate90 = true;

    [Header("등장 확률 가중치 (기본 1)")]
    [Range(0f, 5f)] public float Weight = 1f;
}
