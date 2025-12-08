using System.Collections.Generic;
using UnityEngine;

public class ClueJsonGenerator : MonoBehaviour
{
    [Header("랜덤으로 뽑을 단서 프리팹 리스트")]
    public List<GameObject> cluePrefabs = new List<GameObject>();

    public ClueListWrapper ChoiceRandomClues()
    {
        ClueListWrapper wrapper = new ClueListWrapper();

        if (cluePrefabs == null || cluePrefabs.Count == 0)
        {
            Debug.LogWarning("[ClueJsonGenerator] 등록된 단서 프리팹이 없습니다.");
            return wrapper; // 빈 배열
        }

        int maxCount = Mathf.Min(cluePrefabs.Count, 10);
        int minCount = Mathf.Min(maxCount, 8);   // 프리팹이 8개 미만이면 있는 만큼만

        int targetCount = Random.Range(minCount, maxCount + 1); // [min, max] 포함

        HashSet<int> usedIndices = new HashSet<int>();

        while (wrapper.clues.Count < targetCount && usedIndices.Count < cluePrefabs.Count)
        {
            int idx = Random.Range(0, cluePrefabs.Count);
            if (!usedIndices.Add(idx))
                continue;

            GameObject prefab = cluePrefabs[idx];
            if (prefab == null)
            {
                Debug.LogWarning("[ClueJsonGenerator] 비어 있는 프리팹 슬롯이 있습니다.");
                continue;
            }

            ClueMeta meta = prefab.GetComponent<ClueMeta>();
            if (meta == null)
            {
                Debug.LogWarning($"[ClueJsonGenerator] {prefab.name} 에 ClueMeta 컴포넌트가 없습니다.");
                continue;
            }

            wrapper.clues.Add(meta.ToData());
        }

        return wrapper;
    }
}
