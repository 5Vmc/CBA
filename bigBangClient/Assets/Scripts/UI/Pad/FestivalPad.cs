using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.SpriteNames;

namespace BigBang.UI
{

    public class FestivalPad : MonoBehaviour, IActivity
    {
        [SerializeField] private Image bgImage;
        [SerializeField] private Button btnSign;
        [SerializeField] private Transform redNode;
        [SerializeField] private List<InventoryBaseItem> skillItemList;

        private ActivityData data;

        private int signActivityId = 13001;
        protected void OnEnable()
        {
            btnSign.onClick.AddListener(openSign);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        }

        protected void OnDisable()
        {
            btnSign.onClick.RemoveListener(openSign);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        }

        private void RefreshRedDot(object[] args)
        {
            RedDotNode node1 = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FestivalLogin, "");
            node1.IsRed(redNode);
        }

        private void openSign()
        {
            //UIController.Instance.OpenWindow<SevenDaysLoginUI>(new SevenDaysLoginUIProperties(signActivityId), true);
            TriggerManager.Instance.JumpPanel(TriggerModuleType.Welfare);
        }

        public void LoadActivity(ActivityData _data)
        {
            data = _data;

            SetCardGiftSkill(skillItemList, 104035);

            RefreshRedDot(null);
        }

        private async void SetCardGiftSkill(List<InventoryBaseItem> giftSkillList, int cardId)
        {
            PlayerCard Card = PlayerCard.GetEmptyCard(cardId);
            //天赋技能
            var giftSkillTemplateIdList = Configs.CardModel.GetConfig(cardId).GiftIds.ToList();
            var cfg = Configs.CardUpgrade.GetConfigList().FirstOrDefault(p => p.CardId == cardId && p.Star == 5 && p.Quality == 5);
            var skillLvLst = cfg != null ? cfg.Sklv : new Dictionary<int, int> { { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 } };
            for (var index = 0; index < 4; index++)
            {
                if (index >= giftSkillTemplateIdList.Count)
                {
                    giftSkillList[index].gameObject.SetActive(false);
                }
                else
                {
                    giftSkillList[index].gameObject.SetActive(true);

                    var _skId = giftSkillTemplateIdList[index] + (skillLvLst[index + 1] - 1) * 10;
                    var skillActived = true;
                    var _skCfg = Configs.GiftSkill.GetConfig(_skId);

                    var sp = await SpriteProxy.GetGiftSkillImg(_skCfg);
                    var fireSection = PlayerCard.GetSkillFireSection(_skCfg);
                    giftSkillList[index].SetData(_skCfg.Name, _skCfg.Desc, sp, _skCfg.Sklv, skillActived, false, true, _skCfg.Fire > 0, fireSection);
                    giftSkillList[index].SetText("Lv." + skillLvLst[index + 1].ToString());
                }
            }
        }

        private async void setData(InventoryBaseItem slot, GiftSkillConfig cfg)
        {
            var sprite = await SpriteProxy.GetGiftSkillImg(cfg);

            slot.SetData("", "", sprite, 2, true, false, true, false);
            slot.SetFire(true);
            slot.OpenTips = true;
            slot.SetText("");
        }
    }
}