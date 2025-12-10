using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("소환할 NPC 프리팹 리스트")]
    public List<GameObject> npcPrefabs = new List<GameObject>();

    [Header("디버그용")]
    public List<GameObject> spawnedNpcs = new List<GameObject>();

    public void SpawnRandomNpcs(List<RoomMeta> roomMetas, int maxNpcCount = -1)
    {
        // 기존에 소환된 NPC 정리 (필요 없으면 지우고 시작)
        ClearSpawnedNpcs();

        if (npcPrefabs == null || npcPrefabs.Count == 0)
        {
            Debug.LogWarning("[NPCSpawner] npcPrefabs 리스트가 비어있습니다.");
            return;
        }

        if (roomMetas == null || roomMetas.Count == 0)
        {
            Debug.LogWarning("[NPCSpawner] roomMetas 리스트가 비어있습니다.");
            return;
        }

        // NPC를 놓을 수 있는 방만 필터링 (npcSpawnPoints가 있는 방)
        List<RoomMeta> candidateRooms = new List<RoomMeta>();
        foreach (var room in roomMetas)
        {
            if (room == null) continue;
            if (room.npcSpawnPoints == null || room.npcSpawnPoints.Length == 0) continue;

            candidateRooms.Add(room);
        }

        if (candidateRooms.Count == 0)
        {
            Debug.LogWarning("[NPCSpawner] npcSpawnPoints가 있는 방이 없습니다.");
            return;
        }

        // 몇 명을 소환할지 결정
        int npcCount = (maxNpcCount > 0) ? maxNpcCount : npcPrefabs.Count;
        npcCount = Mathf.Min(npcCount, npcPrefabs.Count);      // 프리팹 수 이상은 안됨
        npcCount = Mathf.Min(npcCount, candidateRooms.Count);  // 방 개수 이상도 안됨 (한 방 1명 제한)

        // 랜덤용
        System.Random rng = new System.Random();

        // 방 리스트 섞기 (Fisher–Yates)
        Shuffle(candidateRooms, rng);

        // 프리팹 리스트도 복사해서 섞기
        List<GameObject> shuffledPrefabs = new List<GameObject>(npcPrefabs);
        Shuffle(shuffledPrefabs, rng);

        // 실제 소환
        for (int i = 0; i < npcCount; i++)
        {
            RoomMeta room = candidateRooms[i]; // 각 방은 최대 한 번만 사용됨
            Transform[] points = room.npcSpawnPoints;

            if (points == null || points.Length == 0)
                continue;

            // 방 안에서 랜덤 포인트 선택
            int spIdx = rng.Next(0, points.Length);
            Transform spawnPoint = points[spIdx];

            GameObject prefab = shuffledPrefabs[i];
            GameObject npc = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            spawnedNpcs.Add(npc);

            // 필요하면 여기서 NPC에 "자기 방 정보" 넘겨줄 수 있음
            // var npcMeta = npc.GetComponent<NpcMeta>();
            // if (npcMeta != null) npcMeta.currentRoomId = room.Id;
        }
    }

    /// <summary>
    /// 이미 소환된 NPC들을 모두 제거
    /// </summary>
    public void ClearSpawnedNpcs()
    {
        if (spawnedNpcs == null) return;

        for (int i = 0; i < spawnedNpcs.Count; i++)
        {
            if (spawnedNpcs[i] != null)
                Destroy(spawnedNpcs[i]);
        }
        spawnedNpcs.Clear();
    }

    /// <summary>
    /// 제네릭 리스트 셔플
    /// </summary>
    private void Shuffle<T>(List<T> list, System.Random rng)
    {
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int swapIndex = rng.Next(i, n);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
