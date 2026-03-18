using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using BigBang;
using GameConfig.Config;
using System.Collections.Generic;
using static BigBang.ClassicManager;
using Protocol;
using static BigBang.HeroManager;
using Google.Protobuf.Collections;
using System.Linq;

namespace BigBang.UI
{
    public class ClassicEnterFightUIProperties : WindowProperties
    {
        public int formationID = FormationID.PVE;

        public ClassicTeamData classicTeamData;
        public ClassicEnterFightUIProperties(ClassicTeamData _data)
        {
            this.formationID = FormationID.PVE;
            this.classicTeamData = _data;
        }

        public HeroClubData heroClubData;
        public ClassicEnterFightUIProperties(HeroClubData _data)
        {
            this.formationID = FormationID.HERO;
            this.heroClubData = _data;
        }

        public TowerLevelData towerLevelData;
        public ClassicEnterFightUIProperties(TowerLevelData _data)
        {
            this.formationID = FormationID.TOWER;
            this.towerLevelData = _data;
        }
    }

    public class ClassicEnterFightUI : AWindowController<ClassicEnterFightUIProperties>
    {

        #region 初始化
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            fightBtn.OnPreClick += OnPreClickFightButton;
            fightBtn.OnClick += OnClickFightButton;
            formationButtonBlue.OnClick += OnClickFormationButton;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            fightBtn.OnPreClick -= OnPreClickFightButton;
            fightBtn.OnClick -= OnClickFightButton;
            formationButtonBlue.OnClick -= OnClickFormationButton;
        }

        public ClassicEnterFightUIAnim anim;
        [SerializeField] private ClassicEnterFightUIGuide classicEnterFightUIGuide;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            isFightDoing = false;
            isExitAniFinish = false;
            isNetDataFinish = false;

            SetTitle();
            SetStar();
            SetLimit();
            SetNpcPlayerRed();
            SetUserPlayerBlue();
            RefreshUpSign();
            SetFormationButton();
            SetFightPoint();
            SetClubIcon();
            SetTactics();
            SetFightText();

            anim.PlayEnter();

