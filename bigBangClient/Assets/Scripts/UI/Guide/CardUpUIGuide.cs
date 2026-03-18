using System;
using System.Collections.Generic;
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

namespace BigBang.UI
{
    public class CardUpUIGuide : MonoBehaviour
    {
        [SerializeField] private RectTransform guideLayer;
        [SerializeField] private Image clickMaskImage = null;
        [SerializeField] private BlackHole blackHole = null;

        public void CheckGuide()
        {
            if (!GuideManager.IsFinished(GuideID.guideUpLevelPlayer))
            {
                OnGuideClickUpStart();
                return;
            }
        }

        private void OnEnable()
        {
            autoUpLevelBtn.OnClick += OnGuideClickUpEnd;
            closeBtn.onClick.AddListener(OnGuideGotoFightAfterUpLevelEnd);
        }
        private void OnDisable()
        {
            autoUpLevelBtn.OnClick -= OnGuideClickUpEnd;
            closeBtn.onClick.RemoveListener(OnGuideGotoFightAfterUpLevelEnd);
        }

        #region 引导点击一键升级

        [SerializeField] private BabuButton autoUpLevelBtn = null;
        [SerializeField] private RectTransform levelPanel = null;
        bool isGuideClickUpDoing = false;
        public void OnGuideClickUpStart()
        {
            isGuideClickUpDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            autoUpLevelBtn.transform.SetParent(guideLayer.transform);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                if (autoUpLevelBtn.gameObject.activeInHierarchy == false)
                {
                    OnGuideClickUpEnd();
                    return;
                }
                blackHole.Locate(autoUpLevelBtn.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        public void OnGuideClickUpEnd(BabuButton _)
        {
            OnGuideClickUpEnd();
        }
        public void OnGuideClickUpEnd()
        {
            if (isGuideClickUpDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.SetActive(false);
            autoUpLevelBtn.transform.SetParent(levelPanel);
            isGuideClickUpDoing = false;
            OnGuideGameCircleTip();
        }

        //战斗循环介绍
        private void OnGuideGameCircleTip()
        {
            var properties = new GuideTalkUIProperties("就是这样，参加经典赛，拿契约，培养球员，继续经典赛。。。", () =>
            {
                GuideManager.Finish(GuideID.guideUpLevelPlayer);
                OnGuideGotoFightAfterUpLevelStart();
            });
            // 打开对话面板
            UIController.Instance.OpenWindow<GuideTalkUI>(properties);
        }

        #endregion

        #region 升级卡牌后引导返回战斗

        [SerializeField] private Image bottom = null;
        [SerializeField] private Button closeBtn = null;
        bool isGuideGotoFightAfterUpLevel = false;
        public void OnGuideGotoFightAfterUpLevelStart()
        {
            if (!GuideManager.IsGuideDoing(GuideID.guideGotoFightAfterUpLevel)) GuideManager.DoGuide(GuideID.guideGotoFightAfterUpLevel);
            isGuideGotoFightAfterUpLevel = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            closeBtn.transform.SetParent(guideLayer.transform);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                if (closeBtn.gameObject.activeInHierarchy == false)
                {
                    OnGuideGotoFightAfterUpLevelEnd();
                    return;
                }
                blackHole.Locate(closeBtn.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        public void OnGuideGotoFightAfterUpLevelEnd()
        {
            if (isGuideGotoFightAfterUpLevel == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.SetActive(false);
            closeBtn.transform.SetParent(bottom.transform);
            isGuideGotoFightAfterUpLevel = false;
        }


        #endregion
    }
}