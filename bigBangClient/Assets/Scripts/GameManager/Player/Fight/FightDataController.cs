using System.Collections;
using Protocol;
using System.Collections.Generic;
using BigBang.UI;

namespace BigBang
{
    public class FightDataController
    {
        private PlayerFightManager _manager;

        private string _curFightId;
        public Dictionary<string, FightData> FightDic { get; set; } = new Dictionary<string, FightData>();

        public FightDataController(PlayerFightManager manager)
        {
            _manager = manager;
        }

        public FightData GetFightData(string fightId)
        {
            if (FightDic.ContainsKey(fightId)) return FightDic[fightId];
            return null;
        }

        public FightData GetCurFightData()
        {
            return GetFightData(_curFightId);
        }

        public void WatchFight(string fightId)
        {
            if (!FightDic.ContainsKey(fightId))
            {
                AddNewFightData(fightId);
            }

            NetworkManager.Instance.WatchFight(fightId);
        }

        public void EndWatchFight(string fightId)
        {
            var fight = GetFightData(fightId);
            if (fight == null)
            {
                return;
            }
        }

        public void UpdateFightBeginData(FightBeginDataNotify data)
        {
            // var fightId = data.FightId;
            // // if (!FightDic.ContainsKey(fightId)) AddNewFightData(fightId);
            // var fight = GetFightData(fightId);
            // if (fight != null)
            // {
            //     _curFightId = fightId;
            //     fight.UpdateFightBeginData(data);
            //     fight.BeginFetch();
            //     // 打开加载界面
            //     UIController.Instance.OpenWindow<MatchLoadingUI>(new MatchLoadingUIProperties(() =>
            //     {
            //         UIController.Instance.ShowPanel<MatchAgainstUI>(new MatchAgainstUIProperties(fight));
            //     }));
            // }
        }

        public void UpdateFightFrameData(FightFrameDataNotify data)
        {
            var fightId = data.FightId;
            var fight = GetFightData(fightId);
            _curFightId = fightId;
            fight?.UpdateFightFrameData(data);
        }

        public void NotifyRecalculateFrame(FightRecalculateframeNotify data)
        {
            var fightId = data.FightId;
            var fight = GetFightData(fightId);
            if (fight == null) return;

            fight.NotifyRecalculateFrame(data.RecalculateFrame);
        }

        public void NotifyReportReady(FightReportReadyNotify data)
        {
            var fightId = data.FightId;
            var fight = GetFightData(fightId);
            if (fight == null) return;

            fight.IsReportDataReady = true;
        }

        private void AddNewFightData(string fightId)
        {
            FightDic.Add(fightId, new FightData(fightId));
        }
    }
}