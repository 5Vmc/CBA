using Babu;
using BigBang.Animation;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CompitionItem : MonoBehaviour
    {
        public int ID = CompitionID.None;

        [SerializeField] private Button rewardBtn;
        [SerializeField] private Button compitionBtn;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text roundText;
        [SerializeField] private TMP_Text currentText;
        [SerializeField] private Transform pos1;
        [SerializeField] private Transform pos2;
        [SerializeField] private Image reddotimg;

        private int leagueID;
        private bool isInit = false;
        public CompitionItemAnim Anim;
        private int leagueLevel = 1;
        private bool isLeagueNotReady = false;
        private bool isCupNotReady = false;

        private void OnEnable()
        {
            rewardBtn.onClick.AddListener(OnReward);
            compitionBtn.onClick.AddListener(OnCompition);
        }

        private void OnDisable()
        {
            rewardBtn.onClick.RemoveListener(OnReward);
            compitionBtn.onClick.RemoveListener(OnCompition);
        }

        public void SetData(PlayerLeagueInfo data)
        {
            refreshRedDot();
            if (data == null || data.LeagueId == 0)
            {
                currentText.text = Lang.Error(ErrorID.CompitionIsNotReady);
                currentText.gameObject.SetActive(true);
                isLeagueNotReady = true;
                isCupNotReady = true;
                roundText.text = string.Empty;
                return;
            }
            leagueID = data.LeagueId;
            isInit = false;
            leagueLevel = data.LeagueLevel;
            if (ID == CompitionID.League)
            {
                isInit = true;
                // 设置联赛名称
                nameText.text = Lang.Get(LangID.CompitionNameText).Replace("{level}", data.LeagueLevel.ToString()).Replace("{name}", Lang.Get(LangID.LeagueNameText));
                if (data.LeagueRoundId <= 0)
                {
                    currentText.gameObject.SetActive(false);
                    roundText.transform.SetParent(pos2);
                    roundText.transform.localPosition = Vector3.zero;
                    roundText.text = Lang.Error(ErrorID.CompitionIsNotReady);
                    isLeagueNotReady = true;
                }
                else
                {
                    currentText.gameObject.SetActive(false);
                    roundText.transform.SetParent(pos1);
                    roundText.transform.localPosition = Vector3.zero;
                    roundText.text = Lang.Get(LangID.RoundText).Replace("{value}", data.LeagueRoundId.ToString());
                    isLeagueNotReady = false;
                }
            }
            if (ID == CompitionID.Cup)
            {
                // 设置杯赛名称
                nameText.text = Lang.Get(LangID.CompitionNameText).Replace("{level}", data.LeagueLevel.ToString()).Replace("{name}", Lang.Get(LangID.CupNameText));
                var cfg = Configs.CupCourse.GetConfig(data.LeagueRoundId);
                if (cfg == null)
                {
                    roundText.text = string.Empty;
                    isCupNotReady = true;
                    return;
                }
                isCupNotReady = false;
                isInit = true;
                roundText.text = cfg.RoundName;
                Debug.Log("杯赛RoundID=" + data.LeagueRoundId);
            }
        }

        private void refreshRedDot()
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "/" + ID.ToString());
            node.IsRed(reddotimg.transform);
        }

        /// <summary>
        /// 领取日常比赛奖励
        /// </summary>
        private void getCommonMatchReward()
        {
            NetworkManager.Instance.GetPVPReward(ID, (resp) =>
            {
                if (resp.ReceiveSucceed)
                {
                    var properties = new InventoryObtainedUIProperties(Player.PVPManager.tmpRewards[ID]);
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);// 打开通用收益界面
                    //清理临时数据和小红点
                    Player.PVPManager.tmpRewards[ID] = new System.Collections.Generic.List<Utils.GameItem.GameItem>();
                    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "/" + ID.ToString());
                    node.AddValue(-1);
                    refreshRedDot();
                }
            });
        }

        private void OnCompition()
        {
            if (Player.PVPManager.tmpRewards[ID].Count > 0)
            {
                getCommonMatchReward();
            }

            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            //if (!isInit)
            //{
            //    Debug.LogError("无数据");
            //    return;
            //}

            switch (ID)
            {
                case CompitionID.League:
                    if (isLeagueNotReady)
                    {
                        //联赛正在筹备中提示
                        Tips.PopError(ErrorID.LeagueNotReadyTip);
                    }
                    else
                    {
                        // 打开联赛界面
                        UIController.Instance.ShowPanel<LeagueUI>(new LeagueUIProperties(leagueID, nameText.text, leagueLevel));
                    }

                    break;
                case CompitionID.Cup:
                    if (isCupNotReady)
                    {
                        //完成一届联赛后开放杯赛提示
                        Tips.PopError(ErrorID.CupNotReadyTip);
                    }
                    else
                    {
                        // 打开杯赛界面
                        UIController.Instance.ShowPanel<CupUI>(new CupUIProperties(leagueID, nameText.text, leagueLevel));
                    }

                    break;
            }
        }

        private void OnReward()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_TARGET);
            UIController.Instance.OpenWindow<LeagueRewardsUI>(new LeagueRewardsUIProperties(ID, leagueLevel));
        }
    }
}
