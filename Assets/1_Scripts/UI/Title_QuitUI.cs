using UnityEngine;

public class Title_QuitUI : UIBase
{
    public void OnClick_Quit()
    {
        Application.Quit();
    }

    public void OnClick_Cancel()
    {
        UIManager.Instance.HideUI(UIState.Title_QuitUI);
        UIManager.Instance.ShowUI(UIState.Title_MainUI);
    }
}
