using UnityEngine;

public class Game_QRUI : UIBase
{
    public void OnClick_StartGame()
    {
        UIManager.Instance.HideUI(UIState.Game_QRUI);
        UIManager.Instance.ShowUI(UIState.Game_PublicViewUI);
    }

    public void OnClick_PopupPhoneQRWeb()
    {
        InGameManager.Instance.NetworkingManager.PopupPhoneQRWeb(InGameManager.Instance.PlayerId);
    }
}
