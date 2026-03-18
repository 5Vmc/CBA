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
    public class HomeUIGuide : MonoBehaviour
    {
        [SerializeField] private RectTransform guideLayer;
        [SerializeField] private Image clickMaskImage = null;
        [SerializeField] private BlackHole blackHole = null;

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            OnGuideClickRecruitEnd();
            OnGuideClickCardEnd();
            //OnGuideClickMailButtonEnd();
            OnGuideClickClassicEnd();
        }

        public void CheckGuide()
        {
            // 进入这个界面代表最开始的董事会来信过了
            if (!GuideManager.IsFinished(GuideID.directorsLetter))
            {
                GuideManager.Finish(GuideID.directorsLetter);
            }

            // 弹窗后与董事会谈话
            if (!GuideManager.IsFinished(GuideID.directorsTalk))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    var content = Lang.Get(LangID.Guide1Txt).Replace("{name}", Player.Name);
                    // 打开弹窗
                    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(content, Lang.Get(LangID.ConfirmTxt), OnGuideDirectorsTalk));
                });
                return;
            }

            // 弹窗后与球队成员谈话
            if (!GuideManager.IsFinished(GuideID.teamTalk))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideTeamTalk();
                });
                return;
            }

            // 引导战斗展示
            if (!GuideManager.IsFinished(GuideID.fightShow))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideFightShow();
                });
                return;
            }

            //// 引导战斗展示
            //if (!GuideManager.IsFinished(GuideID.fightShow))
            //{
            //    TouchManager.Instance.DisableTouch();
            //    Timer.Register(this.gameObject, 0.3f, () =>
            //    {
            //        TouchManager.Instance.EnableTouch();
            //        OnGuideFightShow();
            //    });
            //    return;
            //}

            // // 发送引导邮件
            // if (!GuideManager.IsFinished(GuideID.sendGuideMail))
            // {
            //     GuideManager.DoGuide(GuideID.sendGuideMail);
            //     TouchManager.Instance.DisableTouch();
            //     GuideManager.SendGuideEndEmail();
            //     GuideManager.Finish(GuideID.sendGuideMail);
            //     Timer.Register(this.gameObject, 0.3f, () =>
            //     {
            //         TouchManager.Instance.EnableTouch();
            //         OnGuideUseGuideMail();
            //     });
            //     return;
            // }
            // 领取邮件附件
            // if (!GuideManager.IsFinished(GuideID.UseGuideMail))
            // {
            //     //GuideManager.DoGuide(GuideID.UseGuideMail);
            //     TouchManager.Instance.DisableTouch();
            //     Timer.Register(this.gameObject, 0.3f, () =>
            //     {
            //         TouchManager.Instance.EnableTouch();
            //         OnGuideUseGuideMail();
            //     });
            //     return;
            // }

            //战斗总结
            //if (!GuideManager.IsFinished(GuideID.popWindowAfterFightShow))
            //{
            //    TouchManager.Instance.DisableTouch();
            //    Timer.Register(this.gameObject, 0.3f, () =>
            //    {
            //        TouchManager.Instance.EnableTouch();
            //        // 完成引导4
            //        GuideManager.Finish(GuideID.popWindowAfterFightShow);
            //        var content = Lang.Get(LangID.Guide4Txt);
            //        // 打开引导4弹窗
            //        UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(content, Lang.Get(LangID.GoTxt), OnGuideTalkClassic));
            //    });
            //    return;
            //}

            //通过1-3
            if (!GuideManager.IsFinished(GuideID.guidePass13))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.6f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideTalkClassic();
                });
                return;
            }

            //领取第三个箱子
            if (!GuideManager.IsFinished(GuideID.guideGetProgressBox3))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideTalkProgress();
                });
                return;
            }

            //去抽卡
            if (!GuideManager.IsFinished(GuideID.guideGetNewPlayer))
            {
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideSwitchToRecruit();
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
                    OnGuideUpLevelPlayerTip();
                });
                return;
            }

            //升级卡牌后返回战斗
            if (!GuideManager.IsFinished(GuideID.guideGotoFightAfterUpLevel))
            {
                if (!GuideManager.IsGuideDoing(GuideID.guideGotoFightAfterUpLevel)) GuideManager.DoGuide(GuideID.guideGotoFightAfterUpLevel);
                TouchManager.Instance.DisableTouch();
                Timer.Register(this.gameObject, 0.6f, () =>
                {
                    TouchManager.Instance.EnableTouch();
                    OnGuideClickClassicStart();
                });
                return;
            }
        }



        // 和懂事会对话引导
        private void OnGuideDirectorsTalk()
        {
            if (GuideManager.IsFinished(GuideID.directorsTalk)) return;
            GuideManager.DoGuide(GuideID.directorsTalk);
            var properties = new Guide2UIProperties(Configs.GuideDialogue.GetConfig(StartDialogueID.Dialogue1), OnGuideTeamTalk);
            // 打开对话面板
            UIController.Instance.OpenWindow<Guide2UI>(properties);
        }

        // 和球队球员见面
        private void OnGuideTeamTalk()
        {
            if (GuideManager.IsFinished(GuideID.teamTalk)) return;
            GuideManager.DoGuide(GuideID.teamTalk);
            var content = Lang.Get(LangID.Guide2Txt);
            UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(content, Lang.Get(LangID.GoTxt), () =>
            {
                GuideManager.Finish(GuideID.teamTalk);
                var properties = new Guide2UIProperties(Configs.GuideDialogue.GetConfig(StartDialogueID.Dialogue2), OnGuideFightShow);
                // 打开对话面板
                UIController.Instance.OpenWindow<Guide2UI>(properties);
            }));
        }

        // 开始比赛
        private void OnGuideFightShow()
        {
            if (GuideManager.IsFinished(GuideID.fightShow)) return;
            GuideManager.DoGuide(GuideID.fightShow);
            var content = Lang.Get(LangID.Guide3Txt);

            // 打开弹窗
            UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(content, Lang.Get(LangID.GoTxt), () =>
            {
                // 开始比赛
                NetworkManager.Instance.GuideChallenge(response =>
                {
                    Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Guide;
                    Player.BattleManager.SetFightInfo(FightType.PVE, response.Fight);
                    Player.BattleManager.StartPlayFight();
                    GuideManager.Finish(GuideID.fightShow);
                });
            }));
        }

        #region 引导领取邮件附件

        //[SerializeField] private Button emailButton = null;
        //[SerializeField] private GridLayoutGroup rightButtonPanelLayout = null;
        //bool isGuideClickMailButtonDoing = false;
        //private void OnGuideUseGuideMail()
        //{
        //    if (GuideManager.IsFinished(GuideID.UseGuideMail)) return;
        //    if (!GuideManager.IsGuideDoing(GuideID.UseGuideMail)) GuideManager.DoGuide(GuideID.UseGuideMail);
        //    isGuideClickMailButtonDoing = true;
        //    guideLayer.gameObject.SetActive(true);
        //    blackHole.gameObject.DOKill();
        //    blackHole.gameObject.SetActive(true);
        //    blackHole.Radius = 5000;
        //    rightButtonPanelLayout.enabled = false;
        //    emailButton.transform.SetParent(guideLayer);
        //    Timer.Register(this.gameObject, 0.25f, () =>
        //    {
        //        blackHole.Locate(emailButton.transform);
        //        // 黑圈缩小
        //        DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
        //        {
        //            Sequence sequence = DOTween.Sequence();
        //            sequence.AppendInterval(5.0f);
        //            sequence.Append(DOTween.To(value => blackHole.Radius = value, 150, 1500, 0.3f));
        //            sequence.Append(DOTween.To(value => blackHole.Radius = value, 1500, 150, 0.8f));
        //            sequence.SetTarget(blackHole.gameObject);
        //            sequence.SetLoops(-1);
        //        });
        //    });
        //}
        //public void OnGuideClickMailButtonEnd()
        //{
        //    if (isGuideClickMailButtonDoing == false) return;
        //    guideLayer.gameObject.SetActive(false);
        //    blackHole.gameObject.DOKill();
        //    blackHole.gameObject.SetActive(false);
        //    emailButton.transform.SetParent(rightButtonPanelLayout.transform);
        //    rightButtonPanelLayout.enabled = true;
        //    isGuideClickMailButtonDoing = false;
        //}

        #endregion

        #region 引导前往经典赛
        //经典赛介绍
        private void OnGuideTalkClassic()
        {
            if (GuideManager.IsFinished(GuideID.guidePass13)) return;
            GuideManager.DoGuide(GuideID.guidePass13);
            var properties = new GuideTalkUIProperties("球队需要在磨砺中成长，我们首先去参加经典赛，与各地的队伍较量，获得成长经验。", () =>
            {
                OnGuideClickClassicStart();
            });
            // 打开对话面板
            UIController.Instance.OpenWindow<GuideTalkUI>(properties);
        }

        [SerializeField] private Button classicPVEBtn = null;
        private bool isGuideClickClassicDoing = false;
        //引导点击经典赛
        private void OnGuideClickClassicStart()
        {
            isGuideClickClassicDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            classicPVEBtn.transform.SetParent(guideLayer);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                blackHole.Locate(classicPVEBtn.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }

        [SerializeField] private RectTransform navigationItemParent = null;
        private void OnGuideClickClassicEnd()
        {
            if (!isGuideClickClassicDoing) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(false);
            classicPVEBtn.transform.SetParent(navigationItemParent);
            isGuideClickClassicDoing = false;
        }
        #endregion

        #region 引导点击经典赛国家界面点星星奖励中最后一个
        //进度奖励介绍
        private void OnGuideTalkProgress()
        {
            GuideManager.DoGuide(GuideID.guideGetProgressBox3);
            GuideManager.DoGuide(GuideID.guideGetProgressBox3Tip);
            var properties = new GuideTalkUIProperties("通关后的星星可以累积来获取奖励，我们还有奖励未领取，现在让我们领取一下", () =>
            {
                GuideManager.Finish(GuideID.guideGetProgressBox3Tip);
                OnGuideClickClassicStart();
            });
            // 打开对话面板
            UIController.Instance.OpenWindow<GuideTalkUI>(properties);
        }
        #endregion

        #region 引导前往抽卡

        [SerializeField] private RectTransform homeCenterButtonRecruit = null;
        [SerializeField] private BabuButton recruitBabuButton = null;
        [SerializeField] private RectTransform fingerPanel = null;

        //前往抽卡
        private void OnGuideSwitchToRecruit()
        {
            if (!GuideManager.IsGuideDoing(GuideID.guideGetNewPlayer)) GuideManager.DoGuide(GuideID.guideGetNewPlayer);
            if (GuideManager.IsFinished(GuideID.guideGetNewPlayerTip) == false)
            {
                GuideManager.DoGuide(GuideID.guideGetNewPlayerTip);
                var properties = new GuideTalkUIProperties("我们已经拥有了一张球探契约，现在，让我们去发掘一名新的球星", () =>
                {
                    GuideManager.Finish(GuideID.guideGetNewPlayerTip);
                    OnGuideClickRecruitStart();
                });
                // 打开对话面板
                UIController.Instance.OpenWindow<GuideTalkUI>(properties);
            }
            else
            {
                OnGuideClickRecruitStart();
            }
        }
        bool isGuideClickRecruitDoing = false;
        public void OnGuideClickRecruitStart()
        {
            isGuideClickRecruitDoing = true;
            recruitBabuButton.transform.SetParent(guideLayer);
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(false);
            blackHole.Radius = 0;
            float y = Utility.ConvertLocalPosition(homeCenterButtonRecruit.transform.parent, homeCenterButtonRecruit.transform.localPosition, fingerPanel.parent).y;
            float x = Utility.ConvertLocalPosition(recruitBabuButton.transform.parent, recruitBabuButton.transform.localPosition, fingerPanel.parent).x;
            fingerPanel.localPosition = new Vector3(x, y, 0);
            fingerPanel.SetAsLastSibling();
            fingerPanel.gameObject.SetActive(true);
        }
        public void OnGuideClickRecruitEnd()
        {
            if (isGuideClickRecruitDoing == false) return;
            fingerPanel.gameObject.SetActive(false);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(false);
            guideLayer.gameObject.SetActive(false);
            recruitBabuButton.transform.SetParent(homeCenterButtonRecruit.transform);
            recruitBabuButton.transform.SetAsFirstSibling();
            isGuideClickRecruitDoing = false;
        }

        #endregion

        #region 引导前往球员升级

        //球员升级介绍
        private void OnGuideUpLevelPlayerTip()
        {
            if (!GuideManager.IsGuideDoing(GuideID.guideUpLevelPlayer)) GuideManager.DoGuide(GuideID.guideUpLevelPlayer);
            if (GuideManager.IsFinished(GuideID.guideUpLevelPlayerTip) == false)
            {
                GuideManager.DoGuide(GuideID.guideUpLevelPlayerTip);
                var properties = new GuideTalkUIProperties("现在让我们对一名球员进行培养，提升他的能力", () =>
                {
                    GuideManager.Finish(GuideID.guideUpLevelPlayerTip);
                    OnGuideClickCardStart();
                });
                // 打开对话面板
                UIController.Instance.OpenWindow<GuideTalkUI>(properties);
            }
            else
            {
                OnGuideClickCardStart();
            }
        }

        [SerializeField] private Button cardBtn = null;
        bool isGuideClickCardDoing = false;
        public void OnGuideClickCardStart()
        {
            Debug.Log("OnGuideClickCardStart");
            isGuideClickCardDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            cardBtn.transform.SetParent(guideLayer);
            Timer.Register(this.gameObject, 0.25f, () =>
            {
                blackHole.Locate(cardBtn.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {

                });
            });
        }
        public void OnGuideClickCardEnd()
        {
            Debug.Log("OnGuideClickCardEnd");
            if (isGuideClickCardDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.DOKill();
            blackHole.gameObject.SetActive(false);
            cardBtn.transform.SetParent(navigationItemParent);
            isGuideClickCardDoing = false;
        }

        #endregion


    }
}