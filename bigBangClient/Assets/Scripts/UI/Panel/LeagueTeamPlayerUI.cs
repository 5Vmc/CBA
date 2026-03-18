using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;
using Protocol;

namespace BigBang.UI
{
    public class LeagueTeamPlayerUIProperties : WindowProperties
    {
        public CourseTeamData data = null;
        public LeagueTeamPlayerUIProperties(CourseTeamData data)
        {
            this.data = data;
        }
    }
    public class LeagueTeamPlayerUI : AWindowController<LeagueTeamPlayerUIProperties>
    {
        #region 初始化与监听
        [SerializeField] private BabuButton closeBtn = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickCloseBtn;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickCloseBtn;
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<LeagueTeamPlayerUI>();
        }
        #endregion

        #region 数据刷新与显示刷新
        [SerializeField] private ClubIconItem teamIcon = null;
        [SerializeField] private TMP_Text teamNameText = null;
        [SerializeField] private ImageFont homeFightPointImageFont = null;
        [SerializeField] private TMP_Text tacticTextAtk = null;
        [SerializeField] private TMP_Text tacticTextDef = null;
        [SerializeField] List<LeagueTeamPlayerItem> firstItemList = new();
        [SerializeField] List<LeagueTeamPlayerItem> subItemList = new();
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            teamIcon.SetIcon(Properties.data.Team.TeamIcon);
            teamNameText.text = Properties.data.Team.TeamName;
            homeFightPointImageFont.text = Properties.data.Strength.ToString();
            string atkText = "", defText = "";
            DataConvUtil.TacticsIdList2AtkDef(Properties.data.TacticsIdList.ToList(), ref atkText, ref defText);
            tacticTextAtk.text = atkText;
            tacticTextDef.text = defText;
            SetItems(firstItemList, Properties.data.BoardCardMap.Values.ToList());
            SetItems(subItemList, Properties.data.SubstituteCardMap.Values.ToList());
        }
        public static void SetItems(List<LeagueTeamPlayerItem> playerItemList, List<PlayerCardMiniInfo> playerInfoList)
        {
            for (int i = 0; i < playerItemList.Count; i++)
            {
                LeagueTeamPlayerItem playerItem = playerItemList[i];
                if (i < playerInfoList.Count)
                {
                    PlayerCardMiniInfo playerInfo = playerInfoList[i];
                    playerItem.SetData(playerInfo);
                    playerItem.gameObject.SetActive(true);
                }
                else
                {
                    playerItem.gameObject.SetActive(false);
                }
            }
        }
        #endregion


    }
}