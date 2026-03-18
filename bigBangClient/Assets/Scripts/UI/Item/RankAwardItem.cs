using System.Collections.Generic;
using System.Linq;
using Babu;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class RankAwardItemData
    {
        public ActivityData activityData;
        public ActivityRankInfo activityRankInfo;//服务器发来的排名信息
        public int virtualRank = -1;//虚位以待的排名序号
    }
    public class RankAwardItem : MonoBehaviour
    {
        [SerializeField] private Image rankImg;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private ClubIconItem clubIcon;
        [SerializeField] private TMP_Text rankText = null;
        [SerializeField] private TMP_Text clubNameText = null;
        [SerializeField] private TMP_Text enemyCountryText = null;
        [SerializeField] private TMP_Text enemyClubNameText = null;
        [SerializeField] private TMP_Text topCardNameText = null;
        [SerializeField] private TMP_Text combatCountText = null;
        [SerializeField] private List<Image> starList;
        [SerializeField] private HorizontalLayoutGroup combatPanel = null;
        [SerializeField] private HorizontalLayoutGroup starLayout = null;
        [SerializeField] private TMP_Text noPlayerText = null;
        [SerializeField] private BabuButton detailButton = null;

        private Color white = new Color(166 / 255f, 177 / 255f, 185 / 255f, 1);
        private Color green = new Color(19 / 255f, 178 / 255f, 55 / 255f, 1);

        private Color winColor = new Color(166 / 255f, 177 / 255f, 185 / 255f, 1);
        private Color redColor = new Color(187 / 255f, 48 / 255f, 49 / 255f, 1);

        RankAwardItemData rankAwardItemData;

        public void SetData(RankAwardItemData rankAwardItemData)
        {
            this.rankAwardItemData = rankAwardItemData;

            clubIcon.gameObject.SetActive(false);
            clubNameText.gameObject.SetActive(false);
            enemyClubNameText.gameObject.SetActive(false);
            enemyCountryText.gameObject.SetActive(false);
            topCardNameText.gameObject.SetActive(false);
            combatPanel.gameObject.SetActive(false);
            noPlayerText.gameObject.SetActive(false);
            starLayout.gameObject.SetActive(false);

            if (rankAwardItemData.activityRankInfo == null)
            {
                noPlayerText.gameObject.SetActive(true);
                SetRankNumInColumn0(rankAwardItemData.virtualRank);
                SetRewardsInColumn3();
                return;
            }

            SetRankNumInColumn0(rankAwardItemData.activityRankInfo.Rank);
            SetClubInColumn1();

            for (var index = 0; index < 5; index++)
            {
                starList[index].gameObject.SetActive(false);
            }
            switch (rankAwardItemData.activityData.cfg.Param1)
            {
                case 1:
                    SetCityInColumn2();
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    SetPlayerNameInColumn2();
                    SetPlayerStarInColumn2();
                    break;
                case 7:
                    SetCombatInColumn2();
                    break;
                default:
                    break;
            }
            SetRewardsInColumn3();
        }

        List<GameItem> rewardGameItemList = new();
        private async void SetRankNumInColumn0(int rank)
        {
            if (rank == -1)
            {
                rankImg.gameObject.SetActive(false);
                rankText.gameObject.SetActive(true);
                rankText.text = ">50";
                rankText.color = green;
                rewardGameItemList.Clear();
                ActivityTopRewardConfig activityTopRewardConfigSmall = Configs.ActivityTopReward.GetConfigList().Where(cfg => cfg.ActivityId == rankAwardItemData.activityData.cfg.Id).OrderBy(cfg => cfg.Max).LastOrDefault();
                rewardGameItemList = GameItemUtils.CreateGameItems(activityTopRewardConfigSmall.Rewards).ToList();
                return;
            }

            bool needUseRankImage = rank <= 3;
            rankImg.gameObject.SetActive(needUseRankImage);
            rankText.gameObject.SetActive(!needUseRankImage);
            if (needUseRankImage)
            {
                rankImg.sprite = await SpriteProxy.GetRank(rank);
            }
            else
            {
                rankText.text = rank.ToString();
                if (rankAwardItemData.activityRankInfo != null && rankAwardItemData.activityRankInfo.Gbid == Player.GbId)
                {
                    rankText.color = green;
                }
                else
                {
                    rankText.color = white;
                }
            }
            ActivityTopRewardConfig activityTopRewardConfig = Configs.ActivityTopReward.GetConfigList().Where(cfg => cfg.ActivityId == rankAwardItemData.activityData.cfg.Id).FirstOrDefault((cfg) =>
            {
                int rank = rankAwardItemData.activityRankInfo == null ? rankAwardItemData.virtualRank : rankAwardItemData.activityRankInfo.Rank;
                return cfg.Min <= rank && cfg.Max >= rank;
            });
            rewardGameItemList = GameItemUtils.CreateGameItems(activityTopRewardConfig.Rewards).ToList();
        }
        private void SetClubInColumn1()
        {
            clubIcon.SetIcon(rankAwardItemData.activityRankInfo.Icon);// 设置球队图片
            clubNameText.text = rankAwardItemData.activityRankInfo.Name;// 设置球队名称
            clubIcon.gameObject.SetActive(true);
            clubNameText.gameObject.SetActive(true);
        }
        private void SetCityInColumn2()
        {
            int clubId = rankAwardItemData.activityRankInfo.ClubId;
            ChallengeClubConfig challengeClubConfig = Configs.ChallengeClub.GetConfig(clubId);
            if (challengeClubConfig == null)
            {
                Debug.LogWarningFormat("RankAwardItem , SetCityInColumn2 , challengeClubConfig == null , clubId = {0}", clubId);
                return;
            }
            enemyClubNameText.text = "{0}-{1} {2}".SafeFormat(challengeClubConfig.Id / 100 % 100, challengeClubConfig.Id % 100, challengeClubConfig.Name);
            enemyClubNameText.gameObject.SetActive(true);
            int countryId = challengeClubConfig.Country;
            ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(countryId);
            if (challengeCountryConfig == null)
            {
                Debug.LogWarningFormat("RankAwardItem , SetCityInColumn2 , challengeCountryConfig == null , countryId = {0}, clubId = {0}", countryId, clubId);
                return;
            }
            enemyCountryText.text = "[{0}]{1}".SafeFormat(challengeCountryConfig.LevelTxt, challengeCountryConfig.Name);
            enemyCountryText.gameObject.SetActive(true);
        }
        private void SetPlayerNameInColumn2()
        {
            if (rankAwardItemData.activityRankInfo.CardId == -1)
            {
                topCardNameText.text = "暂无此位置球员";
                topCardNameText.color = CBAColorUtil.Instance.GetColor(rankAwardItemData.activityRankInfo.Quality);
                topCardNameText.gameObject.SetActive(true);
                return;
            }
            topCardNameText.color = CBAColorUtil.Instance.GetColor(rankAwardItemData.activityRankInfo.Quality);
            int cardId = rankAwardItemData.activityRankInfo.CardId;
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
            if (cardModelConfig == null)
            {
                Debug.LogWarningFormat("RankAwardItem , SetPlayerNameInColumn2 , cardModelConfig == null , cardId = {0}", cardId);
                return;
            }
            topCardNameText.text = PlayerCard.GetFullName(cardModelConfig);
            topCardNameText.gameObject.SetActive(true);
        }
        private async void SetPlayerStarInColumn2()
        {
            int colorfulStarCount = rankAwardItemData.activityRankInfo.Star - 5;
            for (var index = 0; index < 5; index++)
            {
                if (index > rankAwardItemData.activityRankInfo.Star - 1)
                {
                    starList[index].gameObject.SetActive(false);
                }
                else
                {
                    starList[index].gameObject.SetActive(true);
                    if (index + 1 <= colorfulStarCount)
                        starList[index].sprite = await SpriteProxy.GetColorfulStar();
                    else
                        starList[index].sprite = await SpriteProxy.GetYellowStar();
                }
            }
            starLayout.gameObject.SetActive(true);
        }
        private void SetCombatInColumn2()
        {
            combatPanel.gameObject.SetActive(true);
            combatCountText.text = rankAwardItemData.activityRankInfo.Combat.ToString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(combatCountText.GetComponent<RectTransform>());
        }
        private void SetRewardsInColumn3()
        {
            SetInventory();
        }

        public void SetBackgroundColor(Color c)
        {
            backgroundImg.color = c;
        }

        [SerializeField] private List<InventoryItem> rewardsList;
        private void SetInventory()
        {
            int rewardsItemsCount = rewardGameItemList.Count;

            for (int index = 0; index < 4; index++)
            {
                if (rewardsItemsCount <= index)
                {
                    rewardsList[index].gameObject.SetActive(false);
                }
                else
                {
                    rewardsList[index].gameObject.SetActive(true);
                    rewardsList[index].SetData(rewardGameItemList[index]);
                }
            }
        }

        #region 查看详细信息

        private void OnEnable()
        {
            detailButton.OnClick += OnClickDetailButton;
        }
        private void OnDisable()
        {
            detailButton.OnClick -= OnClickDetailButton;
        }
        private void OnClickDetailButton(BabuButton _)
        {
            if (rankAwardItemData.activityRankInfo != null && rankAwardItemData.activityRankInfo.Rank == -1)//自己未上榜时，自己构造数据
            {
                switch (rankAwardItemData.activityData.cfg.Param1)
                {
                    case 1:
                        ShowSelfTeamDetail();
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        ShowSelfCardDetail();
                        break;
                    case 7:
                        ShowSelfTeamDetail();
                        break;
                    default:
                        break;
                }
                return;
            }

            if (rankAwardItemData.activityRankInfo == null) return;
            if (rankAwardItemData.activityRankInfo.Rank == -1) return;

            switch (rankAwardItemData.activityData.cfg.Param1)
            {
                case 1:
                    ShowTeamDetail();
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    ShowCardDetail();
                    break;
                case 7:
                    ShowTeamDetail();
                    break;
                default:
                    break;
            }
        }

        private void ShowSelfCardDetail()
        {
            PlayerCard playerCard = Player.CardManager.GetCard(rankAwardItemData.activityRankInfo.CardId);
            UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(playerCard, true));
        }
        private void ShowSelfTeamDetail()
        {
            RankTeamInfo rankTeamInfo = new();
            rankTeamInfo.TeamIcon = Player.Icon;
            rankTeamInfo.TeamName = Player.Name;
            rankTeamInfo.Strength = Player.Strength;
            Formation formation = Player.FightManager.FormationController.GetFormation(FightType.PVE);
            rankTeamInfo.TacticsIdList.AddRange(formation.TacticsIdList);
            foreach (var item in Player.FightManager.FormationController.TacticsLevelDic)
            {
                rankTeamInfo.TacticsLevels.Add(item.Key, item.Value);
            }
            rankTeamInfo.Level = Player.Level;
            foreach (var item in Player.TrainManager.TrainDic)
            {
                rankTeamInfo.TrainLevels.Add(item.Key, item.Value.Level);
            }
            UIController.Instance.OpenWindow<OtherPlayerTeamUI>(new OtherPlayerTeamProperties(rankTeamInfo, true));
        }

        private void ShowCardDetail()
        {
            int rankType = rankAwardItemData.activityData.cfg.Param1;
            string gbid = rankAwardItemData.activityRankInfo.Gbid;
            int cardId = rankAwardItemData.activityRankInfo.CardId;
            NetworkManager.Instance.GetRankCardDetail(rankType, gbid, cardId, OnServerCardDetailBack);
        }
        private void OnServerCardDetailBack(PlayerCardInfo playerCardInfo)//服务器返回信息后调用
        {
            PlayerCard playerCard = new(playerCardInfo.CardId);
            playerCard.UnPack(playerCardInfo);
            UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(playerCard, false));
        }

        private void ShowTeamDetail()
        {
            int rankType = rankAwardItemData.activityData.cfg.Param1;
            string gbid = rankAwardItemData.activityRankInfo.Gbid;
            NetworkManager.Instance.GetRankTeamDetail(rankType, gbid, OnServerTeamDetailBack);
        }
        private void OnServerTeamDetailBack(RankTeamInfo rankTeamInfo)//服务器返回信息后调用
        {
            UIController.Instance.OpenWindow<OtherPlayerTeamUI>(new OtherPlayerTeamProperties(rankTeamInfo));
        }

        #endregion

    }
}