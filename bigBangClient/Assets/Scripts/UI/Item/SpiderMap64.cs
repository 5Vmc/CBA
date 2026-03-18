using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SpiderMap64Data
    {
        public List<LeagueCourseItemData> Data32 { get; private set; }
        public List<LeagueCourseItemData> Data16 { get; private set; }
        public List<LeagueCourseItemData> Data8 { get; private set; }
        public List<LeagueCourseItemData> Data4 { get; private set; }
        public List<LeagueCourseItemData> Data2 { get; private set; }
        public List<LeagueCourseItemData> Data1 { get; private set; }

        public SpiderMap64Data(
            List<LeagueCourseItemData> data32, List<LeagueCourseItemData> data16,
            List<LeagueCourseItemData> data8, List<LeagueCourseItemData> data4,
            List<LeagueCourseItemData> data2, List<LeagueCourseItemData> data1)
        {
            Data32 = data32;
            Data16 = data16;
            Data8 = data8;
            Data4 = data4;
            Data2 = data2;
            Data1 = data1;
        }
    }

    public class SpiderMap64 : MonoBehaviour
    {
        public Action<CupScoreboardPadItem> OnClickItem = null;
        public Func<CupScoreboardPadItem, bool> NeedShowDetail = null;
        public Action<CupScoreboardPadItem> OnClickDetail = null;

        [SerializeField] private RectTransform content64;
        [SerializeField] private RectTransform content32;
        [SerializeField] private RectTransform content16;
        [SerializeField] private RectTransform content8;
        [SerializeField] private RectTransform content4;
        [SerializeField] private RectTransform content2;
        [SerializeField] private RectTransform content1;
        [SerializeField] private ScrollRect scrollView;

        private VerticalAdapter adapter64;
        private VerticalAdapter adapter32;
        private VerticalAdapter adapter16;
        private VerticalAdapter adapter8;
        private VerticalAdapter adapter4;
        private VerticalAdapter adapter2;

        private void Awake()
        {
            adapter64 = content64.GetComponent<VerticalAdapter>();
            adapter32 = content32.GetComponent<VerticalAdapter>();
            adapter16 = content16.GetComponent<VerticalAdapter>();
            adapter8 = content8.GetComponent<VerticalAdapter>();
            adapter4 = content4.GetComponent<VerticalAdapter>();
            adapter2 = content2.GetComponent<VerticalAdapter>();
        }

        private void OnEnable()
        {
            scrollView.onValueChanged.AddListener(OnViewMove);
        }

        private void OnDisable()
        {
            scrollView.onValueChanged.RemoveListener(OnViewMove);
        }

        // 缩放
        private void OnViewMove(UnityEngine.Vector2 vector2)
        {
            var ratio = Mathf.Clamp(1 - vector2.x, 0.1f, 1);
            float height = 48;
            adapter2.Gap = 1980 * ratio;
            adapter4.Gap = (adapter2.Gap - height) / 2f;
            adapter8.Gap = (adapter4.Gap - height) / 2f;
            adapter16.Gap = (adapter8.Gap - height) / 2f;
            adapter32.Gap = (adapter16.Gap - height) / 2f;
            adapter64.Gap = (adapter32.Gap - height) / 2f;
        }

        private void SetList(List<LeagueCourseItemData> data, List<CupScoreboardPadItem> list, int lineId, GetHundredCourseResponse getHundredCourseResponse = null)
        {
            int index = 0;
            if (data == null || data.Count <= 0)
            {
                Debug.LogWarning("List<LeagueCourseItemData>data==null,lineId = " + lineId);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SetData(null, false, OnClickItem, NeedShowDetail, OnClickDetail, lineId, index, "", false);
                }
                return;
            }
            for (int i = 0; i < data.Count; i++)
            {
                var (homeData, awayData) = GetDataProvider(data[i], getHundredCourseResponse);
                var homeItem = list[index];
                var awayItem = list[index + 1];
                homeItem.SetData(homeData, lineId > 1 && isShowDetailButton && homeData != null, OnClickItem, NeedShowDetail, OnClickDetail, lineId, index, homeData?.FightID, false);
                awayItem.SetData(awayData, lineId > 1 && isShowDetailButton && awayData != null, OnClickItem, NeedShowDetail, OnClickDetail, lineId, index + 1, awayData?.FightID, true);
                index += 2;
                if (homeData == null) homeItem.SetAsNone();
                if (awayData == null) awayItem.SetAsNone();
                if (homeData != null && homeData.Score == -1) homeItem.SetAsNotFight();
                if (awayData != null && awayData.Score == -1) awayItem.SetAsNotFight();
                if (homeData != null && awayData != null && homeData.Score >= 0 && awayData.Score >= 0)
                {
                    if (homeData.Score >= awayData.Score)
                    {
                        awayItem.SetAsFailed(homeData.ClubID == Player.GbId);
                        homeItem.SetAsWin();
                    }
                    else
                    {
                        homeItem.SetAsFailed(awayData.ClubID == Player.GbId);
                        awayItem.SetAsWin();
                    }
                }
            }
        }

        private bool isShowDetailButton = false;
        public void SetShowDetailButton(bool isShowDetailButton)
        {
            this.isShowDetailButton = isShowDetailButton;
        }

        public void SetData(SpiderMap64Data provider, GetHundredCourseResponse getHundredCourseResponse = null)
        {
            if (provider == null)
            {
                scrollView.gameObject.SetActive(false);
                return;
            }
            scrollView.gameObject.SetActive(true);

            var list64 = content64.GetChildren<CupScoreboardPadItem>().ToList();
            var list32 = content32.GetChildren<CupScoreboardPadItem>().ToList();
            var list16 = content16.GetChildren<CupScoreboardPadItem>().ToList();
            var list8 = content8.GetChildren<CupScoreboardPadItem>().ToList();
            var list4 = content4.GetChildren<CupScoreboardPadItem>().ToList();
            var list2 = content2.GetChildren<CupScoreboardPadItem>().ToList();
            var list1 = content1.GetChildren<CupScoreboardPadItem>().ToList();

            // 64强32场比赛
            SetList(provider.Data32, list64, 1, getHundredCourseResponse);
            // 32强16场比赛
            SetList(provider.Data16, list32, 2, getHundredCourseResponse);
            // 16强8场比赛
            SetList(provider.Data8, list16, 3, getHundredCourseResponse);
            // 8强4场比赛
            SetList(provider.Data4, list8, 4, getHundredCourseResponse);
            // 4强2场比赛
            SetList(provider.Data2, list4, 5, getHundredCourseResponse);
            // 2强1场比赛
            SetList(provider.Data1, list2, 6, getHundredCourseResponse);
            // 设置冠军
            CupScoreboardPadItemData winner = null;
            string winnerFightId = "";
            bool winnerIsAway = false;
            if (provider != null && provider.Data1.Count > 0 && provider.Data1[0] != null)
                if (provider.Data1[0].HomeGoal > provider.Data1[0].AwayGoal)
                {
                    winner = new CupScoreboardPadItemData(provider.Data1[0].FightId, provider.Data1[0].HomeTeam.TeamId,
                        provider.Data1[0].HomeTeam.TeamName, provider.Data1[0].HomeGoal,
                        provider.Data1[0].HomeTeam.TeamIcon);
                    winner.getHundredCourseResponse = getHundredCourseResponse;
                    winner.CourseId = provider.Data1[0].CourseId;
                    winner.TeamId = provider.Data1[0].HomeTeam.TeamId;
                    winnerFightId = winner.FightID;
                    winnerIsAway = false;
                }
                else if (provider.Data1[0].HomeGoal < provider.Data1[0].AwayGoal)
                {
                    winner = new CupScoreboardPadItemData(provider.Data1[0].FightId, provider.Data1[0].AwayTeam.TeamId,
                        provider.Data1[0].AwayTeam.TeamName, provider.Data1[0].AwayGoal,
                        provider.Data1[0].AwayTeam.TeamIcon);
                    winner.getHundredCourseResponse = getHundredCourseResponse;
                    winner.CourseId = provider.Data1[0].CourseId;
                    winner.TeamId = provider.Data1[0].AwayTeam.TeamId;
                    winnerFightId = winner.FightID;
                    winnerIsAway = true;
                }
            list1[0].SetData(winner, isShowDetailButton && winner != null, OnClickItem, NeedShowDetail, OnClickDetail, 7, 0, winnerFightId, winnerIsAway);
        }

        private (CupScoreboardPadItemData, CupScoreboardPadItemData) GetDataProvider(LeagueCourseItemData data, GetHundredCourseResponse getHundredCourseResponse = null)
        {
            if (data == null) return (null, null);
            CupScoreboardPadItemData homeData = null;
            CupScoreboardPadItemData awayData = null;
            if (data.HomeTeam != null)
            {
                homeData = new CupScoreboardPadItemData(data.FightId, data.HomeTeam.TeamId, data.HomeTeam.TeamName, data.HomeGoal,
                   data.HomeTeam.TeamIcon);
                homeData.getHundredCourseResponse = getHundredCourseResponse;
                homeData.CourseId = data.CourseId;
                homeData.TeamId = data.HomeTeam.TeamId;
            }
            if (data.AwayTeam != null)
            {
                awayData = new CupScoreboardPadItemData(data.FightId, data.AwayTeam.TeamId, data.AwayTeam.TeamName, data.AwayGoal,
                    data.AwayTeam.TeamIcon);
                awayData.getHundredCourseResponse = getHundredCourseResponse;
                awayData.CourseId = data.CourseId;
                awayData.TeamId = data.AwayTeam.TeamId;
            }
            return (homeData, awayData);
        }

        #region 编辑器

        // 连接节点
        [EditorButton("连接节点", false)]
        private void LinkTheNode()
        {
            for (int i = 0; i < 64; i++)
            {
                if (i < 64)
                {
                    var target = content32.GetChild(i / 2).GetComponent<LinkItem>().InPosition;
                    content64.GetChild(i).GetComponent<LinkItem>().SetTarget(target);
                }
                if (i < 32)
                {
                    var target = content16.GetChild(i / 2).GetComponent<LinkItem>().InPosition;
                    content32.GetChild(i).GetComponent<LinkItem>().SetTarget(target);
                }
                if (i < 16)
                {
                    var target = content8.GetChild(i / 2).GetComponent<LinkItem>().InPosition;
                    content16.GetChild(i).GetComponent<LinkItem>().SetTarget(target);
                }
                if (i < 8)
                {
                    var target = content4.GetChild(i / 2).GetComponent<LinkItem>().InPosition;
                    content8.GetChild(i).GetComponent<LinkItem>().SetTarget(target);
                }
                if (i < 4)
                {
                    var target = content2.GetChild(i / 2).GetComponent<LinkItem>().InPosition;
                    content4.GetChild(i).GetComponent<LinkItem>().SetTarget(target);
                }
                if (i < 2)
                {
                    var target = content1.GetChild(i / 2).GetComponent<LinkItem>().InPosition;
                    content2.GetChild(i).GetComponent<LinkItem>().SetTarget(target);
                }
            }
        }

        [EditorButton("清空节点", false)]
        private void ClearNode()
        {
            foreach (var item in content64.GetChildren()) DestroyImmediate(item.gameObject);
            foreach (var item in content32.GetChildren()) DestroyImmediate(item.gameObject);
            foreach (var item in content16.GetChildren()) DestroyImmediate(item.gameObject);
            foreach (var item in content8.GetChildren()) DestroyImmediate(item.gameObject);
            foreach (var item in content4.GetChildren()) DestroyImmediate(item.gameObject);
            foreach (var item in content2.GetChildren()) DestroyImmediate(item.gameObject);
            foreach (var item in content1.GetChildren()) DestroyImmediate(item.gameObject);
        }

        #endregion
    }
}