using UnityEngine;
using UnityEngine.UI;

public class Game_ResultUI : UIBase
{
    public GameObject SuccessImgs;
    public GameObject FailImgs;

    public Text ResultText;

    public void SetResult(bool IsSuccess, string ResultText)
    {
        SuccessImgs.SetActive(IsSuccess);
        FailImgs.SetActive(IsSuccess == false);

        this.ResultText.text = ResultText;
    }

    public void OnClick_ToTitle()
    {
        SceneLoader.Load(SceneType.TitleScene);
    }
}
