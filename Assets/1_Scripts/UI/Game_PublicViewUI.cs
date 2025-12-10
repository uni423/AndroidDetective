using UnityEngine;
using UnityEngine.UI;

public class Game_PublicViewUI : UIBase
{

    public Text publicView;

    public override void ShowUI()
    {
        base.ShowUI();

        if (InGameManager.Instance.LastScenario != null)
        {
            this.publicView.text = "사건 명 : " + InGameManager.Instance.LastScenario.title;
            this.publicView.text += "\n\n사건 개요 : " + InGameManager.Instance.LastScenario.publicView.overview;
            this.publicView.text += "\n\n사건 배경 : " + InGameManager.Instance.LastScenario.background;
        }
    }

    public void OnClickStartGame()
    {
        UIManager.Instance.HideUI(UIState.Game_PublicViewUI);
        UIManager.Instance.ShowUI(UIState.Game_MainUI);

        InGameManager.Instance.ChangeInGameStep(InGameStep.Playing);
    }
}
