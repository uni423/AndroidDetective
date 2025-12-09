using UnityEngine;

public class Game_PauseUI : UIBase
{
    public void OnClickResumt()
    {
        InGameManager.Instance.DoResume();

        UIManager.Instance.HideUI(UIState.Game_PauseUI);
        UIManager.Instance.ShowUI(UIState.Game_MainUI);
    }

    public void OnClickRestart()
    {
        SceneLoader.Load(SceneType.TitleScene);
    }

    public void OnClickQuit()
    {
        UIManager.Instance.HideUI(UIState.Game_PauseUI);
        UIManager.Instance.ShowUI(UIState.Game_QuitUI);
    }
}
