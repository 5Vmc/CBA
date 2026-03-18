using System;
using Babu;
using BigBang.Animation;
using BigBang.Battle;
using DG.Tweening;
using GameConfig;
using Spine;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using static BigBang.ClassicManager;
using static BigBang.SpriteNames;

namespace BigBang.UI
{
    public class RecruitUIGuide : MonoBehaviour
    {
        [SerializeField] private RectTransform guideLayer;
        [SerializeField] private Image clickMaskImage = null;
        [SerializeField] private BlackHole blackHole = null;

        public void CheckGuide()
        {
            if (GuideManager.IsGuideDoing(GuideID.guideGetNewPlayer))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideRecruitOnceStart();
                });
                return;
            }

            //去球员升级
            if (!GuideManager.IsFinished(GuideID.guideUpLevelPlayer))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideClickCardStart();
                });
                return;
            }
        }

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.OnStartRecruit, OnStartRecruit);
        }
        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.OnStartRecruit, OnStartRecruit);
            OnGuideClickCountryEnd();
            HideGuide();
            OnGuideClickCardEnd();
        }

        #region 引导点击抽卡

        [SerializeField] private RectTransform recruitBtnParent = null;
        [SerializeField] private RectTransform recruitOnceBtnTrans = null;
        [SerializeField] private Button recruitOnceBtn = null;
        [SerializeField] private RectTransform circleMidPos = null;
        bool isGuideClickRecruitDoing = false;
        public void OnGuideRecruitOnceStart()
        {
            isGuideClickRecruitDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            recruitOnceBtnTrans.SetParent(guideLayer.transform);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                int piecesId = GoodsId.RecruitPoint;
                var goods = new GoodsData(piecesId, 1);

                if (Player.PackageManager.IsGoodsEnough(goods) == false)
                {
                    OnGuideClickCountryEnd();
                    HideGuide();
                    UIController.Instance.ShowPanel<HomeUI>();
                    return;
                }
                blackHole.Locate(circleMidPos);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        private void OnStartRecruit(object[] _)
        {
            OnGuideClickCountryEnd();
        }
        public void HideGuide()
        {
            if (isGuideClickRecruitDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.SetActive(false);
            isGuideClickRecruitDoing = false;
        }
        public void OnGuideClickCountryEnd()
        {
            if (isGuideClickRecruitDoing == false) return;
            blackHole.gameObject.SetActive(false);
            recruitOnceBtnTrans.SetParent(recruitBtnParent.transform);
            GuideManager.Finish(GuideID.guideGetNewPlayer);
        }

        #endregion

        #region 引导前往球员升级

        [SerializeField] private RectTransform closeParent = null;
        [SerializeField] private BabuButton closeBtn = null;
        bool isGuideClickCardDoing = false;
        public void OnGuideClickCardStart()
        {
            isGuideClickCardDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            closeBtn.transform.SetParent(guideLayer);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                blackHole.Locate(closeBtn.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        public void OnGuideClickCardEnd()
        {
            if (isGuideClickCardDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.SetActive(false);
            closeBtn.transform.SetParent(closeParent);
            isGuideClickCardDoing = false;
        }

        #endregion
    }
}