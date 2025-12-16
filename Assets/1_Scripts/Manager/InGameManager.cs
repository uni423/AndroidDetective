using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;

    public LevelGenerator LevelGenerator;
    public ClueJsonGenerator clueJsonGenerator;

    public List<ClueData> clues;

    public NPCSpawner npcSpawner;

    public string PlayerId;
    public PlayerController playerController;

    public NetworkingManager NetworkingManager;
    public ScenarioResponse LastScenario;

    public static bool IsPlaying;
    public static bool IsContinue;
    public static bool IsReSetting;

    private float time;
    public float gameTime;

    public InGameStep m_InGameStep;

    protected void Awake()
    {
        Instance = this;

        IsPlaying = false;
        IsContinue = false;
        IsReSetting = false;

        //unitManager = new UnitManager();
        //unitManager.Initialize();

        UIManager.Instance.Init();
        NetworkingManager.Init();

        m_InGameStep = InGameStep.CreateGameLoading;

        //playerController.gameObject.SetActive(false);

        DoGameSettingFlow();

        //DoGameStart();

        gameTime = 60;
    }

    public void DoGameSettingFlow()
    {
        // 맵 생성 (테마 선택) 
        LevelGenerator.GenerateHubAuto();
        //Debug.Log(LevelGenerator.ExportMapJson());

        // 증거품 선택 
        clues = clueJsonGenerator.ChoiceRandomClues(LevelGenerator.GetPlacedRoomMetas());

        string guid = System.Guid.NewGuid().ToString("N");
        PlayerId = guid.Substring(0, 8);

        NetworkingManager.StartGame(PlayerId, LevelGenerator.ExportMapJson(), clues,
            onSuccess: (scenario) =>
            {
                var roomMetas = LevelGenerator.GetPlacedRoomMetas();
                npcSpawner.SpawnRandomNpcs(roomMetas, scenario.suspects.ToList());
                
                LastScenario = scenario;
                ChangeInGameStep(InGameStep.QRConnectWait);
            },
            onError: (err) => { Debug.LogError("StartGame 실패: " + err); });

        UIManager.Instance.ShowUI(UIState.Game_CreateLoadingUI);
    }

    public void DoPause()
    {
        ChangeInGameStep(InGameStep.Pause);
        IsPlaying = false;
    }

    public void DoResume()
    {
        ChangeInGameStep(InGameStep.Playing);
        IsPlaying = true;
    }

    private void Update()
    {
        //if (IsPlaying == false)
        //    return;

        //time += Time.deltaTime;
        //gameTime -= Time.deltaTime;

        //if (gameTime <= 0)
        //{
        //    IsPlaying = false;
        //    UIManager.Instance.HideUI(UIState._InGameUI);
        //    UIManager.Instance.ShowUI(UIState._ResultUI);
        //    return;
        //}

        //SpawnRabbit();

        //unitManager.OnUpdate(Time.deltaTime);
    }

    public void ChangeInGameStep(InGameStep ChangeStep)
    {
        switch (m_InGameStep)
        {
            case InGameStep.CreateGameLoading:
                if (ChangeStep == InGameStep.QRConnectWait)
                {
                    NetworkingManager.PopupPhoneQRWeb(PlayerId);
                    UIManager.Instance.HideUI(UIState.Game_CreateLoadingUI);
                    UIManager.Instance.ShowUI(UIState.Game_QRUI);
                }
                break;
            case InGameStep.QRConnectWait:
                if (ChangeStep == InGameStep.Playing)
                {
                    GameManager.Instance.gameStep = GameStep.Playing;

                    IsPlaying = true;
                }
                break;
            case InGameStep.Playing:
                break;
            case InGameStep.Pause:
                break;
        }

        m_InGameStep = ChangeStep;
    }


    public Suspect CurChatNPC = new Suspect();
    public void NPCChatStart(Suspect ChatSuspect)
    {
        CurChatNPC = ChatSuspect;
        DoPause();
        UIManager.Instance.HideUI(UIState.Game_MainUI);
        UIManager.Instance.ShowUI(UIState.Game_NPCChatUI);

        (UIManager.Instance.uiDataLists[(int)UIState.Game_NPCChatUI] as Game_NPCChatUI).SetNPC(ChatSuspect.name);
    }

    public void SendNPCChat(string SendMessageText)
    {
        NetworkingManager.SendNPCChat(PlayerId, SendMessageText, CurChatNPC.id, clueJsonGenerator.GetFindClueList(),
            onSuccess: replyText =>
            {
                Debug.Log("NPC 답변: " + replyText);
                (UIManager.Instance.uiDataLists[(int)UIState.Game_NPCChatUI] as Game_NPCChatUI).SetChat(replyText);
            },
            onError: err => { Debug.LogError("NPC 대화 실패: " + err);} );
    }

    public void FindGetClue(ClueMeta clue)
    {
        clue.isFind = true;

        NetworkingManager.SendGetClueInfo(PlayerId, clue);

        DoPause();

        UIManager.Instance.HideUI(UIState.Game_MainUI);
        UIManager.Instance.ShowUI(UIState.Game_GetClueUI);

        (UIManager.Instance.uiDataLists[(int)UIState.Game_GetClueUI] as Game_GetClueUI).SetClue(clue);
    }

    public void SendResult(string SelectedNpcId, string InputReasonText)
    {
        NetworkingManager.SendResult(PlayerId, SelectedNpcId, InputReasonText,
            onSuccess: resp =>
            {
                Debug.Log($"정답 여부: {resp.correct}, 진범: {resp.killerName}");
                Debug.Log(resp.caseSummary);

                ChangeInGameStep(InGameStep.Result);

                UIManager.Instance.HideUI(UIState.Game_SendResultUI);
                UIManager.Instance.ShowUI(UIState.Game_ResultUI);

                (UIManager.Instance.uiDataLists[(int)UIState.Game_ResultUI] as Game_ResultUI).SetResult(resp.correct, resp.caseSummary);
            },
            onError: (err) => { Debug.LogError("SendResult 실패: " + err); });
    }

    //private void LateUpdate()
    //{
    //    unitManager.OnLateUpdate(Time.deltaTime);
    //}

    //public void SpawnRabbit()
    //{
    //    if (time >= rabbitSpawnTime)
    //    {
    //        time -= rabbitSpawnTime;

    //        int randomInt = 0;

    //        switch (GameManager.Instance.UserInfoData.selectedStage)
    //        {
    //            case 1: randomInt = Random.Range(0, 2); break;
    //            case 2: randomInt = Random.Range(0, 3); break;
    //            case 3: randomInt = Random.Range(0, 5); break;
    //            default: randomInt = 0; break;
    //        }

    //        RabbitUnit rabbit = null;
    //        Vector3 getPoint = Random.onUnitSphere;
    //        getPoint.y = 0.0f;
    //        switch ((Unit_Type)randomInt)
    //        {
    //            case Unit_Type.Rabbit_Normal:
    //                rabbit = new RabbitUnit();
    //                rabbit.SetUnitTable(201);
    //                break;
    //            case Unit_Type.Rabbit_Baby:
    //                //아기 토끼의 경우 여러마리가 동시 소환 되야 하기 때문에 처리를 다르게 실행
    //                StartCoroutine(BabyRbSpawn(getPoint));
    //                return;
    //            case Unit_Type.Rabbit_Strong:
    //                rabbit = new StrongRbUnit();
    //                rabbit.SetUnitTable(203);
    //                break;
    //            case Unit_Type.Rabbit_Evolve:
    //                rabbit = new EvolveRbUnit();
    //                rabbit.SetUnitTable(204);
    //                getPoint.y = 1f;
    //                break;
    //            case Unit_Type.Rabbit_BulkUp:
    //                rabbit = new BulkUpRbUnit();
    //                rabbit.SetUnitTable(205);
    //                break;
    //        }
    //        rabbit.Initialize();
    //        rabbit.unitObject.cachedTransform.SetPositionAndRotation(
    //            (getPoint * rabbitSpawnRadius) + playerControl.transform.position
    //            , Quaternion.Euler(0, Random.Range(0, 360f), 0));

    //        unitManager.Regist(rabbit);
    //    }
    //}

    //public void AddScore(int addScore, bool isCombo = false)
    //{
    //    score += addScore;
    //    UIManager.Instance.RefreshUserInfo();
    //    (UIManager.Instance.GetUI(UIState._InGameUI) as IngameUI).AddScoreUI(addScore, isCombo);
    //}

    //IEnumerator BabyRbSpawn(Vector3 getPoint)
    //{
    //    int babyCount = Random.Range(3, 6);
    //    for (int i = 0; i < babyCount; i++)
    //    {
    //        BabyRbUnit baby = new BabyRbUnit();
    //        baby.SetUnitTable(202);
    //        baby.Initialize();
    //        baby.unitObject.cachedTransform.SetPositionAndRotation(
    //            ((getPoint * rabbitSpawnRadius) + Random.onUnitSphere) + playerControl.transform.position
    //            , Quaternion.Euler(0, Random.Range(0, 360f), 0));
    //        unitManager.Regist(baby);
    //        yield return new WaitForSeconds(0.1f);
    //    }
    //}
}
