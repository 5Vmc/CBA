using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;
using BigBang.Animation;
using Babu;
using System;
using Google.Protobuf.Collections;
using Protocol;

namespace BigBang.UI
{
    public class ClassicTaskProgressItem : MonoBehaviour
    {
        [SerializeField] private Image progressValue;
        [SerializeField] private RectTransform obtain;
        [SerializeField] private List<BabuButton> rewardList;
        [SerializeField] private List<TMP_Text> pointList;
        [SerializeField] private List<RectTransform> positions;
        [SerializeField] private List<InventoryItem> items;
        [SerializeField] private TMP_Text pointTxt;
        [SerializeField] private RectTransform pointImgRect;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite dailySprite;
        [SerializeField] private Sprite weeklySprite;

        [SerializeField] public TaskProgressItemAnim Anim;

        [SerializeField] private BabuButton closeTipButton;


        // 活跃点位置,数值飞去的目标位置
        public static RectTransform PointImgPos;

        private List<ProgressBoxInfo> progressBoxInfoList = new();
        private class ProgressBoxInfo
        {
            public int id;//1,2,3
            public int needStar;
            public string reward;
        }

        // 最大奖励数量
        private const int MAX_REWARD_ITEM = 3;

        private void Awake()
        {
            // 关闭动画
            rewardList.ForEach(item => item.Anim = null);
            // 启用点击弹出道具Tips
            items.ForEach(item => item.canShowTip = true);
            PointImgPos = pointImgRect;
        }

        /// <summary> 屏幕被点击后隐藏弹窗 </summary>
        private void OnClickCloseTipButton(BabuButton sender)
        {
            obtain.gameObject.SetActive(false);
            closeTipButton.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            rewardList.ForEach(item => item.OnClick += OnReward);
            closeTipButton.OnClick += OnClickCloseTipButton;
        }

        private void OnDisable()
        {
            rewardList.ForEach(item => item.OnClick -= OnReward);
            closeTipButton.OnClick -= OnClickCloseTipButton;
        }

        private Action<int> afterGetRewardCallBack;
        public void SetAfterGetRewardCallBack(Action<int> afterGetRewardCallBack)
        {
            this.afterGetRewardCallBack = afterGetRewardCallBack;
        }

