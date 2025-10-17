using UnityEngine;
using UnityEngine.UI;

public class GenerateButton : MonoBehaviour
{
    public LevelGenerator Generator;
    public InputField SeedInput;

    public void OnClickGenerate()
    {
        Generator.Generate();
    }
}
