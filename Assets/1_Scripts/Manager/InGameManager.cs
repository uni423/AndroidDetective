using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;

    public float score = 0;
    public GameObject[] mapObjArr;

    public PlayerControl playerControl;

    public UnitManager unitManager { private set; get; }

    public static ObjectPooling ObjectPooling { get; private set; }

    public static bool IsPlaying;
    public static bool IsContinue;
    public static bool IsReSetting;

    private float time;
    private float rabbitSpawnTime = 3f;
    private float rabbitSpawnRadius = 10f;
    public float gameTime;


    //public void Init()
    protected void Awake()
    {
        Instance = this;

        ObjectPooling = FindObjectOfType<ObjectPooling>();

        IsPlaying = false;
        IsContinue = false;
        IsReSetting = false;

        unitManager = new UnitManager();
        unitManager.Initialize();

        UIManager.Instance.Init();

        for (int i = 0; i < mapObjArr.Length; i++)
            mapObjArr[i].SetActive(i == GameManager.Instance.UserInfoData.selectedStage);

        DoGameStart();

        gameTime = 60;
    }

    public static void DoGameStart()
    {
        GameManager.Instance.gameStep = GameStep.Playing;

        IsPlaying = true;
        IsReSetting = false;

    }

    private void Update()
    {
        if (IsPlaying == false)
            return;

        time += Time.deltaTime;
        gameTime -= Time.deltaTime;

        if (gameTime <= 0)
        {
            IsPlaying = false;
            UIManager.Instance.HideUI(UIState._InGameUI);
            UIManager.Instance.ShowUI(UIState._ResultUI);
            return;
        }

        SpawnRabbit();

        unitManager.OnUpdate(Time.deltaTime);
    }

    private void LateUpdate()
    {
        unitManager.OnLateUpdate(Time.deltaTime);
    }

    public void SpawnRabbit()
    {
        if (time >= rabbitSpawnTime)
        {
            time -= rabbitSpawnTime;

            int randomInt = 0;
            
            switch (GameManager.Instance.UserInfoData.selectedStage)
            {
                case 1: randomInt = Random.Range(0, 2); break;
                case 2: randomInt = Random.Range(0, 3); break;
                case 3: randomInt = Random.Range(0, 5); break;
                default: randomInt = 0; break;
            }

            RabbitUnit rabbit = null;
            Vector3 getPoint = Random.onUnitSphere;
            getPoint.y = 0.0f;
            switch ((Unit_Type)randomInt)
            {
                case Unit_Type.Rabbit_Normal:
                    rabbit = new RabbitUnit();
                    rabbit.SetUnitTable(201);
                    break;
                case Unit_Type.Rabbit_Baby:
                    //아기 토끼의 경우 여러마리가 동시 소환 되야 하기 때문에 처리를 다르게 실행
                    StartCoroutine(BabyRbSpawn(getPoint));
                    return;
                case Unit_Type.Rabbit_Strong:
                    rabbit = new StrongRbUnit();
                    rabbit.SetUnitTable(203);
                    break;
                case Unit_Type.Rabbit_Evolve:
                    rabbit = new EvolveRbUnit();
                    rabbit.SetUnitTable(204);
                    getPoint.y = 1f;
                    break;
                case Unit_Type.Rabbit_BulkUp:
                    rabbit = new BulkUpRbUnit();
                    rabbit.SetUnitTable(205);
                    break;
            }
            rabbit.Initialize();
            rabbit.unitObject.cachedTransform.SetPositionAndRotation(
                (getPoint * rabbitSpawnRadius) + playerControl.transform.position
                , Quaternion.Euler(0, Random.Range(0, 360f), 0));

            unitManager.Regist(rabbit);
        }
    }

    public void AddScore(int addScore, bool isCombo = false)
    {
        score += addScore;
        UIManager.Instance.RefreshUserInfo();
        (UIManager.Instance.GetUI(UIState._InGameUI) as IngameUI).AddScoreUI(addScore, isCombo);
    }

    IEnumerator BabyRbSpawn(Vector3 getPoint)
    {
        int babyCount = Random.Range(3, 6);
        for (int i = 0; i < babyCount; i++)
        {
            BabyRbUnit baby = new BabyRbUnit();
            baby.SetUnitTable(202);
            baby.Initialize();
            baby.unitObject.cachedTransform.SetPositionAndRotation(
                ((getPoint * rabbitSpawnRadius) + Random.onUnitSphere) + playerControl.transform.position
                , Quaternion.Euler(0, Random.Range(0, 360f), 0));
            unitManager.Regist(baby);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
