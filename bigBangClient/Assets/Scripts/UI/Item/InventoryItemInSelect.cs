using System;
using Babu;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class InventoryItemInSelect : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] public TMP_Text countText;
        // 选中框
        [SerializeField] private Image selectBorder;
        [SerializeField] private Image backgroundImg;
        [SerializeField] public Image blackImg;
        [SerializeField] private Button selfBtn;
        [SerializeField] private TMP_Text txtPropname;

        public event Action OnSub;

        public GameItem gameItem;

        [HideInInspector] public bool canShowTip = true;

        private void OnEnable()
        {
            selfBtn?.onClick.AddListener(OnOpenTips);
        }

        private void OnDisable()
        {
            selfBtn?.onClick.RemoveListener(OnOpenTips);
        }

        private void OnSubClick()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT);
            OnSub?.Invoke();
        }

        private void OnOpenTips()
        {
            if (canShowTip == false)
            {
                Debug.Log("OnOpenTips,OpenTips == fals");
            }
            if (!canShowTip) return;
            if (gameItem == null)
            {
                Debug.Log("OnOpenTips,no goodsitem");
            }
            if (gameItem != null)
            {
                if (gameItem.Type == GameItemType.Card)
                {
                    var cfg = Configs.CardModel.GetConfig(gameItem.Id);
                    UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(cfg));
                }
                else
                {
                    UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
                }
            }
        }

        /// <summary>
        /// 根据道具id，数量创建
        /// </summary>
        /// <param name="itemid"></param>
        /// <param name="itemCount"></param>
        /// <param name="tips"></param>
        public void SetData(int itemid, int itemCount, bool tips = false, bool enableRedDot = false)
        {
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, itemid, itemCount);
            SetData(gameItem, tips, enableRedDot);
        }

        public async void SetData(GameItem gameItem, bool tips = true, bool enableRedDot = false)
        {
            SetImageAndCount(await gameItem.GetIcon(), gameItem.CountString());
            SetGameItemViews(gameItem);
            SetGameItem(gameItem);
            
            var config = gameItem.GetName();

            txtPropname.text = gameItem.GetName();
            txtPropname.color = CBAColorUtil.Instance.GetColor(gameItem.GetQuality());
            canShowTip = tips;
        }

        // 显示选中边框
        public void ShowSelectBorder()
        {
            selectBorder.gameObject.SetActive(true);
        }

        // 隐藏选中边框
        public void HidSelectBorder()
        {
            selectBorder.gameObject.SetActive(false);
        }


        public void SetImageAndCount(Sprite obtainImg, string count)
        {
            blackImg.gameObject.SetActive(false);
            countText.text = count;
            icon.sprite = obtainImg;
        }

        public void SetCount(string count)
        {
            countText.text = count;
        }

        public void SetImageAndTxt(Sprite obtainImg, string txt)
        {
            blackImg.gameObject.SetActive(false);
            countText.text = txt;
            icon.sprite = obtainImg;
        }

        public GameItem GetGameItem()
        {
            return gameItem;
        }

        public void SetGameItem(GameItem gameItem)
        {
            this.gameItem = gameItem;
        }

        public async void SetGameItemViews(GameItem gameItem)
        {
            if (gameItem == null) return;
            backgroundImg.sprite = await SpriteProxy.GetInvetoryQuality(gameItem.GetQuality());
        }

        public async void SetGameItemData(GameItem gameItem)
        {
            this.gameItem = gameItem;
            blackImg.gameObject.SetActive(false);
            countText.text = gameItem.CountString();
            icon.sprite = await gameItem.GetIcon();
        }

        public void SetCountTextActive(bool value)
        {
            countText.gameObject.SetActive(value);
        }

        public void SetBlack(bool isBlack)
        {
            blackImg.gameObject.SetActive(isBlack);
        }
    }
}