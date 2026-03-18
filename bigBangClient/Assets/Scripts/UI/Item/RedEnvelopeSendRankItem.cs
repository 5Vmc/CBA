using Babu;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class RedEnvelopeSendRankItem : MonoBehaviour
    {
        public RedPacketRankInfo redPacketRankInfo = null;
        public void SetData(RedPacketRankInfo redPacketRankInfo)
        {
            this.redPacketRankInfo = redPacketRankInfo;
            RefreshUI();
        }

        [SerializeField] private Image first3BgImage = null;
        [SerializeField] private Image normalBgImage = null;
        [SerializeField] private TMP_Text rankText = null;
        [SerializeField] private TMP_Text sendCountText = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text upCountText = null;
        [SerializeField] private Image rank1Image = null;
        [SerializeField] private Image rank2Image = null;
        [SerializeField] private Image rank3Image = null;

        [SerializeField] private Image redEnvelopeImage = null;
        [SerializeField] private Image upImage = null;

        [SerializeField] private Color mineNameColor = new();
        [SerializeField] private Color first3NameColor = new();
        [SerializeField] private Color otherNameColor = new();
        [SerializeField] private Color first3UpColor = new();
        [SerializeField] private Color otherUpColor = new();

        private void RefreshUI()
        {
            bool isFirst3 = redPacketRankInfo.Rank <= 3;
            first3BgImage.gameObject.SetActive(isFirst3);
            normalBgImage.gameObject.SetActive(!isFirst3);
            rankText.gameObject.SetActive(!isFirst3);
            if (!isFirst3) rankText.text = redPacketRankInfo.Rank.ToString();
            rank1Image.gameObject.SetActive(redPacketRankInfo.Rank == 1);
            rank2Image.gameObject.SetActive(redPacketRankInfo.Rank == 2);
            rank3Image.gameObject.SetActive(redPacketRankInfo.Rank == 3);
            sendCountText.text = redPacketRankInfo.SendPacket.ToString("N0");
            upCountText.text = redPacketRankInfo.LikePacket.ToString("N0");
            nameText.text = "[{0}区]{1}".SafeFormat(redPacketRankInfo.ServerId, redPacketRankInfo.Name);
            Color nameColor = mineNameColor;
            if(redPacketRankInfo.Gbid != Player.GbId)
            {
                nameColor = isFirst3 ? first3NameColor : otherNameColor;
            }
            nameText.color = nameColor;
            sendCountText.color = nameColor;
            upCountText.color = nameColor;
            redEnvelopeImage.color = isFirst3 ? first3UpColor : otherUpColor;
            upImage.color = isFirst3 ? first3UpColor : otherUpColor;
        }
    }
}
