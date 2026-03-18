using System;
using Babu;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] public TMP_Text countText;
        // 选中框
        [SerializeField] private Image selectBorder;
        [SerializeField] private Image backgroundImg;
        [SerializeField] public Image blackImg;
        [SerializeField] private GameObject newTag;
        [SerializeField] private GameObject timeTag;
        [SerializeField] private GameObject overdueTag;
        [SerializeField] public Button subBtn;
        [SerializeField] public GameObject sub;
        [SerializeField] public TMP_Text subText;
        [SerializeField] private Button selfBtn;
        [SerializeField] private Image reddotimg;
        [SerializeField] public Image imgShouTong;

        public event Action OnSub;
        public GoodsData Data { get; private set; }

        public GameItem gameItem;

        [HideInInspector] public bool canShowTip = true;
        [HideInInspector] public string dotnodePath = "";
        [HideInInspector] public bool EnableRedDot = false;

        private void OnEnable()
        {
            subBtn.onClick.AddListener(OnSubClick);
            selfBtn?.onClick.AddListener(OnOpenTips);
        }

        private void OnDisable()
        {
            subBtn.onClick.RemoveListener(OnSubClick);
            selfBtn?.onClick.RemoveListener(OnOpenTips);
        }

        private void OnSubClick()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT);
            OnSub?.Invoke();
        }

        public void ShowSubButton()
        {
            sub.SetActive(true);
        }

        public void HidSubButton()
        {
            sub.SetActive(false);
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
                    if(cfg == null)
                    {
                        Debug.LogWarning("InventoryItem , OnOpenTips , CardModelConfig is null , gameItem.Id = " + gameItem.Id);
                        return;
                    }
                    UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(cfg));
                }
                else
                {
                    UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
                }
            }
        }

        private void checkreddot()
        {
            if (EnableRedDot && dotnodePath != "" && gameItem != null)
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(dotnodePath, "/" + gameItem.Id);

                if (gameItem.Type == GameItemType.Goods)
                {
                    var cfg = Configs.Goods.GetConfig(gameItem.Id);
                    if (gameItem.Count > 0 && cfg != null && cfg.Type == 2 && cfg.Uselv <= Player.Level)
                    {
                        node.AddValue(1);
                    }
                    else
                    {
                        node.AddValue(-1);
                    }
                    node.IsRed(reddotimg.transform);
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
            SetGameItem(gameItem);
            SetImageAndCount(await gameItem.GetIcon(), gameItem.CountString());
            SetGameItemViews(gameItem);
            canShowTip = tips;
            this.EnableRedDot = enableRedDot;
            checkreddot();
        }

        public async void SetData(GoodsData data, bool tips = true, bool enableRedDot = false)
        {
            Data = data;

            this.EnableRedDot = enableRedDot;
            var gameItem = data.ToGameItem();

            SetGameItem(gameItem);

            canShowTip = tips;

            // 设置物品图片
            icon.sprite = await gameItem.GetIcon();
            // 设置数量
            countText.text = data.Count.ToString();
            // 设置物品质量背景图片
            backgroundImg.sprite = await SpriteProxy.GetInvetoryQuality(data.Config.Quality);
            var expirationTime = data.Config.ExpirationTime;
            if (expirationTime > 0)
            {
                // 有时限物品
                timeTag.SetActive(true);
                if (Utils.DataConvUtil.ServerTime < expirationTime)
                {
                    // 状态为有时限时，为亮色并打上限时标签
                    blackImg.gameObject.SetActive(false);
                    overdueTag.SetActive(false);
                }
                else
                {
                    // 状态为已过期时，为灰色并打上已过期标签
                    blackImg.gameObject.SetActive(true);
                    // 打上过期标签
                    overdueTag.SetActive(true);
                }
            }
            else
            {
                // 无时限物品
                timeTag.SetActive(false);
                overdueTag.gameObject.SetActive(false);
                // 状态为无时限时，为亮色
                blackImg.gameObject.SetActive(false);
            }
            // 判断是否过期
            bool expiration = false;
            blackImg.gameObject.SetActive(expiration);
            // 判断是否是第一次获得
            newTag.SetActive(data.IsNew);
            if (data.IsNew)
            {
                Player.PackageManager.NewToOld.Add(data.Id);
            }

            checkreddot();
        }

        /// <summary>
        /// 是否显示首通
        /// </summary>
        /// <param name="first"></param>
        public void EnableShouTong(bool first)
        {
            imgShouTong.gameObject.SetActive(first);
        }

        public void SetData(GoodsData data, int count, bool tips = true, bool enableRedDot = false)
        {
            SetData(data, tips, enableRedDot);
            // 设置物品数量
            countText.text = count.ToString();
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

        // 隐藏new标签
        public void HidNewTag()
        {
            if (newTag.activeInHierarchy || Data.IsNew)
            {
                Data.IsNew = false;
                NetworkManager.Instance.SetGoodsAsOldRequest(new int[] { Data.Id }, response => { Debug.Log("点击去除New标签"); });
            }
            newTag.SetActive(false);
        }


        public void SetImageAndCount(Sprite obtainImg, string count)
        {
            newTag.SetActive(false);
            timeTag.SetActive(false);
            overdueTag.SetActive(false);
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
            newTag.SetActive(false);
            timeTag.SetActive(false);
            overdueTag.SetActive(false);
            blackImg.gameObject.SetActive(false);
            countText.text = txt;
            icon.sprite = obtainImg;
        }

        public void SetSubText(int select, int have)
        {
            subText.text = $"{select}/{have}";
        }

        public void SetSubText(string value)
        {
            subText.text = value;
        }

        public GameItem GetGameItem()
        {
            return gameItem;
        }

        private void SetGameItem(GameItem gameItem)
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
            newTag.SetActive(false);
            timeTag.gameObject.SetActive(false);
            overdueTag.SetActive(false);
            blackImg.gameObject.SetActive(false);
            countText.text = gameItem.CountString();
            icon.sprite = await gameItem.GetIcon();
            backgroundImg.sprite = await SpriteProxy.GetInvetoryQuality(gameItem.GetQuality());
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