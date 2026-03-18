using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CollectionUI : AWindowController<WindowProperties>
    {

        [SerializeField] private CardGridAdapter osa;
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuButton helpBtn = null;
        [SerializeField] private BabuButton upButton = null;
        [SerializeField] private BabuButton downButton = null;
        [SerializeField] private RectTransform cardPanel = null;
        [SerializeField] private BabuButton canNotDownButton = null;
        [SerializeField] private RectTransform emptyPanel = null;
        [SerializeField] private RectTransform loadingPanel = null;
        [SerializeField] private RectTransform contentPanel = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnCloseBtn;
            helpBtn.OnClick += OnHelpBtn;
            upButton.OnClick += OnUpBtn;
            downButton.OnClick += OnDownBtn;
            canNotDownButton.OnClick += OnCanNotDownButton;
            EventManager.Instance.Register(EventID.OnClickCollectionUICard, OnClickCollectionUICard);
            EventManager.Instance.Register(EventID.RefreshCollectionUI, RefreshCollectionUI);
            EventManager.Instance.Register(EventID.OnApplicationFocusTrue, OnApplicationFocusTrue);
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnCloseBtn;
            helpBtn.OnClick -= OnHelpBtn;
            upButton.OnClick -= OnUpBtn;
            downButton.OnClick -= OnDownBtn;
            canNotDownButton.OnClick -= OnCanNotDownButton;
            EventManager.Instance.Unregister(EventID.OnClickCollectionUICard, OnClickCollectionUICard);
            EventManager.Instance.Unregister(EventID.RefreshCollectionUI, RefreshCollectionUI);
            EventManager.Instance.Unregister(EventID.OnApplicationFocusTrue, OnApplicationFocusTrue);
        }
        private void OnCloseBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<CollectionUI>();
        }
        private void OnHelpBtn(BabuButton _)
        {
            UIController.Instance.OpenWindow<CollectionHelpUI>();
        }
        private void OnUpBtn(BabuButton _)
        {
            if (CollectionManager.Instance.selectedPlayerCard == null)
            {
                Tips.PopTips("请选择一名球员");
                return;
            }
            var owerList = Player.CardManager.GetCardList();
            int delCount = 1;
            if (owerList.Count - delCount <= GameConst.FIRE_MIN_LEFT)
            {
                Tips.PopTips($"俱乐部球员不可少于{GameConst.FIRE_MIN_LEFT}个。");
                return;
            }
            PlayerCard card = CollectionManager.Instance.selectedPlayerCard;
            if (card.IsStarter())
            {
                Tips.PopTips($"经典赛首发球员不能转移。");
                return;
            }
            if (card.IsStarter1())
            {
                Tips.PopTips($"赛事首发球员不能转移。");
                return;
            }
            if (card.IsStarter2())
            {
                Tips.PopTips($"排位赛首发球员不能转移。");
                return;
            }
            if (card.IsStarter3())
            {
                Tips.PopTips($"篮球殿堂首发球员不能转移。");
                return;
            }
            if (card.IsStarter4())
            {
                Tips.PopTips($"百分大战上场球员不能转移。");
                return;
            }
            if (card.IsUsingInBounty)
            {
                Tips.PopTips($"悬赏任务已派遣球员不能转移。");
                return;
            }
            if (card.SkillTrainRoomId != 0)
            {
                Tips.PopTips($"特级训练中的球员不能转移。");
                return;
            }
            CollectionManager.Instance.UpCard(CollectionManager.Instance.selectedPlayerCard.CardId);
        }
        private void OnDownBtn(BabuButton _)
        {
            if (CollectionManager.Instance.selectedPlayerCard == null)
            {
                Tips.PopTips("请选择一名球员");
                return;
            }
            if (Player.CardManager.CardList.Exists(playcard => playcard.CardId == CollectionManager.Instance.selectedPlayerCard.CardId))
            {
                Tips.PopTips("已有相同球员，无法收回");
                return;
            }
            CollectionManager.Instance.DownCard(CollectionManager.Instance.selectedPlayerCard.PropId);
        }
        private void OnCanNotDownButton(BabuButton _)
        {
            if (CollectionManager.Instance.selectedPlayerCard == null)
            {
                Tips.PopTips("请选择一名球员");
                return;
            }
            Tips.PopTips("正在出售，无法收回");
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            CollectionManager.Instance.noServerData = true;
            RefreshUI();
            CollectionManager.Instance.GetCollectionList();
        }
        private void OnApplicationFocusTrue(object[] _)
        {
            CollectionManager.Instance.noServerData = true;
            RefreshUI();
            CollectionManager.Instance.GetCollectionList();
        }
        private void RefreshCollectionUI(object[] _)
        {
            RefreshUI();
        }
        private void RefreshUI()
        {
            loadingPanel.gameObject.SetActive(CollectionManager.Instance.noServerData);
            contentPanel.gameObject.SetActive(false);
            if (CollectionManager.Instance.noServerData)
            {
                upButton.gameObject.SetActive(false);
                downButton.gameObject.SetActive(false);
                canNotDownButton.gameObject.SetActive(false);
            }
            else
            {
                List<PlayerCard> playerCardList = CollectionManager.Instance.GetCollectionUIData();

                osa.isUsingInCillectionUI = true;
                osa.SetData(playerCardList, true);

                bool isEmpty = playerCardList.Count <= 0;
                emptyPanel.gameObject.SetActive(isEmpty);
                contentPanel.gameObject.SetActive(!isEmpty);
                if (!isEmpty)
                {
                    if (CollectionManager.Instance.selectedPlayerCard == null || CollectionManager.Instance.selectedPlayerCardIndex < 0 || CollectionManager.Instance.selectedPlayerCardIndex >= playerCardList.Count)
                    {
                        CollectionManager.Instance.selectedPlayerCard = playerCardList[0];
                        CollectionManager.Instance.selectedPlayerCardIndex = 0;
                    }
                    else
                    {
                        CollectionManager.Instance.selectedPlayerCard = playerCardList[CollectionManager.Instance.selectedPlayerCardIndex];
                    }
                    UnityTimer.Timer.Register(this.gameObject, 0.01f, () =>
                    {
                        osa.RefreshSelectCard();
                        osa.PlayHighlightAnim();
                    });
                }
                else
                {
                    CollectionManager.Instance.selectedPlayerCard = null;
                    CollectionManager.Instance.selectedPlayerCardIndex = -1;
                }
                RefreshButton();
            }
        }
        private void OnClickCollectionUICard(object[] args)
        {
            PlayerCard playerCard = args[0] as PlayerCard;
            int index = (int)args[1];
            CollectionManager.Instance.selectedPlayerCard = playerCard;
            CollectionManager.Instance.selectedPlayerCardIndex = index;
            osa.RefreshSelectCard();
            osa.PlayHighlightAnim();
            RefreshButton();
        }
        private void RefreshButton()
        {
            upButton.gameObject.SetActive(false);
            downButton.gameObject.SetActive(false);
            canNotDownButton.gameObject.SetActive(false);
            bool isSelect = CollectionManager.Instance.selectedPlayerCard != null;
            if (isSelect == false)
            {
                return;
            }
            if (CollectionManager.Instance.selectedPlayerCard.isCollectionCard == false)
            {
                upButton.gameObject.SetActive(true);
                return;
            }
            bool isSelling = CollectionManager.Instance.selectedPlayerCard.PropStatus == 1;
            downButton.gameObject.SetActive(!isSelling);
            canNotDownButton.gameObject.SetActive(isSelling);
        }
    }
}