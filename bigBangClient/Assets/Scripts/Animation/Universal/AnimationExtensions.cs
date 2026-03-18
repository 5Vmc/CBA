using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public static class AnimationExtensions
    {
        /// <summary>
        /// 打字机效果
        /// 不支持富文本
        /// 如果文本要动态更新，需手动设置maxVisibleCharacters
        /// </summary>
        public static Tweener DOText(this TMP_Text txt, string endValue, float duration)
        {
            txt.text = endValue;
            txt.maxVisibleCharacters = 0;
            return DOTween.To(value => txt.maxVisibleCharacters = (int)value, 0, endValue.Length, duration);
        }

        public static Tweener DOText(this TMP_Text txt, float duration)
        {
            txt.maxVisibleCharacters = 0;
            return DOTween.To(value => txt.maxVisibleCharacters = (int)value, 0, txt.text.Length, duration);
        }

        /// <summary>
        /// 滚动数字动画(整数)
        /// </summary>
        /// <param name="stringFormat">文字格式，设置为“{0}”</param>
        public static Tweener DOChangeNumber(this TMP_Text txt, int toNum, float duration, int fromNum = 0, string stringFormat = "")
        {
            if (string.IsNullOrWhiteSpace(stringFormat))
            {
                return DOTween.To(value => txt.text = ((int)value).ToString(), fromNum, toNum, duration);
            }
            else
            {
                return DOTween.To(value => txt.text = string.Format(stringFormat, (int)value), fromNum, toNum, duration);
            }
        }

        /// <summary>
        /// 滚动数字动画(两位小数，浮点)
        /// </summary>
        /// <param name="stringFormat">文字格式，设置为“{0}”</param>
        public static Tweener DOChangeNumber(this TMP_Text txt, float toNum, float duration, float fromNum = 0, string stringFormat = "")
        {
            if (string.IsNullOrWhiteSpace(stringFormat))
            {
                return DOTween.To(value => txt.text = value.ToString("0.00"), fromNum, toNum, duration);
            }
            else
            {
                return DOTween.To(value => txt.text = string.Format(stringFormat, value.ToString("0.00")), fromNum, toNum, duration);
            }
        }

        /// <summary>
        /// 开头带放大效果的 滚动数字动画
        /// </summary>
        /// <param name="stringFormat">文字格式，设置为“###,###”，变为“123,456,789”</param>
        public static Sequence DOChangeNumberEx(this TMP_Text txt, int toNum, float duration, float scaleValue, int fromNum = 0, string stringFormat = "")
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(txt.DOScale(scaleValue, 0.2f));
            seq.Append(DOTween.To(value => txt.text = ((int)value).ToString(stringFormat), fromNum, toNum, duration));
            seq.Append(txt.DOScale(1f, 0.2f));
            return seq;
        }

        /// <summary>
        /// 开头带放大效果的 滚动数字动画
        /// </summary>
        /// <param name="stringFormat">文字格式，设置为“###,###”，变为“123,456,789”</param>
        public static Sequence DOChangeNumberEx(this TMP_Text txt, int toNum, float duration, float scaleValue, int fromNum, string stringFormat, Transform scaleTrans)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(scaleTrans.DOScale(scaleValue, 0.2f));
            seq.Append(DOTween.To(value => txt.text = ((int)value).ToString(stringFormat), fromNum, toNum, duration));
            seq.Append(scaleTrans.DOScale(1f, 0.2f));
            return seq;
        }

        /// <summary>
        /// 用于图片拼接文字的动画
        /// 滚动数字动画(两位小数，浮点)
        /// </summary>
        /// <param name="stringFormat">文字格式，设置为“{0}”</param>
        public static Tweener DOChangeNumber(this ImageFont txt, float toNum, float duration, float fromNum = 0, string stringFormat = "")
        {
            if (string.IsNullOrWhiteSpace(stringFormat))
            {
                return DOTween.To(value => txt.text = value.ToString("0.00"), fromNum, toNum, duration);
            }
            else
            {
                return DOTween.To(value => txt.text = string.Format(stringFormat, value.ToString("0.00")), fromNum, toNum, duration);
            }
        }

        /// <summary>
        /// 用于图片拼接文字的动画
        /// 滚动数字动画(整数)
        /// </summary>
        /// <param name="stringFormat">文字格式，设置为“{0}”</param>
        public static Tweener DOChangeNumber(this ImageFont txt, int toNum, float duration, int fromNum = 0, string stringFormat = "")
        {
            if (string.IsNullOrWhiteSpace(stringFormat))
            {
                return DOTween.To(value => txt.text = Mathf.FloorToInt(value).ToString(), fromNum, toNum, duration);
            }
            else
            {
                return DOTween.To(value => txt.text = string.Format(stringFormat, Mathf.FloorToInt(value)), fromNum, toNum, duration);
            }
        }

        /// <summary>
        /// 震动
        /// </summary>
        public static Tweener DOShake(this GameObject obj)
        {
            return obj.transform.DOShakePosition(3f, 8, 8, 90, false, true).SetLoops(-1);
        }

        public static void DOKill(this GameObject obj)
        {
            obj.transform.DOKill(true);
        }

        //经典错误示范，单独调用没问题，但不能放在Sequence内使用，因为sequence在Append时会先调用此方法来获取Tween，
        //此时队列中各个透明度Tween的“var alpha = obj.GetAlpha();”中获取的alpha都是一瞬间完成的，将会导致所有透明度起始值都一样，
        //应当使用DOTween.To提供的DOGetter委托来进行此类操作。
        //DOSetter委托会在每帧调用，“obj.SetAlpha(value)”里的操作过多，应当简化。
        /// <summary>
        /// 透明度动画
        /// </summary>
        /// <param name="endValue">目标透明度</param>
        /// <param name="duration">持续时间</param>
        //public static Tweener DOFade(this GameObject obj, float endValue, float duration)
        //{
        //    var alpha = obj.GetAlpha();
        //    return DOTween.To(value => obj.SetAlpha(value), alpha, endValue, duration);
        //}

        /// <summary>
        /// 透明度动画
        /// </summary>
        /// <param name="alphaEnd">目标透明度</param>
        /// <param name="duration">持续时间</param>
        public static TweenerCore<float, float, FloatOptions> DOFade(this GameObject obj, float alphaEnd, float duration)
        {
            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = obj.AddComponent<CanvasGroup>();
            }
            if (alphaEnd > 1) alphaEnd = 1;
            else if (alphaEnd < 0) alphaEnd = 0;

            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () =>
                {
                    return canvasGroup.alpha;
                },
                (value) =>
                {
                    canvasGroup.alpha = value;
                }
                , alphaEnd, duration);
            tweenerCore.SetTarget(obj);
            return tweenerCore;
        }

        /// <summary>
        /// RawImage的透明度动画
        /// </summary>
        /// <param name="alphaEnd">目标透明度</param>
        /// <param name="duration">持续时间</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFade(this RawImage rawImage, float alphaEnd, float duration)
        {
            if (alphaEnd > 1) alphaEnd = 1;
            else if (alphaEnd < 0) alphaEnd = 0;
            TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => rawImage.color, x => rawImage.color = x, alphaEnd, duration);
            tweenerCore.SetTarget(rawImage);
            return tweenerCore;
        }
        /// <summary>
        /// 设置RawImage的透明度
        /// </summary>
        /// <param name="alpha">目标透明度（0f-1f）</param>
        public static void SetAlpha(this RawImage rawImage, float alpha)
        {
            rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, alpha);
        }

        /// <summary>
        /// 模型高亮动画
        /// 需要使用符合要求的shader(Additive)
        /// </summary>
        /// <param name="lightEnd">高亮程度（0-1）</param>
        /// <param name="duration">持续时间</param>
        public static TweenerCore<float, float, FloatOptions> DOHighlight(this MeshRenderer meshRenderer, float lightEnd, float duration)
        {
            if (lightEnd > 1) lightEnd = 1;
            else if (lightEnd < 0) lightEnd = 0;
            lightEnd = Mathf.Lerp(0f, 0.5f, lightEnd);
            //Material material = meshRenderer.material;

            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () =>
                {
                    return meshRenderer.material.GetColor("_TintColor").a;
                },
                (value) =>
                {
                    Color oldColor = meshRenderer.material.GetColor("_TintColor");
                    meshRenderer.material.SetColor("_TintColor", new Color(oldColor.r, oldColor.g, oldColor.b, value));
                }
                , lightEnd, duration);
            tweenerCore.SetTarget(meshRenderer);
            return tweenerCore;
        }
        /// <summary>
        /// 设置模型的高亮
        /// 需要使用符合要求的shader(Additive)
        /// </summary>
        /// <param name="light">高亮程度（0-1）</param>
        public static void SetHighlight(this MeshRenderer meshRenderer, float light)
        {
            Color oldColor = meshRenderer.material.GetColor("_TintColor");
            meshRenderer.material.SetColor("_TintColor", new Color(oldColor.r, oldColor.g, oldColor.b, Mathf.Lerp(0f, 0.5f, light)));
        }

        /// <summary>
        /// 摇摆动画
        /// </summary>
        /// <param name="distance">摇摆距离</param>
        /// <param name="count">摇摆次数</param>
        /// <param name="time">摇摆1次的时间</param>
        public static Sequence DOWobble(this RectTransform rectTransform, Vector2 distance, int count, float time)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    sequence.Append(rectTransform.DoRelativeAnchorPos(distance, time).SetEase(Ease.Linear));
                }
                else
                {
                    sequence.Append(rectTransform.DoRelativeAnchorPos(-distance, time).SetEase(Ease.Linear));
                }
            }
            sequence.Append(rectTransform.DoRelativeAnchorPos(Vector2.zero, time).SetEase(Ease.Linear));
            return sequence;
        }

        /// <summary>
        /// 旋转动画
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="count">旋转次数</param>
        /// <param name="time">旋转1次的时间</param>
        public static Sequence DOSpin(this RectTransform rectTransform, float angle, int count, float time)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    sequence.Append(rectTransform.DOLocalRotate(Vector3.forward * angle, time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
                    sequence.Append(rectTransform.DOLocalRotate(Vector3.back * angle, time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
                }
                else
                {
                    sequence.Append(rectTransform.DOLocalRotate(Vector3.back * angle, time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
                    sequence.Append(rectTransform.DOLocalRotate(Vector3.forward * angle, time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
                }
            }
            return sequence;
        }

        /// <summary>
        /// 翻牌效果,仅对原始缩放为1的物体有效
        /// </summary>
        /// <param name="start">开始缩放</param>
        /// <param name="end">结束缩放</param>
        /// <param name="duration">翻转时间</param>
        public static Sequence DOTurn(this RectTransform rectTransform, Vector3 side, float duration)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() => rectTransform.localScale = Vector3.one - 2 * side);
            sequence.Append(rectTransform.DOScale(Vector3.one, duration));
            return sequence;
        }

        /// <summary>
        /// 翻牌效果,仅对原始缩放为1的物体有效
        /// </summary>
        /// <param name="start">开始缩放</param>
        /// <param name="end">结束缩放</param>
        /// <param name="duration">翻转时间</param>
        public static Sequence DOTurn(this Transform transform, Vector3 side, float duration)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() => transform.localScale = Vector3.one - 2 * side);
            sequence.Append(transform.DOScale(Vector3.one, duration));
            return sequence;
        }

        public static DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> DORelativePositionY(this Transform position, float displacement, float duration, bool snapping = false)
        {
            return position.DOLocalMoveY(position.localPosition.y + displacement, duration, snapping);
        }

        public static Tweener ScroolToTop(this ScrollRect rect, float duration)
        {
            return rect.content.DOAnchorPosY(0, duration);
        }

        public static Tweener ScrollToBottom(this ScrollRect rect, float duration)
        {
            return rect.content.DOAnchorPosY(Mathf.Max(0, rect.content.rect.height - rect.viewport.rect.height), duration);
        }



        #region 透明度

        /// <summary>
        /// 更改文字或图片的的Alpha
        /// </summary>
        /// <param name="text">文字或图片</param>
        /// <param name="alpha">目标alpha值</param>
        public static void SetAlpha<T>(this T graphic, float alpha) where T : Graphic
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// 文字或图片播放透明度改变动画
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">文字或图片</param>
        /// <param name="endAlpha">最终alpha</param>
        /// <param name="duration">变换时间</param>
        /// <returns>Tweener</returns>
        public static Tweener TweenAlpha<T>(this T graphic, float endAlpha, float duration) where T : Graphic
        {
            return graphic.DOFade(endAlpha, duration);
        }

        /// <summary>
        /// 更改gameObject及其子物体的透明度
        /// </summary>
        /// <param name="gameObject">目标物体</param>
        /// <param name="alpha">透明度</param>
        public static void SetAlphaInChildren(this GameObject gameObject, float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            Graphic[] graphicList = gameObject.GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphicList.Length; i++)
            {
                Graphic graphic = graphicList[i];
                graphic.DOKill();
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            }
        }

        /// <summary>
        /// 使用渐变动画来更改gameObject及其子物体的透明度
        /// </summary>
        /// <param name="gameObject">目标物体</param>
        /// <param name="endAlpha">结束时的透明度</param>
        /// <param name="duration">动画时间</param>
        /// <returns>动画列表</returns>
        public static List<Tweener> TweenAlphaInChildren(this GameObject gameObject, float endAlpha, float duration)
        {
            List<Tweener> tweenerList = new List<Tweener>();
            Graphic[] graphicList = gameObject.GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphicList.Length; i++)
            {
                Graphic graphic = graphicList[i];
                graphic.DOKill();
                Tweener tweener = graphic.DOFade(endAlpha, duration);
                tweenerList.Add(tweener);
            }
            return tweenerList;
        }

        #endregion

        #region Text

        /// <summary>
        /// 滚动数字动画
        /// </summary>
        /// <param name="text">文本框控件</param>
        /// <param name="endValue">结束时的数字（整数）</param>
        /// <param name="duration">动画时间</param>
        /// <returns>动画实例</returns>
        public static TweenerCore<float, float, FloatOptions> DOChangeNumber(this Text text, int endNumber, float duration, Ease ease = Ease.OutCubic)
        {
            int startNumber = 0;
            int.TryParse(text.text, out startNumber);
            text.text = startNumber.ToString();
            float nowNum = startNumber;
            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () => 0f,
                (progress) =>
                {
                    nowNum = Mathf.FloorToInt(Mathf.Lerp(startNumber, endNumber, progress));
                    text.text = nowNum.ToString();
                }
                , 1f, duration)
                .SetEase(ease);
            tweenerCore.SetTarget(text);
            return tweenerCore;
        }

        #endregion

        #region 贝塞尔曲线移动

        /// <summary>
        /// 2阶贝塞尔曲线坐标计算
        /// </summary>
        /// <param name="startPos">起始点</param>
        /// <param name="controlPos">控制点</param>
        /// <param name="endPos">结束点</param>
        /// <param name="progress">当前进度（0-1）</param>
        /// <returns>当前坐标</returns>
        public static Vector3 Bezier2(Vector3 startPos, Vector3 controlPos, Vector3 endPos, float progress)
        {
            return (1 - progress) * (1 - progress) * startPos + 2 * progress * (1 - progress) * controlPos + progress * progress * endPos;
        }

        /// <summary>
        /// 3阶贝塞尔曲线坐标计算
        /// </summary>
        /// <param name="startPos">起始点</param>
        /// <param name="controlPos1">控制点1</param>
        /// <param name="controlPos2">控制点2</param>
        /// <param name="endPos">结束点</param>
        /// <param name="progress">当前进度（0-1）</param>
        /// <returns>当前坐标</returns>
        public static Vector3 Bezier3(Vector3 startPos, Vector3 controlPos1, Vector3 controlPos2, Vector3 endPos, float progress)
        {
            float t2 = 1 - progress;
            return t2 * t2 * t2 * startPos
                + 3 * progress * t2 * t2 * controlPos1
                + 3 * progress * progress * t2 * controlPos2
                + progress * progress * progress * endPos;
        }

        /// <summary>
        /// 2阶贝塞尔动画
        /// （世界坐标系下）
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="startPos">起始点</param>
        /// <param name="controlPos">控制点</param>
        /// <param name="endPos">结束点</param>
        /// <param name="duration">动画时间</param>
        /// <param name="ease">缓动曲线</param>
        /// <returns>动画实例</returns>
        public static TweenerCore<float, float, FloatOptions> DOBezier2Move(this Transform transform, Vector3 startPos, Vector3 controlPos, Vector3 endPos, float duration, Ease ease = Ease.InSine)
        {
            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () => 0f,
                (progress) =>
                {
                    transform.position = Bezier2(startPos, controlPos, endPos, progress);
                }
                , 1f, duration)
                .SetEase(ease);
            tweenerCore.SetTarget(transform);
            return tweenerCore;
        }
        /// <summary>
        /// 3阶贝塞尔动画
        /// （世界坐标系下）
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="startPos">起始点</param>
        /// <param name="controlPos1">控制点1</param>
        /// <param name="controlPos2">控制点2</param>
        /// <param name="endPos">结束点</param>
        /// <param name="duration">动画时间</param>
        /// <param name="ease">缓动曲线</param>
        /// <returns>动画实例</returns>
        public static TweenerCore<float, float, FloatOptions> DOBezier3Move(this Transform transform, Vector3 startPos, Vector3 controlPos1, Vector3 controlPos2, Vector3 endPos, float duration, Ease ease = Ease.InSine)
        {
            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () => 0f,
                (progress) =>
                {
                    transform.position = Bezier3(startPos, controlPos1, controlPos2, endPos, progress);
                }
                , 1f, duration)
                .SetEase(ease);
            tweenerCore.SetTarget(transform);
            return tweenerCore;
        }



        /// <summary>
        /// 2阶贝塞尔动画
        /// （本地坐标系下）
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="startPos">起始点</param>
        /// <param name="controlPos">控制点</param>
        /// <param name="endPos">结束点</param>
        /// <param name="duration">动画时间</param>
        /// <param name="ease">缓动曲线</param>
        /// <returns>动画实例</returns>
        public static TweenerCore<float, float, FloatOptions> DOBezier2LocalMove(this Transform transform, Vector3 startPos, Vector3 controlPos, Vector3 endPos, float duration, Ease ease = Ease.InSine)
        {
            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () => 0f,
                (progress) =>
                {
                    transform.localPosition = Bezier2(startPos, controlPos, endPos, progress);
                }
                , 1f, duration)
                .SetEase(ease);
            tweenerCore.SetTarget(transform);
            return tweenerCore;
        }
        /// <summary>
        /// 3阶贝塞尔动画
        /// （本地坐标系下）
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="startPos">起始点</param>
        /// <param name="controlPos1">控制点1</param>
        /// <param name="controlPos2">控制点2</param>
        /// <param name="endPos">结束点</param>
        /// <param name="duration">动画时间</param>
        /// <param name="ease">缓动曲线</param>
        /// <returns>动画实例</returns>
        public static TweenerCore<float, float, FloatOptions> DOBezier3LocalMove(this Transform transform, Vector3 startPos, Vector3 controlPos1, Vector3 controlPos2, Vector3 endPos, float duration, Ease ease = Ease.InSine)
        {
            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () => 0f,
                (progress) =>
                {
                    transform.localPosition = Bezier3(startPos, controlPos1, controlPos2, endPos, progress);
                }
                , 1f, duration)
                .SetEase(ease);
            tweenerCore.SetTarget(transform);
            return tweenerCore;
        }

        /// <summary>
        /// 预设了控制点和缓动曲线的2阶贝塞尔曲线动画
        /// （世界坐标系下）
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="endPos">结束点</param>
        /// <param name="duration">动画时间</param>
        /// <returns>动画实例</returns>
        public static TweenerCore<float, float, FloatOptions> DOBezier2Move(this Transform transform, Vector3 endPos, float duration)
        {
            Vector3 startPos = transform.position;
            Vector3 controlPos = new Vector3();
            bool isNearX = Mathf.Abs(startPos.x - endPos.x) < Mathf.Abs(startPos.y - endPos.y);
            if (isNearX)
            {
                controlPos.x = startPos.x;
                controlPos.y = endPos.y;
            }
            else
            {
                controlPos.x = endPos.x;
                controlPos.y = startPos.y;
            }

            return transform.DOBezier2Move(startPos, controlPos, endPos, duration);
        }

        /// <summary>
        /// 预设了控制点和缓动曲线的2阶贝塞尔曲线动画
        /// （本地坐标系下）
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="endPos">结束点</param>
        /// <param name="duration">动画时间</param>
        /// <returns>动画实例</returns>
        public static TweenerCore<float, float, FloatOptions> DOBezier2LocalMove(this Transform transform, Vector3 endPos, float duration, float jumpHeight = 0)
        {
            Vector3 startPos = transform.localPosition;
            Vector3 controlPos = new Vector3();
            bool isNearX = Mathf.Abs(startPos.x - endPos.x) < Mathf.Abs(startPos.y - endPos.y);
            if (isNearX)
            {
                controlPos.x = startPos.x;
                controlPos.y = endPos.y + jumpHeight;
            }
            else
            {
                controlPos.x = endPos.x;
                controlPos.y = startPos.y + jumpHeight;
            }

            return transform.DOBezier2LocalMove(startPos, controlPos, endPos, duration);
        }




        #endregion

    }
}