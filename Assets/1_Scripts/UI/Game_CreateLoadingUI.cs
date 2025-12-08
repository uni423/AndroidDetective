using UnityEngine;
using DG.Tweening;

public class Game_CreateLoadingUI : UIBase
{
    public Transform loadingImg;
    private Tween rotateTween;

    public override void ShowUI()
    {
        base.ActiveOn();

        if (rotateTween != null && rotateTween.IsActive())
            rotateTween.Kill();

        rotateTween = loadingImg.DORotate(new Vector3(0, 0, -360f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);
    }

    public override void HideUI()
    {
        base.ActiveOff();

        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
            rotateTween = null;
        }
    }
}
