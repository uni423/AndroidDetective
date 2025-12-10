using UnityEngine;
using UnityEngine.UI;

public class Game_PublicViewUI : UIBase
{

    public Text publicView;

    public void OnClickStartGame()
    {
        UIManager.Instance.HideUI(UIState.Game_PublicViewUI);
        UIManager.Instance.ShowUI(UIState.Game_MainUI);

        InGameManager.Instance.ChangeInGameStep(InGameStep.Playing);
    }
}
