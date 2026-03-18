using System;
using Babu;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.BattleManager;

namespace BigBang.UI
{
    public class MyCoursePadItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text roundText;
        // [SerializeField] private TMP_Text homeName;
        [SerializeField] private TMP_Text awayName;
        [SerializeField] private TMP_Text scoreText;
        // [SerializeField] private ClubIconItem homeIcon;
        [SerializeField] private ClubIconItem awayIcon;
        [SerializeField] private BabuButton clickBtn;
        [SerializeField] private Image backgroundImg;

        [SerializeField] private GameObject homeTag;
        [SerializeField] private GameObject awayTag;

        [SerializeField] private GameObject winTag;
        [SerializeField] private GameObject loseTag;

        [SerializeField] private GameObject scorePanel;

        public event Action Click;

        private void OnEnable()
        {
            clickBtn.OnClick += OnClick;
        }

        private void OnDisable()
        {
            clickBtn.OnClick -= OnClick;
        }

        private void OnClick(BabuButton sender)
        {
            Click?.Invoke();
        }

        public BattleEnterType battleEnterType;
        public void SetData(LeagueCourseItemData data, string leagueName, BattleEnterType battleEnterType)
        {
            this.battleEnterType = battleEnterType;
            // 设置时间
            var date = TimeUtils.ToDateTime(data.Time);
            timeText.text = TimeUtils.GetUnixTimeString(data.Time);
            // 设置轮次
            roundText.text = leagueName + Lang.Get(LangID.RoundText).Replace("{value}", data.Round.ToString());
            if (data.HomeTeam.TeamId == Player.GbId)
            {
                awayIcon.SetIcon(data.AwayTeam.TeamIcon);
                awayName.text = data.AwayTeam.TeamName;
                homeTag.SetActive(true);
                awayTag.SetActive(false);
            }
            else
            {
                awayIcon.SetIcon(data.HomeTeam.TeamIcon);
                awayName.text = data.HomeTeam.TeamName;

                homeTag.SetActive(false);
                awayTag.SetActive(true);
            }
            // 设置主队名称
            // homeName.text = data.HomeTeam.TeamName;


            // 设置比分
            if (data.HomeGoal == -1)
            {
                scorePanel.SetActive(false);
                winTag.SetActive(false);
                loseTag.SetActive(false);
                clickBtn.gameObject.SetActive(false);
            }
            else
            {
                clickBtn.gameObject.SetActive(true);
                scorePanel.SetActive(true);

                if (data.HomeGoal > data.AwayGoal)
                    scoreText.text = data.HomeGoal + " : " + data.AwayGoal;
                else
                {
                    scoreText.text = data.AwayGoal + " : " + data.HomeGoal;
                }

                if (this.isWin(data))
                {
                    winTag.SetActive(true);
                    loseTag.SetActive(false);
                }
                else
                {
                    winTag.SetActive(false);
                    loseTag.SetActive(true);
                }

            }
            scoreText.fontSize = 40;
            // if (data.HomeGoal == -1 || data.AwayGoal == -1)
            // {
            //     scoreText.text = Lang.Get(LangID.VSTxt);
            //     scoreText.fontSize = 25;
            // }
            // else
            // {
            //     scoreText.text = data.HomeGoal + "-" + data.AwayGoal;
            //     scoreText.fontSize = 40;
            // }
            // 设置主队图标
            // homeIcon.SetIcon(data.HomeTeam.TeamIcon);

        }

        public void SetBackgroundColor(Color c)
        {
            backgroundImg.color = c;
        }

        private bool isWin(LeagueCourseItemData data)
        {
            if (data.HomeTeam.TeamId == Player.GbId)
            {
                return data.HomeGoal > data.AwayGoal;
            }
            else
            {
                return data.AwayGoal > data.HomeGoal;
            }
        }
    }
}