using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Game_NPCChatUI : UIBase
{
    public InputField SendMessageText;
    public Text NpcName;
    public Text NpcChat;

    public GameObject LoadingObj;
    public Transform loadingImg;
    private Tween rotateTween;


    public override void HideUI()
    {
        base.ActiveOff();

        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
            rotateTween = null;
        }
    }

    public void SetNPC(string NpcName, string NpcChat)
    {
        this.NpcName.text = NpcName;
        this.NpcChat.text = NpcChat;

        LoadingObj.SetActive(false);

        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
            rotateTween = null;
        }
    }

    public void OnClick_SendNPC()
    {
        LoadingObj.SetActive(true);

        if (rotateTween != null && rotateTween.IsActive())
            rotateTween.Kill();

        rotateTween = loadingImg.DORotate(new Vector3(0, 0, -360f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);

        InGameManager.Instance.SendNPCChat(SendMessageText.text);
    }

    public void OnClick_Close()
    {
        InGameManager.Instance.DoResume();

        UIManager.Instance.HideUI(UIState.Game_NPCChatUI);
        UIManager.Instance.ShowUI(UIState.Game_MainUI);
    }
}
