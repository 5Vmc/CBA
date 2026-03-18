using System;
using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    public class HundredTeamDetailCardData
    {
        public Protocol.FightCard fightCard = null;//游戏卡牌默认结构，名字，品质，星级等
        public bool isWin = false;//胜利失败
    }

    public class HundredTeamDetailCardItem : MonoBehaviour
    {
        [SerializeField] private List<Image> qualityImageList = new();
        [SerializeField] private Image playerIconImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] public Image darkImage = null;
        [SerializeField] private Image winImage = null;
        [SerializeField] private Image loseImage = null;
        [SerializeField] private PeakImage peakImage = null;

        public HundredTeamDetailCardData hundredTeamDetailCardData = null;
        public async void SetData(HundredTeamDetailCardData hundredTeamDetailCardData)
        {
            this.hundredTeamDetailCardData = hundredTeamDetailCardData;

            SetBg(hundredTeamDetailCardData.fightCard.Quality);
            SetStar(hundredTeamDetailCardData.fightCard.Star);

            darkImage.gameObject.SetActive(!hundredTeamDetailCardData.isWin);
            winImage.gameObject.SetActive(hundredTeamDetailCardData.isWin);
            loseImage.gameObject.SetActive(!hundredTeamDetailCardData.isWin);
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(hundredTeamDetailCardData.fightCard.CardId);
            bool isPeak = PlayerCard.IsPeak(cardModelConfig);
            if (isPeak)
            {
                nameText.text = cardModelConfig.Name;
            }
            else
            {
                nameText.text = hundredTeamDetailCardData.fightCard.Name;
            }
            peakImage.SetData(cardModelConfig);
            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(hundredTeamDetailCardData.fightCard.Portrait);
        }

        public PlayerCardMiniInfo playerCardMiniInfo = null;
        public async void SetData(PlayerCardMiniInfo playerCardMiniInfo)
        {
            this.playerCardMiniInfo = playerCardMiniInfo;

            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(playerCardMiniInfo.CardId);
            SetBg(playerCardMiniInfo.Quality);
            SetStar(playerCardMiniInfo.Star);
            nameText.text = cardModelConfig.Name;
            darkImage.gameObject.SetActive(false);
            winImage.gameObject.SetActive(false);
            loseImage.gameObject.SetActive(false);
            peakImage.SetData(cardModelConfig);
            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(cardModelConfig.Portrait);
        }

        public Protocol.FightCard fightCard = null;
        public async void SetData(Protocol.FightCard fightCard)
        {
            this.fightCard = fightCard;

            SetBg(fightCard.Quality);
            SetStar(fightCard.Star);
            nameText.text = fightCard.Name;
            darkImage.gameObject.SetActive(false);
            winImage.gameObject.SetActive(false);
            loseImage.gameObject.SetActive(false);
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(fightCard.CardId);
            peakImage.SetData(cardModelConfig);
            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(fightCard.Portrait);
        }

        private void SetBg(int quality)// 设置品质
        {
            for (int i = 0; i < qualityImageList.Count; i++)
            {
                qualityImageList[i].gameObject.SetActive(i == quality - 1);
            }
        }
        [SerializeField] private List<GameObject> stars;
        private async void SetStar(int star)// 设置星级
        {
            if (star > 5)
            {
                int showStar = star - 5;
                for (int i = 0; i < stars.Count; i++)
                {
                    stars[i].SetActive(true);
                    if (i + 1 <= showStar)
                        stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetColorfulStar();
                    else
                        stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
            else
            {
                for (int i = 0; i < stars.Count; i++)
                {
                    stars[i].SetActive(i + 1 <= star);
                    stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
        }
    }
}
