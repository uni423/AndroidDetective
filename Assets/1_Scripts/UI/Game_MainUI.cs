using UnityEngine;

public class Game_MainUI : UIBase
{
    public override void ShowUI()
    {
        base.ShowUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //transform.position = Vector3.zero;
    }

    public override void HideUI()
    {
        base.HideUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickPauseButton();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            OnClickPublicView();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnClickSendResult();
        }
    }

    public void OnClickPauseButton()
    {
        InGameManager.Instance.DoPause();

        UIManager.Instance.HideUI(UIState.Game_MainUI);
        UIManager.Instance.ShowUI(UIState.Game_PauseUI);
    }

    public void OnClickPublicView()
    {
        InGameManager.Instance.DoPause();

        UIManager.Instance.HideUI(UIState.Game_MainUI);
        UIManager.Instance.ShowUI(UIState.Game_PublicViewUI);
    }

    public void OnClickSendResult()
    {
        InGameManager.Instance.DoPause();

        UIManager.Instance.HideUI(UIState.Game_MainUI);
        UIManager.Instance.ShowUI(UIState.Game_SendResultUI);
    }
}
