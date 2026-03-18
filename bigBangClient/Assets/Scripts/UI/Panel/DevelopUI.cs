using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Babu;
using Babu.SDK;
using BigBang.Battle;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;
using static BigBang.Battle.ShootUI;
using static BigBang.ClassicManager;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class DevelopUI : AWindowController
    {
        private long lastPopTipTime = 0;

        [SerializeField] private Button CloseBtn;
        [SerializeField] private TMP_InputField InputText;
        [SerializeField] private TMP_InputField inputCountryId;
        [SerializeField] private Button AddDiamondBtn;
        [SerializeField] private Button AddMoneyBtn;
        [SerializeField] private Button AddGoodsBtn;
        [SerializeField] private Button AddBigExpBtn;
        [SerializeField] private Button AddTeamExpBtn;
        [SerializeField] private Button ClearInviteCDBtn;
        [SerializeField] private Button ClearBigbangCDBtn;
        [SerializeField] private Button SetTrainLevelBtn;
        [SerializeField] private Button SetTrain100Btn;
        [SerializeField] private Button SetExpUnitBtn;
        [SerializeField] private Button DeleteAccountBtn;
        [SerializeField] private Button GetAllPropBtn;
        [SerializeField] private Button SendSysEmailBtn;
        [SerializeField] private Button DailyRefreshBtn;
        [SerializeField] private Button MonthSignBtn;
        [SerializeField] private Button setChallengeID;
        [SerializeField] private Button clearGuideBtn;
        [SerializeField] private Button unLockAllSystemBtn;
        [SerializeField] private Button BattleBtn;
        [SerializeField] private Button BattleWinBtn;
        [SerializeField] private Button BattleLoseBtn;
        [SerializeField] private Button SetFpsBtn;

        [SerializeField] private DevelopGoodsAdapter goodsListAdapter;

        [SerializeField] private Button showGoodsBtn;
        [SerializeField] private GameObject GoodsPanel;

        [SerializeField] private Button goodsTypeAll;
        [SerializeField] private Button goodsType0;
        [SerializeField] private Button goodsType1;
        [SerializeField] private Button goodsType2;
        [SerializeField] private Button goodsType3;
        [SerializeField] private Button goodsType4;
        [SerializeField] private Button goodsType5;
        [SerializeField] private Button goodsType6;
        [SerializeField] private Button goodsType7;
        [SerializeField] private Button goodsType8;
        [SerializeField] private Button BackType;
        [SerializeField] private GameObject EntryPanel;


        [SerializeField] private Button SearchBtn;
        [SerializeField] private TMP_InputField SearchField;

        [SerializeField] private Button SuperCardBtn; //超级卡片

        [SerializeField] private Button cardBattleBtn; //卡牌战斗

        [SerializeField] private Button nftBtn;
        [SerializeField] private Button JumpOnePveBtn;
        [SerializeField] private Button GuidefightBtn;
        [SerializeField] private Button ShootBtn;
        [SerializeField] private Button Fight1Btn;

        [SerializeField] private Button AccountLogout;
        [SerializeField] private Button GetYellow;
        [SerializeField] private Button LogErrorBtn;
        [SerializeField] private Button AutoRunBattleBtn;
        [SerializeField] private Button TestGuideTalkBtn;
        [SerializeField] private Button ServerFinishGuideBtn;
        [SerializeField] private Button Guide2UIBtn;

        [SerializeField] private Button getUI3Btn = null;
        [SerializeField] private Button getUI60Btn = null;

        [SerializeField] private Button enterFightBtn = null;

        [SerializeField] private Button shopReviewBtn = null;

        [SerializeField] public Button hotfixAndServerPanelBtn = null;
        [SerializeField] private Button hundredBtn = null;
        [SerializeField] private Button hundredFightBtn = null;
        [SerializeField] private Button hundredBattleEndBtn = null;
        [SerializeField] private Button shootEndUploadBtn = null;
        [SerializeField] private Button shootEndBtn = null;
        [SerializeField] private Button leagleHomeBtn = null;
        [SerializeField] private Button redEnvelopeTipBtn = null;
        [SerializeField] private Button hundredGuessBtn = null;
        [SerializeField] private Button leagueEndRewardBtn = null;
        [SerializeField] private Button hundredDataBtn = null;

        [SerializeField] private Button miGuLibBtn = null;

        private static DevelopUI _inst;

        private void Awake()
        {
            _inst = this;
        }

        public static DevelopUI Instance
        {
            get { return _inst; }
            private set { }
        }

        protected override void AddListeners()
        {
            base.AddListeners();
            AddDiamondBtn.onClick.AddListener(OnClickAddDiamond);
            AddMoneyBtn.onClick.AddListener(OnClickAddMoney);
            AddGoodsBtn.onClick.AddListener(OnClickAddGoods);
            AddBigExpBtn.onClick.AddListener(OnClickAddExp);
            AddTeamExpBtn.onClick.AddListener(OnClickAddTeamExp);
            ClearInviteCDBtn.onClick.AddListener(OnClickClearInviteCD);
            ClearBigbangCDBtn.onClick.AddListener(OnClickClearBigbangCD);
            SetTrainLevelBtn.onClick.AddListener(OnClickSetTrainLevel);
            SetTrain100Btn.onClick.AddListener(OnClickSetTrain100);
            SetExpUnitBtn.onClick.AddListener(OnClickSetExpUnit);
            DeleteAccountBtn.onClick.AddListener(OnClickDeleteAccount);
            GetAllPropBtn.onClick.AddListener(OnGetAllProp);
            SendSysEmailBtn.onClick.AddListener(OnSendSysEmail);
            DailyRefreshBtn.onClick.AddListener(OnClickDailyRefresh);
            MonthSignBtn.onClick.AddListener(OnClickMonthSignRefresh);
            setChallengeID.onClick.AddListener(OnClickSetChallengeID);
            clearGuideBtn.onClick.AddListener(OnClearGuide);
            unLockAllSystemBtn.onClick.AddListener(OnClickUnlockAll);

            showGoodsBtn.onClick.AddListener(OnClickShowGoodsBtn);
            goodsTypeAll.onClick.AddListener(OnClickGoodsTypeAll);
            goodsType0.onClick.AddListener(OnClickGoodsType0);
            goodsType1.onClick.AddListener(OnClickGoodsType1);
            goodsType2.onClick.AddListener(OnClickGoodsType2);
            goodsType3.onClick.AddListener(OnClickGoodsType3);
            goodsType4.onClick.AddListener(OnClickGoodsType4);
            goodsType5.onClick.AddListener(OnClickGoodsType5);
            goodsType6.onClick.AddListener(OnClickGoodsType6);
            goodsType7.onClick.AddListener(OnClickGoodsType7);
            goodsType8.onClick.AddListener(OnClickGoodsType8);
            BackType.onClick.AddListener(OnClickBackType);

            SearchBtn.onClick.AddListener(OnClickSearchBtn);

            SuperCardBtn.onClick.AddListener(OnClickSuperCardBtn);
            cardBattleBtn.onClick.AddListener(OnClickCardBattleBtn);
            nftBtn.onClick.AddListener(OnClickNftBtn);
            JumpOnePveBtn.onClick.AddListener(OnClickJumpOnePveBtn);
            GuidefightBtn.onClick.AddListener(OnClickGuidefightBtn);
            ShootBtn.onClick.AddListener(OnClickShootBtn);
            Fight1Btn.onClick.AddListener(OnClickFight1Btn);

            AccountLogout.onClick.AddListener(OnClickAccountLogout);
            GetYellow.onClick.AddListener(OnClickGetYellow);
            LogErrorBtn.onClick.AddListener(OnClickLogErrorBtn);
            AutoRunBattleBtn.onClick.AddListener(OnClickAutoRunBattleBtn);
            TestGuideTalkBtn.onClick.AddListener(OnClickTestGuideTalkBtn);
            ServerFinishGuideBtn.onClick.AddListener(OnClickServerFinishGuideBtn);
            Guide2UIBtn.onClick.AddListener(OnClickGuide2UIBtn);

            getUI3Btn.onClick.AddListener(OnClickGetUI3Btn);
            getUI60Btn.onClick.AddListener(OnClickGetUI60Btn);

            SetFpsBtn.onClick.AddListener(OnClickSetFpsBtn);

            CloseBtn.onClick.AddListener(OnClickClose);
            enterFightBtn.onClick.AddListener(OnEnterFight);
            shopReviewBtn.onClick.AddListener(OnShopReview);

            hotfixAndServerPanelBtn.onClick.AddListener(OnHotfixAndServerPanelBtn);
            hundredBtn.onClick.AddListener(OnHundredBtn);
            hundredFightBtn.onClick.AddListener(OnHundredFightBtn);
            hundredBattleEndBtn.onClick.AddListener(OnHundredBattleEndBtn);
            shootEndUploadBtn.onClick.AddListener(OnShootEndUploadBtn);
            shootEndBtn.onClick.AddListener(OnShootEndBtn);
            leagleHomeBtn.onClick.AddListener(OnLeagleHomeBtn);
            redEnvelopeTipBtn.onClick.AddListener(OnRedEnvelopeTipBtn);
            hundredGuessBtn.onClick.AddListener(OnHundredGuessBtn);
            leagueEndRewardBtn.onClick.AddListener(OnLeagueEndRewardBtn);
            hundredDataBtn.onClick.AddListener(OnHundredDataBtn);

            miGuLibBtn.onClick.AddListener(OnMiGuLibBtn);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            AddDiamondBtn.onClick.RemoveListener(OnClickAddDiamond);
            AddMoneyBtn.onClick.RemoveListener(OnClickAddMoney);
            AddGoodsBtn.onClick.RemoveListener(OnClickAddGoods);
            AddBigExpBtn.onClick.RemoveListener(OnClickAddExp);
            AddTeamExpBtn.onClick.RemoveListener(OnClickAddTeamExp);
            ClearInviteCDBtn.onClick.RemoveListener(OnClickClearInviteCD);
            ClearBigbangCDBtn.onClick.RemoveListener(OnClickClearBigbangCD);
            SetTrainLevelBtn.onClick.RemoveListener(OnClickSetTrainLevel);
            SetTrain100Btn.onClick.RemoveListener(OnClickSetTrain100);
            SetExpUnitBtn.onClick.RemoveListener(OnClickSetExpUnit);
            DeleteAccountBtn.onClick.RemoveListener(OnClickDeleteAccount);
            GetAllPropBtn.onClick.RemoveListener(OnGetAllProp);
            SendSysEmailBtn.onClick.RemoveListener(OnSendSysEmail);
            DailyRefreshBtn.onClick.RemoveListener(OnClickDailyRefresh);
            MonthSignBtn.onClick.RemoveListener(OnClickMonthSignRefresh);
            setChallengeID.onClick.RemoveListener(OnClickSetChallengeID);
            clearGuideBtn.onClick.RemoveListener(OnClearGuide);
            unLockAllSystemBtn.onClick.RemoveListener(OnClickUnlockAll);
            showGoodsBtn.onClick.RemoveListener(OnClickShowGoodsBtn);
            goodsTypeAll.onClick.RemoveListener(OnClickGoodsTypeAll);
            goodsType0.onClick.RemoveListener(OnClickGoodsType0);
            goodsType1.onClick.RemoveListener(OnClickGoodsType1);
            goodsType2.onClick.RemoveListener(OnClickGoodsType2);
            goodsType3.onClick.RemoveListener(OnClickGoodsType3);
            goodsType4.onClick.RemoveListener(OnClickGoodsType4);
            goodsType5.onClick.RemoveListener(OnClickGoodsType5);
            goodsType6.onClick.RemoveListener(OnClickGoodsType6);
            goodsType7.onClick.RemoveListener(OnClickGoodsType7);
            goodsType8.onClick.RemoveListener(OnClickGoodsType8);
            BackType.onClick.RemoveListener(OnClickBackType);
            SearchBtn.onClick.RemoveListener(OnClickSearchBtn);
            SuperCardBtn.onClick.RemoveListener(OnClickSuperCardBtn);
            cardBattleBtn.onClick.RemoveListener(OnClickCardBattleBtn);
            nftBtn.onClick.RemoveListener(OnClickNftBtn);
            JumpOnePveBtn.onClick.RemoveListener(OnClickJumpOnePveBtn);
            GuidefightBtn.onClick.RemoveListener(OnClickGuidefightBtn);
            ShootBtn.onClick.RemoveListener(OnClickShootBtn);
            Fight1Btn.onClick.RemoveListener(OnClickFight1Btn);
            AccountLogout.onClick.RemoveListener(OnClickAccountLogout);
            GetYellow.onClick.RemoveListener(OnClickGetYellow);
            LogErrorBtn.onClick.RemoveListener(OnClickLogErrorBtn);
            AutoRunBattleBtn.onClick.RemoveListener(OnClickAutoRunBattleBtn);
            TestGuideTalkBtn.onClick.RemoveListener(OnClickTestGuideTalkBtn);
            ServerFinishGuideBtn.onClick.RemoveListener(OnClickServerFinishGuideBtn);
            Guide2UIBtn.onClick.RemoveListener(OnClickGuide2UIBtn);

            getUI3Btn.onClick.RemoveListener(OnClickGetUI3Btn);
            getUI60Btn.onClick.RemoveListener(OnClickGetUI60Btn);

            SetFpsBtn.onClick.RemoveListener(OnClickSetFpsBtn);

            CloseBtn.onClick.RemoveListener(OnClickClose);
            enterFightBtn.onClick.RemoveListener(OnEnterFight);
            shopReviewBtn.onClick.RemoveListener(OnShopReview);

            hotfixAndServerPanelBtn.onClick.RemoveListener(OnHotfixAndServerPanelBtn);
            hundredBtn.onClick.RemoveListener(OnHundredBtn);
            hundredFightBtn.onClick.RemoveListener(OnHundredFightBtn);
            hundredBattleEndBtn.onClick.RemoveListener(OnHundredBattleEndBtn);
            shootEndUploadBtn.onClick.RemoveListener(OnShootEndUploadBtn);
            shootEndBtn.onClick.RemoveListener(OnShootEndBtn);
            leagleHomeBtn.onClick.RemoveListener(OnLeagleHomeBtn);
            redEnvelopeTipBtn.onClick.RemoveListener(OnRedEnvelopeTipBtn);
            hundredGuessBtn.onClick.RemoveListener(OnHundredGuessBtn);
            leagueEndRewardBtn.onClick.RemoveListener(OnLeagueEndRewardBtn);
            hundredDataBtn.onClick.RemoveListener(OnHundredDataBtn);

            miGuLibBtn.onClick.RemoveListener(OnMiGuLibBtn);
        }

        private void OnMiGuLibBtn()
        {
            //17_10001005_648_0171726739596571_1726802606
            //Debug.Log(ToBase62(17L) + "_" + ToBase62(10001005L) + "_" + ToBase62(648L) + "_" + ToBase62(10171726739596571L) + "_" + ToBase62(1726802606L));
            Debug.Log(ToBase62(999L) + "_" + ToBase62(99999999L) + "_" + ToBase62(999L) + "_" + ToBase62(19991726739596571L) + "_" + ToBase62(1926802606L));
        }
        private readonly string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string ToBase62(long num)
        {
            StringBuilder result = new();
            long baseLength = 62;
            while (num > 0)
            {
                result.Append(chars[(int)(num % baseLength)]);
                num /= baseLength;
            }
            return string.Concat(result.ToString().Reverse());
        }

        private void OnHundredDataBtn()
        {
            UIController.Instance.CloseWindow<DevelopUI>();
            UIController.Instance.ShowPanel<HundredDataUI>();
        }

        private void OnLeagueEndRewardBtn()
        {
            LeagueHistoryData leagueHistoryData = new();
            leagueHistoryData.LeagueLevel = 1;
            leagueHistoryData.Rank = 1;
            leagueHistoryData.TopCards = new TeamTopCards();
            leagueHistoryData.TopCards.PointKing = new TeamTopCardData();
            leagueHistoryData.TopCards.PointKing.LeagueRank = 1;
            leagueHistoryData.TopCards.AssistKing = new TeamTopCardData();
            leagueHistoryData.TopCards.AssistKing.LeagueRank = 1;
            leagueHistoryData.TopCards.ReboundKing = new TeamTopCardData();
            leagueHistoryData.TopCards.ReboundKing.LeagueRank = 1;
            leagueHistoryData.TopCards.StealKing = new TeamTopCardData();
            leagueHistoryData.TopCards.StealKing.LeagueRank = 1;
            leagueHistoryData.TopCards.BlockKing = new TeamTopCardData();
            leagueHistoryData.TopCards.BlockKing.LeagueRank = 1;

            UIController.Instance.OpenWindow<LeagueEndRewardUI>(new LeagueEndRewardUIProperties(leagueHistoryData));
        }

        private void OnHundredGuessBtn()
        {
            UIController.Instance.OpenWindow<HundredGuessUI>(new HundredGuessUIProperties(false));
        }
        private void OnRedEnvelopeTipBtn()
        {
            UIController.Instance.OpenWindow<DragonYearRedEnvelopeTipUI>();
            UIController.Instance.CloseWindow<DevelopUI>();
        }

        private void OnLeagleHomeBtn()
        {
            UIController.Instance.CloseWindow<DevelopUI>();
            UIController.Instance.ShowPanel<MatchHomeUI>();
        }
        private void OnShootEndUploadBtn()
        {
            if (string.IsNullOrWhiteSpace(InputText.text))
            {
                Debug.Log("输入有误");
                return;
            }
            int input = GetInputInt();
            NetworkManager.Instance.GetShootGameReward(0, input, resp =>
            {
                Tips.PopTips("上报分数成功 : input = " + input);
            });
        }

        private void OnShootEndBtn()
        {
            if (string.IsNullOrWhiteSpace(InputText.text))
            {
                Debug.Log("输入有误");
                return;
            }
            string[] strArr = InputText.text.Split('|');
            ShootEndData shootEndData = new();
            shootEndData.oldScore = int.Parse(strArr[0]);
            shootEndData.newScore = int.Parse(strArr[1]);
            shootEndData.ShootUIEnterPos = ShootUIEnterPos.Jump;
            UIController.Instance.OpenWindow<ShootEndRewardUI>(new ShootEndRewardUIProperties(shootEndData));
        }
        private void OnHundredBattleEndBtn()
        {
            bool isLeftWin = Utility.GetRandomBool();
            int leftScore = isLeftWin ? 100 : Utility.GetRandomInt(60, 80);
            int rightScore = isLeftWin ? Utility.GetRandomInt(60, 80) : 100;
            UIController.Instance.OpenWindow<HundredBattleEndUI>(new HundredBattleEndUIProperties(null, null, leftScore, rightScore, null));
        }

        private void OnHundredFightBtn()
        {
            if (string.IsNullOrWhiteSpace(InputText.text))
            {
                Debug.Log("输入有误");
                return;
            }
            UIController.Instance.CloseWindow<DevelopUI>();
            HundredManager.Instance.GetFight(InputText.text, (FightInfo fightInfo) =>
            {
                UIController.Instance.CloseWindow<HundredTeamDetailUI>();
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.HundredTeamDetailUI;
                Player.BattleManager.SetHundredFightInfo(FightType.Hundred, fightInfo, 0, HundredProgress.Fight1);
                Player.BattleManager.StartPlayFight();
            });
        }

        private void OnHundredBtn()
        {
            UIController.Instance.CloseWindow<DevelopUI>();
            UIController.Instance.ShowPanel<HundredHomeUI>(new HundredHomeUIProperties(true));
        }
        private void OnHotfixAndServerPanelBtn()
        {
            UIController.Instance.OpenWindow<TestServerUI>();
        }

        private void OnShopReview()
        {
            GameManager.Instance.TrigIosShopReview();
        }

        private void OnEnterFight()
        {
            int input = GetInputInt();
            ClassicTeamData classicTeamData = new();
            classicTeamData.challengeClubConfig = Configs.ChallengeClub.GetConfig(input);
            classicTeamData.passData = new();
            classicTeamData.passData.Id = classicTeamData.challengeClubConfig.Id;
            classicTeamData.passData.Stars.Add(0);
            classicTeamData.passData.Stars.Add(0);
            classicTeamData.passData.Stars.Add(0);
            classicTeamData.passData.ChallengeTimes = 0;
            classicTeamData.isOpen = true;
            EventManager.Instance.Dispatch(EventID.ClassicCountryUIOnClickClallengeButton, classicTeamData);
            UIController.Instance.OpenWindow<ClassicEnterFightUI>(new ClassicEnterFightUIProperties(classicTeamData));
        }

        private void OnClickSetFpsBtn()
        {
            int input = GetInputInt();
            input = Utility.KeepInRange(input, 15, 360);
            Application.targetFrameRate = input;
            Tips.PopTips("每秒最大帧数设置为：" + input);
        }

        private void OnClickGetUI3Btn()
        {
            string reward3Str = "2:400102:600|2:304001:50|1:2:700000";
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(reward3Str).ToList();
            var properties = new InventoryObtainedUIProperties(gameItemList);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);// 打开通用收益界面
        }

        private void OnClickGetUI60Btn()
        {
            string reward3Str = "2:400102:600|2:304001:50|1:2:700000";
            string reward60Str = "";
            for (int i = 0; i < 20; i++)
            {
                if (i != 0)
                {
                    reward60Str += "|";
                }
                reward60Str += reward3Str;
            }
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(reward60Str).ToList();
            var properties = new InventoryObtainedUIProperties(gameItemList);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);// 打开通用收益界面
        }


        private void OnClickGuide2UIBtn()
        {
            List<ModuleDefineConfig> moduleDefineConfigList = Configs.ModuleDefine.GetConfigList().Where(item => item.Conversation != 0).ToList();
            ModuleDefineConfig moduleDefineConfig = moduleDefineConfigList[Utility.GetRandomInt(0, moduleDefineConfigList.Count - 1)];
            var properties = new Guide2UIProperties(Configs.GuideDialogue.GetConfig(moduleDefineConfig.Conversation), null);
            UIController.Instance.OpenWindow<Guide2UI>(properties);
        }
        private void OnClickServerFinishGuideBtn()
        {
            //将多个放在一条消息内一起发送，没问题
            //GuideManager.Finish(new List<GuideID>() { GuideID.test1, GuideID.test2 });

            //将多个放在不同消息内分别发送，只存了一个
            GuideManager.Finish(GuideID.test1);
            GuideManager.Finish(GuideID.test2);
        }
        private void OnClickTestGuideTalkBtn()
        {
            UIController.Instance.OpenWindow<GuideTalkUI>(new GuideTalkUIProperties("测试一下GuideTalk界面，此界面的文字慢慢出现时不可点击，文字出现结束后出现点击提示，此时点击后关闭界面，并调用回调函数"));
        }

        private void OnClickGoodsTypeAll()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList());
        }
        private void OnClickGoodsType0()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 0; }));
        }
        private void OnClickGoodsType1()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 1; }));
        }
        private void OnClickGoodsType2()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 2; }));
        }
        private void OnClickGoodsType3()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 3; }));
        }
        private void OnClickGoodsType4()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 4; }));
        }
        private void OnClickGoodsType5()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 5; }));
        }
        private void OnClickGoodsType6()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 6; }));
        }
        private void OnClickGoodsType7()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type == 7; }));
        }
        private void OnClickGoodsType8()
        {
            goodsListAdapter.SetItems(Configs.Goods.GetConfigList().FindAll((item) => { return item.Type >= 8; }));
        }

        private void OnClickSearchBtn()
        {
            if ("" == SearchField.text)
                return;

            goodsListAdapter.SearchItem(SearchField.text);
        }

        private void OnClickSuperCardBtn()
        {

            var rewards = GameItemUtils.CreateGameItems(Configs.FirstChargeReward.GetConfigList().First().Reward);
            var cards = rewards.Where(item => item.Type == GameItemType.Card).ToList();
            var gameItems = rewards.Where(item => item.Type != GameItemType.Card).ToList();
            foreach (var item in rewards.Where(item => item.Type == GameItemType.Card && Player.CardManager.GetCard(item.Id) != null))
            {
                var goodsCfg = Configs.Goods.GetConfig(Player.CardManager.GetCard(item.Id).Config.PiecesId);
                gameItems.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, goodsCfg.Id, goodsCfg.Param1));
            }
            UIController.Instance.OpenWindow<SuperCardUI>(new SuperCardUIProperties(false, () =>
            {
                var properties = new InventoryObtainedUIProperties(gameItems, null);
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            }, cards.Select(item => Configs.CardModel.GetConfig(item.Id)).ToList<CardModelConfig>()));
        }

        private void OnClickCardBattleBtn()
        {
            UIController.Instance.CloseWindow<DevelopUI>();


            AudioManager.Instance.PlaySound(AudioNames.BTN_1);

            Player.ChallengeManager.ChallengeStart((resp) =>
            {

                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Debug;
                Player.BattleManager.SetFightInfo(FightType.PVE, resp.Fight);
                Player.BattleManager.StartPlayFight();

            });

        }

        private void OnClickNftBtn()
        {
            UIController.Instance.ShowPanel<NFTChinaUI>();
            UIController.Instance.CloseWindow<DevelopUI>();
        }

        private void OnClickJumpOnePveBtn()
        {
            Player.ChallengeManager.ChallengeStart((resp) =>
            {
                Debug.Log("跳过来一场PVE战斗：" + (resp.Fight.Result.Win ? "失败" : "胜利"));
            });
        }
        private void OnClickGuidefightBtn()
        {
            NetworkManager.Instance.GuideChallenge(response =>
            {
                UIController.Instance.OpenWindow<MatchLoadingUI>(new MatchLoadingUIProperties(() =>
                {
                    Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Guide;
                    Player.BattleManager.SetFightInfo(FightType.PVE, response.Fight);
                    Player.BattleManager.StartPlayFight();
                }));
            });
        }
        private void OnClickShootBtn()
        {
            UIController.Instance.ShowPanel<ShootUI>(new ShootUIProperties(ShootUIEnterPos.Jump));
            UIController.Instance.CloseWindow<DevelopUI>();
        }
        private void OnClickFight1Btn()
        {
            var clubList = Configs.ChallengeClub.GetConfigList().FindAll(P => P.Country == int.Parse(inputCountryId.text));

            StartCoroutine(autobattle(clubList));
        }

        System.Collections.IEnumerator autobattle(List<ChallengeClubConfig> clubList)
        {
            for (var index = 0; index < clubList.Count; index++)
            {
                NetworkManager.Instance.ChallengeStart(clubList[index].Id, (resp) =>
                {
                    var str = "打完了第" + (index + 1).ToString() + "关，";
                    if (resp.Succeed)
                    {
                        str += "赢了，打了" + resp.Stars.Sum().ToString() + "星";
                    }
                    else
                    {
                        str += "输了,日你妈";
                    }

                    Tips.PopTips(str);
                });

                yield return new WaitForSeconds(1);
            }

        }

        private void OnClickAccountLogout()
        {
            UIController.Instance.CloseWindow<DevelopUI>();
            EventManager.Instance.Dispatch(EventID.QUICK_LOGIN_OUT);
        }

        private void OnClickGetYellow()
        {
            //deleted
        }
        private void OnClickLogErrorBtn()
        {
            Debug.LogError("在Develop界面手动触发了一个Debug.LogError");
        }
        private void OnClickAutoRunBattleBtn()
        {
            challengeClubConfigId = GetInputInt();
            if (challengeClubConfigId == 0 || Configs.ChallengeClub.GetConfig(challengeClubConfigId) == null)
            {
                Tips.PopTips("请输入challengeClub表的Id，来作为要打的第一关");
                return;
            }
            RunNextAutoBattle();
        }
        private int challengeClubConfigId = 0;
        private void RunNextAutoBattle()
        {
            NetworkManager.Instance.ChallengeStart(challengeClubConfigId, (resp) =>
            {
                ChallengeStartResponse challengeStartResponse = resp;
                if (resp.Succeed)
                {
                    ClassicManager.Instance.UpdatePassData(Configs.ChallengeClub.GetConfig(challengeClubConfigId).Country, resp.PassData);
                    if (resp.Stars[0] == 0)
                    {
                        Debug.LogWarning("遇到战斗失败，自动通过关卡结束，challengeClubConfigId = " + challengeClubConfigId);
                        RunAutoBattleEnd();
                        return;
                    }
                    Debug.Log("通过关卡，challengeClubConfigId = " + challengeClubConfigId);
                    if (Configs.ChallengeClub.GetConfigList()[^1].Id == challengeClubConfigId)
                    {
                        Debug.LogWarning("通关了全部关卡，自动通过关卡结束，challengeClubConfigId = " + challengeClubConfigId);
                        RunAutoBattleEnd();
                        return;
                    }
                    int index = 0;
                    foreach (var item in Configs.ChallengeClub.GetConfigList())
                    {
                        if (item.Id == challengeClubConfigId) { break; }
                        index++;
                    }
                    challengeClubConfigId = Configs.ChallengeClub.GetConfigList()[index + 1].Id;
                    Timer.Register(this.gameObject, 0.1f, () => { RunNextAutoBattle(); });
                }
                else
                {
                    Debug.LogWarning("遇到战斗失败，自动通过关卡结束，challengeClubConfigId = " + challengeClubConfigId);
                    RunAutoBattleEnd();
                }
            });
        }
        private void RunAutoBattleEnd()
        {
            Tips.PopTips("自动战斗结束");
        }

        private void OnClickBackType()
        {
            EntryPanel.gameObject.SetActive(true);
            GoodsPanel.gameObject.SetActive(false);
        }

        private void OnClickShowGoodsBtn()
        {
            EntryPanel.gameObject.SetActive(false);
            GoodsPanel.gameObject.SetActive(true);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            //goodsListAdapter.SetItems(Configs.Goods.GetConfigList());
            RefreshDefineStr();
        }

        [SerializeField] private TMP_Text defineBabu = null;
        [SerializeField] private TMP_Text defineHotFix = null;
        private void RefreshDefineStr()
        {
            defineBabu.text = DefineBabu.GetDefineStr();
            defineHotFix.text = DefineHotfix.GetDefineStr();
        }

        private void OnClickClose()
        {
            UIController.Instance.CloseWindow<DevelopUI>();
        }

        private void OnClearGuide()
        {
            Debug.Log("已改为服务器记录引导");
            //GuideManager.Clear();
        }

        public int GetInputInt()
        {
            if (string.IsNullOrWhiteSpace(InputText.text))
            {
                Debug.Log("输入有误");
                return 0;
            }
            try
            {
                float f = float.Parse(InputText.text);
                int num = (int)f;
                return num;
            }
            catch (System.Exception ex)
            {
                Debug.Log("输入有误");
                return 0;
            }
        }

        private void OnGetAllProp()
        {
            float count = GetInputInt();
            DevelopUI.Instance.SendCommand(DevelopCommand.AddAllGameItem, count.ToString());
        }

        private float GetInputFloat()
        {
            float f = float.Parse(InputText.text);
            return f;
        }

        private string GetInputString()
        {
            return InputText.text;
        }


        private void OnClickAddDiamond()
        {
            int count = GetInputInt();
            SendCommand(DevelopCommand.AddGameItem,
                ((int)GameItemType.Resource).ToString(), ResourceId.Diamond.ToString(), count.ToString());
        }

        private void OnClickAddMoney()
        {
            int count = GetInputInt();
            SendCommand(DevelopCommand.AddGameItem,
                ((int)GameItemType.Resource).ToString(), ResourceId.Money.ToString(), count.ToString());
        }

        private void OnClickAddGoods()
        {
            string param = GetInputString();
            var list = param.Split(':');
            int id = int.Parse(list[0]);
            int count = int.Parse(list[1]);
            SendCommand(DevelopCommand.AddGameItem,
                ((int)GameItemType.Goods).ToString(), id.ToString(), count.ToString());
        }

        private void OnClickAddExp()
        {
            int count = GetInputInt();
            SendCommand(DevelopCommand.AddGameItem,
                ((int)GameItemType.Resource).ToString(), ResourceId.TrainExpMin.ToString(), count.ToString());
        }
        private void OnClickAddTeamExp()
        {
            int count = GetInputInt();
            SendCommand(DevelopCommand.AddGameItem,
                ((int)GameItemType.Resource).ToString(), ResourceId.PlayerExp.ToString(), count.ToString());
        }

        private void OnClickClearInviteCD()
        {
            SendCommand(DevelopCommand.ClearInviteMatchCd);
        }

        private void OnClickClearBigbangCD()
        {
            SendCommand(DevelopCommand.ClearBigbangCd);
        }

        private void OnClickSetTrainLevel()
        {
            string param = GetInputString();
            var list = param.Split(':');
            int id = int.Parse(list[0]);
            long count = long.Parse(list[1]);
            SendCommand(DevelopCommand.SetTrainLevel, id.ToString(), count.ToString());
        }

        private void OnClickSetTrain100()
        {
            float time = 0;
            for (int i = 1; i <= 10; i++)
            {

                Debug.Log("SendCommand" + i);
                Babu.DelayTaskService.Instance.Run(this.gameObject, time, () =>
                {
                    SendCommand(DevelopCommand.SetTrainLevel, i.ToString(), "100");
                });
                time += 0.01f;
            }

        }

        private void OnClickSetExpUnit()
        {
            int unitId = GetInputInt();
            SendCommand(DevelopCommand.SetExpUnit, unitId.ToString());
        }

        private void OnClickDeleteAccount()
        {
            string param = GetInputString();
            if (param != "2021")
            {
                Tips.PopTips("error");
                return;
            }
            SendCommand(DevelopCommand.ClearAccount);
            PlayerPrefs.DeleteAll();
            SDKManager.Instance.CloseGame();
        }

        private void OnClickDailyRefresh()
        {
            SendCommand(DevelopCommand.DailyRefresh);
        }

        private void OnClickMonthSignRefresh()
        {
            SendCommand(DevelopCommand.MonthSignRefresh);
        }

        private void OnClickUnlockAll()
        {
            StartCoroutine(SendUlockAllCmd());
        }

        System.Collections.IEnumerator SendUlockAllCmd()
        {
            SendCommand(DevelopCommand.UnlockStrengthenBatch);
            yield return new WaitForSeconds(0.2f);
            SendCommand(DevelopCommand.UnlockWish);
            yield return new WaitForSeconds(0.2f);
            SendCommand(DevelopCommand.UnlockInviteMatch);
        }

        private void OnClickSetChallengeID()
        {
            int challengeID = GetInputInt();
            SendCommand(DevelopCommand.SetChallengeId, challengeID.ToString(), "", "");
        }

        public void SendCommand(DevelopCommand command, string param1 = "", string param2 = "", string param3 = "")
        {
            NetworkManager.Instance.SendCommand(command, param1, param2, param3, OnSendCommand);
        }

        private void OnSendCommand(DevelopResponse response)
        {
            switch (response.Command)
            {
                case DevelopCommand.AddGameItem:
                    AddGameItemSuccess(int.Parse(response.Param1), int.Parse(response.Param2),
                        int.Parse(response.Param3));
                    break;
                case DevelopCommand.ClearInviteMatchCd:
                    ClearInviteMatchCdSuccess();
                    break;
                case DevelopCommand.ClearBigbangCd:
                    ClearBigbangCDSuccess();
                    break;
                case DevelopCommand.SetTrainLevel:
                    SetTrainLevelSuccess(int.Parse(response.Param1), int.Parse(response.Param2));
                    break;
                case DevelopCommand.SetExpUnit:
                    SetExpUnitSuccess(int.Parse(response.Param1));
                    break;
                case DevelopCommand.DailyRefresh:
                    DailyRefreshSuccess();
                    break;
                case DevelopCommand.MonthSignRefresh:
                    break;
                case DevelopCommand.UnlockStrengthenBatch:
                    UnLockStrengthenBatch();
                    break;
                case DevelopCommand.UnlockWish:
                    break;
                case DevelopCommand.UnlockInviteMatch:
                    UnlockInviteMatch();
                    break;
                case DevelopCommand.SetChallengeId:
                    ChallengeIDSuccess();
                    break;
                default:
                    break;
            }
            var t = Utils.DataConvUtil.ServerTimeEx;
            if (t - lastPopTipTime > 1000)
            {
                Tips.PopTips("success!");
                lastPopTipTime = t;
            }
            return;
        }

        private void AddGameItemSuccess(int type, int id, int count)
        {

        }

        private void ChallengeIDSuccess()
        {
            // todo refresh
        }

        private void MonthSignRefreshSuccess()
        {
            // todo refresh
        }

        private void ClearInviteMatchCdSuccess()
        {
            Player.TrainManager.InviteMatchController.DevClear();
        }

        private void ClearBigbangCDSuccess()
        {
        }

        private void SetTrainLevelSuccess(int trainId, int level)
        {
            if (level <= 0) return;
            if (trainId <= 0)
            {
                foreach (var trainItem in Player.TrainManager.TrainList())
                {
                    trainItem.DevelopSetLevel(level);
                }
            }
            else
            {
                var train = Player.TrainManager.GetTrainItem(trainId);
                if (train != null)
                {
                    train.DevelopSetLevel(level);
                }
            }
        }

        private void SetExpUnitSuccess(int unitId)
        {
            Player.TrainManager.Exp.UnitId = unitId;
            EventManager.Instance.Dispatch(EventID.OnResourceChange);
        }

        private void UnLockStrengthenBatch()
        {
            //更改总Exp，解锁一键强化
            Player.TrainManager.TotalExp.UnitId = 7;
            Player.TrainManager.TotalExp.Value = 2;
        }

        private void DailyRefreshSuccess()
        {
            // todo refresh
        }

        private void OnSendSysEmail()
        {
            //OnClickClose();
            NetworkManager.Instance.SendTestSysEmail();
        }

        private void UnlockInviteMatch()
        {
            Player.TrainManager.InviteMatchController.Unlock();
        }
    }
}
