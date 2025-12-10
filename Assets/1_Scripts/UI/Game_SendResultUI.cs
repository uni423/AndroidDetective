using DG.Tweening;
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
            foreach(var suspect in InGameManager.Instance.LastScenario.suspects)
            {
                Dropdown.OptionData newItem = new Dropdown.OptionData();
                newItem.text = suspect.name;
                NPCDropDown.options.Add(newItem);
            }
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
        InGameManager.Instance.SendResult(NPCDropDown.captionText.text, SendResultText.text);

        if (rotateTween != null && rotateTween.IsActive())
            rotateTween.Kill();

        rotateTween = loadingImg.DORotate(new Vector3(0, 0, -360f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);
    }

}
