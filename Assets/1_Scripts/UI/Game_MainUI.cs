using UnityEngine;

public class Game_MainUI : UIBase
{
    public override void ShowUI()
    {
        base.ShowUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        if (Input.GetKeyDown(KeyCode.E))
        {

        }
    }

    public void OnClickPauseButton()
    {
        InGameManager.Instance.DoPause();

        UIManager.Instance.HideUI(UIState.Game_MainUI);
        UIManager.Instance.ShowUI(UIState.Game_PauseUI);
    }
}
