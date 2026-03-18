using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig.Config;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class CardUIProperties : PanelProperties
    {
        public CardUI.SubUIID SubUI = CardUI.SubUIID.Card;

        public CardUIProperties(CardUI.SubUIID ui)
        {
            SubUI = ui;
        }
    }

    public class CardUI : APanelController<CardUIProperties>
    {
        public enum SubUIID
        {
            Card,       //卡片列表
            SkillList,  //技能列表
            SkillTrain,  // 特技学习
            Fire, //解雇
        }

        [SerializeField] private CardGridAdapter cardAdapter;
        [SerializeField] private Toggle AllBtn;
        [SerializeField] private Toggle HouWeiBtn;
        [SerializeField] private Toggle QianFengBtn;
        [SerializeField] private Toggle ZhongFengBtn;

        public static RectTransform staticCenterPoint;
        [SerializeField] private RectTransform centerPoint;
        public static CardUIAnim Anim;

        [SerializeField] private BabuToggleGroup toggleGroup;

        [SerializeField] private BabuToggle fireToggle;
        [SerializeField] private BabuToggle cardToggle;
        [SerializeField] private BabuToggle skillToggle;
        [SerializeField] private BabuToggle skillTrainToggle;

        [SerializeField] private BabuButton fireBtn;
        [SerializeField] private BabuButton collectionBtn = null;
        [SerializeField] private GameObject cardPad;
        [SerializeField] private SkillListPad skillListPad;
        [SerializeField] private SkillTrainRoomPad skillTrainPad;

        [SerializeField] private CardFirePad firePad;

        public static bool isTurnCardOnce = true;
        public static bool isTurnSkillOnce = true;
        public static bool isTurnSkillTrainOnce = true;
        public static bool isFirstEnter = true;

        protected override void Awake()
        {
            base.Awake();
            staticCenterPoint = centerPoint;
            Anim = GetComponent<CardUIAnim>();
        }

        protected override void AddListeners()
        {
            //AllBtn.onValueChanged.AddListener(OnAll);
            //HouWeiBtn.onValueChanged.AddListener(OnHouWei);
            //QianFengBtn.onValueChanged.AddListener(OnQianFeng);
            //ZhongFengBtn.onValueChanged.AddListener(OnZhongFeng);

            toggleGroup.OnValueChanged += OnToggleGroupChanged;
            fireBtn.OnClick += OnFireBtnClick;
            collectionBtn.OnClick += OnCollectionBtnClick;
            Babu.EventManager.Instance.Register(EventID.OnStudySkill, OnStudySkill);
            Babu.EventManager.Instance.Register(EventID.OnClickTrainingBtn, OnClickTrainingBtn);
            EventManager.Instance.Register(EventID.RefreshWindow, refreshWindow);
        }

        protected override void RemoveListeners()
        {
            //AllBtn.onValueChanged.RemoveListener(OnAll);
            //HouWeiBtn.onValueChanged.RemoveListener(OnHouWei);
            //QianFengBtn.onValueChanged.RemoveListener(OnQianFeng);
            //ZhongFengBtn.onValueChanged.RemoveListener(OnZhongFeng);

            toggleGroup.OnValueChanged -= OnToggleGroupChanged;
            fireBtn.OnClick -= OnFireBtnClick;
            collectionBtn.OnClick -= OnCollectionBtnClick;
            Babu.EventManager.Instance.Unregister(EventID.OnStudySkill, OnStudySkill);
            Babu.EventManager.Instance.Unregister(EventID.OnClickTrainingBtn, OnClickTrainingBtn);
            EventManager.Instance.Unregister(EventID.RefreshWindow, refreshWindow);
        }

        private void refreshWindow(object[] args)
        {
            if (args != null && args[0].ToString() == "0")
            {
                toggleGroup.Switch(toggleGroup.EnableIndex);
            }
        }

        [SerializeField] private CardUIGuide cardUIGuide;
        protected override void OnPropertiesSet()
        {
#if MiGuNft
            collectionBtn.gameObject.SetActive(true);
#else
            collectionBtn.gameObject.SetActive(false);
#endif
            Player.CardManager.CheckRedDot(0, true);
            //EventManager.Instance.Dispatch(EventID.OnRefreshNavigationUIRedDot);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            // 检测成就
            NetworkManager.Instance.CheckPlayerAchievement();
            if (Properties.SubUI == SubUIID.Card)
            {
                toggleGroup.Switch(0);
                Anim.PlayEnter();
            }
            AudioManager.Instance.PlaySound(AudioNames.ENT_PLAYER);
            cardUIGuide.CheckGuide();
        }

        private void OnToggleGroupChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);

            PositionType position;
            switch (toggleGroup.EnableIndex)
            {
                case 0:
                    position = PositionType.All; break;
                case 1:
                    position = PositionType.HouWei; break;
                case 2:
                    position = PositionType.QianFeng; break;
                case 3:
                    position = PositionType.ZhongFeng; break;
                default:
                    position = PositionType.All; break;
            }

            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            var list = Player.CardManager.GetCardList(position);
            cardAdapter.SetData(list, false);
        }

        private void OnFireBtnClick(BabuButton sender)
        {
            //cardPad.SetActive(false);
            //skillListPad.gameObject.SetActive(false);
            //skillTrainPad.gameObject.SetActive(false);
            //firePad.gameObject.SetActive(false);
            UIController.Instance.OpenWindow<CardFirePad>();
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            //firePad.gameObject.SetActive(true);
            //firePad.OnShow();
        }

        private void OnCollectionBtnClick(BabuButton sender)
        {
            UIController.Instance.OpenWindow<CollectionUI>();
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
        }

        private void OnStudySkill(object[] args)
        {
            // 播放退出动画
            skillListPad.gameObject.DOFade(0, 0.1f).OnComplete(() =>
            {
                skillListPad.gameObject.SetAlpha(1);
                // 播放进入动画
                //toggleGroup.Switch(skillTrainToggle);
                skillTrainPad.SetData();
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    // 打开学习面板
                    UIController.Instance.OpenWindow<SkillTrainRoomSelectUI>(new SkillTrainRoomSelectProperties(args[0] as SkillTrainRoom, selectSkill: args[1] as SkillConfig));
                });
            }).AddTo(this.gameObject);
        }

        //点击学习中按钮
        private void OnClickTrainingBtn(object[] args)
        {
            // 播放退出动画
            skillListPad.gameObject.DOFade(0, 0.1f).OnComplete(() =>
            {
                skillListPad.gameObject.SetAlpha(1);
                // 播放进入动画
                //toggleGroup.Switch(skillTrainToggle);
                skillTrainPad.SetData();
            }).AddTo(this.gameObject);
        }

        protected override void WhileHiding()
        {
            base.WhileHiding();
            // 退出界面时停止滚动
            cardAdapter.StopMovement();
            // UIController.Instance.ShowPanel<HomeUI>();
            //CardUI.Anim.PlayNext(() =>
            //{

            //});
            //第一次点击布尔值都重置为true
            isTurnCardOnce = true;
            isTurnSkillOnce = true;
            isTurnSkillTrainOnce = true;
            isFirstEnter = true;
        }
    }
}