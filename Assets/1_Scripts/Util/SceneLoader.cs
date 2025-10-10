using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    MainScene,
    GameScene,
}

public static class SceneLoader
{
    public static SceneType CurrentScene { get; private set; }
    public static bool IsSceneLoading { get; private set; }

    static SceneLoader()
    {
        CurrentScene = SceneType.MainScene;

        IsSceneLoading = false;

        SceneManager.sceneLoaded += Loaded;
    }

    public static void Load(SceneType type)
    {
        SceneManager.LoadScene((int)type);

        CurrentScene = type;

        IsSceneLoading = true;
    }

    public static void Load(string name)
    {
        SceneManager.LoadScene(name);
        CurrentScene = (SceneType)Enum.Parse(typeof(SceneType), name);
        IsSceneLoading = true;
    }

    public static void ReLoad()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        IsSceneLoading = true;
    }

    private static void Loaded(Scene scene, LoadSceneMode mode)
    {
        IsSceneLoading = false;
    }
}
