using UnityEngine;

public enum DoorType { Standard, Wide, Stairs, Elevator }

[DisallowMultipleComponent]
public class DoorSocket : MonoBehaviour
{
    [Header("문 규격(옵션 A: 타입만 매칭)")]
    public DoorType Type = DoorType.Standard;

    [Tooltip("유효 개구 폭(x) × 높이(y), 1단위=1m 권장 (참고값)")]
    public Vector2 Size = new Vector2(2.0f, 2.2f);

    [Header("디버그")]
    public bool DrawGizmo = true;

    // 옵션 A: 방향 무시, 타입만 호환성 판단
    public bool IsCompatibleWith(DoorSocket other) => Type == other.Type;

    private void OnDrawGizmos()
    {
        if (!DrawGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(Size.x, Size.y, 0.1f));
        Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.8f);
        Gizmos.DrawWireSphere(Vector3.forward * 0.8f, 0.05f);
    }
}
