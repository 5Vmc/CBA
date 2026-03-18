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
    public class AllStarFormationHelpCardItem : MonoBehaviour
    {
        [SerializeField] private List<Image> qualityImageList = new();
        [SerializeField] private Image playerIconImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private PeakImage peakImage = null;

        public CardModelConfig cardModelConfig = null;
        public async void SetData(CardModelConfig cardModelConfig)
        {
            this.cardModelConfig = cardModelConfig;

            SetBg(cardModelConfig.Quality);
            nameText.text = cardModelConfig.Name;
            peakImage.SetData(cardModelConfig);
            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(cardModelConfig.Portrait);
        }

        private void SetBg(int quality)// 设置品质
        {
            for (int i = 0; i < qualityImageList.Count; i++)
            {
                qualityImageList[i].gameObject.SetActive(i == quality - 1);
            }
        }
    }
}
