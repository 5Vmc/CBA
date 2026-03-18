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
    public class LeagueTeamPlayerItem : MonoBehaviour
    {
        [SerializeField] private List<Image> qualityImageList = new();
        [SerializeField] private Image playerIconImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private Image playerIconFgImage = null;
        [SerializeField] private Image nameBgImage = null;
        [SerializeField] private Image stateImage = null;
        [SerializeField] private Image medicalImage = null;
        [SerializeField] private TMP_Text strengthNumText = null;
        [SerializeField] private TMP_Text progressbarPercentText = null;
        [SerializeField] private PeakImage peakImage = null;
        [SerializeField] private TMP_Text positionText = null;

        [SerializeField] private Image progressbarNotFullFgImage = null;
        [SerializeField] private Image progressbarFullFgImage = null;

        public PlayerCardMiniInfo playerInfo = null;
        public async void SetData(PlayerCardMiniInfo playerInfo)
        {
            this.playerInfo = playerInfo;

            SetBg(playerInfo.Quality);
            strengthNumText.text = playerInfo.CombatEffectiveness.ToString("N0");
            bool isHurt = playerInfo.InjuryType == (int)InjuryType.MinorInjury || playerInfo.InjuryType == (int)InjuryType.SeriousInjury;
            medicalImage.gameObject.SetActive(isHurt);
            bool isSingleWarning = playerInfo.Energy < GameConst.CardSingleEnergyWarning;
            progressbarNotFullFgImage.gameObject.SetActive(isSingleWarning);
            progressbarFullFgImage.gameObject.SetActive(!isSingleWarning);
            if (isSingleWarning == false)
            {
                progressbarFullFgImage.fillAmount = Utility.KeepInRange(playerInfo.Energy / GameConst.PlayerMaxEnergy, 0, 1);
            }
            else
            {
                progressbarNotFullFgImage.fillAmount = Utility.KeepInRange(playerInfo.Energy / GameConst.PlayerMaxEnergy, 0, 1);
            }
            progressbarPercentText.text = "{0}%".SafeFormat(Mathf.FloorToInt(Utility.KeepInRange(playerInfo.Energy / GameConst.PlayerMaxEnergy, 0, 1) * 100));
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(playerInfo.CardId);
            nameText.text = cardModelConfig.Name;
            peakImage.SetData(cardModelConfig);
            positionText.text = PlayerCard.GetAdaptPositionAbbreviation(cardModelConfig);
            SetStar(playerInfo.Star);
            stateImage.sprite = await SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[playerInfo.Status]);
            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(cardModelConfig.Portrait);
        }

        private void SetBg(int quality)
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
