using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ArenaResultWinPad : MonoBehaviour
    {


        private void OnEnable()
        {
            InitData();
            SetUI();

            anim.PlayEnter();
        }

        private void OnDisable()
        {

        }

        private Dictionary<int, ArenaRewardConfig> upStageArenaRewardConfigDic = new();
        private Dictionary<int, ArenaRewardConfig> endBattleArenaRewardConfigDic = new();
        private bool isInitData = false;
        private void InitData()
        {
            if (isInitData == true) return;
            isInitData = true;

            foreach (ArenaRewardConfig arenaRewardConfig in Configs.ArenaReward.GetConfigList())
            {
                if (arenaRewardConfig.Type == 1) { upStageArenaRewardConfigDic.Add(arenaRewardConfig.Stage, arenaRewardConfig); }
                if (arenaRewardConfig.Type == 4) { endBattleArenaRewardConfigDic.Add(arenaRewardConfig.Stage, arenaRewardConfig); }
            }
        }

        [SerializeField] private GameObject itemPrefab;
        [SerializeField] public ArenaResultWinPadAnim anim;
        [SerializeField] private TMP_Text successText;

        [SerializeField] private HorizontalAdapter EndBattleRewardLayout;

        [SerializeField] private GameObject UpStageRewardPanel;
        [SerializeField] private HorizontalAdapter UpStageRewardLayout;

        [SerializeField] private GameObject RankChangePanel;
        [SerializeField] private TMP_Text RankTitleText;
        [SerializeField] private TMP_Text rankOldNumText;
        [SerializeField] private TMP_Text rankNewNumText;

        [SerializeField] private GameObject RankNotChangePanel;

        [SerializeField] private GameObject WinPeopleImage;
        [SerializeField] private GameObject WinPeopleCircleImage;
        [SerializeField] private GameObject BadgePeopleCircleImage;
        [SerializeField] private GameObject WinTextBgImage;
        [SerializeField] private GameObject UpStageTitlePanel;
        [SerializeField] private Image BadgeImageOld;
        [SerializeField] private Image BadgeImageNew;

        private async void SetUI()
        {
            int oldSatge = Player.BattleManager.oldArenaInfo.ArenaStage;
            int newSatge = Player.BattleManager.newArenaInfo.ArenaStage;
            bool isUpSatge = oldSatge < newSatge;

            ArenaRewardConfig endBattleArenaRewardConfig = endBattleArenaRewardConfigDic[newSatge];
            SetRewards(endBattleArenaRewardConfig.Reward, EndBattleRewardLayout);

            UpStageRewardPanel.SetActive(isUpSatge);
            if (isUpSatge)
            {
                ArenaRewardConfig upStageArenaRewardConfig = upStageArenaRewardConfigDic[newSatge];
                SetRewards(upStageArenaRewardConfig.Reward, UpStageRewardLayout);
                BadgeImageOld.sprite = await SpriteProxy.GetBadge(oldSatge);
                BadgeImageOld.SetNativeSize();
                BadgeImageNew.sprite = await SpriteProxy.GetBadge(newSatge);
                BadgeImageNew.SetNativeSize();
            }
            WinPeopleImage.SetActive(!isUpSatge);
            WinPeopleCircleImage.SetActive(!isUpSatge);
            BadgePeopleCircleImage.SetActive(isUpSatge);
            WinTextBgImage.SetActive(!isUpSatge);
            UpStageTitlePanel.SetActive(isUpSatge);

            successText.text = "你打败了“{0}”！".SafeFormat(Player.BattleManager.arenaTeamData.Name);

            bool isStageS = oldSatge == 9;
            if (isStageS)
            {
                if (Player.BattleManager.oldArenaInfo.ArenaRank == Player.BattleManager.newArenaInfo.ArenaRank)
                {
                    RankChangePanel.SetActive(false);
                    RankNotChangePanel.SetActive(true);
                }
                else
                {
                    RankChangePanel.SetActive(true);
                    RankNotChangePanel.SetActive(false);
                    RankTitleText.text = "排名";
                    rankOldNumText.text = Player.BattleManager.oldArenaInfo.ArenaRank.ToString();
                    rankNewNumText.text = Player.BattleManager.newArenaInfo.ArenaRank.ToString();
                }
            }
            else
            {
                RankChangePanel.SetActive(true);
                RankNotChangePanel.SetActive(false);
                RankTitleText.text = "分数";
                rankOldNumText.text = Player.BattleManager.oldArenaInfo.ArenaScore.ToString();
                rankNewNumText.text = Player.BattleManager.newArenaInfo.ArenaScore.ToString();
            }
        }

        private void SetRewards(string rewardStr, HorizontalAdapter layout, string rewardStr2 = null)
        {
            Transform layoutTrans = layout.transform;
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            if (string.IsNullOrWhiteSpace(rewardStr2) == false)
            {
                List<GameItem> gameItemList2 = GameItemUtils.CreateGameItems(rewardStr2).ToList();
                gameItemList.AddRange(gameItemList2);
            }
            while (layoutTrans.childCount < gameItemList.Count) Instantiate(itemPrefab, layoutTrans);
            for (int i = 0; i < layoutTrans.childCount; i++)
            {
                if (i < gameItemList.Count)
                {
                    var reward = gameItemList[i];
                    var child = layoutTrans.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.GetComponent<InventoryItem>().SetData(reward);
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
            layout.Calculate();
        }

    }
}
