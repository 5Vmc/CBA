using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
    public class LeagueRankItem : MonoBehaviour
    {
        [SerializeField] private Image bgLightImage = null;
        [SerializeField] private Image bgDarkImage = null;
        [SerializeField] private Image bgSelfImage = null;
        [SerializeField] private RectTransform rankImgPanel = null;
        [SerializeField] private List<Image> rankImgList = new();
        [SerializeField] private RectTransform rankBgImgPanel = null;
        [SerializeField] private List<Image> rankBgImgList = new();
        [SerializeField] private Image rankTextBgImage = null;
        [SerializeField] private TMP_Text rankText = null;
        [SerializeField] private ClubIconItem clubIcon = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text winText = null;
        [SerializeField] private TMP_Text dateText = null;

        [SerializeField] private Color whiteLight = new Color();
        [SerializeField] private Color greenLight = new Color();
        [SerializeField] private Color greyLight = new Color();
        [SerializeField] private Color whiteDark = new Color();
        [SerializeField] private Color greenDark = new Color();
        [SerializeField] private Color greyDark = new Color();
        [SerializeField] private string whiteLightStr = "";
        [SerializeField] private string greenLightStr = "";
        [SerializeField] private string greyLightStr = "";
        [SerializeField] private string whiteDarkStr = "";
        [SerializeField] private string greenDarkStr = "";
        [SerializeField] private string greyDarkStr = "";

        private readonly string nameStrPrefab = "<color=#{2}>[{0}区]</color><color=#{3}>{1}</color>";
        private ChampionTeamData championTeamData = null;
        public void SetData(ChampionTeamData championTeamData, bool isDown, int index)
        {
            this.championTeamData = championTeamData;
            bool isOnRank = championTeamData != null;
            bool isSelf = isOnRank && championTeamData.Team.TeamId == Player.GbId;
            bool isFirst3 = isOnRank && championTeamData.Rank <= 3;
            bool isLight = index % 2 == 0;
            bgLightImage.gameObject.SetActive(isLight && !isDown);
            bgDarkImage.gameObject.SetActive(!isLight && !isDown);
            bgSelfImage.gameObject.SetActive(isDown);
            rankBgImgPanel.gameObject.SetActive(isFirst3 && !isDown);
            if (isFirst3 && !isDown)
            {
                for (int i = 0; i < rankBgImgList.Count; i++)
                {
                    rankBgImgList[i].gameObject.SetActive(i + 1 == championTeamData.Rank);
                }
            }
            rankImgPanel.gameObject.SetActive(isFirst3);
            if (isFirst3)
            {
                for (int i = 0; i < rankImgList.Count; i++)
                {
                    rankImgList[i].gameObject.SetActive(i + 1 == championTeamData.Rank);
                }
            }
            rankTextBgImage.gameObject.SetActive(!isFirst3);
            if (!isFirst3)
            {
                if (isOnRank)
                {
                    rankText.text = championTeamData.Rank.ToString();
                }
                else
                {
                    rankText.text = "<size=20>未上榜</size>";
                }
                rankText.color = isDown ? whiteLight : whiteDark;
            }
            if (isOnRank)
            {
                clubIcon.SetIcon(championTeamData.Team.TeamIcon);
                string nameColorStr;
                if (isFirst3)
                {
                    nameColorStr = isSelf ? greenLightStr : whiteLightStr;
                }
                else
                {
                    nameColorStr = isSelf ? greenDarkStr : whiteDarkStr;
                }
                nameText.text = nameStrPrefab.SafeFormat(championTeamData.Team.ServerId, championTeamData.Team.TeamName, isDown ? greyLightStr : greyDarkStr, nameColorStr);
                winText.text = championTeamData.Champion.ToString();
                dateText.text = DataConvUtil.FormatDateTime(championTeamData.Time, "yy.MM.dd");
            }
            else
            {
                clubIcon.SetIcon(Player.Icon);
                nameText.text = nameStrPrefab.SafeFormat(Player.ServerData.Id, Player.Name, greyLightStr, whiteLightStr);
                winText.text = "";
                dateText.text = "";
            }
            winText.color = isDown || isFirst3 ? whiteLight : whiteDark;
            dateText.color = isDown || isFirst3 ? whiteLight : whiteDark;
        }
    }
}