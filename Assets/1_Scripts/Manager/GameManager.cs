using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //public UserInfoData UserInfoData;

    public GameStep gameStep;

    protected void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        //StartCoroutineMethod(TableBase.LoadAllDataTable());

        Initialize();

        SceneLoader.Load("MainScene");
    }

    public void Initialize()
    {
        //UserInfoData = new UserInfoData();
        //UserInfoData.InitData();
        //UserInfoData.SaveData();
    }

#region Coroutine
    public static Coroutine StartCoroutineMethod(IEnumerator enumerator)
    {
        return Instance.StartCoroutine(enumerator);
    }

    public static void StopCoroutineMethod(string name)
    {
        Instance.StopCoroutine(name);
    }

    public static void StopAllCoroutineMethod()
    {
        Instance.StopAllCoroutines();
    }
#endregion
}
