using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using Protocol;
using UnityEngine;

namespace BigBang.UI
{
    public class InviteMatchPad : MonoBehaviour
    {
        [SerializeField] private List<InviteMatchItem> matchList;

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.OnInviteMatchRefresh, OnRefresh);
        }

        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.OnInviteMatchRefresh, OnRefresh);
        }

        private bool NeedFetch()
        {
            for (int i = 1; i <= GameConst.MaxInviteMatchKen; i++)
            {
                var match = Player.TrainManager.InviteMatchController.GetMatch(i);
                if (match == null || match.IsCdEnd())
                {
                    return true;
                }
            }

            return false;
        }

        public void SetData()
        {
            OnRefresh(null);
        }

        private void OnRefresh(object[] args)
        {
            if (NeedFetch())
            {
                NetworkManager.Instance.FetchInviteMatchInfo(OnFetchInviteMatchInfo);
            }
            else
            {
                UpdateMatchList();
            }
        }

        private void OnFetchInviteMatchInfo(FetchInviteMatchInfoResponse response)
        {
            Player.TrainManager.InviteMatchController.UnPack(response.Info);
            UpdateMatchList();
        }

        private void UpdateMatchList()
        {
            InviteMatch showMatch = null;
            foreach (var item in matchList)
            {
                var match = Player.TrainManager.InviteMatchController.GetMatch(item.Index);
                item.SetData(match);
                if (match is { State: InviteMatchState.End })
                {
                    showMatch = match;
                }
            }

            if (showMatch != null)
            {
                UIController.Instance.OpenWindow<InviteMatchResultUI>(
                    new InviteMatchResultProperties(showMatch)
                );
            }
        }

        //播放进入动画
        public void PlayAnim()
        {
            for (int i = 0; i < matchList.Count; i++)
            {
                var item = matchList[i];
                if (!item.CurrentState == null)
                {
                    item.Anim.Play(i * 0.15f, false);
                }
                else
                {
                    item.Anim.Play(i * 0.15f, !item.CurrentState.Value);
                }
            }
        }
    }
}