using System.Collections;
using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Utils;

public class LabourDayHomeChessItem : MonoBehaviour
{
    [SerializeField] public RectTransform chessItem = null;
    [SerializeField] public RectTransform itemRoot = null;
    [SerializeField] public RectTransform upDownMoveTrans = null;

    public int mapIndex = 0;
    public int mapInnerIndex = 0;

    public void SetData(int mapIndex, int mapInnerIndex)
    {
        this.mapIndex = mapIndex;
        this.mapInnerIndex = mapInnerIndex;
    }
    public void PlayCollectAnim(LabourDayHomeTileItem tileItem, float moveTime)
    {
        Sequence collectSequence = DOTween.Sequence();
        collectSequence.SetTarget(this.gameObject);
        collectSequence.AddTo(this.gameObject);
        GameObject fakeGoods = GameObject.Instantiate(tileItem.iconImage.gameObject, itemRoot);
        fakeGoods.transform.localPosition = Utility.ConvertLocalPosition(tileItem.iconImage.transform, Vector3.zero, itemRoot);
        collectSequence.Append(DOBezier2LocalMove(fakeGoods.transform, itemRoot, moveTime, 100));//.SetEase(Ease.OutBack));
        collectSequence.Join(fakeGoods.transform.DOScale(Vector3.one * 0.5f, moveTime));//.SetEase(Ease.OutBack));
        // collectSequence.AppendCallback(() =>
        // {
        //     Destroy(fakeGoods);
        // });
    }
    // /// <summary>
    // /// 预设了控制点和缓动曲线的2阶贝塞尔曲线动画
    // /// （本地坐标系下）
    // /// </summary>
    // /// <param name="transform">变换组件</param>
    // /// <param name="endPos">结束点</param>
    // /// <param name="duration">动画时间</param>
    // /// <returns>动画实例</returns>
    // public static TweenerCore<float, float, FloatOptions> DOBezier2LocalMove(Transform transform, Vector3 endPos, float duration, float jumpHeight = 0)
    // {
    //     Vector3 startPos = transform.localPosition;
    //     Vector3 controlPos = new Vector3();
    //     controlPos.x = startPos.x;
    //     controlPos.y = endPos.y + jumpHeight;
    //     return transform.DOBezier2LocalMove(startPos, controlPos, endPos, duration);
    // }
    public static TweenerCore<float, float, FloatOptions> DOBezier2LocalMove(Transform moveTransform, Transform followTrans, float duration, float jumpHeight, Ease ease = Ease.InSine)
    {
        Vector3 startPos = moveTransform.localPosition;
        TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
            () => 0f,
            (progress) =>
            {
                Vector3 endPos = Utility.ConvertLocalPosition(followTrans, Vector3.zero, moveTransform.parent);
                Vector3 controlPos = new Vector3();
                controlPos.x = startPos.x;
                controlPos.y = endPos.y + jumpHeight;
                moveTransform.localPosition = AnimationExtensions.Bezier2(startPos, controlPos, endPos, progress);
            }
            , 1f, duration)
            .SetEase(ease);
        tweenerCore.SetTarget(moveTransform);
        return tweenerCore;
    }
    public void ClearAllFakeGoods()
    {
        for (int i = 0; i < itemRoot.childCount; i++)
        {
            Destroy(itemRoot.GetChild(i).gameObject);
        }
    }
}
