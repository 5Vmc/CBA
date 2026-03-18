using System;
using System.Collections.Generic;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class DragonBoatRankItem : MonoBehaviour
    {
        [SerializeField] private Color mineNameColor = new Color();
        [SerializeField] private Color othersNameColor = new Color();

        [SerializeField] private Image bgImageLeft = null;
        [SerializeField] private Image bgImageRight = null;
        [SerializeField] private TMP_Text rankText = null;
        [SerializeField] private Image rankFlagImg1 = null;
        [SerializeField] private Image rankFlagImg2 = null;
        [SerializeField] private Image rankFlagImg3 = null;
        [SerializeField] private Image rankImg1 = null;
        [SerializeField] private Image rankImg2 = null;
        [SerializeField] private Image rankImg3 = null;
        [SerializeField] private Image leftIconImage = null;
        [SerializeField] private Image rightIconImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text meterText = null;

        [HideInInspector] public AllStarRankInfo allStarRankInfo = null;
        [HideInInspector] public int index = 0;

        public void SetData(AllStarRankInfo allStarRankInfo, int index)
        {
            this.allStarRankInfo = allStarRankInfo;
            this.index = index;

            HideAll();
            if (index == 0)
            {
                rankImg1.gameObject.SetActive(true);
                rankFlagImg1.gameObject.SetActive(true);
            }
            else if (index == 1)
            {
                rankImg2.gameObject.SetActive(true);
                rankFlagImg2.gameObject.SetActive(true);
            }
            else if (index == 2)
            {
                rankImg3.gameObject.SetActive(true);
                rankFlagImg3.gameObject.SetActive(true);
            }
            else
            {
                rankText.gameObject.SetActive(true);
                rankText.text = allStarRankInfo.Rank.ToString();
            }
            bool isLeft = allStarRankInfo.Side == 1;
            bgImageLeft.gameObject.SetActive(isLeft);
            bgImageRight.gameObject.SetActive(!isLeft);
            leftIconImage.gameObject.SetActive(isLeft);
            rightIconImage.gameObject.SetActive(!isLeft);
            nameText.text = "【{0}区】{1}".SafeFormat(allStarRankInfo.ServerId, allStarRankInfo.Name);
            meterText.text = allStarRankInfo.Record.ToString("###,###");
            if (Player.GbId == allStarRankInfo.Gbid)
            {
                SetColor(mineNameColor);
            }
            else
            {
                SetColor(othersNameColor);
            }
        }
        private void SetColor(Color color)
        {
            rankText.color = color;
            nameText.color = color;
            meterText.color = color;
        }
        private void HideAll()
        {
            rankFlagImg1.gameObject.SetActive(false);
            rankFlagImg2.gameObject.SetActive(false);
            rankFlagImg3.gameObject.SetActive(false);
            rankImg1.gameObject.SetActive(false);
            rankImg2.gameObject.SetActive(false);
            rankImg3.gameObject.SetActive(false);
            rankText.gameObject.SetActive(false);
        }
    }
}