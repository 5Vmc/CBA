using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Utils;
using System;
using DG.Tweening;
using GameConfig.Config;
using System.Collections.Generic;
using TMPro;
using GameConfig;
using BigBang.Animation;

namespace BigBang.UI
{
    public class BigBreakthroughDIANJIUIProperties : WindowProperties
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public List<int> IntsList { get; set; }
        public RegularTrainItem Item { get; set; }
        public TrainConfig TrainCfg { get; set; }
        public BigBreakthroughDIANJIUIProperties(string name, int level, List<int> intsList, RegularTrainItem item, TrainConfig trainCfg)
        {
            Name = name;
            Level = level;
            IntsList = intsList;
            Item = item;
            TrainCfg = trainCfg;

        }

    }

    public class BigBreakthroughDIANJIUI : AWindowController<BigBreakthroughDIANJIUIProperties>
    {
        [SerializeField] private BigBreakthroughDIANJIUIComponent com;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle levelToggle1;
        [SerializeField] private BabuToggle levelToggle2;
        [SerializeField] private BabuToggle levelToggle3;
        [SerializeField] private BabuToggle levelToggle4;
        [SerializeField] private List<BabuToggle> levelToggleTab;
        [SerializeField] private BigBreakthroughDIANJIUIAnim anim;

        public PlayerTrainItem ptiIndex;
        private List<BreakConfig> _breakConfigs = new List<BreakConfig>();
        private Dictionary<int, BreakConfig> _breakConfigMap = new Dictionary<int, BreakConfig>();
        protected override void AddListeners()
        {
            com.CloseBtn.onClick.AddListener(OnClose);
            toggleGroup.OnValueChanged += OnToggleGroupChanged;
        }

        protected override void RemoveListeners()
        {
            com.CloseBtn.onClick.RemoveListener(OnClose);
            toggleGroup.OnValueChanged -= OnToggleGroupChanged;
        }

        private void OnToggleGroupChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle.transform.DOScale(0.96f, 0.1f);
            newToggle.transform.DOScale(1.1f, 0.1f);
            newToggle.transform.GetChild(1).gameObject.SetActive(true);
            newToggle.transform.GetChild(4).gameObject.SetActive(true);
            oldToggle.transform.GetChild(1).gameObject.SetActive(false);
            oldToggle.transform.GetChild(4).gameObject.SetActive(false);
            for (int i = 0; i < 4; i++)
            {
                if (toggleGroup.transform.GetChild(i).name == newToggle.name)
                {
                    ShowPartPanel(i);
                }

            }
        }

        private void OnClose()
        {
            anim.PlayNext(() =>
            {
                TouchManager.Instance.EnableTouch();
                UIController.Instance.CloseWindow<BigBreakthroughDIANJIUI>();
            });

        }

        protected override void OnPropertiesSet()
        {
            ptiIndex = Properties.Item.Item;
            TouchManager.Instance.DisableTouch();
            toggleGroup.Switch(levelToggle1);
            ShowPartPanel(0);
            com.BreakItemNameText.text = Properties.Name;
            com.DescText.text = Properties.TrainCfg.Desc;
            SetToggleText();
            anim.Play(() =>
            {
                TouchManager.Instance.EnableTouch();
            });

        }

        private void ShowPartPanel(int num)
        {
            com.BreakLevelText.text = Properties.Level.ToString() + "/" + Properties.IntsList[num].ToString() + Lang.Get(LangID.LvTxt);
            var cfg = ptiIndex.SetBuffTypeData(Properties.IntsList[num]);
            var bf = ptiIndex.SetBuffData(Properties.IntsList[num]);
            var cardcfg = ptiIndex.SetCardBuffData(Properties.IntsList[num]);
            com.ItemAddNameText.text = cfg.DescPart1 + ColorString.GetColorString("#98bdd8", cfg.DescPart2);
            com.PropertsNameText.text = cardcfg.Desc + ColorString.GetColorString("#98bdd8", cardcfg.DescPart2);
            SetItemTrainCountsText(cfg, bf, cardcfg);
        }

        //按钮等级显示
        private void SetToggleText()
        {
            for (int i = 0; i < Properties.IntsList.Count; i++)
            {
                for (int t = 2; t < 5; t++)
                {
                    levelToggleTab[i].transform.GetChild(t).GetComponent<TMP_Text>().text = Properties.IntsList[i].ToString();
                }
            }
        }
        //文本显示
        private void SetItemTrainCountsText(BuffTypeConfig cfg, BreakConfig bf, CardBuffTypeConfig cardcfg)
        {
            var count = cfg.DescOperator + bf.BuffValue.ToString();
            var count1 = cardcfg.DescOperator + bf.CardBuffValue.ToString();
            if (bf.BuffValue == 0)
            {
                com.TrainItemAddTypeModule.gameObject.SetActive(false);
            }
            else
            {
                com.TrainItemAddTypeModule.gameObject.SetActive(true);
                for (int i = 0; i < com.ItemTrainCountsText.Count; i++)
                {
                    if (i < count.Length)
                    {
                        com.ItemTrainCountsText[i].text = count[i].ToString();
                        com.ItemTrainCountsText[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        com.ItemTrainCountsText[i].gameObject.SetActive(false);
                    }
                }
            }
            if (bf.CardBuffValue == 0)
            {
                com.PlayerAddTypeModule.gameObject.SetActive(false);
            }
            else
            {
                com.PlayerAddTypeModule.gameObject.SetActive(true);
                for (int i = 0; i < com.PropertsCountsText.Count; i++)
                {
                    if (i < count1.Length)
                    {
                        com.PropertsCountsText[i].text = count1[i].ToString();
                        com.PropertsCountsText[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        com.PropertsCountsText[i].gameObject.SetActive(false);
                    }
                }
            }
        }
        //当前选中按钮界面数据
        private void SetSelectedLevelData()
        {

        }


    }
}