using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class TacticsSetPad : MonoBehaviour
    {
        private FormationBase _formation;

        [SerializeField] private List<TacticCardItem> DEFCardList = new();
        [SerializeField] private List<TacticCardItem> ATKCardList = new();
        [SerializeField] private RectTransform UpgradePanel;
        [SerializeField] private RectTransform MaxPanel;
        [SerializeField] private RectTransform MinPanel;
        [SerializeField] private RectTransform CostPanel;
        [SerializeField] private Button UseButton;
        [SerializeField] private TMP_Text UseTextText;
        [SerializeField] private TMP_Text TacticNameText;
        [SerializeField] private TMP_Text TacticLevelText;
        [SerializeField] private Button HelpButton;
        [SerializeField] private Button CostIconImage;

        //[SerializeField] private TacticsSelectAdapter osa;

        TacticsSetPadAnim Anim;

        private void Awake()
        {
            if (Anim == null)
            {
                Anim = GetComponent<TacticsSetPadAnim>();
            }
        }


        private void OnEnable()
        {
            UseButton.onClick.AddListener(OnClickUseButton);
            HelpButton.onClick.AddListener(OnClickHelpOpenButton);
            UpgradeButton.onClick.AddListener(OnClickUpgradeButton);
            CostIconImage.onClick.AddListener(OnClickCostIconImage);
            EventManager.Instance.Register(EventID.OnRefreshGoods, OnRefreshGoods);

            Anim.PlayEnter();
        }

        private void OnDisable()
        {
            UseButton.onClick.RemoveListener(OnClickUseButton);
            HelpButton.onClick.RemoveListener(OnClickHelpOpenButton);
            UpgradeButton.onClick.RemoveListener(OnClickUpgradeButton);
            CostIconImage.onClick.RemoveListener(OnClickCostIconImage);
            EventManager.Instance.Unregister(EventID.OnRefreshGoods, OnRefreshGoods);
        }

        public void OnClickHelpOpenButton()
        {
            UIController.Instance.OpenWindow<TacticHelpUI>(new TacticHelpUIProperties(selectTacticCardItem.tacticCfg.Id));
        }

        public void OnShow()
        {
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.ARENA, formation =>
            {
                OnShow(formation);
            });
        }
        public void OnShow(FormationBase formation)
        {
            SetStaticConfig();

            _formation = formation;

            RefreshData();
        }

        private bool isConfigInited = false;
        private void SetStaticConfig()
        {
            if (isConfigInited == true) return;
            isConfigInited = true;
            for (int i = 0; i < 5; i++)
            {
                TacticCardItem defTacticCardItem = DEFCardList[i];
                int defId = 200 + i + 1;
                TacticsConfig defTacticCfg = Configs.Tactics.GetDataDictionary()[defId];
                defTacticCardItem.SetConfig(defTacticCfg, this);

                TacticCardItem atkTacticCardItem = ATKCardList[i];
                int atkId = 100 + i + 1;
                TacticsConfig atkTacticCfg = Configs.Tactics.GetDataDictionary()[atkId];
                atkTacticCardItem.SetConfig(atkTacticCfg, this);
            }
        }

        private void RefreshData()
        {
            for (int i = 0; i < 5; i++)
            {
                DEFCardList[i].SetData(_formation);
                ATKCardList[i].SetData(_formation);

                if (selectTacticCardItem == null)
                {
                    int defaultSelectId = 101;
                    if (_formation != null && _formation.TacticsIdList != null && _formation.TacticsIdList.Count > 0)
                    {
                        defaultSelectId = _formation.TacticsIdList[0];
                    }
                    if (DEFCardList[i].tacticCfg.Id == defaultSelectId)
                    {
                        OnClickTacticCardItem(DEFCardList[i]);
                    }
                    if (ATKCardList[i].tacticCfg.Id == defaultSelectId)
                    {
                        OnClickTacticCardItem(ATKCardList[i]);
                    }
                }
                else
                {
                    OnClickTacticCardItem(selectTacticCardItem);
                }
            }
        }

        TacticCardItem selectTacticCardItem = null;
        public void OnClickTacticCardItem(TacticCardItem tacticCardItem)
        {
            selectTacticCardItem = tacticCardItem;
            for (int i = 0; i < 5; i++)
            {
                DEFCardList[i].SetSelect(false);
                ATKCardList[i].SetSelect(false);
            }
            tacticCardItem.SetSelect(true);

            RefreshDetailInfo();
        }

        public void OnClickUseButton()
        {
            if (selectTacticCardItem.level == 0)
            {
                Tips.PopTips("该战术未解锁");
                return;
            }
            foreach (int TacticsId in _formation.TacticsIdList)
            {
                if (TacticsId == selectTacticCardItem.tacticCfg.Id)
                {
                    Tips.PopTips("该战术已在使用中");
                    return;
                }
            }
            if (selectTacticCardItem.tacticCfg.Type == 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    ATKCardList[i].SetUse(false);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    DEFCardList[i].SetUse(false);
                }

            }
            selectTacticCardItem.SetUse(true);
            List<int> TacticsIdList = new();
            for (int i = 0; i < 5; i++)
            {
                if (ATKCardList[i].isUse)
                {
                    TacticsIdList.Add(ATKCardList[i].tacticCfg.Id);
                }
                if (DEFCardList[i].isUse)
                {
                    TacticsIdList.Add(DEFCardList[i].tacticCfg.Id);
                }
            }
            _formation.TacticsIdList = TacticsIdList;
            NetworkManager.Instance.SaveFormation(_formation.FormationId, (Formation)_formation, (_) =>
             {
                 if (_formation.FormationId == FormationID.PVE) Player.CardManager.CheckRedDot(0, true);
                 RefreshData();
             });
        }

        public void RefreshDetailInfo()
        {
            UseTextText.text = selectTacticCardItem.tacticCfg.Type == 1 ? "设置进攻战术" : "设置防守战术";
            TacticNameText.text = selectTacticCardItem.tacticCfg.Name;
            KeZhiText.text = selectTacticCardItem.tacticCfg.Restrain.ToString();
            bool isUnlock = selectTacticCardItem.level > 0;
            TacticLevelText.gameObject.SetActive(isUnlock);
            if (isUnlock)
            {
                TacticLevelText.text = string.Format("{0}级", selectTacticCardItem.level);
            }
            int nextId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level + 1;
            bool isMax = Configs.TacticsUpgrade.GetDataDictionary().ContainsKey(nextId);

            MinPanel.gameObject.SetActive(false);
            MaxPanel.gameObject.SetActive(false);
            UpgradePanel.gameObject.SetActive(false);
            CostPanel.gameObject.SetActive(false);

            if (isUnlock == false)
            {
                MinPanel.gameObject.SetActive(true);
                RefreshDetailInfoMin();
                CostPanel.gameObject.SetActive(true);
                RefreshCostPanel();
            }
            else if (isMax == false)
            {
                MaxPanel.gameObject.SetActive(true);
                RefreshDetailInfoMax();
            }
            else
            {
                UpgradePanel.gameObject.SetActive(true);
                RefreshDetailInfoMid();
                CostPanel.gameObject.SetActive(true);
                RefreshCostPanel();
            }
        }

        public void OnRefreshGoods(object[] objs)
        {
            RefreshCostPanel();
        }

        [SerializeField] private TMP_Text CostNumText;
        public void RefreshCostPanel()//消耗多少东西
        {
            int nextUpId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level + 1;
            bool isMax = Configs.TacticsUpgrade.GetDataDictionary().ContainsKey(nextUpId) == false;
            if (isMax) return;
            TacticsUpgradeConfig tacticsUpgradeConfigNext = Configs.TacticsUpgrade.GetDataDictionary()[nextUpId];
            int needCost = tacticsUpgradeConfigNext.CostCard;
            int nowCost = Player.PackageManager.GetGoodsNumber(GoodsId.TacticsCard);
            bool isEnoughToUpgrade = needCost <= nowCost;
            string enoughStr = "<color=#4DC471>{0}</color><color=#FFFFFF>/{1}</color>";
            string notEnoughStr = "<color=#e56e92>{0}</color><color=#FFFFFF>/{1}</color>";
            string costStr = isEnoughToUpgrade ? enoughStr : notEnoughStr;
            CostNumText.text = string.Format(costStr, nowCost, needCost);
        }

        [SerializeField] private List<TMP_Text> MinNextLevelTextList;
        public void RefreshDetailInfoMin()//未解锁
        {
            int nextUpId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level + 1;
            TacticsUpgradeConfig tacticsUpgradeConfigNext = Configs.TacticsUpgrade.GetDataDictionary()[nextUpId];
            for (int i = 0; i < 5; i++)
            {
                MinNextLevelTextList[i].text = "<color=#FFFFFF>" + tacticsUpgradeConfigNext.PositionAdd[i + 1].ToString() + "%</color>";
            }
        }

        [SerializeField] private TMP_Text DetailTitleText;
        [SerializeField] private TMP_Text KeZhiText;
        [SerializeField] private List<TMP_Text> NowLevelTextList;
        [SerializeField] private List<TMP_Text> NextLevelTextList;

        [SerializeField] private Button UpgradeButton;
        public void RefreshDetailInfoMid()//可升级
        {
            DetailTitleText.text = selectTacticCardItem.tacticCfg.Type == 1 ? "进攻加成效果" : "防守加成效果";
            int nowUpId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level;
            TacticsUpgradeConfig tacticsUpgradeConfigNow = Configs.TacticsUpgrade.GetDataDictionary()[nowUpId];
            int nextUpId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level + 1;
            TacticsUpgradeConfig tacticsUpgradeConfigNext = Configs.TacticsUpgrade.GetDataDictionary()[nextUpId];
            for (int i = 0; i < 5; i++)
            {
                NowLevelTextList[i].text = tacticsUpgradeConfigNow.PositionAdd[i + 1].ToString() + "%";
                bool isBigger = tacticsUpgradeConfigNow.PositionAdd[i + 1] < tacticsUpgradeConfigNext.PositionAdd[i + 1];
                if (isBigger)
                {
                    NextLevelTextList[i].text = "<color=#4DC471>" + tacticsUpgradeConfigNext.PositionAdd[i + 1].ToString() + "%</color>";
                }
                else
                {
                    NextLevelTextList[i].text = "<color=#FFFFFF>" + tacticsUpgradeConfigNext.PositionAdd[i + 1].ToString() + "%</color>";
                }
            }
        }
        public void OnClickUpgradeButton()
        {
            int nextUpId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level + 1;
            TacticsUpgradeConfig tacticsUpgradeConfigNext = Configs.TacticsUpgrade.GetDataDictionary()[nextUpId];
            int needCost = tacticsUpgradeConfigNext.CostCard;
            int nowCost = Player.PackageManager.GetGoodsNumber(GoodsId.TacticsCard);
            bool isEnoughToUpgrade = needCost <= nowCost;
            if (isEnoughToUpgrade == false)
            {
                UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(GameItemType.Goods, GoodsId.TacticsCard, needCost));
                return;
            }
            NetworkManager.Instance.UpgradeTactics(selectTacticCardItem.tacticCfg.Id, (resp) =>
            {
                if (resp.Success == false)
                {
                    Tips.PopTips("升级失败");
                    return;
                }
                if (Player.FightManager.FormationController.TacticsLevelDic.ContainsKey(selectTacticCardItem.tacticCfg.Id) == false)
                {
                    Player.FightManager.FormationController.TacticsLevelDic.Add(selectTacticCardItem.tacticCfg.Id, 0);
                }
                Player.FightManager.FormationController.TacticsLevelDic[selectTacticCardItem.tacticCfg.Id]++;
                RefreshData();
            });
        }
        public void OnClickCostIconImage()
        {
            int nextUpId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level + 1;
            TacticsUpgradeConfig tacticsUpgradeConfigNext = Configs.TacticsUpgrade.GetDataDictionary()[nextUpId];
            int needCost = tacticsUpgradeConfigNext.CostCard;
            UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(GameItemType.Goods, GoodsId.TacticsCard, needCost));
        }

        [SerializeField] private List<TMP_Text> MaxNowLevelTextList;
        public void RefreshDetailInfoMax()//已满级
        {
            int nowUpId = selectTacticCardItem.tacticCfg.Id * 100 + selectTacticCardItem.level;
            TacticsUpgradeConfig tacticsUpgradeConfigNow = Configs.TacticsUpgrade.GetDataDictionary()[nowUpId];
            for (int i = 0; i < 5; i++)
            {
                MaxNowLevelTextList[i].text = tacticsUpgradeConfigNow.PositionAdd[i + 1].ToString() + "%";
            }
        }



    }
}