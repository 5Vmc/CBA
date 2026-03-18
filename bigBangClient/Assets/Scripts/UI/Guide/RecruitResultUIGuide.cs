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

namespace BigBang.UI
{
    public class RecruitResultUIGuide : MonoBehaviour
    {
        [SerializeField] private RectTransform guideLayer;
        [SerializeField] private Image clickMaskImage = null;
        [SerializeField] private BlackHole blackHole = null;

        public void CheckGuide()
        {
            if (!GuideManager.IsFinished(GuideID.guideUpLevelPlayer))
            {
                GuideManager.DoGuide(GuideID.guideUpLevelPlayer);
                TouchManager.Instance.DisableTouch();
                return;
            }
        }
        public void AfterRecruitAnimPlayEnd()
        {
            TouchManager.Instance.EnableTouch();
            OnGuideUpLevelPlayerTip();
        }

        #region 引导点击关闭

        //球员升级介绍
        private void OnGuideUpLevelPlayerTip()
        {
            if (GuideManager.IsFinished(GuideID.guideUpLevelPlayerTip) == false)
            {
                GuideManager.DoGuide(GuideID.guideUpLevelPlayerTip);
                var properties = new GuideTalkUIProperties("我们获得了一名新球员，现在让我们对他进行培养", () =>
                {
                    GuideManager.Finish(GuideID.guideUpLevelPlayerTip);
                    OnGuideCloseWindowStart();
                });
                // 打开对话面板
                UIController.Instance.OpenWindow<GuideTalkUI>(properties);
            }
            else
            {
                if (GuideManager.IsGuideDoing(GuideID.guideUpLevelPlayer))
                {
                    OnGuideCloseWindowStart();
                }
            }
        }

        [SerializeField] private RectTransform recruitResultUI = null;
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private RectTransform circleMidPos = null;
        bool isGuideCloseWindowDoing = true;
        public void OnGuideCloseWindowStart()
        {
            isGuideCloseWindowDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            closeBtn.transform.SetParent(guideLayer.transform);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                blackHole.Locate(circleMidPos);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }

        private void OnEnable()
        {
            closeBtn.onClick.AddListener(OnGuideCloseWindowEnd);
        }
        private void OnDisable()
        {
            closeBtn.onClick.RemoveListener(OnGuideCloseWindowEnd);
            OnGuideCloseWindowEnd();
        }

        public void OnGuideCloseWindowEnd()
        {
            if (isGuideCloseWindowDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.SetActive(false);
            closeBtn.transform.SetParent(recruitResultUI);
            isGuideCloseWindowDoing = false;
        }

        #endregion
    }
}