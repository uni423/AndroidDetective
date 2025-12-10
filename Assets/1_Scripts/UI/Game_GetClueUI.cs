using UnityEngine;
using UnityEngine.UI;

public class Game_GetClueUI : UIBase
{

    public Text ClueNameText;
    public Text ClueDiscriptionText;
    public Image ClueImage;

    public void SetClue(ClueMeta clue)
    {
        ClueNameText.text = clue.clueName;
        ClueDiscriptionText.text = clue.description;

        ClueImage.sprite = clue.clueImage;
    }

    public void OnClick_Resume()
    {
        InGameManager.Instance.DoResume();

        UIManager.Instance.HideUI(UIState.Game_GetClueUI);
        UIManager.Instance.ShowUI(UIState.Game_MainUI);
    }
}
