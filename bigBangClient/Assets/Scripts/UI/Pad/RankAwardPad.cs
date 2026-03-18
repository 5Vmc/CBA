using System.Collections.Generic;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using BigBang.Animation;
using GameConfig.Config;
using GameConfig;
using Utils;
using Babu;
using Utils.GameItem;

namespace BigBang.UI
{

    public class RankAwardPad : MonoBehaviour, IActivity
    {
        [SerializeField] private RankAwardAdapter adapter;
        [SerializeField] private Button giftBtn;
        [SerializeField] private Button introduceBtn;
        [SerializeField] private RankAwardItem myRankItem;
        [SerializeField] private TMP_Text txtLeftTime;
        [SerializeField] private List<TMP_Text> titleTxt;
        [SerializeField] private Button helpbtn;

        public ActivityClientType activityType = ActivityClientType.RankAwards;

        private ActivityData activityData;

        private void OnEnable()
        {
            giftBtn.onClick.AddListener(OnGift);
            helpbtn.onClick.AddListener(OnHelp);
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
            EventManager.Instance.Register(EventID.RefreshWindow, OnServerPushRefresh);
        }

        private void OnDisable()
        {
            giftBtn.onClick.RemoveListener(OnGift);
            helpbtn.onClick.RemoveListener(OnHelp);
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            EventManager.Instance.Unregister(EventID.RefreshWindow, OnServerPushRefresh);
        }

        private void OnGift()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_TARGET);
            List<ActivityTopRewardConfig> RewardsConfigList;
            RewardsConfigList = Configs.ActivityTopReward.GetConfigList().FindAll(p => p.ActivityId == activityData.cfg.Id);
            UIController.Instance.OpenWindow<RankAwardPreviewUI>(new RankAwardPreviewUIProperties(activityData.cfg.Name + "奖励预览", RewardsConfigList));
        }
        private void OnHelp()
        {
            UIController.Instance.OpenWindow<ArenaRuleUI>(new ArenaRuleUIProperties(1));
        }

        private void RefreshLeftTime()
        {
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            txtLeftTime.text = "活动结束：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }

        private void OnServerPushRefresh(object[] objects)
        {
            if ((int)objects[0] != activityData.cfg.Id) return;
            RefreshInfo();
        }
        private void RefreshInfo()
        {
            //设置自己的信息
            activityData.MyRankData = null;
            int targetScrollIndex = -1;
            for (int i = 0; i < activityData.RankData.Count; i++)
            {
                RankAwardItemData rankAwardItemDataItem = activityData.RankData[i];
                if (rankAwardItemDataItem.activityRankInfo != null && rankAwardItemDataItem.activityRankInfo.Gbid == Player.GbId)
                {
                    activityData.MyRankData = rankAwardItemDataItem;
                    targetScrollIndex = i;
                    break;
                }
            }
            if (activityData.MyRankData == null)
            {
                RankAwardItemData rankAwardItemData = new();
                rankAwardItemData.activityData = activityData;
                ActivityRankInfo activityRankInfo = new();
                rankAwardItemData.activityRankInfo = activityRankInfo;
                activityData.MyRankData = rankAwardItemData;

                activityRankInfo.Rank = activityData.cfg.Param1;
                activityRankInfo.Gbid = Player.GbId;
                activityRankInfo.Name = Player.Name;
                activityRankInfo.Icon = Player.Icon;
                activityRankInfo.Rank = -1;
                switch (activityData.cfg.Param1)
                {
                    case 1:
                        activityRankInfo.ClubId = ClassicManager.Instance.GetLastPassedLevel();
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        PlayerCard playerCard = Player.CardManager.GetBestCard((PositionSeparatedType)(activityData.cfg.Param1 - 1));
                        if (playerCard != null)
                        {
                            activityRankInfo.CardId = playerCard.CardId;
                            activityRankInfo.Quality = playerCard.Quality;
                            activityRankInfo.Star = playerCard.Star;
                        }
                        else
                        {
                            activityRankInfo.CardId = -1;
                            activityRankInfo.Quality = 0;
                            activityRankInfo.Star = 0;
                        }
                        break;
                    case 7:
                        activityRankInfo.Combat = Player.Strength;
                        break;
                    default:
                        break;
                }
                activityRankInfo.Gbid = Player.GbId;
            }
            myRankItem.SetData(activityData.MyRankData);

            //设置OSA列表
            adapter.SetData(activityData.RankData);
            if (targetScrollIndex == -1) targetScrollIndex = 0;
            targetScrollIndex -= 2;
            targetScrollIndex = Utility.KeepInRange(targetScrollIndex, 0, activityData.RankData.Count - 1);
            if (activityData.RankData.Count > 0) adapter.ScrollTo(targetScrollIndex);
            adapter.PlayAnim();
            myRankItem.gameObject.SetActive(true);
        }

        public void LoadActivity(ActivityData _data)
        {

            activityData = _data;

            //设置标题
            string[] titleTxtArr = _data.cfg.Param2.Split("|");
            for (var index = 0; index < titleTxtArr.Length; index++)
            {
                titleTxt[index].text = titleTxtArr[index];
            }
            adapter.InitAnim();

            ActivityController.Instance.GetRankInfo(activityData);
        }
    }
}