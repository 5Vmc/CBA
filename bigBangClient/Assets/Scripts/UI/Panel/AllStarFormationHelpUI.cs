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

namespace BigBang.UI
{
    public class AllStarFormationHelpUI : AWindowController
    {
        #region 初始化与监听
        protected override void AddListeners()
        {
            toggleGroup.OnValueChanged += OnToggleChanged;
            closeBtn.OnClick += OnClickClose;
        }
        protected override void RemoveListeners()
        {
            toggleGroup.OnValueChanged -= OnToggleChanged;
            closeBtn.OnClick -= OnClickClose;
        }
        protected override void OnPropertiesSet()
        {
            if(AllStarManager.Instance.serverData.Area == (int)Area.South)
            {
                toggleGroup.Switch(0);
            }
            else
            {
                toggleGroup.Switch(1);
            }
            scrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                scrollView.verticalNormalizedPosition = 1f;
                scrollView.enabled = true;
            });
        }
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<AllStarFormationHelpUI>();
        }
        #endregion

        #region 底部页签

        [SerializeField] private ScrollRect scrollView = null;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private List<Image> northImageList;
        [SerializeField] private List<Image> southImageList;
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            int selectIndex = toggleGroup.EnableIndex;
            Area area = selectIndex == 0 ? Area.South : Area.North;
            SetPlayers(area);
            foreach (var item in northImageList)
            {
                item.gameObject.SetActive(area == Area.North);
            }
            foreach (var item in southImageList)
            {
                item.gameObject.SetActive(area == Area.South);
            }
            scrollView.verticalNormalizedPosition = 1f;
        }

        #endregion

        #region 设置球员

        [SerializeField] private GridLayoutGroup cardContent1 = null;
        [SerializeField] private GridLayoutGroup cardContent2 = null;
        [SerializeField] private GridLayoutGroup cardContent3 = null;
        [SerializeField] private VerticalLayoutGroup contentPanel = null;
        [SerializeField] private GameObject itemPrefab = null;
        [SerializeField] private List<AllStarFormationHelpCardItem> cardTypeList1 = new();
        [SerializeField] private List<AllStarFormationHelpCardItem> cardTypeList2 = new();
        [SerializeField] private List<AllStarFormationHelpCardItem> cardTypeList3 = new();
        private void SetPlayers(Area area)
        {
            SetPlayers(area, Type.Up, cardTypeList1, cardContent1);
            SetPlayers(area, Type.First, cardTypeList2, cardContent2);
            SetPlayers(area, Type.Substitute, cardTypeList3, cardContent3);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.transform as RectTransform);
        }
        private void SetPlayers(Area area, Type type, List<AllStarFormationHelpCardItem> cardTypeList, GridLayoutGroup cardContent)
        {
            List<AllStarAdditionConfig> allStarAdditionConfigList = Configs.AllStarAddition.GetConfigList().Where(cfg => cfg.Type == (int)type && cfg.Area == (int)area).ToList();
            int needItem = allStarAdditionConfigList.Count - cardTypeList.Count;
            if (needItem > 0)
            {
                for (int i = 0; i < needItem; i++)
                {
                    GameObject itemGo = GameObject.Instantiate(itemPrefab, cardContent.transform);
                    AllStarFormationHelpCardItem item = itemGo.GetComponent<AllStarFormationHelpCardItem>();
                    cardTypeList.Add(item);
                }
            }
            for (int i = 0; i < Mathf.Max(allStarAdditionConfigList.Count, cardTypeList.Count); i++)
            {
                AllStarFormationHelpCardItem cardItem = cardTypeList[i];
                if (i >= allStarAdditionConfigList.Count)
                {
                    cardItem.gameObject.SetActive(false);
                    continue;
                }
                CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(allStarAdditionConfigList[i].Id);
                if (cardModelConfig == null)
                {
                    cardItem.gameObject.SetActive(false);
                    Debug.LogWarning("AllStarFormationHelpUI , SetPlayers , cardModelConfig == null , allStarAdditionConfigList[i].Id = " + allStarAdditionConfigList[i].Id);
                    continue;
                }
                cardItem.gameObject.SetActive(true);
                cardItem.SetData(cardModelConfig);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardContent.transform as RectTransform);
        }

        #endregion

    }
}