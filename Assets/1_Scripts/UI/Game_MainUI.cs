using UnityEngine;

public class Game_MainUI : UIBase
{
    public void OnCLickPauseButton()
    {
        InGameManager.Instance.DoPause();

        UIManager.Instance.HideUI(UIState.Game_MainUI);
        UIManager.Instance.ShowUI(UIState.Game_PauseUI);
    }
}
