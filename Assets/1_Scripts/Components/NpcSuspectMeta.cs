using UnityEngine;

[DisallowMultipleComponent]
public class NpcSuspectMeta : MonoBehaviour
{
    [Header("Scenario Suspect Data")]
    public Suspect data;

    public void Initialize(Suspect info)
    {
        data = info;
    }
}
