using UnityEngine;
using DG.Tweening;

public class Game_CreateLoadingUI : UIBase
{
    public Transform loadingImg;
    private Tween rotateTween;

    public override void ActiveOn()
    {
        base.ActiveOn();
        Debug.Log("ActiveOnChild");

        if (rotateTween != null && rotateTween.IsActive())
            rotateTween.Kill();

        rotateTween = loadingImg.DORotate(new Vector3(0, 0, 360f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);
    }

    public override void ActiveOff()
    {
        base.ActiveOff();

        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
            rotateTween = null;
        }
    }
}
