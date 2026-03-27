using System;
using Babu;
using TMPro;
using UnityEngine;
using Utils;
using UnityEngine.UI;
using System.Collections.Generic;
using GameConfig.Config;
using GameConfig;

namespace BigBang.UI
{
    public class ResourceTitle : MonoBehaviour
    {
        [SerializeField] TMP_Text secText;
        [SerializeField] ResourceTitleCD countDown;
        [SerializeField] List<TMP_Text> txtList;

        [SerializeField] private GameObject resouorcesTitleItemPrefab;
        private Dictionary<int, ResouorcesTitleItem> resouorcesTitleItemDic = new();
        public void SetOnlyShowGoodsList(List<int> goodsIdList)
        {
            for (var index = 0; index < dataEnableList?.Count; index++)
            {
                dataEnableList[index] = false;
                dataList[index].SetActive(false);
            }
            foreach (var item in resouorcesTitleItemDic.Values)
            {
                item.gameObject.SetActive(false);
            }
            foreach (int goodsId in goodsIdList)
            {
                if (resouorcesTitleItemDic.ContainsKey(goodsId) == false)
                {
                    ResouorcesTitleItem resouorcesTitleItem = GameObject.Instantiate(resouorcesTitleItemPrefab, resourcesLayout.transform).GetComponent<ResouorcesTitleItem>();
                    resouorcesTitleItemDic.Add(goodsId, resouorcesTitleItem);
                    resouorcesTitleItem.SetGoodsId(goodsId);
                }
                resouorcesTitleItemDic[goodsId].gameObject.SetActive(true);
            }
        }

        #region 字段启用设置
        [SerializeField] List<GameObject> dataList;

        List<bool> dataEnableList;
        /// <summary>
        /// 钻石字段
        /// </summary>
        [CustomLabel("钻石")]
        public bool FieldDiamond;
        /// <summary>
        /// 欧元
        /// </summary>
        [CustomLabel("欧元")]
        public bool FieldMoney;
        /// <summary>
        /// 大数值经验
        /// </summary>
        [CustomLabel("大数值经验")]
        public bool FieldBigBangExp;
        /// <summary>
        /// 合同碎片
        /// </summary>
        [CustomLabel("合同碎片")]
        public bool FieldContractFragments;
        /// <summary>
        /// 招募道具
        /// </summary>
        [CustomLabel("招募道具")]
        public bool FieldRecruitItem;
        /// <summary>
        /// 高级招募道具
        /// </summary>
        [CustomLabel("高级招募道具")]
        public bool FieldRecruitItem1;
        /// <summary>
        /// 体力
        /// </summary>
        [CustomLabel("体力")]
        public bool PowerItem;

        /// <summary>
        /// 殿堂荣誉
        /// </summary>
        [CustomLabel("殿堂荣誉")]
        public bool TowerMoneyItem;
        /// <summary>
        /// 篮球殿堂当前星星
        /// </summary>
        [CustomLabel("篮球殿堂当前星星")]
        public bool TowerStarItem;
        /// <summary>
        /// 篮球殿堂累计星星
        /// </summary>
        [CustomLabel("篮球殿堂累计星星")]
        public bool TowerTotalStarItem;


        #endregion
        private void Awake()
        {
            refreshActive();
        }

        private void refreshActive()
        {
            dataEnableList = new()
            {
                FieldDiamond,
                FieldMoney,
                FieldBigBangExp,
                FieldContractFragments,
                FieldRecruitItem,
                FieldRecruitItem1,
                PowerItem,
                TowerMoneyItem,
                TowerStarItem,
                TowerTotalStarItem
            };
            //字段可见性
            for (var index = 0; index < dataEnableList.Count; index++)
            {
                dataList[index].SetActive(dataEnableList[index]);
            }
        }

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.OnResourceChange, UpdateField);
            EventManager.Instance.Register(EventID.OnRefreshGoods, UpdateField);
            EventManager.Instance.Register(EventID.OnCostTowerHoner, UpdateField);
            EventManager.Instance.Register(EventID.OnServerPushPackageChange, UpdateField);
            if (PowerItem)
            {
                countDown.Regist();
            }

