using System;
using System.Collections.Generic;
using System.Linq;
using BigBang.Animation;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.BattleManager;

namespace BigBang.UI
{
    public class MyCoursePad : MonoBehaviour
    {
        [SerializeField] private MyCoursePadAdapter adapter;
        [SerializeField] private Top top;

        public MyCoursePadAnim Anim;

        public void AddItemClickListener(Action<LeagueCourseItemData, BattleEnterType> callback)
        {
            adapter.OnItemClick += callback;
        }

        public void RemoveItemClickListener(Action<LeagueCourseItemData, BattleEnterType> callback)
        {
            adapter.OnItemClick -= callback;
        }

        // 设置联赛数据
        public void SetData(GetLeagueCourseResponse response, CourseTeamData myData, int compitionID, string leagueName, BattleEnterType battleEnterType)
        {
            // 获得联赛战绩数据
            top.SetData(compitionID, myData);
            // 设置我的赛程数据
            adapter.SetData(response, leagueName, battleEnterType);
            Anim.PlayEnter();
        }

        [System.Serializable]
        private class Top
        {
            [SerializeField] private ClubIconItem clubIcon;
            [SerializeField] private TMP_Text rankText;
            [SerializeField] private TMP_Text recordText;
            [SerializeField] private TMP_Text rankTitle1;
            [SerializeField] private TMP_Text rankTitle2;
            [SerializeField] private List<Image> recordIcon;

            public async void SetData(int compitionID, CourseTeamData data)
            {
                if (compitionID == CompitionID.League)
                {
                    rankText.text = Lang.Get(LangID.LeagueNameText) + Lang.Get(LangID.RankText) + data.Rank.ToString();
                    rankTitle1.text =
                    rankTitle2.text = Lang.Get(LangID.LeagueRecordTxt);
                }
                else
                {
                    rankText.text = Lang.Get(LangID.CupNameText) + Lang.Get(LangID.RankText) + data.Rank.ToString();
                    rankTitle1.text =
                    rankTitle2.text = Lang.Get(LangID.CupRecordTxt);
                }
                clubIcon.SetIcon(data.Team.TeamIcon);
                recordText.text = Lang.Get(LangID.StatisticsText)
                    .Replace("{win}", data.Win.ToString())
                    .Replace("{deuce}", data.Deuce.ToString())
                    .Replace("{faild}", data.Failed.ToString());
                // 设置最近5场比分
                var record = data.Record.Reverse().Take(5).ToArray();
                for (int i = 0; i < 5; i++)
                {
                    if (i < data.Record.Count)
                    {
                        recordIcon[i].sprite = await SpriteProxy.GetGameResult(record[i]);
                    }
                    else
                    {
                        recordIcon[i].sprite = await SpriteProxy.GetGameResult(GameResultType.None);
                    }
                }
            }
        }
    }
}
