using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;
using static BigBang.AllStarManager;
using Utils.GameItem;
using System;
using Protocol;
using GameItem = Utils.GameItem.GameItem;
using Babu;

namespace BigBang.UI
{
    public class HonourDetailUIProperties : WindowProperties
    {
        public HonourItem honourItem;
        public AchievementData achievementData;
        public HonourGroupData honourGroupData;
        public HonourDetailUIProperties(HonourItem honourItem, HonourGroupData honourGroupData, AchievementData achievementData)
        {
            this.honourItem = honourItem;
            this.achievementData = achievementData;
            this.honourGroupData = honourGroupData;
        }
    }
    public class HonourDetailUI : AWindowController<HonourDetailUIProperties>
    {
        #region 初始化与监听

        [SerializeField] private TMP_Text honourTitleText = null;
        [SerializeField] private TMP_Text honourFirstTimeText = null;
        [SerializeField] private TMP_Text honourCountNumText = null;
        [SerializeField] private Image hasGetImage = null;
        [SerializeField] private Image notGetImage = null;
        [SerializeField] private RectTransform cupImageRoot = null;
        [SerializeField] private Image cupImagePrefab = null;
        [SerializeField] private BabuButton leftButton = null;
        [SerializeField] private BabuButton rightButton = null;
        [SerializeField] private DarkLightItem leftButtonDarkLightItem = null;
        [SerializeField] private DarkLightItem rightButtonDarkLightItem = null;
        [SerializeField] private TMP_Text honourDetailText = null;
        [SerializeField] private HorizontalLayoutGroup pointPanel = null;
        [SerializeField] private GameObject pointItemPrefab = null;
        [SerializeField] private HonourDetailUIAnim anim = null;

        private RectTransform cupImageRect = null;
        private ComponentPool<RectTransform> cupPoolComponent = new();
        [SerializeField] private RectTransform leftPosition = null;
        [SerializeField] private RectTransform centerPosition = null;
        [SerializeField] private RectTransform rightPosition = null;

        protected override void Awake()
        {
            base.Awake();
            cupPoolComponent.InitComponentPool(cupImagePrefab.gameObject, 3, cupImageRoot);
        }

        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
            leftButton.OnClick += OnClickLeft;
            rightButton.OnClick += OnClickRight;
        }
        [SerializeField] private ParticleSystem particle = null;
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
            leftButton.OnClick -= OnClickLeft;
            rightButton.OnClick -= OnClickRight;
        }
        protected override void OnPropertiesSet()
        {
            cupPoolComponent.ClearOutComponent();
            InitPoint();
            achievementData = Properties.achievementData;
            cupImageRect = cupPoolComponent.GetComponentFormPool();
            particle.Stop();
            particle.Clear();
            RefreshUI();
            anim.PlayShowAni(cupImageRect, Properties.honourItem.cupImage.transform);
        }
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<HonourDetailUI>();
        }
        #endregion

        #region 通用的部分

        private void RefreshUI()
        {
            RefreshCup();
            RefreshPoint();
            RefreshButton();
        }

        private AchievementData achievementData = null;
        private async void RefreshCup()
        {
            honourTitleText.text = "{0}-{1}".SafeFormat(achievementData.Config.GroupTitle, achievementData.Config.Name);
            bool isFinish = achievementData.IsComplete || achievementData.Received == 1;
            if (isFinish)
            {
                honourFirstTimeText.text = "{0}首次达成".SafeFormat(TimeUtils.GetUnixTimeString(achievementData.time, "yyyy.MM.dd"));
                honourCountNumText.text = "累计获得<color=#FFEE7D>{0}</color>次".SafeFormat(achievementData.HonourCurrentShow);
            }
            else
            {
                honourFirstTimeText.text = "";
                honourCountNumText.text = "";
            }
            honourDetailText.text = achievementData.Config.Desc;
            hasGetImage.gameObject.SetActive(isFinish);
            notGetImage.gameObject.SetActive(!isFinish);
            Image cupImage = cupImageRect.GetComponent<Image>();
            cupImage.sprite = await SpriteProxy.GetHonourCup(achievementData.Config.Icon);
            cupImage.SetGray(!isFinish);
            cupImage.SetAlpha(isFinish ? 1 : 0.5f);
            if (isFinish)
            {
                particle.Play();
            }
            else
            {
                particle.Stop();
                particle.Clear();
            }
        }
        private void RefreshButton()
        {
            int indexNow = Properties.honourGroupData.list.IndexOf(achievementData);
            leftButtonDarkLightItem.SetLight(indexNow > 0);
            rightButtonDarkLightItem.SetLight(indexNow < Properties.honourGroupData.list.Count - 1);
        }

        private void OnClickLeft(BabuButton _)
        {
            int indexNow = Properties.honourGroupData.list.IndexOf(achievementData);
            if (indexNow <= 0)
            {
                Tips.PopTips("已经是第一个了");
                return;
            }
            achievementData = Properties.honourGroupData.list[indexNow - 1];
            CreateCupAndMode(false);
            RefreshUI();
        }
        private void OnClickRight(BabuButton _)
        {
            int indexNow = Properties.honourGroupData.list.IndexOf(achievementData);
            if (indexNow >= Properties.honourGroupData.list.Count - 1)
            {
                Tips.PopTips("已经是最后一个了");
                return;
            }
            achievementData = Properties.honourGroupData.list[indexNow + 1];
            CreateCupAndMode(true);
            RefreshUI();
        }
        private void CreateCupAndMode(bool moveToLeft)
        {
            RectTransform oldCupImageRect = cupImageRect;
            anim.PlayMoveAni(oldCupImageRect, moveToLeft ? leftPosition : rightPosition, 0.5f, () =>
            {
                cupPoolComponent.ReturnComponentToPool(oldCupImageRect);
            });
            cupImageRect = cupPoolComponent.GetComponentFormPool();
            cupImageRect.localPosition = moveToLeft ? rightPosition.localPosition : leftPosition.localPosition;
            anim.PlayMoveAni(cupImageRect, centerPosition, 1f);
        }

        private List<PointItem> pointItemList = new();
        private void InitPoint()
        {
            int maxCount = Mathf.Max(Properties.honourGroupData.list.Count, pointItemList.Count);
            for (int i = 0; i < maxCount; i++)
            {
                if (i < Properties.honourGroupData.list.Count && i >= pointItemList.Count)
                {
                    GameObject pointItemGameObject = GameObject.Instantiate(pointItemPrefab, pointPanel.transform);
                    PointItem pointItemAdd = pointItemGameObject.GetComponent<PointItem>();
                    pointItemList.Add(pointItemAdd);
                }
                PointItem pointItem = pointItemList[i];
                if (i >= Properties.honourGroupData.list.Count)
                {
                    pointItem.gameObject.SetActive(false);
                }
                else
                {
                    AchievementData achievementData = Properties.honourGroupData.list[i];
                    pointItem.SetData(i, OnClickPoint);
                    pointItem.gameObject.SetActive(true);
                }
            }
        }
        private void RefreshPoint()
        {
            int selectIndex = Properties.honourGroupData.list.IndexOf(achievementData);
            foreach (var pointItem in pointItemList)
            {
                pointItem.SetLight(pointItem.Index == selectIndex);
            }
        }
        private void OnClickPoint(int index)
        {
            int selectIndex = Properties.honourGroupData.list.IndexOf(achievementData);
            if (selectIndex == index) return;
            achievementData = Properties.honourGroupData.list[index];
            CreateCupAndMode(selectIndex < index);
            RefreshUI();
        }

        #endregion 

        #region 动画

        #endregion

    }
}