        // 点击领取奖励
        private void OnReward(BabuButton sender)
        {
            // 点击的按钮下标
            int index = rewardList.IndexOf(sender);
            // 如果可以领取,则领取宝箱
            if (currentStar >= progressBoxInfoList[index].needStar)
            {
                // 如果已经领取过了
                if (gotRewardList[index] == 1)
                {
                    AudioManager.Instance.PlaySound(AudioNames.BTN_2);
                    SetBoxContent(index);
                    return;
                }
                sender.GetComponent<RewardBoxAnim>().Play();//只更新这一个箱子就够了
                AudioManager.Instance.PlaySound(AudioNames.BTN_STREN);
                NetworkManager.Instance.CollectChapterBoxReward(showCountryId, progressBoxInfoList[index].id, response =>
                {
                    if (uiType == UIType.Classic)
                    {
                        ChapterInfo chapterInfo = ClassicManager.Instance.chapterInfoDic[showCountryId];
                        chapterInfo.Rewards[index] = 1;

                        var chapterInfo1 = ClassicManager.Instance.classicCountryLevelDataDic[showCountryId];
                        chapterInfo1.Rewards[index] = 1;
                    }
                    if (uiType == UIType.Hero)
                    {
                        ChapterInfo chapterInfo = HeroManager.Instance.chapterInfoDic[showCountryId];
                        chapterInfo.Rewards[index] = 1;

                        var chapterInfo1 = HeroManager.Instance.heroChapterDataDic[showCountryId];
                        chapterInfo1.Rewards[index] = 1;
                    }

                    gotRewardList[index] = 1;

                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(progressBoxInfoList[index].reward).ToList(), () =>
                    {
                        afterGetRewardCallBack?.Invoke(index);
                    });
                    // 打开通用收益界面
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);

                    SetBox();
                });
            }
            else
            {
                // 显示宝箱内容
                SetBoxContent(index);
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            }
        }

        private int oldStar = 0;
        private int currentStar = 0;
        private int oldShowCountryId = 0;
        private int showCountryId = 0;
        private int maxStar = 0;
        public RepeatedField<int> gotRewardList = new() { 0, 0, 0 };

        public string RedDotPath { get; set; }


        public enum UIType
        {
            Classic,
            Hero,
        }

        private UIType uiType = UIType.Classic;


        public void SetClassicData(int showCountryId, int currentStar, int maxStar, RepeatedField<int> gotRewardList, bool playAnim = true)
        {
            uiType = UIType.Classic;
            this.oldShowCountryId = this.showCountryId;
            this.showCountryId = showCountryId;
            this.oldStar = this.currentStar;
            this.currentStar = currentStar;
            this.maxStar = maxStar;
            this.gotRewardList = gotRewardList;

            if (oldShowCountryId != showCountryId)
            {
                this.oldStar = 0;
            }

            progressBoxInfoList.Clear();
            ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(showCountryId);
            ProgressBoxInfo progressBoxInfo1 = new();
            progressBoxInfo1.id = 1;
            progressBoxInfo1.needStar = challengeCountryConfig.Star1;
            progressBoxInfo1.reward = challengeCountryConfig.Reward1;
            progressBoxInfoList.Add(progressBoxInfo1);
            ProgressBoxInfo progressBoxInfo2 = new();
            progressBoxInfo2.id = 2;
            progressBoxInfo2.needStar = challengeCountryConfig.Star2;
            progressBoxInfo2.reward = challengeCountryConfig.Reward2;
            progressBoxInfoList.Add(progressBoxInfo2);
            ProgressBoxInfo progressBoxInfo3 = new();
            progressBoxInfo3.id = 3;
            progressBoxInfo3.needStar = challengeCountryConfig.Star3;
            progressBoxInfo3.reward = challengeCountryConfig.Reward3;
            progressBoxInfoList.Add(progressBoxInfo3);

            RefreshShow(playAnim);
        }

        public void SetHeroData(int showCountryId, int currentStar, int maxStar, RepeatedField<int> gotRewardList, bool playAnim = true)
        {
            uiType = UIType.Hero;
            this.oldShowCountryId = this.showCountryId;
            this.showCountryId = showCountryId;
            this.oldStar = this.currentStar;
            this.currentStar = currentStar;
            this.maxStar = maxStar;
            this.gotRewardList = gotRewardList;

            if (oldShowCountryId != showCountryId)
            {
                this.oldStar = 0;
            }

            progressBoxInfoList.Clear();
            ChallengeHeroChapterConfig challengeHeroChapterConfig = Configs.ChallengeHeroChapter.GetConfig(showCountryId);
            ProgressBoxInfo progressBoxInfo1 = new();
            progressBoxInfo1.id = 1;
            progressBoxInfo1.needStar = challengeHeroChapterConfig.Star1;
            progressBoxInfo1.reward = challengeHeroChapterConfig.Reward1;
            progressBoxInfoList.Add(progressBoxInfo1);
            ProgressBoxInfo progressBoxInfo2 = new();
            progressBoxInfo2.id = 2;
            progressBoxInfo2.needStar = challengeHeroChapterConfig.Star2;
            progressBoxInfo2.reward = challengeHeroChapterConfig.Reward2;
            progressBoxInfoList.Add(progressBoxInfo2);
            ProgressBoxInfo progressBoxInfo3 = new();
            progressBoxInfo3.id = 3;
            progressBoxInfo3.needStar = challengeHeroChapterConfig.Star3;
            progressBoxInfo3.reward = challengeHeroChapterConfig.Reward3;
            progressBoxInfoList.Add(progressBoxInfo3);

            RefreshShow(playAnim);
        }

        private void RefreshShow(bool playAnim = true)
        {
            // 任务
            obtain.gameObject.SetActive(false);
            closeTipButton.gameObject.SetActive(false);
            if (oldStar != currentStar)
            {
                if (playAnim)
                {
                    // 播放进度条动画
                    Anim.PlayProgressValueAnim(oldStar / (float)maxStar * 100f, currentStar / (float)maxStar * 100f);
                    this.oldStar = this.currentStar;
                }
                else
                {
                    pointTxt.text = currentStar.ToString();
                    progressValue.fillAmount = currentStar / (float)maxStar;
                }
            }
            // 设置宝箱
            SetBox();
        }

        // 设置宝箱
        private void SetBox()
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(RedDotPath, "");
            bool isRed = false;
            // 设置宝箱状态
            for (int i = 0; i < rewardList.Count; i++)
            {
                var boxItem = rewardList[i];
                boxItem.gameObject.DOKill();
                //红点节点
                if (currentStar >= progressBoxInfoList[i].needStar)
                {
                    // 已领取
                    if (gotRewardList[i] == 1)
                    {
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Open, s => boxItem.image.sprite = s);
                    }
                    // 未领取
                    else
                    {
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Obtain, s => boxItem.image.sprite = s);
                        boxItem.gameObject.DOShake();
                        //可领取小红点状态
                        isRed = true;
                    }
                    boxItem.transform.localScale = Vector3.one * 1.2f;
                }
                else
                {
                    // 未解锁
                    SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Close, s => boxItem.image.sprite = s);
                    boxItem.transform.localScale = Vector3.one;
                }
                // 设置活跃点
                pointList[i].text = progressBoxInfoList[i].needStar.ToString();
            }
            node.AddValue(isRed ? 1 : -1);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        // 设置宝箱内容
        private void SetBoxContent(int index)
        {
            closeTipButton.gameObject.SetActive(true);
            obtain.gameObject.SetActive(true);
            obtain.transform.SetParent(positions[index]);
            obtain.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            //// 宝箱内容
            var gameItems = GameItemUtils.CreateGameItems(progressBoxInfoList[index].reward).ToArray();
            for (int i = 0; i < MAX_REWARD_ITEM; i++)
            {
                if (i < gameItems.Length)
                {
                    items[i].gameObject.SetActive(true);
                    items[i].SetData(gameItems[i]);
                }
                else
                {
                    items[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
