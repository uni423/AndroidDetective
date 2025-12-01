using UnityEngine;

public class Title_MainUI : UIBase
{
    public void OnClick_NewGame()
    {
        SceneLoader.Load(SceneType.GameScene);
    }

    public void OnClick_Continue()
    {

    }

    public void OnClick_HighScore()
    {

    }

    public void OnClick_Option()
    {

    }

    public void OnClick_Quit()
    {
        UIManager.Instance.HideUI(UIState.Title_MainUI);
        UIManager.Instance.ShowUI(UIState.Title_QuitUI);
    }
}
