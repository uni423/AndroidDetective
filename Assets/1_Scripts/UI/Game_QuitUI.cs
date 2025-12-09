using UnityEngine;

public class Game_QuitUI : UIBase
{
    public void OnClick_Quit()
    {
        Application.Quit();
    }

    public void OnClick_Cancel()
    {
        UIManager.Instance.HideUI(UIState.Game_QuitUI);
        UIManager.Instance.ShowUI(UIState.Game_PauseUI);
    }
}
