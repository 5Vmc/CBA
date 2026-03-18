using GameConfig;
using GameConfig.Config;
using Protocol;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Babu;
using Utils;
using CBA;
using Babu.Config;
using System.Threading.Tasks;

namespace BigBang
{
    public class PlayerCard : ICloneable
    {

        /// <summary>
        /// 是空白卡片信息
        /// </summary>
        public bool isEmptyCard = false;
        public static PlayerCard GetEmptyCard(int cardId)//获取空白卡片信息，用于新卡展示等
        {
            PlayerCard card = new PlayerCard(cardId);
            card.isEmptyCard = true;
            PlayerCardInfo info = CreateEmptyPlayerCardInfo(card);
            card.UnPack(info);
            return card;
        }
        /// <summary>
        /// 是数字藏品信息
        /// </summary>
        public bool isCollectionCard = false;
        public static PlayerCard GetCollectionCard(PlayerCardInfo playerCardInfo)//获取数字藏品卡片信息
        {
            PlayerCard card = new PlayerCard(playerCardInfo.CardId);
            card.isEmptyCard = false;
            card.isCollectionCard = true;
            card.UnPack(playerCardInfo);
            return card;
        }

        public static PlayerCardInfo CreateEmptyPlayerCardInfo(PlayerCard playerCard)
        {
            PlayerCardInfo playerCardInfo = new PlayerCardInfo();
            playerCardInfo.CardId = playerCard.CardId;
            playerCardInfo.Level = 0;
            playerCardInfo.Exp = 0;
            playerCardInfo.Star = 0;
            playerCardInfo.PlayerCardNumber = playerCard.Config.Number;
            playerCardInfo.Status = (int)PlayerCardStatus.VeryGood;
            playerCardInfo.Quality = playerCard.Config.Quality;
            playerCardInfo.Jerseys.Add(0);
            playerCardInfo.Jerseys.Add(0);
            playerCardInfo.Jerseys.Add(0);
            playerCardInfo.Jerseys.Add(0);
            playerCardInfo.PerformanceData = new();
            return playerCardInfo;
        }

        private int _fightpoint;
        public int CardId { get; set; }
        public int ServerStrength { get; set; }

        public string PropId = "";
        public int PropStatus = 0;

        /// <summary>
        /// 战力
        /// </summary>
        public int FightPoint
        {
            get
            {
                if (_fightpoint == 0)
                {
                    _fightpoint = GetCombatEffectiveness(0, false, false);
                }
                return _fightpoint;
            }
            set { _fightpoint = value; }
        }

        public CardModelConfig Config { get; set; }

