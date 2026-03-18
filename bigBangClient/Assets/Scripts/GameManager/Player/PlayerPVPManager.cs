using Babu;
using BigBang.UI;
using Protocol;
using System;
using System.Collections.Generic;
using Utils;

namespace BigBang
{
    public class PlayerPVPManager
    {
        #region old
        public Dictionary<int, List<Utils.GameItem.GameItem>> tmpRewards = new() { { CompitionID.League, new List<Utils.GameItem.GameItem>() }, { CompitionID.Cup, new List<Utils.GameItem.GameItem>() } };
        public int LeagueTrophyCount { get; set; } = 0;
        public int CupTrophyCount { get; set; } = 0;
        public int NextCompitionType { get; set; }
        /// <summary>
        /// 比赛的简要信息
        /// </summary>
        public GetMainUIMatchResponse resp;

        /// <summary>
        /// 是否有比赛
        /// </summary>
        public bool HasCompition;

        public string StatusStr = "";
        private bool isred;
        public void UnPack(LeagueTrophyCountNotify data)
        {
            LeagueTrophyCount = data.LeagueTrophyCount;
        }
        public void UnPack(CupTrophyCountNotify data)
        {
            CupTrophyCount = data.CupTrophyCount;
        }

        public UpdatePVPInfoNotify updatePVPInfoNotify = null;
        public void UnPack(UpdatePVPInfoNotify data)
        {
            this.updatePVPInfoNotify = data;

            Player.PVPManager.tmpRewards[CompitionID.League] = new System.Collections.Generic.List<Utils.GameItem.GameItem>();
            Player.PVPManager.tmpRewards[CompitionID.Cup] = new System.Collections.Generic.List<Utils.GameItem.GameItem>();

            foreach (var rewards in data.LeagueRewards)
            {
                tmpRewards[CompitionID.League].Add(Utils.GameItem.GameItemUtils.CreateGameItem((GameItemType)rewards.Type, rewards.Id, rewards.Count));
            }

            foreach (var rewards in data.CupRewards)
            {
                tmpRewards[CompitionID.Cup].Add(Utils.GameItem.GameItemUtils.CreateGameItem((GameItemType)rewards.Type, rewards.Id, rewards.Count));
            }

            if (tmpRewards[CompitionID.Cup].Count > 0)
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "/" + CompitionID.Cup.ToString() + "/reward");
                node.AddValue(1);
            }

            RefreshLeagueRedDot();
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_ClassicPVP, 3);
        }

        public void RefreshLeagueRedDot()
        {
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "/" + CompitionID.League.ToString() + "/basicReward");
                node.AddValue(tmpRewards[CompitionID.League].Count > 0 ? 1 : -1);
            }
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "/" + CompitionID.League.ToString() + "/endReward");
                node.AddValue(updatePVPInfoNotify.LeagueSettle ? 1 : -1);
            }
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "/" + CompitionID.League.ToString() + "/sign");
                bool isCanSign = (TeamState)updatePVPInfoNotify.LeagueTeamState == TeamState.INIT || (TeamState)updatePVPInfoNotify.LeagueTeamState == TeamState.SETTLE;
                node.AddValue(isCanSign ? 1 : -1);
            }
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public void GetRecentlyMatch(Action callback = null)
        {
            NetworkManager.Instance.GetMainUIMatch(response =>
            {
                resp = response;
                HasCompition = response != null && response.LeagueId != 0 && response.HomeTeam != null && response.AwayTeam != null;
                StatusStr = "";
                if (HasCompition)
                {
                    if (response.CompitionId == CompitionID.League) // 联赛
                    {
                        Player.PVPManager.NextCompitionType = (int)FightType.League;
                        if (response.TeamState == (int)TeamState.MATCHING && response.HomeTeam == null && response.AwayTeam == null)
                        {
                            // 联赛结算中
                            StatusStr = "联赛结算中";
                            return;
                        }
                        else
                        {
                        }
                    }
                    else if (response.CompitionId == CompitionID.Cup) // 杯赛
                    {
                        if (response.TeamState == (int)TeamState.MATCHING && response.HomeTeam == null && response.AwayTeam == null)
                        {
                            // 杯赛结算中
                            StatusStr = "杯赛结算中";
                            return;
                        }
                        if (response.HomeTeam == null || response.AwayTeam == null)
                        {
                            StatusStr = "下一轮对阵还未开始";
                            // 杯赛对手未产生
                            return;
                        }
                    }
                }
                else
                {
                    StatusStr = "等待新赛季开启";
                }
                callback?.Invoke();
            });
        }

        //public void CheckRedData(Action callback = null)
        //{
        //    callback?.Invoke();


        //    //if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicPVP, false)) return;
        //    //GetRecentlyMatch(() =>
        //    //{
        //    //    callback?.Invoke();
        //    //});
        //}
        #endregion

        public DateTime serverLeagueDataDateTime;
        public GetLeagueDataResponse serverLeagueData = null;
        public void GetNewLeagueData(Action action = null)
        {
            int LastLeagueId = UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKeys.LeagueFirstEnter + Player.GbId, 0);
            NetworkManager.Instance.GetLeagueData(LastLeagueId, (GetLeagueDataResponse getLeagueDataResponse) =>
            {
                serverLeagueData = getLeagueDataResponse;
                serverLeagueDataDateTime = DataConvUtil.ServerDateTime;
                action?.Invoke();
                CheckLeagueSignRedDot();
            });
        }
        public void CheckLeagueSignRedDot()
        {
            bool isCanSign = (TeamState)serverLeagueData.TeamState == TeamState.INIT || (TeamState)serverLeagueData.TeamState == TeamState.SETTLE;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "/" + CompitionID.League.ToString() + "/sign");
            node.AddValue(isCanSign ? 1 : -1);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

    }
}