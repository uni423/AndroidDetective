using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("카탈로그")]
    public List<RoomDefinition> Catalog;

    [Header("생성 옵션")]
    [Min(1)] public int TargetRoomCount = 5;
    public bool GenerateOnStart = true;

    [Header("겹침 검사")]
    public LayerMask OverlapMask;       // 방 레이어(예: Rooms)
    public float BoundsPadding = 0.05f; // 여유값

    [Header("부모")]
    public Transform Root;              // 생성될 방 부모

    // 내부 상태
    private readonly List<GameObject> _placed = new();
    private readonly List<(GameObject room, DoorSocket socket)> _openSockets = new();

    private void Start()
    {
        if (GenerateOnStart) Generate();
    }

    [ContextMenu("Generate Now")]
    public void Generate()
    {
        ClearAll();

        var rng = new System.Random();

        // 1) 시작 방 선택/배치
        var startDef = RandomWeighted(Catalog, rng);
        var start = PlaceRoom(startDef, Vector3.zero, Quaternion.identity);
        RegisterOpenSockets(start);

        // 2) 프론티어 확장
        while (_placed.Count < TargetRoomCount && _openSockets.Count > 0)
        {
            int idx = rng.Next(_openSockets.Count);
            var (baseRoom, baseSock) = _openSockets[idx];

            // 후보 방(가중치 반영) 섞기
            var candidates = WeightedPool(Catalog, rng);
            bool success = false;

            foreach (var cand in candidates)
            {
                foreach (var rot in CandidateRotations(cand))
                {
                    // cand 프리팹의 DoorSocket들 읽기(임시 인스턴스)
                    using var temp = new TempRoomInstance(cand.Prefab, rot);
                    var candSockets = temp.Sockets;

                    foreach (var candSock in candSockets)
                    {
                        if (!baseSock.IsCompatibleWith(candSock)) continue;

                        // 정합 변환 계산( candSock 을 baseSock 에 맞추기 )
                        var (pos, rotWorld) = Align(candSock.transform, baseSock.transform);

                        // 실제 배치 후 겹침 확인
                        var inst = Instantiate(cand.Prefab, pos, rotWorld, Root);
                        if (!IsOverlapping(inst))
                        {
                            _placed.Add(inst);

                            // 연결된 양쪽 소켓 닫기
                            _openSockets.RemoveAt(idx);
                            CloseConnectedSocket(inst, candSock.name);

                            // 새 방의 나머지 소켓은 프론티어에 추가
                            RegisterOpenSockets(inst, exceptName: candSock.name);

                            success = true;
                            goto NextFrontier;
                        }
                        DestroyImmediate(inst);
                    }
                }
            }

            // 실패 시: 해당 프론티어 포기(간단 백오프)
            _openSockets.RemoveAt(idx);

        NextFrontier:
            if (success) continue;
        }

        // 필요 시: 남은 소켓 End-Cap 처리 (아래 선택 기능 참고)
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        foreach (var go in _placed)
            if (go) DestroyImmediate(go);
        _placed.Clear();
        _openSockets.Clear();

        if (!Root) Root = transform;
        var toDel = new List<GameObject>();
        foreach (Transform t in Root) toDel.Add(t.gameObject);
        foreach (var g in toDel) DestroyImmediate(g);
    }

    private GameObject PlaceRoom(RoomDefinition def, Vector3 pos, Quaternion rot)
    {
        if (!Root) Root = transform;
        var inst = Instantiate(def.Prefab, pos, rot, Root);
        _placed.Add(inst);
        return inst;
    }

    private void RegisterOpenSockets(GameObject room, string exceptName = null)
    {
        var sockets = room.GetComponentsInChildren<DoorSocket>(includeInactive: true);
        foreach (var s in sockets)
        {
            if (!string.IsNullOrEmpty(exceptName) && s.name == exceptName) continue;
            _openSockets.Add((room, s));
        }
    }

    private void CloseConnectedSocket(GameObject inst, string candSocketName)
    {
        var s = inst.GetComponentsInChildren<DoorSocket>(true)
                    .FirstOrDefault(x => x.name == candSocketName);
        // 필요 시 s에 "사용됨" 플래그를 두고 관리해도 됨.
    }

    private static (Vector3 pos, Quaternion rot) Align(Transform srcSocket, Transform dstSocket)
    {
        // 1) 소켓의 forward를 수평(XZ) 평면에 투영
        Vector3 srcF = srcSocket.forward; srcF.y = 0f;
        Vector3 dstF = -dstSocket.forward; dstF.y = 0f; // 서로 '정반대'가 되도록 -dst.forward
        if (srcF.sqrMagnitude < 1e-6f) srcF = Vector3.forward;
        if (dstF.sqrMagnitude < 1e-6f) dstF = Vector3.forward;
        srcF.Normalize(); dstF.Normalize();

        // 2) 수평에서의 '요(yaw)'만 맞추는 각도
        float deltaYaw = Vector3.SignedAngle(srcF, dstF, Vector3.up);
        Quaternion yawOnly = Quaternion.AngleAxis(deltaYaw, Vector3.up);

        // 3) 방 루트 기준으로 소켓 오프셋을 회전시켜 정렬
        Transform srcRoot = srcSocket.root;
        Vector3 localFromRootToSocket = srcSocket.position - srcRoot.position;

        // 방 전체의 최종 회전: 기존 루트 회전에 'yawOnly'만 더함 (Pitch/Roll 불변)
        Quaternion worldRot = yawOnly * srcRoot.rotation;

        // 방 전체의 최종 위치: 소켓이 정확히 dst 위치에 오도록 평행이동
        Vector3 worldPos = dstSocket.position - (yawOnly * localFromRootToSocket);

        return (worldPos, worldRot);
    }

    private Bounds ComputeProxyBounds(GameObject go, int procLayer)
    {
        // go 하위에서 "layer == procLayer" 이고, IsTrigger인 Collider만 취합
        var cols = go.GetComponentsInChildren<Collider>(true)
                     .Where(c => c != null && c.gameObject.layer == procLayer && c.isTrigger)
                     .ToArray();

        if (cols.Length == 0)
        {
            // 프록시 콜라이더가 없다면 경고 및 대체(렌더러 기준) — 개발 중 편의용
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) return default;

            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }

        Bounds bb = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++) bb.Encapsulate(cols[i].bounds);
        return bb;
    }

    private bool IsOverlapping(GameObject inst)
    {
        // OverlapMask 에 반드시 ProcBounds 레이어만 들어있다고 가정
        if (OverlapMask == 0)
        {
            Debug.LogWarning("[LevelGenerator] OverlapMask가 비어 있습니다. ProcBounds 레이어를 설정하세요.");
            return false;
        }

        // layerMask → 단일/복수 레이어를 모두 지원
        // 여기서는 "ProcBounds"만 들어있도록 권장
        // 첫 번째 세트의 비트를 실제 layer 번호로 취득(여러개여도 문제 없음)
        int procLayer = GetFirstLayerFromMask(OverlapMask);

        // 1) 프록시 콜라이더들의 합성 바운즈 계산
        Bounds b = ComputeProxyBounds(inst, procLayer);
        if (b.size == Vector3.zero) return false; // 프록시/렌더러 둘 다 없으면 스킵

        // 살짝 여유
        b.Expand(BoundsPadding);

        // 2) OverlapBox — ProcBounds 레이어만 검색
        var hits = Physics.OverlapBox(
            b.center, b.extents, inst.transform.rotation,
            OverlapMask, QueryTriggerInteraction.Collide // 트리거 포함
        );

        foreach (var h in hits)
        {
            // 자기 자신(inst) 하위는 무시
            if (h.transform.IsChildOf(inst.transform)) continue;

            // 내가 배치한 것들만 충돌로 인정 (Root 하위)
            if (h.transform.IsChildOf(Root)) return true;
        }

        return false;
    }

    private int GetFirstLayerFromMask(LayerMask mask)
    {
        int m = mask.value;
        for (int layer = 0; layer < 32; layer++)
            if ((m & (1 << layer)) != 0) return layer;
        return 0;
    }

    private IEnumerable<Quaternion> CandidateRotations(RoomDefinition def)
    {
        if (!def.AllowRotate90)
        {
            yield return Quaternion.identity; yield break;
        }
        yield return Quaternion.identity;
        yield return Quaternion.Euler(0, 90, 0);
        yield return Quaternion.Euler(0, 180, 0);
        yield return Quaternion.Euler(0, 270, 0);
    }

    private IEnumerable<RoomDefinition> WeightedPool(List<RoomDefinition> list, System.Random rng)
    {
        // 간단 가중치 섞기 (작을수록 앞에 오도록)
        return list.OrderBy(r => rng.NextDouble() / Mathf.Max(0.0001f, r.Weight));
    }

    private RoomDefinition RandomWeighted(List<RoomDefinition> list, System.Random rng)
    {
        float sum = list.Sum(x => x.Weight);
        double pick = rng.NextDouble() * sum;
        float acc = 0f;
        foreach (var r in list) { acc += r.Weight; if (pick <= acc) return r; }
        return list[^1];
    }

    // 임시 인스턴스를 using 블록으로 안전하게 파괴하기 위한 helper
    private readonly struct TempRoomInstance : System.IDisposable
    {
        public readonly GameObject Inst;
        public readonly List<DoorSocket> Sockets;

        public TempRoomInstance(GameObject prefab, Quaternion rot)
        {
            Inst = Object.Instantiate(prefab);
            Inst.transform.rotation = rot;
            Sockets = Inst.GetComponentsInChildren<DoorSocket>(true).ToList();
        }
        public void Dispose() { if (Inst) Object.DestroyImmediate(Inst); }
    }
}