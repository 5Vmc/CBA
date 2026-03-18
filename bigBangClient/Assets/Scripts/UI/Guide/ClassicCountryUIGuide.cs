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
    public class ClassicCountryUIGuide : MonoBehaviour
    {
        [SerializeField] private Image guideLayer = null;
        [SerializeField] private ClassicTeamItemAdapter classicTeamItemAdapter;

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.ClassicCountryUIOnClickClallengeButton, ClassicCountryUIOnClickClallengeButton);
        }
        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.ClassicCountryUIOnClickClallengeButton, ClassicCountryUIOnClickClallengeButton);
        }

        public void CheckGuide()
        {
            if (GuideManager.IsGuideDoing(GuideID.guidePass13))
            {
                OnGuideClickClubStart();
                return;
            }
            if (GuideManager.IsGuideDoing(GuideID.guideGetProgressBox3) || !GuideManager.IsFinished(GuideID.guideGetProgressBox3))
            {
                OnGuideTalkProgress();
                return;
            }
            if (GuideManager.IsGuideDoing(GuideID.guideGotoFightAfterUpLevel))
            {
                GuideManager.Finish(new List<GuideID>() { GuideID.guideGotoFightAfterUpLevel, GuideID.starterGuide });
                return;
            }
        }

        [SerializeField] private RectTransform fingerPanel = null;

        #region 引导点击经典赛国家界面点列表中最后一个俱乐部

        private bool isGuideClickClubDoing = false;
        public bool IsGuideClickCountryDoing
        {
            get
            {
                return isGuideClickClubDoing;
            }
        }
        ClassicTeamItem endClassicTeamItem = null;
        public void OnGuideClickClubStart()
        {
            isGuideClickClubDoing = true;
            guideLayer.gameObject.SetActive(true);
            guideLayer.SetAlpha(0);
            Timer.Register(this.gameObject, 0.6f, () =>
            {
                endClassicTeamItem = classicTeamItemAdapter.GetItemViewsHolder(classicTeamItemAdapter.VisibleItemsCount - 1).item;
                classicTeamItemAdapter.enabled = false;
                endClassicTeamItem.transform.SetParent(guideLayer.transform);
                guideLayer.TweenAlpha(100 / 255f, 0.6f).OnComplete(() =>
                {
                    fingerPanel.gameObject.SetActive(true);
                    Transform challengeOnceButtonTrans = endClassicTeamItem.GetChallengeOnceButtonTrans();
                    Vector3 offset = new Vector3(20, 0, 0);
                    fingerPanel.localPosition = Utility.ConvertLocalPosition(challengeOnceButtonTrans.parent, challengeOnceButtonTrans.localPosition, fingerPanel.parent) + offset;
                    fingerPanel.gameObject.SetAlpha(0);
                    fingerPanel.gameObject.DOFade(1, 0.6f);
                    fingerPanel.SetAsLastSibling();
                });
            });
        }
        private void ClassicCountryUIOnClickClallengeButton(object[] _)
        {
            OnGuideClickClubEnd();
        }
        public void OnGuideClickClubEnd()
        {
            if (isGuideClickClubDoing == false) return;
            if (endClassicTeamItem != null)
            {
                endClassicTeamItem.transform.SetParent(classicTeamItemAdapter.Content);
                endClassicTeamItem = null;
            }
            fingerPanel.gameObject.SetActive(false);
            guideLayer.gameObject.SetActive(false);
            isGuideClickClubDoing = false;
            classicTeamItemAdapter.enabled = true;
        }

        #endregion

        #region 引导点击经典赛国家界面点星星奖励中最后一个

        //进度奖励介绍
        private void OnGuideTalkProgress()
        {
            if (!GuideManager.IsGuideDoing(GuideID.guideGetProgressBox3)) GuideManager.DoGuide(GuideID.guideGetProgressBox3);
            if (GuideManager.IsFinished(GuideID.guideGetProgressBox3Tip) == false)
            {
                GuideManager.DoGuide(GuideID.guideGetProgressBox3Tip);
                var properties = new GuideTalkUIProperties("通关后的星星可以累积来获取奖励，我们还有奖励未领取，现在让我们领取一下", () =>
                {
                    GuideManager.Finish(GuideID.guideGetProgressBox3Tip);
                    OnGuideClickBox3Start();
                });
                // 打开对话面板
                UIController.Instance.OpenWindow<GuideTalkUI>(properties);
            }
            else
            {
                OnGuideClickBox3Start();
            }
        }

        private bool isGuideClickBox3Doing = false;
        public bool IsGuideClickBox3Doing
        {
            get
            {
                return isGuideClickBox3Doing;
            }
        }
        [SerializeField] ClassicTaskProgressItem taskProgressItem = null;
        [SerializeField] private RectTransform titles = null;
        [SerializeField] private RectTransform Box3 = null;
        public void OnGuideClickBox3Start()
        {
            taskProgressItem.SetAfterGetRewardCallBack(AfterGetBox3Reward);
            isGuideClickBox3Doing = true;
            if (taskProgressItem.gotRewardList[2] == 1)
            {
                OnGuideClickBox3End();
                return;
            }
            guideLayer.gameObject.SetActive(true);
            guideLayer.SetAlpha(0);
            Timer.Register(this.gameObject, 0.6f, () =>
            {
                taskProgressItem.transform.SetParent(guideLayer.transform);
                guideLayer.TweenAlpha(100 / 255f, 0.6f).OnComplete(() =>
                {
                    fingerPanel.gameObject.SetActive(true);
                    Vector3 offset = new Vector3(10, 0, 0);
                    fingerPanel.localPosition = Utility.ConvertLocalPosition(Box3.parent, Box3.localPosition, fingerPanel.parent) + offset;
                    fingerPanel.gameObject.SetAlpha(0);
                    fingerPanel.gameObject.DOFade(1, 0.6f);
                    fingerPanel.SetAsLastSibling();
                });
            });
        }
        private void AfterGetBox3Reward(int boxIndex)
        {
            if (boxIndex != 2) return;
            OnGuideClickBox3End();
        }
        public void OnGuideClickBox3End()
        {
            if (isGuideClickBox3Doing == false) return;
            taskProgressItem.transform.SetParent(titles);
            fingerPanel.gameObject.SetActive(false);
            guideLayer.gameObject.SetActive(false);
            isGuideClickBox3Doing = false;
            GuideManager.Finish(GuideID.guideGetProgressBox3);
            OnGuideGetNewPlayer();
        }

        #endregion

        #region 引导前往抽卡

        //进度奖励介绍
        private void OnGuideGetNewPlayer()
        {
            GuideManager.DoGuide(GuideID.guideGetNewPlayer);
            GuideManager.DoGuide(GuideID.guideGetNewPlayerTip);
            var properties = new GuideTalkUIProperties("恭喜您获得了一张球探契约，现在，让我们去发掘一名新的球星", () =>
            {
                GuideManager.Finish(GuideID.guideGetNewPlayerTip);
                UIController.Instance.CloseAllPanelAndWindow();
                UIController.Instance.ShowPanel<HomeUI>();
            });
            // 打开对话面板
            UIController.Instance.OpenWindow<GuideTalkUI>(properties);
        }

        #endregion
    }
}