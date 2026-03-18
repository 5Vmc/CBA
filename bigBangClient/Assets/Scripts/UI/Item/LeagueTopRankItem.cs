using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class LeagueTopRankItem : MonoBehaviour
    {
        [SerializeField] private ClubIconItem clubIcon = null;
        [SerializeField] private TMP_Text clubNameText = null;
        [SerializeField] private TMP_Text emptyText = null;

        [SerializeField] private Color whiteLight = new Color();
        [SerializeField] private Color greenLight = new Color();

        public ChampionTeamData championTeamData = null;
        public void SetData(ChampionTeamData championTeamData)
        {
            this.championTeamData = championTeamData;
            bool isEmpty = championTeamData == null;

            clubIcon.gameObject.SetActive(!isEmpty);
            clubNameText.gameObject.SetActive(!isEmpty);
            emptyText.gameObject.SetActive(isEmpty);

            if (!isEmpty)
            {
                clubIcon.SetIcon(championTeamData.Team.TeamIcon);
                clubNameText.text = championTeamData.Team.TeamName;
                clubNameText.color = championTeamData.Team.TeamId == Player.GbId ? greenLight : whiteLight;
            }
        }
    }
}