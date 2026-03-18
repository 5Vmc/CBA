using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;
using Utils.GameItem;
using System.Linq;

namespace BigBang.UI
{
    public class SkillLearnUIProperties : WindowProperties
    {
        public Skill SkillData {
            get;
            set;
        }

        public int CardId{
            get;
            set;
        }
       
        public SkillLearnUIProperties(int cardId, Skill data)
        {
            CardId = cardId;
            SkillData = data;
        }
    }
    public class SkillLearnUI : AWindowController<SkillLearnUIProperties>
    {
        [SerializeField] private SkillIcon2 SkillItem;
        [SerializeField] private TMP_Text TargetLevelText;
        [SerializeField] private TMP_Text EffectNowText;
        [SerializeField] private TMP_Text EffectNextText;
        [SerializeField] private TMP_Text  EffectMaxText;
        [SerializeField] private GameObject UpgradePanel;
        [SerializeField] private GameObject MaxPanel;

        [SerializeField] private  PropItem[] Props;
       
        [SerializeField] private Button UpgradeBtn;

        [SerializeField] private Button CloseBtn;

        [SerializeField] private TMP_Text  EffectDesText;

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            Skill data = Properties.SkillData;

            this.UpdateUI(data);
        }

        private void UpdateUI(Skill data)
        {
            if(data.Level >= GameConst.MaxSkillLevel){
                UpgradePanel.SetActive(false);
                MaxPanel.SetActive(true);
                
                EffectMaxText.text = data.GetEffectaddValue().ToString();
                TargetLevelText.text = (data.Level + 1).ToString();
                SkillItem.SetData(data);

                return;
            }

            var buffCfg = Configs.SkillAddition.GetConfig(data.Config.BuffType);
            
            EffectDesText.text = buffCfg.Name.Replace("{value}",  "");//$"<color=#0EDE35>{data.GetNextEffectaddValue()}</color>" );

            UpgradePanel.SetActive(true);
            MaxPanel.SetActive(false);
            TargetLevelText.text = (data.Level + 1).ToString();

            SkillItem.SetData(data);
            EffectNowText.text = data.GetEffectaddValue().ToString();

            EffectNextText.text = data.GetNextEffectaddValue().ToString();

            List<GameItem> propNeeds = this. GetUpgradeNeeds( data );

            for(int i=0; i<Props.Length; i++){
                if(i >= propNeeds.Count)break;
                Props[i].SetData(propNeeds[i], true);
            }
        }

        private List<GameItem> GetUpgradeNeeds( Skill data )
        {
            List<GameItem> retList = new List<GameItem>();
            SkillUpgradeConfig conf = Configs.SkillUpgrade.GetConfigList().Find((item)=>{return item.Quality == data.Config.Quality && item.TargetLevel == data.Level+1;});
            
            KeyValuePair<int, int> costGoods = conf.CostGoods.FirstOrDefault();

            retList.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, costGoods.Key, costGoods.Value) );
            
            retList.Add(GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Money, conf.CostMoney) );

            return retList;
        }

        protected override void AddListeners()
        {
            CloseBtn.onClick.AddListener(OnClose);
            UpgradeBtn.onClick.AddListener(OnUpgrade);
           
        }

        protected override void RemoveListeners()
        {
            CloseBtn.onClick.RemoveListener(OnClose);
            UpgradeBtn.onClick.RemoveListener(OnUpgrade);
        }


        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            //anim.PlayExit(() =>
            //{
                UIController.Instance.CloseWindow<SkillLearnUI>();
                TouchManager.Instance.EnableTouch();
                
           // });
        }

        private void OnUpgrade()
        {
            GameItem notEnoughItem  = null;
            foreach(PropItem pItem in Props){
                if(pItem.ownerEnough() == false){
                    notEnoughItem = pItem.ItemData;
                    break;
                }

            }
            
            if(notEnoughItem != null){
                UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(notEnoughItem));
                return;
            }
            NetworkManager.Instance.UpgradeSkill(Properties.CardId, Properties.SkillData.Id, (resp)=>{
                Properties.SkillData.LevelUpgrade();
                this.UpdateUI(Properties.SkillData);

                CardDetailUI.Instance.skillPad.UpdateLevel(Properties.SkillData.Id);
            
            });
        }
    }
}