            closeBtn.gameObject.SetActive(true);
            formationButtonBlue.gameObject.SetActive(true);
            classicEnterFightUIGuide.CheckGuide();
        }
        #endregion

        #region 关闭
        [SerializeField] private BabuButton closeBtn;
        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<ClassicEnterFightUI>();
        }
        #endregion

        #region 开打

        [SerializeField] private TMP_Text fightText = null;
        private void SetFightText()
        {
            if (Properties.formationID == FormationID.TOWER && FBTowerController.Instance.IsCanRaid == true)
            {
                fightText.text = "扫荡";
            }
            else
            {
                fightText.text = "挑战";
            }
        }

        [SerializeField] private BabuButton fightBtn;
        private bool isFightDoing = false;
        private ChallengeStartResponse challengeStartResponse;

        private void OnPreClickFightButton(BabuButton _)
        {
            if (isFightDoing == true) return;
            if (Properties.formationID == FormationID.TOWER && FBTowerController.Instance.IsCanRaid == true) return;
            if (GuideManager.InForceGuide == false && Player.Level < 20 && Properties.formationID == FormationID.PVE && Player.FightManager.FormationController.isNeedRecommendedFormation)//引导期间不弹，20级后不弹 
            {
                if (!Player.FightManager.FormationController.IsBestClassicFormation(FightType.PVE, true, out string info))
                {
                    UIController.Instance.OpenWindow<ConfirmBoxCheckUI>(new ConfirmBoxCheckUIProperties("经理，您派出的球员不是最强阵容，建议您作如下调整:\n\n" + info + "\n是否按照这个部署调整阵容？", () =>
                    {
                        Formation formation = Player.FightManager.FormationController.GetFormation(FightType.PVE);
                        formation.Analysis();
                        formation.oldfireSection = formation.fireSection;
                        int savedMainTotalCombat = formation.GetMainTotalCombat();
                        Player.FightManager.FormationController.ChangeClassicFormationToBest();
                        int newMainTotalCombat = formation.GetMainTotalCombat();

                        string tipsText = "";
                        if (newMainTotalCombat > savedMainTotalCombat)
                        {
                            tipsText += "球队战力 <color=#13b237>+{0}</color>".SafeFormat(newMainTotalCombat - savedMainTotalCombat);
                        }
                        formation.Analysis();
                        if (formation.oldfireSection.Key != formation.fireSection.Key || formation.oldfireSection.Value != formation.fireSection.Value)
                        {
                            if (formation.fireSection.Value == 0)
                            {
                                tipsText += "|" + string.Format("第{0}节有{1}名球员爆发", formation.fireSection.Key, formation.fireSection.Value);
                                tipsText += "|" + string.Format("球队爆发预测：(爆发球员全能力+{0}%)", 5);
                                formation.oldfireSection = formation.fireSection;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(tipsText) == false)
                        {
                            tipsText += "|已为您更换最强阵容";
                            Tips.PopTips(tipsText);
                        }
                    }, () =>
                    {
                        OnFight();
                    }, !Player.FightManager.FormationController.isNeedRecommendedFormation, "不再提醒推荐", (bool isCheck) => { Player.FightManager.FormationController.isNeedRecommendedFormation = !isCheck; }));
                    return;
                }
            }

            OnFight();
        }
        private void OnClickFightButton(BabuButton _)
        {
            switch (Properties.formationID)
            {
                case FormationID.PVE:
                    {
                        isExitAniFinish = true;
                        CheckChangeToClassicFightUI();
                    }
                    break;
                case FormationID.HERO:
                    {
                        Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.HERO, formation =>
                        {
                            UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, false, FormationUI.FormationShowType.Formation, FormationID.HERO));
                            isFightDoing = false;
                            isNetDataFinish = true;
                            UIController.Instance.CloseWindow<ClassicEnterFightUI>();
                        });
                    }
                    break;
                case FormationID.TOWER:
                    {
                        isExitAniFinish = true;
                        if (FBTowerController.Instance.IsCanRaid == true)
                        {
                            UIController.Instance.CloseWindow<ClassicEnterFightUI>();
                            Babu.EventManager.Instance.Dispatch(EventID.OnTowerRaid);
                        }
                        else
                        {
                            CheckChangeToTowerFightUI();
                        }
                    }
                    break;
            }
        }
        private StartTowerChallengeResponse startTowerChallengeResponse;
        private void OnFight()
        {
            if (isFightDoing == true) return;
            isFightDoing = true;
            isExitAniFinish = false;
            isNetDataFinish = false;

            Player.BattleManager.SaveLevelAndExp();

            switch (Properties.formationID)
            {
                case FormationID.PVE: Player.BattleManager.classicTeamData = Properties.classicTeamData; break;
                case FormationID.HERO:
                    Player.BattleManager.heroClubData = Properties.heroClubData;
                    Player.BattleManager.lastChallengeHeroConfig = Player.BattleManager.heroClubData.challengeHeroConfig;
                    break;
                case FormationID.TOWER: Player.BattleManager.towerLevelData = Properties.towerLevelData; break;
            }

            if (Properties.formationID == FormationID.PVE)
            {
                Player.InBattleAni = true;
                NetworkManager.Instance.ChallengeStart(Properties.classicTeamData.challengeClubConfig.Id, (resp) =>
                {
                    challengeStartResponse = resp;
                    if (resp.Succeed)
                    {
                        if (resp.Stars[0] > 0)
                        {
                            Player.BattleManager.isFirstPass = ClassicManager.Instance.UpdatePassData(Properties.classicTeamData.challengeClubConfig.Country, resp.PassData);
                        }
                        ClassicManager.Instance.CheckRedDot();
                    }
                    CbaLogManager.Instance.AddLog(1002, Properties.classicTeamData.challengeClubConfig.Id, resp.Stars.Count > 0 && resp.Stars[0] > 0 ? 1 : 0, resp.Stars.Sum());
                    //ClassicManager.Instance.updateEquipRouteItemData(resp.PassData);
                    isNetDataFinish = true;
                    CheckChangeToClassicFightUI();
                });
            }
            if (Properties.formationID == FormationID.TOWER)
            {
                Player.InBattleAni = true;
                FBTowerController.Instance.StartBattle((resp) =>
                {
                    startTowerChallengeResponse = resp;
                    isNetDataFinish = true;
                    CheckChangeToTowerFightUI();
                    CbaLogManager.Instance.AddLog(1003, Properties.towerLevelData.towerConfig.Id, resp.Stars.Count > 0 && resp.Stars[0] > 0 ? 1 : 0, resp.Stars.Sum());
                });
            }
        }

        private bool isExitAniFinish = false;
        private bool isNetDataFinish = false;
        private void CheckChangeToClassicFightUI()
        {
            if (!isExitAniFinish) return;
            if (!isNetDataFinish) return;
            Player.BattleManager.challengeStartResponse = challengeStartResponse;

            Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.ClassicUI;
            Player.BattleManager.SetFightInfo(FightType.PVE, challengeStartResponse.Fight);
            //UIController.Instance.ShowPanel<MatchAgainstUI>(new MatchAgainstUIProperties(Properties.data.challengeClubConfig.Id));

            classicEnterFightUIGuide.CheckFinishPass13Guide();

            Player.BattleManager.StartPlayFight();

            UIController.Instance.CloseWindow<ClassicEnterFightUI>();
            isFightDoing = false;
        }
        private void CheckChangeToTowerFightUI()
        {
            if (!isExitAniFinish) return;
            if (!isNetDataFinish) return;
            Player.BattleManager.startTowerChallengeResponse = startTowerChallengeResponse;

            Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.FBTowerHomeUI;
            Player.BattleManager.SetFightInfo(FightType.Tower, startTowerChallengeResponse.Fight);
            Player.BattleManager.StartPlayFight();
            UIController.Instance.CloseWindow<ClassicEnterFightUI>();
            isFightDoing = false;
        }

        #endregion

        #region 标题
        [SerializeField] private TMP_Text titleText;
        private void SetTitle()
        {
            switch (Properties.formationID)
            {
                case FormationID.PVE: titleText.text = Properties.classicTeamData.challengeClubConfig.Name; break;
                case FormationID.HERO: titleText.text = Properties.heroClubData.challengeHeroConfig.Name; break;
                case FormationID.TOWER: titleText.text = Properties.towerLevelData.towerConfig.Name; break;
            }
        }
        #endregion

        #region 星星
        [SerializeField] private List<GameObject> darkStarList = new();
        [SerializeField] private List<GameObject> lightStarList = new();
        [SerializeField] private List<TMP_Text> starTextList = new();
        private void SetStar()
        {
            RepeatedField<int> Stars = null;
            int[] star3 = null;
            switch (Properties.formationID)
            {
                case FormationID.PVE: Stars = Properties.classicTeamData.passData.Stars; star3 = Properties.classicTeamData.challengeClubConfig.Star3; break;
                case FormationID.HERO: Stars = Properties.heroClubData.passData.Stars; star3 = Properties.heroClubData.challengeHeroConfig.Star3; break;
                case FormationID.TOWER:
                    {
                        if (Properties.towerLevelData.towerConfig.Id >= FBTowerController.Instance.FBData.currentDungeonId)
                        {
                            Stars = new() { 0, 0, 0 };
                        }
                        else
                        {
                            Stars = Properties.towerLevelData.passData.Stars;
                        }
                        star3 = Properties.towerLevelData.towerConfig.Star3; break;
                    }
            }

            for (int i = 0; i < 3; i++)
            {
                darkStarList[i].SetActive(Stars[i] <= 0);
                lightStarList[i].SetActive(Stars[i] > 0);
                if (i > 0) starTextList[i].text = Configs.ChallengeRule.GetConfig(star3[i - 1]).Desc;
            }
            RebuildStarLayout();
        }
        private void RebuildStarLayout()
        {
            foreach (TMP_Text starText in starTextList)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(starText.rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(starText.transform.parent.GetComponent<RectTransform>());
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(starTextList[0].transform.parent.parent.GetComponent<RectTransform>());
        }
        #endregion

        #region 限制
        [SerializeField] private TMP_Text noLimitText;
        [SerializeField] private List<TMP_Text> limitTextList = new();
        [SerializeField] private RectTransform limitLayout;
        private void SetLimit()
        {
            //for (int i = 0; i < 2; i++)
            //{
            //    starTextList[i].text = Configs.ChallengeRule.GetConfig(Properties.data.challengeClubConfig.Limit[i]).Desc;
            //    starTextList[i].gameObject.SetActive(true);
            //}
            limitTextList[0].gameObject.SetActive(false);
            limitTextList[1].gameObject.SetActive(true);
            limitTextList[1].text = "无阵容限制";
            LayoutRebuilder.ForceRebuildLayoutImmediate(limitLayout);

            int[] limitIntArr = { };
            switch (Properties.formationID)
            {
                //case FormationID.PVE: limitIntArr = Properties.classicTeamData.challengeClubConfig.OnBattle; break;
                case FormationID.HERO: limitIntArr = Properties.heroClubData.challengeHeroConfig.OnBattle; break;
                    //case FormationID.TOWER: limitIntArr = ""; break;
            }
            SetLimitText(limitIntArr);
        }
        private void SetLimitText(int[] limitIntArr)
        {
            if (limitIntArr.Length <= 0)
            {
                noLimitText.gameObject.SetActive(true);
                limitLayout.gameObject.SetActive(false);
            }
            else
            {
                noLimitText.gameObject.SetActive(false);
                limitLayout.gameObject.SetActive(true);

                List<ChallengeRuleConfig> challengeRuleConfigList = new();
                foreach (int limitInt in limitIntArr)
                {
                    ChallengeRuleConfig challengeRuleConfig = Configs.ChallengeRule.GetConfig(limitInt);
                    if (challengeRuleConfig == null)
                    {
                        Debug.LogWarningFormat("ClassicEnterFightUI , SetLimitText , challengeRuleConfig == null , limitInt = {0} , Properties.heroClubData.challengeHeroConfig.Id = {1}", limitInt, Properties.heroClubData.challengeHeroConfig.Id);
                        continue;
                    }
                    challengeRuleConfigList.Add(challengeRuleConfig);
                }
                for (int i = 0; i < 2; i++)
                {
                    GameObject limitItem = limitTextList[i].gameObject;
                    if (i < challengeRuleConfigList.Count)
                    {
                        limitItem.SetActive(true);
                        TMP_Text limitDescText = limitTextList[i];
                        ChallengeRuleConfig challengeRuleConfig = challengeRuleConfigList[i];
                        limitDescText.text = challengeRuleConfig.Desc;
                    }
                    else
                    {
                        limitItem.SetActive(false);
                    }
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(limitLayout);
            }
        }
        #endregion

        #region 球员列表

        //双方队标
        [SerializeField] private ClubIconItem clubIconBlue = null;
        [SerializeField] private ClubIconItem clubIconRed = null;
        private void SetClubIcon()
        {
            clubIconBlue.SetIcon(Player.Icon);
            switch (Properties.formationID)
            {
                case FormationID.PVE: clubIconRed.SetIcon(Properties.classicTeamData.challengeClubConfig.Icon); break;
                case FormationID.HERO: clubIconRed.SetIcon(Properties.heroClubData.challengeHeroConfig.Icon); break;
                case FormationID.TOWER: clubIconRed.SetIcon(Properties.towerLevelData.towerConfig.Icon); break;
            }
        }


        //敌方球员
        [SerializeField] private List<ClassicFightEnterPlayerItem> playerItemListRed;
        private void SetNpcPlayerRed()
        {
            int clubId = 0;
            switch (Properties.formationID)
            {
                case FormationID.PVE: clubId = Properties.classicTeamData.challengeClubConfig.Id; break;
                case FormationID.HERO: clubId = Properties.heroClubData.challengeHeroConfig.Id; break;
                case FormationID.TOWER: clubId = Properties.towerLevelData.towerConfig.Id; break;
            }

            List<ChallengePlayerConfig> playerCfgList = new List<ChallengePlayerConfig>();
            var cfgList = Configs.ChallengePlayer.GetConfigList();
            for (int i = 0; i < cfgList.Count; i++)
            {
                if (cfgList[i].ClubId == clubId)
                {
                    playerCfgList.Add(cfgList[i]);
                }
            }
            if (playerCfgList.Count < 5)
            {
                Debug.LogError("ClassicEnterFightUI , SetPlayer , playerCfgList.Count < 5 , clubId = " + clubId);
            }
            List<ChallengePlayerConfig> challengePlayerList = PlayerCard.GenChallengeBoardCardMap(playerCfgList);
            if (challengePlayerList.Count < 5)
            {
                Debug.LogError("ClassicEnterFightUI , SetPlayer , challengePlayerList.Count < 5 , clubId = " + clubId);
            }
            for (int i = 0; i < 5; i++)
            {
                ClassicFightEnterPlayerItem playerItem = playerItemListRed[i];
                if (i < challengePlayerList.Count)
                {
                    playerItem.SetData(challengePlayerList[i]);
                    playerItem.gameObject.SetActive(true);
                }
                else
                {
                    playerItem.gameObject.SetActive(false);
                }
            }
        }

        //我方球员
        [SerializeField] private List<ClassicFightEnterPlayerItem> playerItemListBlue;
        private void SetUserPlayerBlue()
        {
            Formation formation = Player.FightManager.FormationController.GetFormation(Properties.formationID);
            foreach (var item in formation.StarterBoardCardDic)
            {
                FormationBoardConfig formationBoardConfig = Configs.FormationBoard.GetDataDictionary()[item.Key];
                int separatedPosition = formationBoardConfig.SeparatedPosition;
                PlayerCard playerCard = Player.CardManager.GetCard(item.Value);
                playerItemListBlue[separatedPosition - 1].SetData(playerCard);
            }
        }

        //刷新上下箭头
        private void RefreshUpSign()
        {
            for (int i = 0; i < 5; i++)
            {
                ClassicFightEnterPlayerItem playerItemBlue = playerItemListBlue[i];
                ClassicFightEnterPlayerItem playerItemRed = playerItemListRed[i];
                bool isBlueUp = playerItemBlue.fightPoint > playerItemRed.fightPoint;
                bool isRedUp = playerItemRed.fightPoint > playerItemBlue.fightPoint;
                playerItemBlue.SetUpImage(isBlueUp);
                playerItemRed.SetUpImage(isRedUp);
            }
        }

        //球队总战力(目前只能计算首发，NPC替补未知)
        [SerializeField] private ImageFont fightPointNumImageFontBlue = null;
        [SerializeField] private ImageFont fightPointNumImageFontRed = null;
        private void SetFightPoint()
        {
            int fightPointBlue = 0;
            int fightPointRed = 0;
            for (int i = 0; i < 5; i++)
            {
                ClassicFightEnterPlayerItem playerItemBlue = playerItemListBlue[i];
                ClassicFightEnterPlayerItem playerItemRed = playerItemListRed[i];
                fightPointBlue += playerItemBlue.fightPoint;
                fightPointRed += playerItemRed.fightPoint;
            }
            fightPointNumImageFontBlue.text = fightPointBlue.ToString();
            fightPointNumImageFontRed.text = fightPointRed.ToString();
        }

        #endregion

        #region 对方阵型
        [SerializeField] private List<TMP_Text> tacticsTextList;
        [SerializeField] private List<Image> tacticsImgList;
        private void SetTactics()
        {
            for (int i = 0; i < 2; i++)
            {
                TMP_Text tacticText = tacticsTextList[i];
                int tacticId = 0;
                switch (Properties.formationID)
                {
                    case FormationID.PVE: tacticId = Properties.classicTeamData.challengeClubConfig.Tactics[i]; break;
                    case FormationID.HERO: tacticId = Properties.heroClubData.challengeHeroConfig.Tactics[i]; break;
                    case FormationID.TOWER: tacticId = Properties.towerLevelData.towerConfig.Tactics[i]; break;
                }
                TacticsConfig tacticsConfig = Configs.Tactics.GetConfig(tacticId);
                if (tacticsConfig == null)
                {
                    int challengeClubId = 0;
                    switch (Properties.formationID)
                    {
                        case FormationID.PVE: challengeClubId = Properties.classicTeamData.challengeClubConfig.Id; break;
                        case FormationID.HERO: challengeClubId = Properties.heroClubData.challengeHeroConfig.Id; break;
                        case FormationID.TOWER: challengeClubId = Properties.towerLevelData.towerConfig.Id; break;
                    }
                    Debug.LogErrorFormat("ClassicEnterFightUI , SetTactics , tacticsConfig == null , Properties.formationID = {0} , challengeClubId = {1} , tacticId = {2}", Properties.formationID, challengeClubId, tacticId);
                    tacticText.text = "--";
                    continue;
                }
                tacticText.text = tacticsConfig.Name;
            }

            Formation formation = Player.FightManager.FormationController.GetFormation(Properties.formationID);
            showKeZhi(formation.TacticsIdList);
        }

        /// <summary>
        /// 显示克制关系
        /// </summary>
        /// <param name="lefttac"></param>
        private async void showKeZhi(List<int> lefttac)
        {
            List<int> righttac = new();
            switch (Properties.formationID)
            {
                case FormationID.PVE: righttac = Properties.classicTeamData.challengeClubConfig.Tactics.ToList(); break;
                case FormationID.HERO: righttac = Properties.heroClubData.challengeHeroConfig.Tactics.ToList(); break;
                case FormationID.TOWER: righttac = Properties.towerLevelData.towerConfig.Tactics.ToList(); break;
                    //如果阵型不存在，接下去都会报错。
            }
            //自己的倒序，防守在前面，敌人的正序，进攻在前面。
            //进攻克防守， 防守克进攻，同类型不会互相克。
            lefttac.Sort((a, b) => -a.CompareTo(b));
            righttac.Sort((a, b) => a.CompareTo(b));

            for (var index = 0; index < lefttac.Count; index++)
            {
                var cfg = Configs.Tactics.GetConfig(lefttac[index]);
                var defcfg = Configs.Tactics.GetConfig(righttac[index]);
                tacticsTextList[index + 2].text = cfg.Name;

                if (cfg.Restrain1.Contains(righttac[index]))
                {
                    tacticsImgList[index].gameObject.SetActive(true);
                    tacticsImgList[index].sprite = await SpriteManager.GetSprite(AtlasNames.Public, "kezhi01");
                }
                else
                {
                    cfg = Configs.Tactics.GetConfig(righttac[index]);
                    if (cfg.Restrain1.Contains(lefttac[index]))
                    {
                        tacticsImgList[index].gameObject.SetActive(true);
                        tacticsImgList[index].sprite = await SpriteManager.GetSprite(AtlasNames.Public, "kezhi02");
                    }
                    else
                    {
                        tacticsImgList[index].gameObject.SetActive(false);
                    }
                }
            }

        }

        #endregion

        #region 布阵按钮

        [SerializeField] private BabuButton formationButtonBlue = null;
        private void SetFormationButton()
        {
            formationButtonBlue.gameObject.SetActive(true);
        }
        private void OnClickFormationButton(BabuButton _)
        {
            Player.BattleManager.SaveLevelAndExp();
            switch (Properties.formationID)
            {
                case FormationID.PVE: Player.BattleManager.classicTeamData = Properties.classicTeamData; break;
                case FormationID.HERO:
                    Player.BattleManager.heroClubData = Properties.heroClubData;
                    Player.BattleManager.lastChallengeHeroConfig = Player.BattleManager.heroClubData.challengeHeroConfig;
                    break;
                case FormationID.TOWER: Player.BattleManager.towerLevelData = Properties.towerLevelData; break;
            }
            Formation formation = Player.FightManager.FormationController.GetFormation(Properties.formationID);
            ClassicManager.Instance.NeedShowClassicCountryUI = true;
            UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, false, FormationUI.FormationShowType.Formation, Properties.formationID));
            UIController.Instance.CloseWindow<ClassicEnterFightUI>();
        }

        #endregion



    }
}