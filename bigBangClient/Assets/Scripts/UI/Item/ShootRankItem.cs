using System.Collections.Generic;
using System.Linq;
using Babu;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class ShootRankItem : MonoBehaviour
    {
        [SerializeField] private Color white = new Color();
        [SerializeField] private Color green = new Color();

        [SerializeField] private Image rankImg = null;
        [SerializeField] private Image rankBg = null;
        [SerializeField] private TMP_Text rankText = null;
        [SerializeField] private ClubIconItem clubIcon = null;
        [SerializeField] private TMP_Text clubNameText = null;
        [SerializeField] private TMP_Text scoreText = null;

        public AllRankInfo allRankInfo = null;

        public async void SetData(AllRankInfo allRankInfo)
        {
            this.allRankInfo = allRankInfo;

            bool needUseRankImage = allRankInfo.Rank <= 3;
            rankImg.gameObject.SetActive(needUseRankImage);
            rankBg.gameObject.SetActive(!needUseRankImage);
            rankText.gameObject.SetActive(!needUseRankImage);
            if (needUseRankImage)
            {
                rankImg.sprite = await SpriteProxy.GetRank(allRankInfo.Rank);
            }
            else
            {
                rankText.text = allRankInfo.Rank.ToString();
                if (allRankInfo.IsSelf)
                {
                    rankText.color = green;
                }
                else
                {
                    rankText.color = white;
                }
            }
            if (allRankInfo.IsSelf)
            {
                clubNameText.color = green;
            }
            else
            {
                clubNameText.color = white;
            }

            clubIcon.SetIcon(allRankInfo.Icon);// 设置球队图片
            clubNameText.text = allRankInfo.Name;// 设置球队名称
            scoreText.text = allRankInfo.Record.ToString();
        }

        [SerializeField] private Image backgroundImg = null;
        public void SetBackgroundColor(Color c)
        {
            backgroundImg.color = c;
        }

        public void SetSelf()
        {
            rankText.color = green;
            clubNameText.color = green;
            scoreText.color = green;
        }

    }
}