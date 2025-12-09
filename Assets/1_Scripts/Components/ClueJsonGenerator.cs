using System.Collections.Generic;
using UnityEngine;

public class ClueJsonGenerator : MonoBehaviour
{
    [Header("랜덤으로 뽑을 단서 프리팹 리스트")]
    public List<GameObject> cluePrefabs = new List<GameObject>();

    public IReadOnlyList<GameObject> SpawnedClues => _spawnedClues;

    private readonly List<GameObject> _spawnedClues = new List<GameObject>();
    private readonly HashSet<Transform> _usedSpawnPoints = new HashSet<Transform>();

    public List<ClueData> ChoiceRandomClues(List<RoomMeta> placedRooms)
    {
        ClearSpawnedClues();

        List<ClueData> result = new List<ClueData>();

        if (cluePrefabs == null || cluePrefabs.Count == 0)
        {
            Debug.LogWarning("[ClueJsonGenerator] 등록된 단서 프리팹이 없습니다.");
            return result;
        }

        if (placedRooms == null || placedRooms.Count == 0)
        {
            Debug.LogWarning("[ClueJsonGenerator] 배치된 RoomMeta 리스트가 비어있습니다.");
            return result;
        }

        int maxCount = Mathf.Min(cluePrefabs.Count, 10);
        int minCount = Mathf.Min(maxCount, 8);   // 프리팹이 8개 미만이면 있는 만큼
        int targetCount = Random.Range(minCount, maxCount + 1); // [min, max] 포함


        HashSet<int> usedPrefabIndices = new HashSet<int>();

        while (result.Count < targetCount && usedPrefabIndices.Count < cluePrefabs.Count)
        {
            int prefabIndex = Random.Range(0, cluePrefabs.Count);
            if (!usedPrefabIndices.Add(prefabIndex))
                continue; // 같은 프리팹 중복 방지

            GameObject prefab = cluePrefabs[prefabIndex];
            if (prefab == null)
            {
                Debug.LogWarning("[ClueJsonGenerator] 비어 있는 프리팹 슬롯이 있습니다.");
                continue;
            }

            ClueMeta prefabMeta = prefab.GetComponent<ClueMeta>();
            if (prefabMeta == null)
            {
                Debug.LogWarning($"[ClueJsonGenerator] {prefab.name} 에 ClueMeta 컴포넌트가 없습니다.");
                continue;
            }

            // 1) 타입(Floor/Wall)에 맞는 SpawnPoint 찾기
            var spawnInfo = FindRandomSpawnPointForType(placedRooms, prefabMeta.spawnType);
            if (spawnInfo.point == null || spawnInfo.roomMeta == null)
            {
                Debug.LogWarning($"[ClueJsonGenerator] {prefab.name} 에 대한 {prefabMeta.spawnType} 타입 스폰 포인트가 없습니다.");
                continue;
            }

            // 2) 실제 단서 프리팹 Instantiate
            GameObject inst = Instantiate(prefab, spawnInfo.point.position, spawnInfo.point.rotation);
            inst.transform.SetParent(spawnInfo.point, true); // SpawnPoint 밑으로 붙이기

            ClueMeta instMeta = inst.GetComponent<ClueMeta>();
            if (instMeta == null)
            {
                Debug.LogWarning("[ClueJsonGenerator] 생성된 단서 오브젝트에 ClueMeta가 없습니다.");
                Destroy(inst);
                continue;
            }

            // 3) JSON location = 방 ID 설정
            instMeta.location = string.IsNullOrEmpty(spawnInfo.roomMeta.Id)
                ? spawnInfo.roomMeta.gameObject.name
                : spawnInfo.roomMeta.Id;

            // 4) JSON용 데이터 추출
            result.Add(instMeta.ToData());

            // 5) 관리용 리스트/SpawnPoint 사용 처리
            _spawnedClues.Add(inst);
            _usedSpawnPoints.Add(spawnInfo.point);
        }

        return result;
    }

    /// <summary>
    /// 이전에 생성된 단서 오브젝트들 제거 및 상태 초기화
    /// </summary>
    public void ClearSpawnedClues()
    {
        foreach (var go in _spawnedClues)
        {
            if (go != null)
                Destroy(go);
        }

        _spawnedClues.Clear();
        _usedSpawnPoints.Clear();
    }

    // 내부용 구조체
    private struct SpawnPointInfo
    {
        public RoomMeta roomMeta;
        public Transform point;
    }

    /// <summary>
    /// 주어진 방 리스트에서, 타입(Floor/Wall)에 맞고 아직 사용되지 않은 SpawnPoint 중 하나를 랜덤 선택
    /// </summary>
    private SpawnPointInfo FindRandomSpawnPointForType(List<RoomMeta> rooms, ClueSpawnType type)
    {
        List<SpawnPointInfo> candidates = new List<SpawnPointInfo>();

        foreach (var roomMeta in rooms)
        {
            if (roomMeta == null) continue;

            Transform[] list = null;
            switch (type)
            {
                case ClueSpawnType.Floor:
                    list = roomMeta.floorSpawnPoints;
                    break;

                case ClueSpawnType.Wall:
                    list = roomMeta.wallSpawnPoints;
                    break;
            }

            if (list == null || list.Length == 0)
                continue;

            foreach (var tr in list)
            {
                if (tr == null) continue;
                if (_usedSpawnPoints.Contains(tr)) continue; // 한 SpawnPoint에 하나만

                candidates.Add(new SpawnPointInfo
                {
                    roomMeta = roomMeta,
                    point = tr
                });
            }
        }

        if (candidates.Count == 0)
            return default;

        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }
}
