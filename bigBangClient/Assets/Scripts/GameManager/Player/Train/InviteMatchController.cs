using System.Collections.Generic;
using Babu;
using BigBang.UI;
using Protocol;

namespace BigBang
{
    public class InviteMatchController
    {
        private PlayerTrainManager _trainManager;
        public bool IsUnlock { get; set; } = false;

        private Dictionary<int, InviteMatch> _matchDic = new Dictionary<int, InviteMatch>();

        public InviteMatchController(PlayerTrainManager trainManager)
        {
            _trainManager = trainManager;
        }

        public void Init()
        {
            IsUnlock = false;
        }

        public void UnPack(InviteMatchControllerInfo data)
        {
            if (data == null) return;
            IsUnlock = data.IsUnlock;
            foreach (var matchData in data.MatchList)
            {
                int id = matchData.Id;
                _matchDic[id] = new InviteMatch(matchData);
            }
        }

        public void Unlock()
        {
            IsUnlock = true;
            EventManager.Instance.Dispatch(EventID.OnUnlockInviteMatch);
        }

        private bool CheckCanDoMatch(int index)
        {
            var match = GetMatch(index);
            if (match == null) return false;
            if (match.State != InviteMatchState.Init) return false;
            return true;
        }

        public InviteMatch GetMatch(int index)
        {
            if (_matchDic.ContainsKey(index)) return _matchDic[index];
            return null;
        }
        
        private void DoMatchImpl(int index)
        {
            NetworkManager.Instance.DoInviteMatch(index, OnDoInviteMatch);
        }

        private void OnDoInviteMatch(DoInviteMatchResponse response)
        {
            var matchInfo = response.MatchInfo;
            var index = matchInfo.Id;
            _matchDic[index] = new InviteMatch(matchInfo);

            CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshBigBangUIRedDot);
            UIController.Instance.OpenWindow<InviteMatchResultUI>(new InviteMatchResultProperties(_matchDic[index]));
        }

        public bool DoMatch(int index)
        {
            if (!CheckCanDoMatch(index))
            {
                return false;
            }

            DoMatchImpl(index);
            return true;
        }

        public void CheckRedDot()
        {
            var isred = false;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/InviteMatch");
            foreach (int key in _matchDic.Keys) {
                var match = _matchDic[key];
                if (match.State == InviteMatchState.Init || match.State == InviteMatchState.End)
                {
                    isred = true;
                    break;
                }
            }
            node.AddValue(isred ? 1 : -1);
        }

        public void DoReward(int index, OfflineExpConfirmType type)
        {
            var match = GetMatch(index);
            if (match == null) return;

            NetworkManager.Instance.DoInviteMatchReward(index, type, callback =>
            {
                var rewardExp = match.BaseReward;
                if (type == OfflineExpConfirmType.Video || type == OfflineExpConfirmType.Diamond) rewardExp *= 10;
                Player.TrainManager.AddExp(rewardExp);

                //UIController.Instance.OpenWindow<ExpRewardUI>(new ExpRewardProperties(rewardExp));
                EventManager.Instance.Dispatch(EventID.OnInviteMatchRefresh);
            });
        }

        public void DevClear()
        {
            foreach (var matchItem in _matchDic)
            {
                var match = matchItem.Value;
                if (match == null) continue;
                match.CdEndTime = Utils.DataConvUtil.ServerTimeEx;
            }
        }
    }
}