using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using GameConfig.Config;
using Babu;
using static BigBang.ClassicManager;
using Protocol;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;
using System.Linq;
using System;
using DG.Tweening;

namespace BigBang.UI
{
    public class CardUpStarItemData
    {
        public int cardId;
        public PlayerCard card;

        public CardUpgradeConfig config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_cfg"></param>
        /// <param name="_cardStar"></param>
        /// <param name="_cardQuality"></param>
        /// <param name="giftskillid"></param>
        /// <param name="skIndex"></param>
        public CardUpStarItemData(int _cfgId)
        {
            config = Configs.CardUpgrade.GetConfig(_cfgId);
            cardId = config.CardId;
            card = Player.CardManager.GetCard(cardId);
        }
    }

    public class CardUpStarItem : MonoBehaviour
    {
        [SerializeField] private List<Image> starList;
        [SerializeField] private Image imgActive;
        [SerializeField] private Image imgBg;
        [SerializeField] private List<InventoryBaseItem> skIconList;


        private CardUpStarItemData data;
        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
        public async void SetData(CardUpStarItemData _data)
        {
            data = _data;
            int colorfulStarCount = _data.config.Star - 5;
            for (var index = 0; index < 5; index++)
            {
                if (index > _data.config.Star - 1)
                {
                    starList[index].gameObject.SetActive(false);
                }
                else
                {
                    starList[index].gameObject.SetActive(true);
                    if (index + 1 <= colorfulStarCount)
                        starList[index].sprite = await SpriteProxy.GetColorfulStar();
                    else
                        starList[index].sprite = await SpriteProxy.GetYellowStar();
                }
            }

            var starActived = data.card.Quality > _data.config.Quality || data.card.Star >= _data.config.Star;
            if (starActived)
            {
                imgActive.gameObject.SetActive(true);
                imgBg.sprite = await SpriteManager.GetSprite(AtlasNames.CardUp, "banner2");
            }
            else
            {
                imgActive.gameObject.SetActive(false);
                imgBg.sprite = await SpriteManager.GetSprite(AtlasNames.CardUp, "banner1");
            }

            var giftSkillTemplateIdList = Configs.CardModel.GetConfig(data.card.CardId).GiftIds.ToList();

            var baseSkillLv = new Dictionary<int, int> { { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 } };
            var skillLvLst = data.config.Sklv;

            for (var index = 0; index < 4; index++)
            {
                if (index >= giftSkillTemplateIdList.Count)
                {
                    skIconList[index].gameObject.SetActive(false);
                }
                else
                {
                    skIconList[index].gameObject.SetActive(true);

                    var skId = giftSkillTemplateIdList[index] + (skillLvLst[index + 1] - 1) * 10;
                    var _skCfg = Configs.GiftSkill.GetConfig(skId);
                    var skillActived = data.card.ActivedGiftSkillCount > index;
                    var _fireSection = PlayerCard.GetSkillFireSection(_skCfg);

                    var sp = await SpriteProxy.GetGiftSkillImg(_skCfg);
                    skIconList[index].SetData(_skCfg.Name, _skCfg.Desc, sp, _skCfg.Sklv, skillActived, false, true, _skCfg.Fire > 0, _fireSection);
                    skIconList[index].SetText("Lv." + skillLvLst[index + 1].ToString());

                }
            }
        }
    }
}
