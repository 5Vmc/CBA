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
using static BigBang.HeroManager;

namespace BigBang.UI
{
    public enum HeroChapterItemState
    {
        Unknow,
        Open,
        Lock
    }

    public class HeroChapterItem : MonoBehaviour
    {

        [SerializeField] private GameObject alphaChangePanel;
        [SerializeField] private Image lockImage;

        [SerializeField] private TMP_Text passCountText;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text chapterNameText;
        [SerializeField] private InventoryItem inventoryItem;
        [SerializeField] private Image playerIconImage;
        [SerializeField] private Image reddot;
        [SerializeField] private PeakImage peakImage = null;

        [SerializeField] private BabuButton itemButton;

        public HeroChapterData data;
        public HeroChapterItemState heroChapterItemState = HeroChapterItemState.Unknow;

        private void OnEnable()
        {
            itemButton.OnClick += OnClickItemButton;
        }
        private void OnDisable()
        {
            itemButton.OnClick -= OnClickItemButton;
        }

        public async void SetData(HeroChapterData data)
        {
            this.data = data;

            if (!data.IsOpen)
                heroChapterItemState = HeroChapterItemState.Lock;
            else
                heroChapterItemState = HeroChapterItemState.Open;

            switch (heroChapterItemState)
            {
                case HeroChapterItemState.Open:
                    {
                        alphaChangePanel.SetAlpha(1f);
                        lockImage.gameObject.SetActive(false);
                        levelText.text = "{0}星".SafeFormat(data.challengeHeroChapterConfig.Star);
                        passCountText.gameObject.SetActive(true);
                        passCountText.text = "<color=#13b237>{0}</color>/{1}".SafeFormat(data.chapterMapInfo.Pass, data.challengeHeroChapterConfig.Number);
                    }
                    break;
                case HeroChapterItemState.Lock:
                    {
                        alphaChangePanel.SetAlpha(0.62f);
                        lockImage.gameObject.SetActive(true);
                        levelText.text = "获得球员开启";
                        passCountText.gameObject.SetActive(false);
                    }
                    break;
            }

            peakImage.SetData(data.cardModelConfig);
            if (data.cardModelConfig == null)
            {
                playerNameText.text = "--";
            }
            else
            {
                playerNameText.text = data.cardModelConfig.Name;
            }
            chapterNameText.text = data.challengeHeroChapterConfig.Name;
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(data.challengeHeroChapterConfig.Showrewards).ToList();
            inventoryItem.SetData(gameItemList[0]);
            playerIconImage.sprite = await SpriteProxy.GetHeroIcon(data.challengeHeroChapterConfig.Hero.ToString());

            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBClassicHero, "/" + data.cardModelConfig.Id.ToString());
            node.IsRed(reddot.transform);
        }

        private void OnClickItemButton(BabuButton sender)
        {
            switch (heroChapterItemState)
            {
                case HeroChapterItemState.Open:
                    {
                        UIController.Instance.ShowPanel<HeroClubUI>(new HeroClubUIProperties(data.challengeHeroChapterConfig.Id, data.challengeHeroChapterConfig.Hero));
                    }
                    break;
                case HeroChapterItemState.Lock:
                    {
                        if (data.cardModelConfig == null || Player.CardManager.GetCard(data.cardModelConfig.Id) == null)
                        {
                            Tips.PopTips("获得该球员后开启");
                        }
                        else
                        {
                            //"{0}星".SafeFormat(data.challengeHeroChapterConfig.Star);
                            Tips.PopTips("{0}达到{1}星后开启".SafeFormat(PlayerCard.GetFullName(data.cardModelConfig), data.challengeHeroChapterConfig.Star));
                        }
                    }
                    break;
            }

        }
    }
}
