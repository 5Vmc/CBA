using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Utils.GameItem;
using System;
using DG.Tweening;
using Babu;
using Utils;
using GameConfig.Config;
using GameConfig;
using System.Linq;
using TMPro;

namespace BigBang.UI
{
    public class InventoryObtainedUIProperties : WindowProperties
    {
        public List<GameItem> Data { get; set; }
        public Action Callback { get; private set; }
        public string TitleStr = "获得物品";


        public InventoryObtainedUIProperties(List<Protocol.GameItem> data)
        {
            //Data = data;
            Data = new List<GameItem>();
            foreach (var item in data)
            {
                Data.Add(GameItemUtils.CreateGameItem((GameItemType)item.Type, item.Id, item.Count));
            }
        }

        public InventoryObtainedUIProperties(GameItem data)
        {
            Data = new List<GameItem>();
            Data.Add(GameItemUtils.CreateGameItem(data.Type, data.Id, data.Count));
        }

        public InventoryObtainedUIProperties(List<GameItem> data, Action callback = null, string titleStr = "获得物品")
        {
            Callback = callback;
            Data = data;
            TitleStr = titleStr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemstr">格式 type:id:count|item:id:count...</param>
        /// <param name="callback"></param>
        /// <param name="titleStr"></param>
        public InventoryObtainedUIProperties(string itemstr, Action callback = null, string titleStr = "获得物品")
        {
            List<GameItem> data = GameItemUtils.CreateGameItems(itemstr).ToList();
            Callback = callback;
            Data = data;
            TitleStr = titleStr;
        }
    }

    public class InventoryObtainedUI : AWindowController<InventoryObtainedUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private GameObject cardItemGroup;
        [SerializeField] private GameObject gameItemGroup;
        [SerializeField] private Button changeButton;
        // [SerializeField] private TMP_Text titleText;

        private int changeCounts = 0;
        private List<CardModelConfig> cardModelConfigs = new List<CardModelConfig>();
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            changeButton.onClick.AddListener(OnChangeCard);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            changeButton.onClick.RemoveListener(OnChangeCard);
        }



        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            //让球员整卡转换为碎片，通知先于回调发送导致不好判断，暂时不使用
            // Properties.Data = GameItemUtils.ChangeCardToPiece(Properties.Data);
            Properties.Data = Properties.Data.Where(item => item != null).ToList();
            GetCardItemFromGameItem();
            if (cardModelConfigs.Count == 0)
            {
                cardItemGroup.SetActive(false);
                gameItemGroup.SetActive(true);
                changeButton.gameObject.SetActive(false);
                GameItemGroupShow();
            }
            else
            {
                cardItemGroup.SetActive(true);
                gameItemGroup.SetActive(false);
                changeButton.gameObject.SetActive(true);
                InitCardPrefab();
                CardItemShow();

            }
        }
        private void OnChangeCard()
        {
            changeCounts++;
            if (changeCounts < cardModelConfigs.Count)
            {
                for (int i = 0; i < cardItemGroup.transform.childCount; i++)
                {
                    cardItemGroup.transform.GetChild(i).gameObject.SetActive(i == changeCounts);
                }
            }
            else
            {
                cardItemGroup.transform.gameObject.SetActive(false);
                gameItemGroup.transform.gameObject.SetActive(true);
                changeButton.gameObject.SetActive(false);
                GameItemGroupShow();
            }
        }

        [SerializeField] private ScrollRect scrollView = null;
        [SerializeField] private RectTransform gameItemGroupRect = null;

        private Sequence itemAniSequence = null;

        //物品栏显示
        private void GameItemGroupShow()
        {
            itemAniSequence?.Kill();
            itemAniSequence = DOTween.Sequence();
            gameItemGroupRect.SetLocalScaleY(0f);
            // titleText.text = Properties.TitleStr;
            content.localPosition = Vector3.zero;
            //InitGameItemPrefab();
            AudioManager.Instance.PlaySound(AudioNames.ENT_FLOPS);
            scrollView.gameObject.SetActive(false);

            itemAniSequence.AppendInterval(0.1f);
            itemAniSequence.Append(gameItemGroupRect.DOScaleY(1f, 0.4f).SetEase(Ease.OutBack));

            SetRewards();

            for (int i = 0; i < content.childCount; i++)
            {
                if (content.GetChild(i).gameObject.activeSelf == false)
                {
                    break;
                }
                var itemTrans = content.GetChild(i);
                itemTrans.localScale = Vector3.zero;
                float insertTime = (i < 15 ? (i * 0.1f) : ((15 * 0.1f) + ((i - 15) * 0.05f))) + 0.5f;
                itemAniSequence.Insert(insertTime, itemTrans.DOScale(1, (i <= 15 ? 0.15f : 0.1f)).SetEase(Ease.OutBack));
                var data = Properties.Data[i];
                itemTrans.GetComponent<InventoryItem>().SetData(data);
            }

            itemAniSequence.InsertCallback(0.51f, () =>
            {
                scrollView.gameObject.SetActive(true);
                scrollView.verticalNormalizedPosition = 1f;
            });
        }
        [SerializeField] private InventoryItem itemPrefab = null;
        private void SetRewards()
        {
            Transform layoutTrans = content;
            List<GameItem> gameItemList = Properties.Data;
            while (layoutTrans.childCount < gameItemList.Count) Instantiate(itemPrefab, layoutTrans);
            for (int i = 0; i < layoutTrans.childCount; i++)
            {
                if (i < gameItemList.Count)
                {
                    var reward = gameItemList[i];
                    var child = layoutTrans.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.GetComponent<InventoryItem>().SetData(reward);
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        //球员卡片大头显示
        private void CardItemShow()
        {
            //InitCardPrefab();
            for (int i = 0; i < cardItemGroup.transform.childCount; i++)
            {
                cardItemGroup.transform.GetChild(i).gameObject.SetActive(i == changeCounts);
                var data = cardModelConfigs[i];
                cardItemGroup.transform.GetChild(i).GetComponent<CardItem>().SetConfigShow(data);
                cardItemGroup.transform.GetChild(i).GetComponent<CardItem>().SetPlayerEffect(Player.CardManager.GetCard(data.Id));
            }

        }
        //初始化卡片预制体
        private void InitCardPrefab()
        {

            for (int i = 0; i < cardModelConfigs.Count; i++)
            {
                Instantiate(cardPrefab, cardItemGroup.transform);
            }
        }
        //获得物品中卡片信息
        private void GetCardItemFromGameItem()
        {
            for (int i = 0; i < Properties.Data.Count; i++)
            {
                var data = Properties.Data[i];
                if (data.Type == GameItemType.Card)
                {
                    cardModelConfigs.Add(Configs.CardModel.GetConfig(data.Id));
                }
            }
            cardModelConfigs = cardModelConfigs.OrderBy(item => item.Id).ToList();
        }
        //删除卡片组下预制
        private void ClearCardItemPrefab()
        {
            for (int i = cardItemGroup.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(cardItemGroup.transform.GetChild(i).gameObject);
            }
        }
        private void OnClose()
        {
            Properties.Callback?.Invoke();
            ClearCardItemPrefab();
            changeCounts = 0;
            cardModelConfigs.Clear();
            UIController.Instance.CloseWindow<InventoryObtainedUI>();
        }
    }
}