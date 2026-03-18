using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using BigBang.Animation;
using System;
using TMPro;
using UnityTimer;
using Babu;
using Utils;

namespace BigBang.UI
{
    public class MatchHomeUIProperites : PanelProperties
    {
        public MatchHomeUIProperites()
        {
        }
    }

    public class MatchHomeUI : APanelController<MatchHomeUIProperites>
    {
        [SerializeField] private BabuButton myMatchBtn;
        [SerializeField] private List<CompitionItem> items;
        [SerializeField] private MatchHomeUIAnim Anim;
        [SerializeField] private TMP_Text txtTime;
        [SerializeField] private Image imgbtn;
        [SerializeField] private Button closeButton = null;

        private Timer timer;
        private int leftSeconds;
        protected override void AddListeners()
        {
            base.AddListeners();
            myMatchBtn.OnClick += toMyMatch;
            closeButton.onClick.AddListener(OnClose);
        }


        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            myMatchBtn.OnClick -= toMyMatch;
            closeButton.onClick.RemoveListener(OnClose);
            timer?.Cancel();
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            UIController.Instance.HidePanel<MatchHomeUI>();
        }


        private void toMyMatch(BabuButton sender)
        {
            if (Player.PVPManager.resp == null)
            {
                Tips.PopTips("等待下一轮比赛开启！");
                return;
            }
            if (Player.PVPManager.HasCompition)
            {
                string compitionName = Player.PVPManager.resp.CompitionId == CompitionID.League ? "联赛" : "杯赛";
                UIController.Instance.ShowPanel<MyGameUI>(new MyGameUIProperties(Player.PVPManager.resp.CompitionId, Player.PVPManager.resp.LeagueId, compitionName));
            }
            else
            {
                Tips.PopTips("等待下一轮比赛开启！");
            }
        }

        protected override void OnPropertiesSet()
        {
            Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Unknown;
            base.OnPropertiesSet();
            NetworkManager.Instance.GetCompitionData(response =>
            {
                // 联赛
                items[0].SetData(response.LeagueInfo);
                // 杯赛
                items[1].SetData(response.CupInfo);
                if (response.LeagueInfo == null) Debug.Log("response.LeagueInfo is null");
                if (response.CupInfo == null) Debug.Log("response.CupInfo is null");
            });

            txtTime.text = "";
            imgbtn.gameObject.SetActive(false);
            Player.PVPManager.GetRecentlyMatch(() =>
            {
                setTimeCD();
            });

            Anim.PlayEnter();
        }

        private void setTimeCD()
        {
            if(Player.PVPManager.resp == null)
            {
                txtTime.text = "";
                imgbtn.gameObject.SetActive(false);
                return;
            }
            leftSeconds = (int)(Player.PVPManager.resp.Time - Utils.DataConvUtil.ServerTime);
            if (leftSeconds > 0)
            {
                CDShow();
                timer = Timer.Register(this.gameObject, 1f, CDShow, null, true, true);
                imgbtn.gameObject.SetActive(true);
            }
            else
            {
                txtTime.text = Player.PVPManager.StatusStr;
                imgbtn.gameObject.SetActive(false);
            }
        }

        private void CDShow()
        {
            leftSeconds--;
            if (leftSeconds <= 0)
            {
                txtTime.text = "";
                timer?.Cancel();
            }
            txtTime.text = "下一场: " + Utils.DataConvUtil.FormatTimeLeft(leftSeconds);
        }
    }
}