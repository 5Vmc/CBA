using System;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    //奖励类型(1:进阶 2:每日 3:结算 4:战斗胜利)
    public enum ArenaStageRewardType
    {
        Promote = 1,
        Daily = 2,

        ActivityEnd = 3, //活动结束时候发的奖励
        Victory = 4
    }
    public class ArenaPad : MonoBehaviour
    {
        //private readonly int BATTLE_TIMES_MAX = 5;
        //private readonly int STAGE_S = 9;
        [SerializeField] private TMP_Text myJjb; //竞技币
        [SerializeField] private TMP_Text myTzcs; //挑战次数

        [SerializeField] private RewardItem[] dailyRewards; //每日奖励

        [SerializeField] private Button changeOpponetBtn; //换对手

        [SerializeField] private Button recordBtn; //查看记录按钮

        [SerializeField] private Button addBattleTimesBtn;

        [SerializeField] private Button rewardsPreBtn; //奖励预览

        [SerializeField] private Button exShopBtn; //竞技场兑换

        [SerializeField] private Button ruleHelpBtn; //规则说明

        [SerializeField] private Button moreRankBtn;
        [SerializeField] private Button rollUpRankBtn;

        //我的信息
        [SerializeField] private TMP_Text myRank;
        [SerializeField] private TMP_Text textMyRankInfo; //前缀
        [SerializeField] private Image myTierIcon;
        [SerializeField] private TMP_Text myTacticDefend;
        [SerializeField] private TMP_Text myTacticAttack;

        //排行
        [SerializeField] private ArenaRankItem[] rankItemList; //排行

        //对手
        [SerializeField] private TMP_Text refreshTime;
        [SerializeField] private ArenaOpponentItem[] opponentItems;

        [SerializeField] private GameObject rankDataPanel;
        [SerializeField] private GameObject noRankDataBg;
        [SerializeField] private GameObject opponentPanel;

        [SerializeField] private ArenaRankListAdapter osa;
        private ArenaInfo _arenaInfo;
        private List<ArenaRankInfo> _topRanks;

        [SerializeField] private RectTransform ArenaRankItemRoot;

        [SerializeField] private HorizontalLayoutGroup topLayout = null;

        [SerializeField] private TMP_Text myPowerText = null;

        private ArenaPadAnim Anim;
        protected void Awake()
        {
            this.Anim = GetComponent<ArenaPadAnim>();
#if UNITY_WEBGL
            topLayout.childAlignment = TextAnchor.UpperLeft;
#endif
        }

        Sequence oneSecondSeq;
        protected void OnEnable()
        {
            changeOpponetBtn.onClick.AddListener(OnChangeOpponent);
            foreach (ArenaOpponentItem item in opponentItems)
            {
                item.ChallengeButton.onClick.AddListener(delegate () { this.OnBeatOpponent(item.Data); });
                item.FirstButton.onClick.AddListener(delegate () { this.OnClickFirstButton(item.Data); });
            }
            addBattleTimesBtn.onClick.AddListener(OnAddBattleTime);
            moreRankBtn.onClick.AddListener(OnMoreRank);
            rollUpRankBtn.onClick.AddListener(OnRollUpRank);
            rewardsPreBtn.onClick.AddListener(OnRewardsPreview);
            ruleHelpBtn.onClick.AddListener(OnClickRuleInfo);
            exShopBtn.onClick.AddListener(OnClickExChangeBtn);
            arenaMoneyBtn.OnClick += OnClickArenaMoneyBtn;
            recordBtn.onClick.AddListener(OnClickRecordBtn);
            UpdateLeftTime();
            oneSecondSeq = DOTween.Sequence().AppendInterval(1f).AppendCallback(UpdateLeftTime).SetLoops(-1);
            RefreshGiftState();
            EventManager.Instance.Register(EventID.OnTriggerGuide2UIClose, OnTriggerGuide2UIClose);
        }
        protected void OnDisable()
        {
            changeOpponetBtn.onClick.RemoveListener(OnChangeOpponent);
            foreach (ArenaOpponentItem item in opponentItems)
            {
                item.ChallengeButton.onClick.RemoveAllListeners();
                item.FirstButton.onClick.RemoveAllListeners();
            }
            addBattleTimesBtn.onClick.RemoveListener(OnAddBattleTime);
            moreRankBtn.onClick.RemoveListener(OnMoreRank);
            rollUpRankBtn.onClick.RemoveListener(OnRollUpRank);
            rewardsPreBtn.onClick.RemoveListener(OnRewardsPreview);
            ruleHelpBtn.onClick.RemoveListener(OnClickRuleInfo);
            exShopBtn.onClick.RemoveListener(OnClickExChangeBtn);
            arenaMoneyBtn.OnClick -= OnClickArenaMoneyBtn;
            recordBtn.onClick.RemoveListener(OnClickRecordBtn);
            oneSecondSeq?.Kill();
            EventManager.Instance.Unregister(EventID.OnTriggerGuide2UIClose, OnTriggerGuide2UIClose);
        }

        private void OnTriggerGuide2UIClose(object[] args)
        {
            if (isNeedOpenGuide2UI) CheckNewSeason();
            isNeedOpenGuide2UI = false;
        }

        private bool isNeedOpenGuide2UI = false;
        public void OnShow(bool isNeedOpenGuide2UI)
        {
            this.isNeedOpenGuide2UI = isNeedOpenGuide2UI;
            SetData();
        }

        ArenaInfoResponse arenaInfoResponse = null;
        protected void SetData()
        {
            this._showMoreRank(false);

            this.myJjb.text = Player.PackageManager.GetGoodsNumber(400501).ToString();
            NetworkManager.Instance.GetArenaInfo(resp =>
            {
                if (resp.Succeed || resp.Info == null)
                {
                    arenaInfoResponse = resp;
                    Player.BattleManager.AddArenaInfo(resp.Info);
                    this._respUpdateUI(resp.Info);
                    this._respUpdateUIList(ProtoUtils.UnPackRepeatedField<ArenaRankInfo>(resp.Tops), ProtoUtils.UnPackRepeatedField<ArenaTeamData>(resp.Opponents), resp.Info.ArenaStage);
                    if (isNeedOpenGuide2UI == false) CheckNewSeason();
                }
                else
                {
                    Tips.PopTips("竞技场数据返回错误");
                    Debug.LogWarning("ArenaPad , SetData , GetArenaInfo , resp.Succeed == false");
                }
            });
        }

        private async void _respUpdateUI(ArenaInfo info)
        {
            this._arenaInfo = info;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicArena, "/lefttime");
            node.AddValue(info.BattleTimesLeft > 0 ? 1 : -1);

            if (info.ArenaStage == GameConst.ArenaSStage)
            {
                this.textMyRankInfo.text = "排名";
                this.myRank.text = info.ArenaRank.ToString();
            }
            else
            {
                this.textMyRankInfo.text = "积分";
                this.myRank.text = info.ArenaScore.ToString();
            }
            Formation formation = Player.FightManager.FormationController.GetFormation(FightType.ARENA);
            int mainTotalCombat = formation.GetMainTotalCombat();
            myPowerText.text = mainTotalCombat.ToString();

            //ArenaStageConfigTable conf = Configs.ArenaStage;
            //string icon = conf.GetConfig(info.ArenaStage).Icon;
            this.myTierIcon.sprite = await SpriteProxy.GetBadge(info.ArenaStage);
            this.myTzcs.text = info.BattleTimesLeft.ToString();

            //当前奖励
            ArenaRewardConfigTable rewardTable = Configs.ArenaReward;

            ArenaRewardConfig dailyReward = null;
            foreach (ArenaRewardConfig reward in rewardTable.GetConfigList())
            {
                if (reward.Stage == info.ArenaStage && reward.Type == (int)ArenaStageRewardType.Daily)
                {
                    dailyReward = reward;
                    break;
                }
            }

            int index = 0;
            List<RewardItemData> rewards = StringUtil.GetRewardByCfg(dailyReward.Reward);

            foreach (RewardItemData data in rewards)
            {
                if (index >= this.dailyRewards.Length)
                    break;
                RewardItem item = this.dailyRewards[index++];
                item.gameObject.SetActive(true);
                var itemData = new RewardItemData()
                {
                    id = data.id,
                    type = data.type,
                    count = data.count
                };

                item.SetData(itemData);

            }

            for (int i = index; i < this.dailyRewards.Length; i++)
            {
                this.dailyRewards[i].gameObject.SetActive(false);
            }

            //对手信息
            this.refreshTime.text = info.RefreshTimesLeft.ToString();

            Player.FightManager.FormationController.GetAndCheckDefaultFormation(
                FormationID.ARENA,

                formation => { this._updateMyTactics(formation); }
            );

            UpdateLeftTime();
            RefreshGiftState();
        }

        private void _respUpdateUIList(List<ArenaRankInfo> rankList, List<ArenaTeamData> opponents, int myState)
        {
            this.__updateRankItems(rankList);
            this.__updateOpponents(opponents, myState);

            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicArena, "/lefttime");
            node.AddValue(_arenaInfo.BattleTimesLeft > 0 ? 1 : -1);

            foreach (ArenaOpponentItem item in opponentItems)
            {
                node.IsRed(item.ChallengeButton.transform.Find("DotNodeImg"));
            }
        }

        private void __updateRankItems(List<ArenaRankInfo> rankList)
        {
            this._topRanks = rankList;
            rankDataPanel.SetActive(rankList.Count > 0);
            Debug.Log(ArenaRankItemRoot.rect.height);
            Debug.Log(rankItemList[0].GetComponent<RectTransform>().rect.height);
            moreRankBtn.gameObject.SetActive(ArenaRankItemRoot.rect.height / rankItemList[0].GetComponent<RectTransform>().rect.height < rankList.Count);
            noRankDataBg.SetActive(rankList.Count == 0);


            int index = 0;
            for (index = 0; index < rankItemList.Length && index < rankList.Count; index++)
            {

                ArenaRankInfo info = rankList[index];
                if (index >= rankItemList.Length)
                    break;
                rankItemList[index].SetData(info);

            }

            for (int i = index; i < rankItemList.Length; i++)
            {
                rankItemList[i].Active(false);
            }
        }

        private void __updateOpponents(List<ArenaTeamData> opponents, int myState)
        {
            for (int i = 0; i < 3; i++)
            {
                this.opponentItems[i].gameObject.SetActive(true);
            }
            int index = 0;
            foreach (ArenaTeamData data in opponents)
            {
                if (index >= this.opponentItems.Length)
                    break;
                this.opponentItems[index].SetData(data, myState);
                index++;
            }
            for (int i = index; i < 3; i++)
            {
                this.opponentItems[i].gameObject.SetActive(false);
            }
        }

        private void _respChangeOpponent(ChangeOpponentResponse resp)
        {
            if (resp.Succeed == true)
            {
                this._arenaInfo.RefreshTimesLeft--;
                this.refreshTime.text = this._arenaInfo.RefreshTimesLeft.ToString();
                __updateOpponents(ProtoUtils.UnPackRepeatedField<ArenaTeamData>(resp.Opponents), this._arenaInfo.ArenaStage);

                if (this.Anim)
                {
                    this.Anim.HideOpponenets();
                    this.Anim.ShowOpponents();
                }

                Anim.DoTextAnim(this.refreshTime);
            }


        }

        private void _updateMyTactics(Formation formation)
        {
            string textATK = "";
            string textDEF = "";
            DataConvUtil.TacticsIdList2AtkDef(formation.TacticsIdList, ref textATK, ref textDEF);

            myTacticDefend.text = textDEF;
            myTacticAttack.text = textATK;

        }

        private void OnChangeOpponent()
        {
            if (this._arenaInfo.RefreshTimesLeft <= 0)
            {
                Tips.PopTips("没有刷新次数了");
                return;
            }
            NetworkManager.Instance.ChangeOpponent(resp =>
            {
                _respChangeOpponent(resp);

            });
        }


        //首发按钮
        private void OnClickFirstButton(ArenaTeamData data)
        {
            UIController.Instance.OpenWindow<ArenaFirstInfoUI>(new ArenaFirstInfoUIProperties(data, null));
        }

        private void OnClickRuleInfo()
        {
            UIController.Instance.OpenWindow<ArenaRuleUI>(new ArenaRuleUIProperties(0));
        }


        private void OnClickRecordBtn()
        {
            if (this._arenaInfo == null) return;
            UIController.Instance.OpenWindow<ArenaRecordUI>();
        }
        private void OnClickExChangeBtn()
        {
            if (this._arenaInfo == null) return;
            UIController.Instance.ShowPanel<ArenaExShopUI>();
        }
        private void OnBeatOpponent(ArenaTeamData data)
        {
            if (this._arenaInfo == null) return;
            if (this._arenaInfo.BattleTimesLeft <= 0)
            {
                OnAddBattleTime();
                return;
            }
            //在竞技场前3阶段（包括第3阶段），如果当前阵容不是最强的，提示玩家阵容不是最强的，询问是否布阵，点击确定，跳转竞技场布阵页签。点击取消，开始战斗
            if (GuideManager.InForceGuide == false && this._arenaInfo.ArenaStage <= 3 && Player.FightManager.FormationController.isNeedRecommendedFormation)
            {
                if (!Player.FightManager.FormationController.IsBestClassicFormation(FightType.ARENA, false, out string info))
                {
                    UIController.Instance.OpenWindow<ConfirmBoxCheckUI>(new ConfirmBoxCheckUIProperties("经理，您派出的球员不是最强阵容，强烈建议您调整阵容", () =>
                    {
                        EventManager.Instance.Dispatch(EventID.OnClickArenaPadGotoFormationPad);
                    }, () =>
                    {
                        OnFight(data);
                    }, !Player.FightManager.FormationController.isNeedRecommendedFormation, "不再提醒推荐", (bool isCheck) => { Player.FightManager.FormationController.isNeedRecommendedFormation = !isCheck; }));
                    return;
                }
            }
            OnFight(data);
        }
        private void OnFight(ArenaTeamData data)
        {
            NetworkManager.Instance.BeatOpponent(data.Id, (resp) =>
            {

                Player.BattleManager.battleResponse = resp;
                Player.BattleManager.AddArenaInfo(resp.Info);
                Player.BattleManager.arenaTeamData = data;

                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.ArenaUI;
                Player.BattleManager.SetFightInfo(FightType.ARENA, resp.Fight);
                Player.BattleManager.StartPlayFight();

                bool isMineAway = resp.Fight.Teams.Away.TeamId == Player.GbId;
                CbaLogManager.Instance.AddLog(1005, resp.BattleWin ? 1 : 0, isMineAway ? resp.Fight.Teams.Away.Strength : resp.Fight.Teams.Home.Strength, !isMineAway ? resp.Fight.Teams.Away.Strength : resp.Fight.Teams.Home.Strength);
            });
        }

        private void OnAddBattleTime()
        {
            if (this._arenaInfo.BattleTimesLeft >= GameConst.ArenaFreeTimes)
            {
                Tips.PopTips("挑战次数已满");
                return;
            }

            string message = string.Format("增加<color=#2A874B>{0}</color>次挑战", GameConst.ArenaBuyAddTimes);
            Action action = () =>
            {
                NetworkManager.Instance.AddBattleTimes(resp =>
                {
                    if (resp.Succeed)
                    {
                        Tips.PopTips("购买次数成功");
                        this._arenaInfo.BattleTimesLeft += GameConst.ArenaBuyAddTimes;
                        this._arenaInfo.RefreshTimesLeft += GameConst.ArenaBuyAddRefreshTimes;
                        this.myTzcs.text = _arenaInfo.BattleTimesLeft.ToString();
                        this.refreshTime.text = this._arenaInfo.RefreshTimesLeft.ToString();

                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicArena, "/lefttime");
                        node.AddValue(1);
                        foreach (ArenaOpponentItem item in opponentItems)
                        {
                            node.IsRed(item.ChallengeButton.transform.Find("DotNodeImg"));
                        }

                        Anim.DoTextAnim(this.myTzcs);
                        Anim.DoTextAnim(this.refreshTime);
                    }
                    else
                    {
                        Tips.PopTips("购买次数今日已经用完");
                    }
                });
            };

            Utils.GameItem.GameItem priceItem = GameItemUtils.CreateGameItem(GameItemType.Resource, 1, Mathf.FloorToInt(GameConst.ArenaBuyTimesDiamond * Mathf.Pow(1.5f, this._arenaInfo.BattleTimesBuy)));

            UIController.Instance.OpenWindow<BuySthUI>(new BuySthUIProperty(message, priceItem, action));
        }

        private void _freshMoreRank(List<ArenaRankInfo> rankDataList)
        {
            _showMoreRank(true);
            osa.SetItems(rankDataList);
            osa.InitAnim();
            osa.PlayAnim();

        }
        private void OnMoreRank()
        {

            if (this._topRanks == null)
                return;

            //  if(this._rankDataList.Count <= 3)
            // return;

            NetworkManager.Instance.fetchMoreArenRank(resp =>
            {
                _freshMoreRank(ProtoUtils.UnPackRepeatedField<ArenaRankInfo>(resp.Ranks));
            });
        }


        private void _showMoreRank(bool more)
        {

            this.rankDataPanel.gameObject.SetActive(!more);

            osa.gameObject.SetActive(more);

            //this.moreRankBtn.gameObject.SetActive(!more);
            opponentPanel.SetActive(!more);

            this.rollUpRankBtn.gameObject.SetActive(more);
        }
        private void OnRollUpRank()
        {

            _showMoreRank(false);

            Anim.HideOpponenets();
            Anim.ShowOpponents();
        }

        private void OnRewardsPreview()
        {
            if (this._arenaInfo == null || this._arenaInfo.DailyClaim == true)
            {
                UIController.Instance.OpenWindow<ArenaRewardsUI>();
                return;
            }
            NetworkManager.Instance.CollectArenaDailyAward((_) =>
            {
                this._arenaInfo.DailyClaim = true;
                RefreshGiftState();
                ArenaRewardConfig arenaRewardConfig = null;
                foreach (ArenaRewardConfig arenaRewardConfigi in Configs.ArenaReward.GetConfigList())
                {
                    if (arenaRewardConfigi.Type == (int)ArenaStageRewardType.Daily && arenaRewardConfigi.Stage == this._arenaInfo.ArenaStage)
                    {
                        arenaRewardConfig = arenaRewardConfigi;
                        break;
                    }
                };
                if (arenaRewardConfig != null)
                {
                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(arenaRewardConfig.Reward).ToList(), null, "获得竞技场每日奖励");
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                }
                else
                {
                    Debug.LogWarning("ArenaPad , OnRewardsPreview , arenaRewardConfig == null");
                }

            });
        }
        [SerializeField] private LoopAnim giftLoopAnim;
        private void RefreshGiftState()
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicArena, "/DailyRewards");
            if (this._arenaInfo == null || this._arenaInfo.DailyClaim == true)
            {
                giftLoopAnim.ClearLockShake();
                node.AddValue(-1);
            }
            else
            {
                node.AddValue(1);
                giftLoopAnim.LockShake();
            }
            node.IsRed(rewardsPreBtn.transform.Find("DotNodeImg"));
            this.myJjb.text = Player.PackageManager.GetGoodsNumber(400501).ToString();
        }

        [SerializeField] private TMP_Text leftTimeText;
        private void UpdateLeftTime()
        {
            if (this._arenaInfo == null)
            {
                leftTimeText.text = "";
                return;
            }

            int leftSec = (int)(this._arenaInfo.EndTime - Utils.DataConvUtil.ServerTime);
            if (leftSec <= 0)
            {
                leftTimeText.text = "";
            }
            else
            {
                if (leftSec <= 86400)
                {
                    leftTimeText.text = "比赛重置 <color=#fcd13d>{0}</color>".SafeFormat(TimeUtils.FormatLeftTimeWithHour(leftSec));
                }
                else
                {
                    leftTimeText.text = "比赛重置 <color=#fcd13d>{0}</color>".SafeFormat(TimeUtils.FormatLeftTimeWithDayCn(leftSec));
                }
            }
        }


        private void CheckNewSeason()
        {
            if (arenaInfoResponse == null) return;
            if (arenaInfoResponse.JoinSeason == false) return;
            arenaInfoResponse.JoinSeason = false;
            List<Utils.GameItem.GameItem> gameItemList = new();
            foreach (ArenaRewardConfig arenaRewardConfigi in Configs.ArenaReward.GetConfigList())
            {
                if (arenaRewardConfigi.Type == (int)ArenaStageRewardType.Promote && arenaRewardConfigi.Stage <= arenaInfoResponse.Info.ArenaStage)
                {
                    List<Utils.GameItem.GameItem> gameItemListi = GameItemUtils.CreateGameItems(arenaRewardConfigi.Reward).ToList();
                    gameItemList.AddRange(gameItemListi);
                }
            };
            if (gameItemList.Count < 0) return;
            gameItemList = GameItemUtils.MergeGameItemList(gameItemList);

            var propertiesBig = new BigBoxUIProperties(() =>
            {
                var properties = new InventoryObtainedUIProperties(gameItemList);
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            }, "获得竞技场赛季重置奖励");
            UIController.Instance.OpenWindow<BigBoxUI>(propertiesBig);
        }

        [SerializeField] private BabuButton arenaMoneyBtn = null;
        private void OnClickArenaMoneyBtn(BabuButton _)
        {
            OnClickExChangeBtn();
        }

    }
}