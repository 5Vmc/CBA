using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BigBang;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeagueHistoryDetailKingItem : MonoBehaviour
{
    [SerializeField] private List<Image> headBgImagelist = new();
    [SerializeField] private Image playerHeadImage = null;
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private TMP_Text numText = null;
    [SerializeField] private RectTransform rankPanel = null;
    [SerializeField] private TMP_Text rankNumText = null;
    [SerializeField] private TMP_Text kingTypeText = null;
    [SerializeField] private PeakImage peakImage = null;

    public async Task SetDataAsync(TeamTopCardData teamTopCardData, int index)
    {
        for (int i = 0; i < headBgImagelist.Count; i++)
        {
            headBgImagelist[i].gameObject.SetActive(i + 1 == teamTopCardData.Quality);
        }
        CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(teamTopCardData.CardId);
        playerHeadImage.sprite = await SpriteProxy.GetPlayerPortrait(cardModelConfig.Portrait);
        nameText.text = PlayerCard.GetFullName(cardModelConfig);
        switch (index)
        {
            case 0: numText.text = teamTopCardData.Record + "分"; break;
            case 1: numText.text = teamTopCardData.Record + "次"; break;
            case 2: numText.text = teamTopCardData.Record + "次"; break;
            case 3: numText.text = teamTopCardData.Record + "次"; break;
            case 4: numText.text = teamTopCardData.Record + "次"; break;
        }
        bool isTop9 = teamTopCardData.LeagueRank <= 9;
        rankPanel.gameObject.SetActive(isTop9);
        if (isTop9) rankNumText.text = teamTopCardData.LeagueRank.ToString();
        peakImage.SetData(cardModelConfig);
    }
}
