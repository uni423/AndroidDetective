using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("카탈로그(붙일 방들)")]
    public List<RoomDefinition> Catalog;

    [Header("일반 생성 옵션")]
    [Min(1)] public int TargetRoomCount = 5;
    public bool GenerateOnStart = false; // 허브 전용을 쓰는 경우 보통 끔

    [Header("겹침 검사 (옵션 B: ProcBounds 전용)")]
    public LayerMask OverlapMask;       // 반드시 ProcBounds 레이어만 포함
    public float BoundsPadding = 0.05f; // 여유값
    public Transform Root;              // 생성 부모(없으면 자기 자신)

    // 허브(거실) 생성 옵션
    [Header("허브(거실) 후보 설정")]
    public List<RoomDefinition> HubCandidates; // 비우면 Catalog에서 Tag="Hub" 자동 추출
    public int RequiredHubDoorCount = 0;       // 허브 최소 문 개수(예: 4 또는 5)
    public DoorType[] AllowedHubDoorTypes;     // 비우면 모든 타입 허용

    [Header("허브 연결 옵션")]
    public bool HubOnlyStar = true;            // true: 허브에 붙인 방에서 더 확장 X
    public int MaxTriesPerSocket = 20;         // 각 소켓마다 시도 제한

    // 내부 상태
    private readonly List<GameObject> _placed = new();

    private readonly Dictionary<GameObject, RoomDefinition> _roomDefs = new();
    private readonly Dictionary<GameObject, HashSet<GameObject>> _roomConnections = new();

    private void Start()
    {
        if (GenerateOnStart) GenerateHubAuto(); // 일반 프론티어 모드(원하면 사용)
    }

    // ==========================
    // 일반 모드 (프론티어 확장) — 필요 시 사용
    // ==========================
    [ContextMenu("Generate (Frontier)")]
    public void Generate()
    {
        ClearAll();
        var rng = new System.Random();

        var startDef = RandomWeighted(Catalog, rng);
        var start = PlaceRoom(startDef, Vector3.zero, Quaternion.identity);
        var openSockets = new List<(GameObject room, DoorSocket socket)>();
        RegisterOpenSockets(start, openSockets);

        while (_placed.Count < TargetRoomCount && openSockets.Count > 0)
        {
            int idx = rng.Next(openSockets.Count);
            var (baseRoom, baseSock) = openSockets[idx];

            var candidates = WeightedPool(Catalog, rng);
            bool success = false;

            foreach (var cand in candidates)
            {
                foreach (var rot in CandidateRotations(cand))
                {
                    using var temp = new TempRoomInstance(cand.Prefab, rot);
                    var candSockets = temp.Sockets;
                    foreach (var candSock in candSockets)
                    {
                        if (!baseSock.IsCompatibleWith(candSock)) continue;

                        var (pos, rotWorld) = AlignYawOnly(candSock.transform, baseSock.transform);

                        var inst = Instantiate(cand.Prefab, pos, rotWorld, RootOrSelf());
                        if (!IsOverlapping(inst))
                        {
                            // 방 등록 + 연결 기록
                            RegisterRoomInstance(inst, cand);
                            RegisterConnection(baseRoom, inst);

                            // 연결된 양쪽 소켓 닫기
                            openSockets.RemoveAt(idx);
                            CloseConnectedSocket(inst, candSock.name);

                            // 새 방의 나머지 소켓을 프론티어에 추가
                            RegisterOpenSockets(inst, openSockets, exceptName: candSock.name);

                            success = true;
                            goto NextFrontier;
                        }
                        DestroyImmediate(inst);
                    }
                }
            }

            // 실패: 해당 프론티어 포기
            openSockets.RemoveAt(idx);

        NextFrontier:
            if (success) continue;
        }
    }

    // ==========================
    // 허브 모드
    // ==========================
    [ContextMenu("Generate (Hub Auto Pick)")]
    public void GenerateHubAuto()
    {
        bool isGenerateRoomSuccess = false;

        do
        {
            ClearAll();
            var rng = new System.Random();

            var hubDef = PickHubCandidate(rng);
            if (hubDef == null) return;

            isGenerateRoomSuccess = GenerateHubWith(hubDef, rng);
        }
        while (isGenerateRoomSuccess == false);
    }

    public bool GenerateHubWith(RoomDefinition hubDef, System.Random rng = null)
    {
        if (rng == null) rng = new System.Random();

        var hub = PlaceRoom(hubDef, Vector3.zero, Quaternion.identity);
        var hubSockets = hub.GetComponentsInChildren<DoorSocket>(true).ToList();

        foreach (var hubSock in hubSockets)
        {
            bool attached = TryAttachOneRoomToHubSocket(hubSock, rng);
            if (!attached)
            {
                // 연결 실패 발생 시 방 생성 로직 즉시 중단 
                return false;
            }
        }

        return true;
    }

    private RoomDefinition PickHubCandidate(System.Random rng)
    {
        // 1) 허브 후보 소스:
        //    - HubCandidates가 있으면 그 리스트만 사용
        //    - 없으면 Catalog 전체에서 허브로 쓸 수 있는 방을 고름
        IEnumerable<RoomDefinition> src =
            (HubCandidates != null && HubCandidates.Count > 0)
                ? HubCandidates
                : Catalog;

        // 2) 타입/문 개수 필터링
        var filtered = new List<(RoomDefinition def, int doorCount)>();
        foreach (var def in src)
        {
            using var temp = new TempRoomInstance(def.Prefab, Quaternion.identity);
            var socks = temp.Sockets;

            bool typesOk = true;
            if (AllowedHubDoorTypes != null && AllowedHubDoorTypes.Length > 0)
            {
                var allowed = new HashSet<DoorType>(AllowedHubDoorTypes);
                typesOk = socks.Any(s => allowed.Contains(s.Type));
            }
            if (!typesOk) continue;

            int doorCount = socks.Count;
            if (doorCount <= 0) continue; // 문이 하나도 없는 방은 허브로 쓰지 않음

            filtered.Add((def, doorCount));
        }

        if (filtered.Count == 0)
        {
            Debug.LogWarning("[LevelGenerator] 허브 후보가 없습니다. HubCandidates/Catalog/조건을 확인하세요.");
            return null;
        }

        // 3) 모든 허브가 "동일한 확률"로 선택되도록 균등 랜덤
        int idx = rng.Next(filtered.Count);
        var chosen = filtered[idx];

        // 선택된 허브의 문 개수를 RequiredHubDoorCount에 기록
        RequiredHubDoorCount = chosen.doorCount;

        return chosen.def;
    }

    private bool TryAttachOneRoomToHubSocket(DoorSocket hubSocket, System.Random rng)
    {
        // 허브 소켓 타입과 호환되는 후보 방
        var candidates = Catalog
            .Where(c => HasSocketOfType(c, hubSocket.Type))
            .OrderBy(_ => rng.NextDouble())   // HubAffinity / Weight 대신 단순 랜덤
            .ToList();

        int tries = 0;
        foreach (var cand in candidates)
        {
            foreach (var rot in CandidateRotations(cand))
            {
                using var temp = new TempRoomInstance(cand.Prefab, rot);
                var candSockets = temp.Sockets.Where(s => s.Type == hubSocket.Type).ToList();
                ShuffleInPlace(candSockets, rng);

                foreach (var candSock in candSockets)
                {
                    var (pos, rotWorld) = AlignYawOnly(candSock.transform, hubSocket.transform);

                    var inst = Instantiate(cand.Prefab, pos, rotWorld, RootOrSelf());
                    if (!IsOverlapping(inst))
                    {
                        // 방 등록
                        RegisterRoomInstance(inst, cand);

                        // 허브 방(GameObject) 찾기
                        var hubRoom = FindRoomRootForSocket(hubSocket);
                        if (hubRoom != null)
                        {
                            RegisterConnection(hubRoom, inst);
                        }
                        else
                        {
                            Debug.LogWarning("[LevelGenerator] 허브 소켓 상위에서 배치된 방을 찾지 못했습니다.");
                        }

                        // 연결된 소켓 닫기
                        CloseConnectedSocket(inst, candSock.name);

                        if (!HubOnlyStar)
                        {
                            // 얕은 확장 모드: 필요하면 여기서 RegisterOpenSockets 후 프론티어로 한 단계 더 붙이는 로직 추가 가능
                        }
                        else
                        {
                            // 스타형: 자식 방의 다른 소켓은 End-Cap 처리하거나 무시(현재는 무시)
                        }
                        return true;
                    }
                    DestroyImmediate(inst);

                    if (++tries >= MaxTriesPerSocket) break;
                }
                if (tries >= MaxTriesPerSocket) break;
            }
            if (tries >= MaxTriesPerSocket) break;
        }

        // 실패
        return false;
    }

    // ==========================
    // 공통 유틸
    // ==========================
    private Transform RootOrSelf()
    {
        if (!Root) Root = transform;
        return Root;
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        //Debug.Log("Clear All Start");
        foreach (var go in _placed) if (go) DestroyImmediate(go);
        _placed.Clear();
        _roomDefs.Clear();
        _roomConnections.Clear();

        if (!Root) Root = transform;
        var toDel = new List<GameObject>();
        foreach (Transform t in Root) toDel.Add(t.gameObject);
        foreach (var g in toDel) DestroyImmediate(g);
    }

    private GameObject PlaceRoom(RoomDefinition def, Vector3 pos, Quaternion rot)
    {
        var inst = Instantiate(def.Prefab, pos, rot, RootOrSelf());
        RegisterRoomInstance(inst, def);
        return inst;
    }

    private void RegisterOpenSockets(GameObject room, List<(GameObject room, DoorSocket socket)> list, string exceptName = null)
    {
        var sockets = room.GetComponentsInChildren<DoorSocket>(true);
        foreach (var s in sockets)
        {
            if (!string.IsNullOrEmpty(exceptName) && s.name == exceptName) continue;
            list.Add((room, s));
        }
    }

    private void CloseConnectedSocket(GameObject inst, string candSocketName)
    {
        // 필요 시 "사용됨" 플래그 등을 찍어둘 수 있음. 현재는 이름 매칭만 수행.
        _ = inst.GetComponentsInChildren<DoorSocket>(true)
                .FirstOrDefault(x => x.name == candSocketName);
    }

    private IEnumerable<Quaternion> CandidateRotations(RoomDefinition def)
    {
        yield return Quaternion.identity;
        yield return Quaternion.Euler(0, 90, 0);
        yield return Quaternion.Euler(0, 180, 0);
        yield return Quaternion.Euler(0, 270, 0);
    }

    private IEnumerable<RoomDefinition> WeightedPool(List<RoomDefinition> list, System.Random rng)
    {
        return list.OrderBy(_ => rng.NextDouble());
    }

    private RoomDefinition RandomWeighted(List<RoomDefinition> list, System.Random rng)
    {
        if (list == null || list.Count == 0) return null;
        int index = rng.Next(list.Count);
        return list[index];
    }

    private static void ShuffleInPlace<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private bool HasSocketOfType(RoomDefinition def, DoorType t)
    {
        using var temp = new TempRoomInstance(def.Prefab, Quaternion.identity);
        return temp.Sockets.Any(s => s.Type == t);
    }

    // Y축(요)만 회전 정합
    private static (Vector3 pos, Quaternion rot) AlignYawOnly(Transform srcSocket, Transform dstSocket)
    {
        Vector3 srcF = srcSocket.forward; srcF.y = 0f;
        Vector3 dstF = -dstSocket.forward; dstF.y = 0f; // 반대 방향
        if (srcF.sqrMagnitude < 1e-6f) srcF = Vector3.forward;
        if (dstF.sqrMagnitude < 1e-6f) dstF = Vector3.forward;
        srcF.Normalize(); dstF.Normalize();

        float deltaYaw = Vector3.SignedAngle(srcF, dstF, Vector3.up);
        Quaternion yawOnly = Quaternion.AngleAxis(deltaYaw, Vector3.up);

        Transform srcRoot = srcSocket.root;
        Vector3 localFromRootToSocket = srcSocket.position - srcRoot.position;

        Quaternion worldRot = yawOnly * srcRoot.rotation;
        Vector3 worldPos = dstSocket.position - (yawOnly * localFromRootToSocket);
        return (worldPos, worldRot);
    }

    // ======= 옵션 B: ProcBounds 프록시 기반 겹침 검사 =======
    private Bounds ComputeProxyBounds(GameObject go, int procLayer)
    {
        var cols = go.GetComponentsInChildren<Collider>(true)
                     .Where(c => c != null && c.gameObject.layer == procLayer && c.isTrigger)
                     .ToArray();

        if (cols.Length == 0)
        {
            // 프록시가 없으면 렌더러 합성 Bounds로 폴백(경고)
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
        if (OverlapMask == 0)
        {
            Debug.LogWarning("[LevelGenerator] OverlapMask 비어있음. ProcBounds 레이어를 설정하세요.");
            return false;
        }

        int procLayer = GetFirstLayerFromMask(OverlapMask);

        Bounds b = ComputeProxyBounds(inst, procLayer);
        if (b.size == Vector3.zero) return false;

        b.Expand(BoundsPadding);

        var hits = Physics.OverlapBox(
            b.center, b.extents, inst.transform.rotation,
            OverlapMask, QueryTriggerInteraction.Collide // 트리거 포함
        );

        foreach (var h in hits)
        {
            if (h.transform.IsChildOf(inst.transform)) continue; // 자기 자신 제외
            if (h.transform.IsChildOf(RootOrSelf())) return true; // 내가 배치한 것들과만 충돌로 간주
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

    private void RegisterRoomInstance(GameObject inst, RoomDefinition def = null)
    {
        _placed.Add(inst);

        if (def != null)
        {
            _roomDefs[inst] = def;
        }

        if (!_roomConnections.ContainsKey(inst))
        {
            _roomConnections[inst] = new HashSet<GameObject>();
        }
    }

    private void RegisterConnection(GameObject a, GameObject b)
    {
        if (a == null || b == null || a == b) return;

        if (!_roomConnections.TryGetValue(a, out var listA))
        {
            listA = new HashSet<GameObject>();
            _roomConnections[a] = listA;
        }
        if (!_roomConnections.TryGetValue(b, out var listB))
        {
            listB = new HashSet<GameObject>();
            _roomConnections[b] = listB;
        }

        listA.Add(b);
        listB.Add(a);
    }


    // 임시 인스턴스를 using으로 자동 파괴
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

    private GameObject FindRoomRootForSocket(DoorSocket socket)
    {
        Transform t = socket.transform;
        while (t != null)
        {
            var go = t.gameObject;
            if (_roomConnections.ContainsKey(go))
            {
                // RegisterRoomInstance 로 등록된 방 인스턴스를 찾으면 반환
                return go;
            }

            t = t.parent;
        }
        return null;
    }

    /// <summary>
    /// 현재 배치된 방 정보를 JSON 문자열로 반환
    /// </summary>
    public MapExport ExportMapJson()
    {
        var export = new MapExport
        {
            map = new List<MapRoom>()
        };

        foreach (var roomGo in _placed)
        {
            if (roomGo == null) continue;

            if (!_roomDefs.TryGetValue(roomGo, out var def) || def == null)
                continue;

            var room = new MapRoom
            {
                id = !string.IsNullOrEmpty(def.Id) ? def.Id : roomGo.name,
                name = !string.IsNullOrEmpty(def.DisplayName) ? def.DisplayName : def.name,
                description = def.Description ?? string.Empty,
                connection = new List<MapRoomConnection>()
            };

            if (_roomConnections.TryGetValue(roomGo, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null) continue;
                    if (!_roomDefs.TryGetValue(neighbor, out var nDef) || nDef == null)
                        continue;

                    room.connection.Add(new MapRoomConnection
                    {
                        id = !string.IsNullOrEmpty(nDef.Id) ? nDef.Id : neighbor.name,
                        name = !string.IsNullOrEmpty(nDef.DisplayName) ? nDef.DisplayName : nDef.name,
                        description = nDef.Description ?? string.Empty
                    });
                }
            }

            export.map.Add(room);
        }

        // Unity 기본 JsonUtility 사용
        return export;
    }

}