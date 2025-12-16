using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_SendResultUI : UIBase
{
    public GameObject SendResultObj;
    public Dropdown NPCDropDown;

    public InputField SendResultText;

    public GameObject LoadingObj;
    public Transform loadingImg;
    private Tween rotateTween;


    public override void ShowUI()
    {
        base.ShowUI();

        SendResultObj.SetActive(true);
        LoadingObj.SetActive(false);

        if (InGameManager.Instance.LastScenario != null)
        {
            List<Dropdown.OptionData> optionList = new List<Dropdown.OptionData>();
            foreach(var suspect in InGameManager.Instance.LastScenario.suspects)
            {
                Dropdown.OptionData newItem = new Dropdown.OptionData(suspect.name);
                optionList.Add(newItem);
            }
            NPCDropDown.AddOptions(optionList);
        }
    }

    public override void HideUI()
    {
        base.ActiveOff();

        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
            rotateTween = null;
        }
    }

    public void OnClick_Cancle()
    {
        InGameManager.Instance.DoResume();

        UIManager.Instance.HideUI(UIState.Game_SendResultUI);
        UIManager.Instance.ShowUI(UIState.Game_MainUI);
    }

    public void OnClick_Delete()
    {
        SendResultText.text = "";
    }

    public void OnClick_Send()
    {
        string selectedName = NPCDropDown.captionText.text;
        string killerId = null;

        // 시나리오에서 같은 이름 가진 용의자 찾아서 id 추출
        if (InGameManager.Instance.LastScenario != null &&
            InGameManager.Instance.LastScenario.suspects != null)
        {
            foreach (var suspect in InGameManager.Instance.LastScenario.suspects)
            {
                if (suspect.name == selectedName)
                {
                    killerId = suspect.id;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(killerId))
        {
            Debug.LogError($"[Game_SendResultUI] 선택된 용의자의 id를 찾을 수 없습니다. name={selectedName}");
            return;
        }

        InGameManager.Instance.SendResult(killerId, SendResultText.text);

        if (rotateTween != null && rotateTween.IsActive())
            rotateTween.Kill();

        LoadingObj.SetActive(true);
        rotateTween = loadingImg.DORotate(new Vector3(0, 0, -360f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);
    }

}
