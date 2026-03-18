using System;
using System.Collections.Generic;
using BigBang.Animation;
using BigBang.Battle;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class WorldMapUIGuide : MonoBehaviour
    {
        [SerializeField] private RectTransform guideLayer = null;
        [SerializeField] private Image clickMaskImage = null;
        [SerializeField] private BlackHole blackHole = null;

        private Func<Transform> getFirstNodeTransformFunc;
        private Func<Transform> getLastNodeTransformFunc;
        private Camera camera3D;
        public void CheckGuide(Camera camera3D, Func<Transform> getFirstNodeTransformFunc, Func<Transform> getLastNodeTransformFunc)
        {
            this.camera3D = camera3D;
            this.getFirstNodeTransformFunc = getFirstNodeTransformFunc;
            this.getLastNodeTransformFunc = getLastNodeTransformFunc;
            if (GuideManager.IsGuideDoing(GuideID.guidePass13)
            || GuideManager.IsGuideDoing(GuideID.guideGetProgressBox3))
            {
                OnGuideClickCountryStart(true);
                return;
            }
            if (GuideManager.IsGuideDoing(GuideID.guideGotoFightAfterUpLevel))
            {
                OnGuideClickCountryStart(false);
                return;
            }
        }
        private bool isGuideClickCountryDoing = false;
        public bool IsGuideClickCountryDoing
        {
            get
            {
                return isGuideClickCountryDoing;
            }
        }
        private Transform nodeTransform;
        //引导点击第一个国家
        public void OnGuideClickCountryStart(bool isFirst)
        {
            isGuideClickCountryDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(false);
            Timer.Register(this.gameObject, 0.45f, () =>
            {
                blackHole.gameObject.SetActive(true);
                blackHole.Radius = 5000;
                // 黑圈定位
                if (isFirst)
                    nodeTransform = getFirstNodeTransformFunc?.Invoke();
                else
                    nodeTransform = getLastNodeTransformFunc?.Invoke();
                blackHole.Locate(camera3D, nodeTransform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150 * UIFrame.scale, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        public void OnHideHole()
        {
            if (isGuideClickCountryDoing == false) return;
            DOTween.To(value => blackHole.Radius = value, blackHole.Radius, 5000, 0.4f).OnComplete(() =>
            {

            });
        }
        public void OnGuideClickCountryEnd()
        {
            if (isGuideClickCountryDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            isGuideClickCountryDoing = false;
        }

        private void Update()
        {
            if (isGuideClickCountryDoing == false) return;
            if (nodeTransform == null) return;
            blackHole.Locate(camera3D, nodeTransform);
        }
    }
}