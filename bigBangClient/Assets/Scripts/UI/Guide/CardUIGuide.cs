using System;
using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using BigBang.Battle;
using DG.Tweening;
using GameConfig;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class CardUIGuide : MonoBehaviour
    {
        [SerializeField] private RectTransform guideLayer;
        [SerializeField] private Image clickMaskImage = null;
        [SerializeField] private BlackHole blackHole = null;

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.OnClickCardUICard, OnClickCardUICard);
        }
        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.OnClickCardUICard, OnClickCardUICard);
            OnGuideClickHomeEnd();
        }

        public void CheckGuide()
        {
            if (GuideManager.IsGuideDoing(GuideID.guideUpLevelPlayer))
            {
                OnGuideClickCardStart();
                return;
            }
            if (GuideManager.IsGuideDoing(GuideID.guideGotoFightAfterUpLevel))
            {
                OnGuideClickHomeStart();
                return;
            }
        }

        #region 引导点击指定的球员

        private bool isGuideClickTargetCardDoing = false;
        public bool IsGuideClickTargetCardDoing
        {
            get
            {
                return isGuideClickTargetCardDoing;
            }
        }

        [SerializeField] private CardGridAdapter cardGridAdapter;
        private Transform cardParent;
        public void OnGuideClickCardStart()
        {
            isGuideClickTargetCardDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                if (GuideManager.UpLevelCardItem == null)
                {
                    OnGuideClickCardEnd();
                    GuideManager.Finish(new List<GuideID>() { GuideID.guideUpLevelPlayer, GuideID.starterGuide });
                    return;
                }
                cardGridAdapter.enabled = false;
                cardParent = GuideManager.UpLevelCardItem.transform.parent;
                cardParent.GetComponent<HorizontalLayoutGroup>().enabled = false;
                GuideManager.UpLevelCardItem.transform.SetParent(guideLayer.transform);
                blackHole.Locate(GuideManager.UpLevelCardItem.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        private void OnClickCardUICard(object[] _)
        {
            OnGuideClickCardEnd();
        }
        public void OnGuideClickCardEnd()
        {
            if (isGuideClickTargetCardDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.SetActive(false);

            if (GuideManager.UpLevelCardItem != null && cardParent != null)
            {
                GuideManager.UpLevelCardItem.transform.SetParent(cardParent);
                cardParent.GetComponent<HorizontalLayoutGroup>().enabled = true;
            }
            cardGridAdapter.enabled = true;

            isGuideClickTargetCardDoing = false;
        }

        #endregion


        #region 升级卡牌后引导返回战斗

        [SerializeField] private Button homeBtn = null;
        [SerializeField] private RectTransform navigationItemParent = null;
        bool isGuideGotoFightAfterUpLevel = false;
        public void OnGuideClickHomeStart()
        {
            isGuideGotoFightAfterUpLevel = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            homeBtn.transform.SetParent(guideLayer);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                blackHole.Locate(homeBtn.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        public void OnGuideClickHomeEnd()
        {
            if (isGuideGotoFightAfterUpLevel == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(false);
            homeBtn.transform.SetParent(navigationItemParent.transform);
            isGuideGotoFightAfterUpLevel = false;
        }

        #endregion

    }
}