using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System;

namespace BigBang.UI
{
    public class GamePreviewPad : MonoBehaviour
    {
        [SerializeField] private Top top;
        [SerializeField] private Middle middle;
        [SerializeField] private Bottom bottom;
        [SerializeField] private Button changeTimeBtn;
        [SerializeField] private Button modifyBtn;

        private GetGamePreviewDataResponse previewData;

        public GamePreviewPadAnim Anim;

        private void OnEnable()
        {
            changeTimeBtn.onClick.AddListener(OnChangeTime);
            modifyBtn.onClick.AddListener(OnModify);
            EventManager.Instance.Register(EventID.SetCourseTimeSucceed, OnChangeCourseTimeSucceed);
        }

        private void OnDisable()
        {
            changeTimeBtn.onClick.RemoveListener(OnChangeTime);
            modifyBtn.onClick.RemoveListener(OnModify);
            EventManager.Instance.Unregister(EventID.SetCourseTimeSucceed, OnChangeCourseTimeSucceed);
        }

        public void SetData(int compitionID, GetGamePreviewDataResponse response)
        {

            previewData = response;
            top.SetData(compitionID, response);
            middle.SetData(response);
            bottom.SetData(previewData);
            // 播放动画
            Anim.PlayEnter();
        }

        private void OnModify()
        {
            // 跳转到布阵界面
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVP, formation =>
            {
                UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, false, FormationUI.FormationShowType.Formation));
            });
        }

        private void OnChangeTime()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            if (previewData.HomeTeam.Team.TeamId != Player.GbId)
            {
                Tips.PopError(ErrorID.ChangeTimeError);
                return;
            }
            // 临近比赛时间小于180分钟时，不能再做调整 且比赛时间不能调整到当前时间180分钟内
            if (Mathf.Abs((float)(TimeUtils.ToDateTime(previewData.StandardTime) - TimeUtils.ToDateTime(Utils.DataConvUtil.ServerTime)).TotalMinutes) < 180)
            {
                Tips.PopError(ErrorID.ChangeTimeErrorTip);
                return;
            }
            //UIController.Instance.OpenWindow<ChangeTimeUI>(new ChangeTimeUIProperties(previewData));
        }

        private void OnChangeCourseTimeSucceed(object[] args)
        {
            var courseTime = (long)args[0];
            top.SetTime(courseTime);
            Tips.PopTips(Lang.Get(LangID.TheNewTimeHasBeenSet));
        }

        [System.Serializable]
        private class Top
        {
            [SerializeField] private TMP_Text clubName1;
            [SerializeField] private TMP_Text clubName2;
            [SerializeField] private TMP_Text clubRank1;
            [SerializeField] private TMP_Text clubRank2;
            [SerializeField] private ClubIconItem clubIcon1;
            [SerializeField] private ClubIconItem clubIcon2;
            [SerializeField] private TMP_Text timeText;
            [SerializeField] private TMP_Text titleTxt;

            public void SetData(int compitionID, GetGamePreviewDataResponse data)
            {
                if (data.HomeTeam != null)
                {
                    // 设置主队数据
                    clubName1.text = data.HomeTeam.Team.TeamName;
                    clubRank1.text = data.HomeTeam.Rank.ToString();
                    clubIcon1.SetIcon(data.HomeTeam.Team.TeamIcon);
                }
                else
                {
                    clubName1.text = Lang.Get(LangID.UnknownClub);
                    clubRank1.text = string.Empty;
                    clubIcon1.SetUnknown();
                }

                if (data.AwayTeam != null)
                {
                    // 设置客队数据
                    clubName2.text = data.AwayTeam.Team.TeamName;
                    clubRank2.text = data.AwayTeam.Rank.ToString();
                    clubIcon2.SetIcon(data.AwayTeam.Team.TeamIcon);
                }
                else
                {
                    clubName2.text = Lang.Get(LangID.UnknownClub);
                    clubRank2.text = string.Empty;
                    clubIcon2.SetUnknown();
                }

                // 设置比赛日期
                timeText.text = TimeUtils.GetUnixTimeString(data.Time, "MM/dd HH:mm");

                if (compitionID == CompitionID.League)
                {
                    titleTxt.text = Lang.Get(LangID.LeagueDayTxt);
                }
                if (compitionID == CompitionID.Cup)
                {
                    titleTxt.text = Lang.Get(LangID.CupDayTxt);
                }
            }

            public void SetTime(long time)
            {
                timeText.text = TimeUtils.GetUnixTimeString(time, "MM/dd HH:mm");
            }
        }

        [System.Serializable]
        private class Middle
        {
            [SerializeField] private List<MyGamePadMiddleItem> middleItem;
            [SerializeField] private List<Image> leftItem;
            [SerializeField] private List<Image> rightItem;

            // 设置联赛数据
            public async void SetData(GetGamePreviewDataResponse data)
            {
                //Debug.Log("xxx  middleItem[0]=" +  middleItem[0]);
                Debug.Log("xxx  data.HomeTeam=" + data.HomeTeam);
                Debug.Log("xxx  data.AwayTeam=" + data.AwayTeam);
                middleItem[0].SetData(data.HomeTeam.Strength, data.AwayTeam.Strength);
                middleItem[1].SetData(data.HomeTeam.Attack, data.AwayTeam.Attack);
                middleItem[2].SetData(data.HomeTeam.Defence, data.AwayTeam.Defence);
                // 设置最近战绩
                var homeRecord = data.HomeTeam.Record.Reverse().Take(5).ToArray();
                var awayRecord = data.AwayTeam.Record.Reverse().Take(5).ToArray();
                for (int i = 0; i < 5; i++)
                {
                    if (i < data.HomeTeam.Record.Count)
                    {
                        leftItem[i].sprite = await SpriteProxy.GetGameResult(homeRecord[i]);
                    }
                    else
                    {
                        leftItem[i].sprite = await SpriteProxy.GetGameResult(GameResultType.None);
                    }
                    if (i < data.AwayTeam.Record.Count)
                    {
                        rightItem[i].sprite = await SpriteProxy.GetGameResult(awayRecord[i]);
                    }
                    else
                    {
                        rightItem[i].sprite = await SpriteProxy.GetGameResult(GameResultType.None);
                    }
                }
            }
        }

        [System.Serializable]
        private class Bottom
        {
            [SerializeField] private List<RectTransform> redPoints;
            [SerializeField] private List<RectTransform> bluePoints;
            // [SerializeField] private TMP_Text formationText1;
            // [SerializeField] private TMP_Text formationText2;

            [SerializeField] private TMP_Text homeDefText;
            [SerializeField] private TMP_Text homeAtkText;

            [SerializeField] private TMP_Text awayDefText;
            [SerializeField] private TMP_Text awayAtkText;

            [SerializeField] private TMP_Text formationTitle1;
            [SerializeField] private TMP_Text formationTitle2;

            [SerializeField] private MyGamePreviewStarterItem[] homeStarters; //主队首发
            [SerializeField] private MyGamePreviewStarterItem[] awayStarters; //客队首发

            // 设置联赛数据
            public void SetData(GetGamePreviewDataResponse data)
            {
                if (Player.GbId == data.HomeTeam.Team.TeamId)
                {
                    formationTitle1.text = Lang.Get(LangID.HomeFormationText);
                    formationTitle2.text = Lang.Get(LangID.AwayFormationText);
                }
                else
                {
                    formationTitle1.text = Lang.Get(LangID.AwayFormationText);
                    formationTitle2.text = Lang.Get(LangID.HomeFormationText);
                }
                // 主队
                string atkText = "", defText = "";
                DataConvUtil.TacticsIdList2AtkDef(data.HomeTeam.TacticsIdList.ToList(), ref atkText, ref defText);
                this.homeAtkText.text = atkText;
                this.homeDefText.text = defText;
                foreach (var item in data.HomeTeam.BoardCardMap)
                {
                    int pos = Configs.FormationBoard.GetDataDictionary()[item.Key].SeparatedPosition;
                    MyGamePreviewStarterItem starter = this.findStarterItem(pos, true);
                    if (starter != null)
                    {
                        starter.SetData(item.Value);
                    }

                    //item.Key
                    // var cfg = Configs.FormationBoard.GetConfig(item.Key);
                    // bluePoints[index].anchoredPosition = new UnityEngine.Vector2(-cfg.PadPosX * 7f + 70, cfg.PadPosY * 7f);
                    // index++;
                }
                // 客队
                DataConvUtil.TacticsIdList2AtkDef(data.AwayTeam.TacticsIdList.ToList(), ref atkText, ref defText);
                this.awayAtkText.text = atkText;
                this.awayDefText.text = defText;

                foreach (var item in data.AwayTeam.BoardCardMap)
                {
                    int pos = Configs.FormationBoard.GetDataDictionary()[item.Key].SeparatedPosition;
                    MyGamePreviewStarterItem starter = this.findStarterItem(pos, false);
                    if (starter != null)
                    {
                        starter.SetData(item.Value);
                    }
                }

            }

            private MyGamePreviewStarterItem findStarterItem(int pos, bool isHome)
            {
                if (isHome)
                {
                    foreach (MyGamePreviewStarterItem item in this.homeStarters)
                    {
                        if ((int)item.PosType == pos) return item;
                    }
                }
                else
                {
                    foreach (MyGamePreviewStarterItem item in this.awayStarters)
                    {
                        if ((int)item.PosType == pos) return item;
                    }
                }
                return null;
            }
        }
    }
}
