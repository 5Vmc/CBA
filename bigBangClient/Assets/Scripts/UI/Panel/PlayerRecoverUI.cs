using Babu;
using deVoid.UIFramework;
using GameConfig;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public enum PlayerRecoverType
    {
        RecoverMedical = 1,
        RecoverEnergy = 2,
        Coach2State = 3,
    }
    public class PlayerRecoverUIProperties : WindowProperties
    {
        public PlayerCard Card
        {
            get;
            private set;
        }
        public PlayerRecoverType RecoverType
        {
            get;
            private set;
        }
        public PlayerRecoverUIProperties(PlayerCard playerCard, PlayerRecoverType recoverType)
        {
            this.Card = playerCard;
            this.RecoverType = recoverType;
        }
    }
    public class PlayerRecoverUI : AWindowController<PlayerRecoverUIProperties>
    {

        // 状态类型标题
        [SerializeField] private TMP_Text txtStateTitle;
        // 状态类型文本
        [SerializeField] private TMP_Text txtStateSubtitle;

        [SerializeField] private Button closeBtn;

        [SerializeField] private PropButton propBtn;

        [SerializeField] private PropButton advPropBtn;

        [SerializeField] private GameObject medicalPanel;
        [SerializeField] private TMP_Text medicalText;

        [SerializeField] private GameObject energyPanel;
        [SerializeField] private TMP_Text energyText;

        [SerializeField] private GameObject statePanel;
        [SerializeField] private Image stateImg;

        [SerializeField] private CardItem cardItem;

        [SerializeField] private GameObject extraInfoPanel;

        [SerializeField] private TMP_Text cdTime;


        [SerializeField] private TMP_Text propDescText;
        [SerializeField] private TMP_Text advPropDescText;

        [SerializeField] private Image propEffectImg;
        [SerializeField] private Image advPropEffectImg;

        private int cdLeftTime;
        private GameItem gameItem;
        private GameItem advGameItem;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);

            propBtn.Btn.onClick.AddListener(OnClickProBtn);
            advPropBtn.Btn.onClick.AddListener(OnClickAdvPropBtn);
            EventManager.Instance.Register(EventID.OnCardRefreshData, reloadCardData);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            propBtn.Btn.onClick.RemoveListener(OnClickProBtn);
            advPropBtn.Btn.onClick.RemoveListener(OnClickAdvPropBtn);
            EventManager.Instance.Unregister(EventID.OnCardRefreshData, reloadCardData);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            NetworkManager.Instance.RefreshCardData(Properties.Card.CardId, (resp) => { });
            reloadCardData();
            this.UpdateUI(Properties.RecoverType);
        }

        private void reloadCardData(object[] args = null)
        {
            cardItem.SetData(Properties.Card);
        }

        private void SetCdTime()
        {
            long endTime = Properties.Card.InjuryEndTime;
            this.cdLeftTime = (int)(endTime - TimeUtils.Now());
            Debug.Log("...>>>>>endTime=" + endTime);
            if (this.cdLeftTime > 0)
            {
                cdTime.text = TimeUtils.FormatLeftTimeWithHourCn(this.cdLeftTime);
                // UnityTimer.Timer.Register(this.gameObject, )
                //TimeTickManager.Instance.RegistAction(this.CdTimeCountDonw);
            }
            else
            {
                extraInfoPanel.SetActive(false);
            }
        }

        /*private void CdTimeCountDonw()
        {
            if(this.cdLeftTime > 0){
                cdTime.text = TimeUtils.FormatLeftTimeWithHour( this.cdLeftTime );
            }
        }*/

        private void UpdateUI(PlayerRecoverType type)
        {
            int gid = 0, advGid = 0;
            medicalPanel.SetActive(false);
            energyPanel.SetActive(false);
            statePanel.SetActive(false);
            String effectText1 = null;
            String effectText2 = null;
            // Sprite effectImg1 = null, effectImg2=null;
            propEffectImg.gameObject.SetActive(false);
            advPropEffectImg.gameObject.SetActive(false);
            switch (type)
            {
                case PlayerRecoverType.RecoverMedical:
                    gid = GoodsId.MedicalBox;
                    effectText1 = "-" + TimeUtils.FormatHourTime(Configs.Goods.GetConfig(gid).Param2);
                    advGid = GoodsId.AdvMedicalBox;
                    effectText2 = "-" + TimeUtils.FormatHourTime(Configs.Goods.GetConfig(advGid).Param2);
                    medicalPanel.SetActive(true);
                    extraInfoPanel.SetActive(true);
                    txtStateTitle.text = "伤病处理";
                    txtStateSubtitle.text = "身体状况";
                    SetCdTime();
                    break;

                case PlayerRecoverType.RecoverEnergy:
                    gid = GoodsId.EnergyDrink;
                    advGid = GoodsId.AdvEnergyDrink;
                    energyPanel.SetActive(true);
                    extraInfoPanel.SetActive(false);
                    effectText1 = "+" + Configs.Goods.GetConfig(gid).Param2;
                    effectText2 = "+" + Configs.Goods.GetConfig(advGid).Param2;
                    txtStateTitle.text = "体能补充";
                    txtStateSubtitle.text = "体能状况";
                    break;

                case PlayerRecoverType.Coach2State:
                    gid = GoodsId.CoachQuotes;
                    advGid = GoodsId.AdvCoachQuotes;
                    statePanel.SetActive(true);
                    extraInfoPanel.SetActive(false);
                    propEffectImg.gameObject.SetActive(true);
                    advPropEffectImg.gameObject.SetActive(true);
                    SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[Configs.Goods.GetConfig(gid).Param2], (s) => { propEffectImg.sprite = s; });

                    SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[Configs.Goods.GetConfig(advGid).Param2], (s) => { advPropEffectImg.sprite = s; });

                    txtStateTitle.text = "状态调整";
                    txtStateSubtitle.text = "当前状态";
                    break;
            }

            propDescText.text = Configs.Goods.GetConfig(gid).Desc;
            advPropDescText.text = Configs.Goods.GetConfig(advGid).Desc;

            this.gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, gid, 0);
            propBtn.SetData(this.gameItem);

            this.advGameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, advGid, 0);
            advPropBtn.SetData(this.advGameItem);

            // 设置球员伤病状态
            LangID healthLangID = new LangID[] { 0, LangID.HealthText, LangID.MinorInjuryText, LangID.SeriousInjury }[(int)Properties.Card.InjuryType];
            medicalText.text = Lang.Get(healthLangID);

            // 设置球员状态
            SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)Properties.Card.Status], (s) => { stateImg.sprite = s; });

            // 设置球员体能
            //energyText.text = "{0}%(储备{1}%)".SafeFormat(Properties.Card.SingleEnergyRatio.ToString("f2"), Properties.Card.BackupEnergyRatio.ToString("f2"));
            energyText.text = "{0}%".SafeFormat(Properties.Card.TotalEnergyRatio.ToString("f2"));
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<PlayerRecoverUI>();
            EventManager.Instance.Dispatch(EventID.RefreshWindow, 99);
        }

        private bool CheckFull(bool advGid = false)
        {
            switch (Properties.RecoverType)
            {
                case PlayerRecoverType.RecoverMedical:
                    if (Properties.Card.InjuryType == InjuryType.Health || Properties.Card.InjuryType == InjuryType.None)
                    {
                        Tips.PopTips("球员没有伤病");
                        return true;
                    }
                    break;
                case PlayerRecoverType.RecoverEnergy:
                    if (Properties.Card.Energy >= GameConst.CardInitEnergy)
                    {
                        Tips.PopTips("球员能量满满");
                        return true;
                    }
                    break;
                case PlayerRecoverType.Coach2State:
                    if (advGid == true)
                    {
                        if (Properties.Card.Status == PlayerCardStatus.VeryGood)
                        {
                            Tips.PopTips("球员状态爆棚");
                            return true;
                        }
                    }
                    else
                    {
                        if (Properties.Card.Status >= PlayerCardStatus.Ordinary)
                        {
                            Tips.PopTips("球员状态平稳");
                            return true;
                        }
                    }
                    break;

            }
            return false;
        }
        private void OnClickProBtn()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            if (true == this.CheckFull()) return;
            if (this.gameItem.GetPlayerCount() <= 0)
            {
                UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(GameItemType.Goods, this.gameItem.Id, 1));
            }
            else
            {
                NetworkManager.Instance.RecoverPlayer(Properties.Card.CardId, this.gameItem.Id, (resp) =>
                {
                    this.UpdateUI(Properties.RecoverType);
                    EventManager.Instance.Dispatch(EventID.RefreshCardRecoverProperties);
                });
            }
        }

        public void Update()
        {
            if (Properties.RecoverType == PlayerRecoverType.RecoverEnergy)
            {
                //energyText.text = "{0}%(储备{1}%)".SafeFormat(Properties.Card.SingleEnergyRatio.ToString("f2"), Properties.Card.BackupEnergyRatio.ToString("f2"));
                energyText.text = "{0}%".SafeFormat(Properties.Card.TotalEnergyRatio.ToString("f2"));
            }
        }

        private void OnClickAdvPropBtn()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            if (true == this.CheckFull(true)) return;
            if (this.advGameItem.GetPlayerCount() <= 0)
            {
                UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(GameItemType.Goods, this.advGameItem.Id, 1));
            }
            else
            {
                NetworkManager.Instance.RecoverPlayer(Properties.Card.CardId, this.advGameItem.Id, (resp) =>
                {
                    this.UpdateUI(Properties.RecoverType);
                    EventManager.Instance.Dispatch(EventID.RefreshCardRecoverProperties);
                });
            }
        }
    }
}