            UpdateField();
            UnityTimer.Timer.Register(this.gameObject, 0f, () => { UpdateField(); });
            AddBtn();
        }

        private void OnDisable()
        {
            EventManager.Instance?.Unregister(EventID.OnResourceChange, UpdateField);
            EventManager.Instance?.Unregister(EventID.OnRefreshGoods, UpdateField);
            EventManager.Instance?.Unregister(EventID.OnCostTowerHoner, UpdateField);
            EventManager.Instance?.Unregister(EventID.OnServerPushPackageChange, UpdateField);
            countDown?.UnRegist();
            RemoveBtn();
        }

        private void UpdateField(object[] args = null)
        {
            refreshActive();
            if (dataEnableList[0]) txtList[0].text = Player.PackageManager.Diamond.ToString();
            if (dataEnableList[1]) txtList[1].text = Player.PackageManager.Money.ToString();
            if (dataEnableList[2]) txtList[2].text = Player.TrainManager.Exp.ToFormatString();
            if (dataEnableList[3]) txtList[3].text = Player.PackageManager.GetGoodsNumber(GoodsId.ContractFragment).ToString();
            if (dataEnableList[4]) txtList[4].text = Player.PackageManager.GetGoodsNumber(GoodsId.RecruitPoint).ToString();
            if (dataEnableList[5]) txtList[5].text = Player.PackageManager.GetGoodsNumber(GoodsId.ActRecruitPoint).ToString();
            if (dataEnableList[6]) txtList[6].text = Player.PackageManager.Energy.ToString();
            if (dataEnableList[7]) txtList[7].text = Player.PackageManager.GetGoodsNumber(GoodsId.TowerMoney).ToString();
            if (dataEnableList[8]) txtList[8].text = FBTowerController.Instance.FBData.currentStar.ToString();
            if (dataEnableList[9]) txtList[9].text = FBTowerController.Instance.FBData.totalStar.ToString();
            ForceRebuildLayout();
        }

        [SerializeField] private HorizontalLayoutGroup resourcesLayout = null;
        //强制重建布局
        private void ForceRebuildLayout()
        {
            foreach (var item in resouorcesTitleItemDic.Values)
            {
                item.ForceRebuildLayout();
            }
            foreach (var item in txtList)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(item.transform as RectTransform);
            }
            foreach (var item in dataList)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(item.transform as RectTransform);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(resourcesLayout.transform as RectTransform);
        }

        [SerializeField] private BabuButton diamondObj = null;
        [SerializeField] private BabuButton moneyObj = null;
        [SerializeField] private BabuButton bigBangObj = null;
        [SerializeField] private BabuButton contractObj = null;
        [SerializeField] private BabuButton recruitObj = null;
        [SerializeField] private BabuButton recruitObj1 = null;
        [SerializeField] private BabuButton powerObj = null;
        [SerializeField] private BabuButton towerMoneyObj = null;
        [SerializeField] private BabuButton towerStarObj = null;
        [SerializeField] private BabuButton towerTotalStarObj = null;

        private void AddBtn()
        {
            diamondObj.OnClick += OnClickDiamondObj;
            moneyObj.OnClick += OnClickMoneyObj;
            bigBangObj.OnClick += OnClickBigBangObj;
            contractObj.OnClick += OnClickContractObj;
            recruitObj.OnClick += OnClickRecruitObj;
            recruitObj1.OnClick += OnClickRecruitObj1;
            powerObj.OnClick += OnClickPowerObj;
            towerMoneyObj.OnClick += OnClickTowerMoneyObj;
            towerStarObj.OnClick += OnClickTowerStarObj;
            towerTotalStarObj.OnClick += OnClickTowerTotalStarObj;
        }

        private void RemoveBtn()
        {
            diamondObj.OnClick -= OnClickDiamondObj;
            moneyObj.OnClick -= OnClickMoneyObj;
            bigBangObj.OnClick -= OnClickBigBangObj;
            contractObj.OnClick -= OnClickContractObj;
            recruitObj.OnClick -= OnClickRecruitObj;
            recruitObj1.OnClick -= OnClickRecruitObj1;
            powerObj.OnClick -= OnClickPowerObj;
            towerMoneyObj.OnClick -= OnClickTowerMoneyObj;
            towerStarObj.OnClick -= OnClickTowerStarObj;
            towerTotalStarObj.OnClick -= OnClickTowerTotalStarObj;
        }

        private void OnClickDiamondObj(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Resource, ResourceId.Diamond, Player.PackageManager.Diamond);
            itemtipsUIProperties.SetPos(diamondObj.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void OnClickMoneyObj(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Resource, ResourceId.Money, Player.PackageManager.Money);
            itemtipsUIProperties.SetPos(moneyObj.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void OnClickBigBangObj(BabuButton button)
        {
            GoodsConfig goodsConfig = Configs.Goods.GetConfig(ResourceId.TrainExpMin);
            UIController.Instance.OpenWindow<PoptipsUI>(new PoptipsUIProperties(goodsConfig.Desc, bigBangObj.transform, new Vector3(0, -20f, 0), TextAlignmentOptions.Midline));
        }
        private void OnClickContractObj(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Goods, GoodsId.ContractFragment, Player.PackageManager.GetGoodsNumber(GoodsId.ContractFragment));
            itemtipsUIProperties.SetPos(contractObj.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void OnClickRecruitObj(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Goods, GoodsId.RecruitPoint, Player.PackageManager.GetGoodsNumber(GoodsId.RecruitPoint));
            itemtipsUIProperties.SetPos(recruitObj.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void OnClickRecruitObj1(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Goods, GoodsId.ActRecruitPoint, Player.PackageManager.GetGoodsNumber(GoodsId.ActRecruitPoint));
            itemtipsUIProperties.SetPos(recruitObj1.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void OnClickPowerObj(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Resource, ResourceId.Energy, Player.PackageManager.Energy);
            itemtipsUIProperties.SetPos(powerObj.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void OnClickTowerMoneyObj(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Goods, GoodsId.TowerMoney, Player.PackageManager.GetGoodsNumber(GoodsId.TowerMoney));
            itemtipsUIProperties.SetPos(towerMoneyObj.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void OnClickTowerStarObj(BabuButton button)
        {
            UIController.Instance.OpenWindow<PoptipsUI>(new PoptipsUIProperties("在篮球殿堂本局游戏中拥有的星星", towerStarObj.transform, new Vector3(0, -20f, 0), TextAlignmentOptions.Midline));
        }
        private void OnClickTowerTotalStarObj(BabuButton button)
        {
            UIController.Instance.OpenWindow<PoptipsUI>(new PoptipsUIProperties("在篮球殿堂中累计获得的星星", towerTotalStarObj.transform, new Vector3(0, -20f, 0), TextAlignmentOptions.Midline));
        }

    }
}