        /// <summary> 加上巅峰年限后的全名 </summary>
        public static string GetFullName(CardModelConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.PeakYear))
            {
                return config.Name;
            }
            else
            {
                return "{0}·{1}".SafeFormat(config.PeakYear, config.Name);
            }
        }
        /// <summary> 是巅峰球员 </summary>
        public static bool IsPeak(CardModelConfig config)
        {
            if (config == null) return false;
            return string.IsNullOrWhiteSpace(config.PeakLogo) == false;
        }
        public bool IsPeak()
        {
            return IsPeak(Config);
        }

        /// <summary>
        /// 身上挂的各种buff和爆发数据
        /// </summary>
        public Dictionary<string, FActionResult> ActionResultDict;
        /// <summary>
        /// 名字的品质色
        /// </summary>
        public Color NameColor
        {
            get
            {
                return CBAColorUtil.Instance.GetColor(Quality);
            }
        }

        //星级
        public int Star { get; set; }

        //品质， 需要服务器增加
        private int _quality = -1;
        public int Quality
        {
            get
            {
                return this._quality;
            }
            set
            {
                this._quality = value;
            }
        }

        //顶峰、上升、普通、下滑、低谷5个档次
        public PlayerCardStatus Status { get; set; }

        public string GetPlayerCardStatusStr()
        {
            switch (Status)
            {
                case PlayerCardStatus.VeryGood: return "顶峰";
                case PlayerCardStatus.Good: return "上升";
                case PlayerCardStatus.Ordinary: return "普通";
                case PlayerCardStatus.Down: return "下滑";
                case PlayerCardStatus.VeryDown: return "低谷";
            }
            return "";
        }
        public Task<Sprite> GetPlayerCardStatusSprite()
        {
            return SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)Status]);
        }

        /// <summary>
        /// 体能，自动恢复系数
        /// </summary>
        public float Energy
        {
            get
            {
                var now = Utils.DataConvUtil.ServerTime;
                //_energy = 0;
                var recoverPerSec = 8f / TimeUtils.Hour;
                var cur = _energy + Mathf.Max(0, now - _energy_last_update_time) * recoverPerSec;
                if (cur > GameConst.CardInitEnergy)
                    cur = GameConst.CardInitEnergy;
                return cur;
            }
            private set { }
        }
        private float _energy;
        private long _energy_last_update_time;

        public float TotalEnergyRatio//总计体力百分比
        {
            get { return Utility.KeepInRange(Energy / GameConst.PlayerMaxEnergy, 0, 1) * 100; }
        }
        public float SingleEnergyRatio//单场体力百分比
        {
            get { return Utility.KeepInRange(Energy / GameConst.CardSingleEnergy, 0, 1) * 100; }
        }
        public float BackupEnergyRatio//储备体力百分比
        {
            get { return Utility.KeepInRange((Energy - GameConst.CardSingleEnergy) / GameConst.CardSingleEnergy, 0, GameConst.CardInitEnergy / GameConst.CardSingleEnergy - 1) * 100; }
        }

        //伤病
        private InjuryType _injuryType;
        public InjuryType InjuryType
        {
            get
            {
                if (InjuryEndTime <= Utils.DataConvUtil.ServerTime)
                {
                    return InjuryType.Health;
                }
                return _injuryType;
            }
            private set
            {
                _injuryType = value;
            }
        }
        public string GatPlayerCardInjuryTypeStr()
        {
            LangID healthLangID = new LangID[] { LangID.HealthText, LangID.HealthText, LangID.MinorInjuryText, LangID.SeriousInjury }[(int)InjuryType];
            return Lang.Get(healthLangID);
        }

        public long InjuryEndTime { get; set; }

        public int Level { get; set; }//球员等级

        public int EquipGrade { get; set; }         //球员阶段

        public List<int> EquipLevels { get; set; } = new() { 0, 0, 0, 0 };  //各装备等级

        public int Exp { get; set; }

        /// <summary>
        /// 装备状态
        /// </summary>
        public CardEquipStatus EquipStatus;

        /// <summary>
        /// 球员评价
        /// </summary>
        public int Power
        {
            get
            {
                if (Config.Quality == Quality && Star == 0)
                {
                    return Config.Power;
                }
                else
                {
                    var config = Configs.CardUpgrade.GetConfig(CardId * 1000 + Quality * 100 + Star);
                    if (config == null) return 0;

                    return config.Power;
                }
            }
        }
        /// <summary>
        /// 默认场上位置
        /// </summary>
        public int DefaultPosition
        {
            get => Config.AdaptPosition[0];
        }

        public bool IsUsingInBounty
        {
            get
            {
                return BountyTaskManager.Instance.IsPlayerCardUsing(CardId);
            }
        }

        //表现数据(PVP联赛和PVP杯赛）
        //public Dictionary<FightType, PerformanceData> PerformanceDataDic { get; set; } = new Dictionary<FightType, PerformanceData>();
        //阵容数据(PVP联赛和PVP杯赛）

        public PerformanceData PerformanceData = new PerformanceData();
        public Dictionary<int, FormationData> FormationDataDic { get; set; } = new Dictionary<int, FormationData>();
        //战斗中的FightFormationData，区别于上面平时的PVE和PVP的
        public FormationData FightFormationData { get; set; } = new FormationData();
        public Dictionary<int, PlayerCardSkill> SkillDic { get; set; } = new Dictionary<int, PlayerCardSkill>();

        //正在训练的特技训练室
        public int SkillTrainRoomId { get; set; } = 0;

        public Dictionary<int, int> BanCountDic = new Dictionary<int, int>();
        public Dictionary<int, int> YellowCardDic = new Dictionary<int, int>();

        public long CTime { get; set; }

        //球员号码
        public int PlayerCardNumber { get; set; }

        public float ExpProgress
        {
            get
            {
                return GetExpProgress(Level, Exp);
            }
        }
        public float LevelNowExp
        {
            get
            {
                return GetLevelNowExp(Level, Exp);
            }
        }
        public float LevelMaxExp
        {
            get
            {
                return GetLevelMaxExp(Level);
            }
        }

        public int ActivedGiftSkillCount
        {
            get
            {
                if (EquipGrade == 0) return 0;
                var config = Configs.JerseyBreak.GetConfig(DefaultPosition * 1000 + EquipGrade);
                return config != null ? config.Talent : 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_cfg"></param>
        /// <returns></returns>
        public static int GetSkillFireSection(GiftSkillConfig _cfg)
        {
            if (_cfg.Fire > 0)
            {
                if (_cfg.When == FActionTimeType.OnBattle)
                {
                    return 1;
                }
                else if (_cfg.When == FActionTimeType.OnSection)
                {
                    return _cfg.Wparam2;
                }
            }
            return 0;
        }

        public static float GetExpProgress(int level, int exp)
        {
            CardLevelConfig userLevelConfig = Configs.CardLevel.GetConfig(level);
            int maxExp = userLevelConfig.Exp;
            int nowExp = exp - userLevelConfig.ExpTotal;
            float progress = nowExp / (float)maxExp;
            return progress;
        }
        public static int GetLevelNowExp(int level, int exp)
        {
            CardLevelConfig userLevelConfig = Configs.CardLevel.GetConfig(level);
            int nowExp = exp - userLevelConfig.ExpTotal;
            return nowExp;
        }
        public static int GetLevelMaxExp(int level)
        {
            CardLevelConfig userLevelConfig = Configs.CardLevel.GetConfig(level);
            int maxExp = userLevelConfig.Exp;
            return maxExp;
        }

        public static Tuple<int, float> GetLevelAndExpProgress(int totalExp)
        {
            if (totalExp < 0) totalExp = 0;
            CardLevelConfig userLevelConfig = null;
            foreach (CardLevelConfig userLevelConfigi in Configs.CardLevel.GetConfigList())
            {
                if (totalExp >= userLevelConfigi.ExpTotal)
                {
                    userLevelConfig = userLevelConfigi;
                }
                else
                {
                    break;
                }
            }
            int level = userLevelConfig.Id;
            int maxExp = userLevelConfig.Exp;
            int nowExp = totalExp - userLevelConfig.ExpTotal;
            float progress = nowExp / (float)maxExp;
            return new Tuple<int, float>(level, progress);
        }

        public PlayerCard(int cardId)
        {
            CardId = cardId;
            Star = 0;
            _energy = GameConst.CardInitEnergy;
            _energy_last_update_time = 0;
            Config = Configs.CardModel.GetConfig(CardId);
            if (Config == null)
            {
                Debug.LogError("PlayerCard , PlayerCard , Config == null , CardId = " + CardId);
            }

            this._quality = Config.Quality;

            Status = PlayerCardStatus.Ordinary;

            FormationDataDic.Add(FormationID.PVE, new FormationData());
            FormationDataDic.Add(FormationID.PVP, new FormationData());
            FormationDataDic.Add(FormationID.ARENA, new FormationData());
            FormationDataDic.Add(FormationID.HERO, new FormationData());
            FormationDataDic.Add(FormationID.TOWER, new FormationData());
            FormationDataDic.Add(FormationID.Hundred, new FormationData());

            // PerformanceDataDic.Add(FightType.PVE, new PerformanceData());
            // PerformanceDataDic.Add(FightType.League, new PerformanceData());
        }

        public void UnPack(PlayerCardInfo data)
        {
            CardId = data.CardId;
            Star = data.Star;
            Quality = data.Quality;
            Status = (PlayerCardStatus)data.Status;
            _energy = data.Energy;
            _energy_last_update_time = data.EnergyLastUpdateTime;
            InjuryType = (InjuryType)data.InjuryType;
            InjuryEndTime = data.InjuryEndTime;
            Level = data.Level;
            EquipGrade = data.JerseyBreak;
            EquipLevels = data.Jerseys.ToList<int>();
            Exp = data.Exp;
            ServerStrength = data.Strength;
            PropId = data.PropId;
            PropStatus = data.PropStatus;
            // BanCountDic = data.BanCountDic.ToDictionary(x => x.Key, y => y.Value);
            // YellowCardDic = data.YellowCardDic.ToDictionary(x => x.Key, y => y.Value);
            //RedCardBanCount = data.RedCardBanCount;

            //todo performance data
            // foreach (var kv in data.YellowCardDic)
            // {
            //     YellowCardDic.Add(kv.Key, kv.Value);
            // }

            SkillDic.Clear();
            foreach (var skill in data.SkillList)
            {
                if (!SkillDic.ContainsKey(skill.Id))
                {
                    SkillDic.Add(skill.Id, new PlayerCardSkill(skill.Id, skill.Level));
                }
            }
            Config = Configs.CardModel.GetConfig(CardId);
            if (Config == null)
            {
                Debug.LogError("PlayerCard , UnPack , Config == null , CardId = " + CardId);
            }
            CTime = data.CTime;
            //PlayCardNumber = data.PlayerCardNumber;
            PlayerCardNumber = data.PlayerCardNumber;

            if (data.PerformanceData == null)
            {
                Debug.LogError("PlayerCard , UnPack , data.PerformanceData == null , CardId = " + CardId);
            }
            else
            {
                PerformanceData.PlayingCount = data.PerformanceData.Court;
            }

            int court = data.PerformanceData.Court;
            if (court == 0) court = 1;

            PerformanceData.AssistsAverage = Math.Round(data.PerformanceData.Assist * 1.0 / court, 1);
            PerformanceData.ReboundAverage = Math.Round(data.PerformanceData.Rebound * 1.0 / court, 1);
            PerformanceData.ScoreAverage = Math.Round(data.PerformanceData.Point * 1.0 / court, 1);
        }

        private int GetAbilityLevelAdd(int ability, int level)
        {
            var config = Configs.CardLevel.GetConfig(level);
            if (config == null) return 0;
            return config.Ability[ability];
        }
        private int GetAbilityBreakAdd(int ability)
        {
            if (EquipGrade == 0) return 0;
            var config = Configs.JerseyBreak.GetConfig(DefaultPosition * 1000 + EquipGrade);
            if (config == null) return 0;

            return config.Ability[ability];
        }

        /// <summary>
        /// 获取装备部位属性加成
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        private int GetAbilityEquipLvAdd(int ability)
        {
            int _ability = 0;
            for (var index = 0; index < 4; index++)
            {
                //默认是0级
                if (EquipLevels[index] <= 0) continue;

                var config = Configs.JerseyUpgrade.GetConfig((index + 1) * 100000 + DefaultPosition * 1000 + EquipLevels[index]);
                if (config != null && config.Ability.ContainsKey(ability))
                {
                    _ability += config.Ability[ability];
                }
            }

            return _ability;
        }

        /// <summary>
        /// 获取装备升级配置表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<JerseyUpgradeConfig> GetEquipLevelsConfig(List<int> list)
        {
            List<JerseyUpgradeConfig> configList = new List<JerseyUpgradeConfig>();

            for (int index = 0; index < 4; index++)
            {
                var config = Configs.JerseyUpgrade.GetConfig((index + 1) * 100000 + DefaultPosition * 1000 + list[index] + 1);
                configList.Add(config);
            }

            return configList;
        }

        private int GetAbilityStarAdd(int ability, int quality, int star)
        {
            var config = Configs.CardUpgrade.GetConfig(CardId * 1000 + quality * 100 + star);
            if (config == null) return 0;

            return config.AbilityAdd[ability];
        }

        private int GetAbilityStarAdd_Value(int ability, int quality, int star)
        {
            var config = Configs.CardUpgrade.GetConfig(CardId * 1000 + quality * 100 + star);
            if (config == null) return 0;

            return config.AbilityAdd[ability];
        }

        private int GetAbilityStarAdd_Rate(int ability, int quality, int star)
        {
            //没升星过，从球员模板表取
            if (Config.Quality == quality && star == 0)
            {
                return Config.AbilityRatio[ability];
            }
            else
            {
                //升星过，从升星表取
                var config = Configs.CardUpgrade.GetConfig(CardId * 1000 + quality * 100 + star);
                if (config == null) return 0;

                return config.AbilityRatio[ability];
            }
        }

        /**
        是否满级0星
        **/
        public bool IsMaxAndZeroStar()
        {
            if (Star == 0)
            {
                return !CouldUpgradeStarInThisQuality();
            }
            return false;
        }

        //是否满级了
        public bool IsStarAndQualityMax()
        {
            return CouldUpgradeStarInThisQuality() == false && CouldUpgradeQuality() == false;
        }

        // public bool IsMax()
        // {
        //     List<CardUpgradeConfig> list =  Configs.CardUpgrade.GetConfigList().FindAll((item)=>{return item.CardId == CardId;});  
        //     foreach(CardUpgradeConfig conf in list){
        //         if(conf.Quality > Quality)
        //             return false;
        //         if(conf.Quality == Quality && conf.Star > Star)
        //             return false;
        //     }  
        //     return true;
        // }


        /**
         /**
        ****是否还有下一阶
        **/
        public bool CouldUpgradeQuality()
        {
            if (Star > 0)
            {
                CardUpgradeConfig conf = Configs.CardUpgrade.GetConfigList().Find((item) => { return item.CardId == CardId && item.Quality > Quality; });
                return conf != null;
            }
            return false;
        }


        /**
        ****本品质下还可以升星
        **/
        public bool CouldUpgradeStarInThisQuality()
        {
            CardUpgradeConfig conf = Configs.CardUpgrade.GetConfigList().Find((item) => { return item.CardId == CardId && item.Quality == Quality && item.Star > Star; });
            return conf != null;
        }

        public int GetAbility(int abilityId, int quality = -1, int star = -1, int positionId = -1)
        {
            if (star == -1) star = Star;
            if (quality == -1) quality = Quality;

            int configAbility = Config.Ability[abilityId];
            int abilityAdd = Player.TrainManager.GetAbilityAdd(abilityId);
            int starAdd = GetAbilityStarAdd_Value(abilityId, quality, star);
            int levelAdd = GetAbilityLevelAdd(abilityId, Level);
            int jerseyAdd = GetAbilityEquipLvAdd(abilityId) + GetAbilityBreakAdd(abilityId);
            int abilityRatio = GetAbilityStarAdd_Rate(abilityId, quality, star);
            int finalAbility = configAbility + abilityAdd + (starAdd + levelAdd) * abilityRatio / 100 + jerseyAdd;
            //Debug.LogWarning($"CardId = {CardId} , Config.Name = {Config.Name} , abilityId = {abilityId} , (starAdd + levelAdd) * abilityRatio = {(starAdd + levelAdd) * abilityRatio} , Mathf.FloorToInt((starAdd + levelAdd) * abilityRatio) = {Mathf.FloorToInt((starAdd + levelAdd) * abilityRatio)}");
            //Debug.LogWarning($"CardId = {CardId} , Config.Name = {Config.Name} , abilityId = {abilityId} , configAbility = {configAbility} , abilityAdd = {abilityAdd} , starAdd = {starAdd} , levelAdd = {levelAdd} , jerseyAdd = {jerseyAdd} , abilityRatio = {abilityRatio} , finalAbility = {finalAbility}");
            return finalAbility;
        }
        private int FlootToInt(float f)
        {
            return (int)Mathf.Floor(f + 0.5f);
        }

        //获得升级后加多少能力值 // star ----> star+1
        public int GetAbilityStarUpgradeAdd(int abilityId, int quality = -1)
        {
            if (quality == -1) quality = Quality;
            var ret = GetAbilityStarAdd(abilityId, quality, Star + 1) - GetAbilityStarAdd(abilityId, quality, Star);
            return ret <= 0 ? 0 : ret;
        }

        public int GetAbilityQualityUpgradeAdd(int abilityId)
        {
            List<CardUpgradeConfig> list = Configs.CardUpgrade.GetConfigList().FindAll((item) => { return item.CardId == CardId && item.Quality == Quality; });

            int star = 0;
            foreach (CardUpgradeConfig conf in list)
            {
                if (conf.Star > star)
                    star = conf.Star;
            }
            // Debug.Log("========>GetAbilityStarAdd after=" + GetAbilityStarAdd(abilityId, Quality+1, 0) + ", before=" +  GetAbilityStarAdd(abilityId, Quality, star));
            var ret = GetAbilityStarAdd(abilityId, Quality + 1, 0) - GetAbilityStarAdd(abilityId, Quality, star);
            return ret <= 0 ? 0 : ret;
        }

        /// <summary>
        /// 计算战力
        /// </summary>
        /// <param name="separatedPosition"></param>
        /// <param name="TipFightPoint">是否飘战力</param>
        /// <returns></returns>
        public int GetCombatEffectiveness(int separatedPosition = 0, bool TipFightPoint = false, bool ForceRecacluate = false)
        {
            var newFightPoint = 0;
            if (ForceRecacluate || _fightpoint == 0)
            {
                newFightPoint = GetStarCombatEffectiveness(Star, -1, separatedPosition);
                var diffFightPoint = newFightPoint - _fightpoint;
                _fightpoint = Math.Max(newFightPoint, _fightpoint);

                if (TipFightPoint && diffFightPoint > 0)
                {
                    Utils.FightPoint.PopTips(_fightpoint, diffFightPoint);
                }
            }

            return newFightPoint;
        }

        public int GetUpgradeStarCombatEffectivenessAdd()
        {
            return GetStarCombatEffectiveness(Star + 1, -1, 0, true)
                   - GetStarCombatEffectiveness(Star);
        }
        public int GetUpgradeQualityCombatEffectivenessAdd()
        {
            int nowQuality = Quality;
            int nowStar = Star;
            int nowFight = this.GetStarCombatEffectiveness(nowStar, nowQuality);

            int nextQuality = nowQuality + 1;
            int nextStar = 0;
            int nextFight = this.GetStarCombatEffectiveness(nextStar, nextQuality);
            return nextFight - nowFight;
        }

        public int GetStarCombatEffectiveness(int star, int quality = -1, int separatedPosition = 0, bool autoUpdateAsNew = false)
        {
            if (quality == -1) quality = Quality;

            if (separatedPosition == 0)
            {
                separatedPosition = Config.AdaptPosition[0];
            }
            var positionCfg = Configs.SeparatedPosition.GetConfig(separatedPosition);
            if (positionCfg == null) return 0;
            float sum = 0;
            for (int i = AbilityId.Shoot; i <= AbilityId.Will; i++)
            {
                sum += GetAbility(i, quality, star, positionCfg.Id) * positionCfg.AbilityRatio[i];
            }
            //Debug.LogWarning($"CardId = {CardId} , Config.Name = {Config.Name} , abilitySum = {sum}");
            sum /= GameConst.ABILITY_NORMAL;

            //加特技加成分
            if (SkillDic.Count != 0)
            {
                var tempList = SkillDic.ToList();
                for (int i = 0; i < tempList.Count; i++)
                {
                    sum += tempList[i].Value.GetEffectaddValue();
                }
            }

            var _ft = (int)Mathf.Floor(sum + 0.5f) + Config.ExtraAbility;
            if (autoUpdateAsNew && _ft > FightPoint) FightPoint = _ft;

            int endCombat = (int)Mathf.Floor(sum + 0.5f) + Config.ExtraAbility;
            //Debug.LogWarning($"CardId = {CardId} , Config.Name = {Config.Name} , endCombat = {endCombat}");
            return endCombat;
        }

        /// <summary>
        /// 计算Npc战力
        /// </summary>
        public static int GetNpcPlayerCombat(ChallengePlayerConfig playerCfg, SeparatedPositionConfig positionCfg)
        {
            float sum = 0;
            for (int i = AbilityId.Shoot; i <= AbilityId.Will; i++)
            {
                sum += playerCfg.Ability[i] * positionCfg.AbilityRatio[i];
            }
            return Mathf.FloorToInt(sum / GameConst.ABILITY_NORMAL + 0.5f);
        }
        /// <summary>
        /// 选取首发5个人
        /// </summary>
        public static List<ChallengePlayerConfig> GenChallengeBoardCardMap(List<ChallengePlayerConfig> playerCfgList)
        {
            var retList = new List<ChallengePlayerConfig>();
            var sysFormationCfg = Configs.SysFormation.GetConfig(1);
            foreach (var boardId in sysFormationCfg.BoardIdList)
            {
                var separatedPostion = Configs.FormationBoard.GetConfig(boardId).SeparatedPosition;
                var positionCfg = Configs.SeparatedPosition.GetConfig(separatedPostion);

                int targetIndex = 0;
                int maxScore = -1;
                for (int i = 0; i < playerCfgList.Count; i++)
                {
                    if (retList.Contains(playerCfgList[i])) continue;
                    bool isFind = false;
                    foreach (var item in playerCfgList[i].AdaptPosition)
                    {
                        if (separatedPostion == item)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (isFind == false) continue;
                    var score = GetNpcPlayerCombat(playerCfgList[i], positionCfg);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        targetIndex = i;
                    }
                }
                retList.Add(playerCfgList[targetIndex]);
            }
            return retList;
        }

        public int GetStarCombatSeparatedPosition(int separatedPosition)
        {
            return Player.CalFightPoint_Single(CardId, false, separatedPosition);
        }
        //对应小位置的可发挥能力分
        public int GetSeparatePositionEnergy(int i, int separatedPosition)
        {
            if (separatedPosition == 0)
            {
                separatedPosition = Config.AdaptPosition[0];
            }
            var positionCfg = Configs.SeparatedPosition.GetConfig(separatedPosition);
            if (positionCfg == null) return 0;
            return positionCfg.AbilityRatio[i];
        }

        public override string ToString()
        {
            return $"card name = {PlayerCard.GetFullName(Config)} ,id = {CardId} star = {Star}";
        }

        public string GetPositionName()
        {
            var cfg = Configs.Position.GetConfig(Config.Position);

            if (cfg == null) return "";
            return cfg.Name;
        }

        public string GetPositionAbbreviation()
        {
            var cfg = Configs.Position.GetConfig(Config.Position);

            if (cfg == null) return "";
            return cfg.Abbreviation;
        }

        /// <summary>
        /// 5 个小位置
        /// </summary>
        public string GetAdaptPositionAbbreviation()
        {
            return GetAdaptPositionAbbreviation((PositionSeparatedType)Config.AdaptPosition[0]);
        }
        public static string GetAdaptPositionAbbreviation(CardModelConfig cardModelConfig)
        {
            if (cardModelConfig == null) return "";
            return GetAdaptPositionAbbreviation((PositionSeparatedType)cardModelConfig.AdaptPosition[0]);
        }
        public static string GetAdaptPositionAbbreviation(PositionSeparatedType positionSeparatedType)
        {
            var cfg = Configs.SeparatedPosition.GetConfig((int)positionSeparatedType);
            if (cfg == null) return "";
            return cfg.Abbreviation;
        }

        public PositionSeparatedType GetAdaptPosition()
        {
            return (PositionSeparatedType)Config.AdaptPosition[0];
        }

        public void UpgradeStar()
        {
            Star += 1;
            if (Star > GameConst.MaxCardStar)
            {
                Star = GameConst.MaxCardStar;
            }
        }

        public void UpgradeQuality()
        {
            Quality += 1;
            Star = 0;
        }

        public bool CanTrainSkill(int skillId)
        {
            var skillConfig = Configs.Skill.GetConfig(skillId);
            if (skillConfig == null) return false;
            if (skillConfig.Quality > Quality) return false;
            if (HaveSkill(skillId)) return false;

            return true;
        }
        public bool HaveSkill(int skillId)
        {
            return SkillDic.ContainsKey(skillId);
        }

        public void SkillTrainBeing(int roomId)
        {
            SkillTrainRoomId = roomId;
        }
        public void SkillTrainComplete(int skillId)
        {
            SkillTrainRoomId = 0;
            if (!SkillDic.ContainsKey(skillId))
            {
                SkillDic.Add(skillId, new PlayerCardSkill(skillId, 1));
            }
        }
        /// <summary>
        /// 是否是首发球员
        /// </summary>
        /// <returns>是首发球员返回true，不是首发球员返回false</returns>
        public bool IsStarter()
        {
            return FormationDataDic[FormationID.PVE].State == FormationCardState.Starter;//经典赛
            // var starterDic = Player.FightManager.FormationController.DefaultPvpFormation.StarterBoardCardDic;
            // return starterDic.Values.Any(cardId => cardId == CardId);
        }
        public bool IsStarter1()
        {
            return FormationDataDic[FormationID.PVP].State == FormationCardState.Starter;//赛事
            // var starterDic = Player.FightManager.FormationController.DefaultPvpFormation.StarterBoardCardDic;
            // return starterDic.Values.Any(cardId => cardId == CardId);
        }

        public bool IsStarter2()
        {
            return FormationDataDic[FormationID.ARENA].State == FormationCardState.Starter;//排位赛
        }
        public bool IsStarter3()
        {
            return FormationDataDic[FormationID.TOWER].State == FormationCardState.Starter;//篮球殿堂
        }
        public bool IsStarter4()
        {
            return FormationDataDic[FormationID.Hundred].State == FormationCardState.Starter;//百分大战
        }

        /// <summary>
        /// 是否是后备队员
        /// </summary>
        /// <returns></returns>
        public bool IsReserve()
        {
            return FormationDataDic[FormationID.PVE].State == FormationCardState.Reserve;
        }
        /// <summary>
        /// 是否是替补队员
        /// </summary>
        /// <returns></returns>
        public bool IsSubstitute()
        {
            return FormationDataDic[FormationID.PVE].State == FormationCardState.Substitute;
        }

        /// <summary>
        /// 获取当前的升星配置和下一级的升星配置
        /// </summary>
        /// <returns></returns>
        public (CardUpgradeConfig, CardUpgradeConfig) GetStar_CurrentAndNext()
        {
            CardUpgradeConfig cfg, cfgNext;
            var cfgList = Configs.CardUpgrade.GetConfigList();
            var currentIndex = cfgList.FindIndex(p => p.CardId == CardId && p.Quality == Quality && p.Star == Star);
            if (currentIndex == -1 && Star == 0)
            {
                cfg = null;
                cfgNext = cfgList.Find(p => p.CardId == CardId && p.Quality == Quality && p.Star == (Star + 1));
            }
            else
            {
                cfg = cfgList[currentIndex];
                if (currentIndex >= cfgList.Count - 1)
                {
                    //模板表到尽头了
                    cfgNext = cfgList[currentIndex];
                }
                else if (cfgList[currentIndex + 1].CardId != CardId)
                {
                    //下一行不是这个球员了
                    cfgNext = cfgList[currentIndex];
                }
                else
                {
                    cfgNext = cfgList[currentIndex + 1];
                }
            }

            return (cfg, cfgNext);
        }

        public QualityStarConfig GetUpgradeStarConfig(int targetStar)
        {
            return Configs.QualityStar.GetConfig(Quality * 1000 + targetStar);
        }

        public QualityUpgradeConfig GetUpgradeQualityConfig(int targetQuality)
        {
            return Configs.QualityUpgrade.GetConfigList().Find((item) => { return item.Quality == targetQuality; });
        }

        public bool CanFight()
        {
            return true;
        }

        public bool IsBanned(FightType fightType = FightType.League)
        {
            //todo ??????
            return BanCountDic[(int)fightType] > 0;
        }

        public bool IsHurt()
        {
            return InjuryType == InjuryType.MinorInjury || InjuryType == InjuryType.SeriousInjury;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
