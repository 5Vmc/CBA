using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Protocol;
using UnityEngine;
using Utils;
using GameConfig.Config;
using GameConfig;
using BigBang.UI;
using Babu.SDK;

namespace BigBang
{
    public class Player
    {
        public static bool isDeveloper = false;

        public static string Name { get; set; }
        public static string GbId { get; set; }

        public static int UpLevel { get; set; }//上报使用
        public static int UpStrengh { get; set; }//上报使用
        public static int UpCreateTime { get; set; }//上报使用

        public static int Icon { get; private set; }

        public static int HomeJersey { get; private set; }
        public static int AwayJersey { get; private set; }
        public static int AlternativeJersey { get; private set; }

        public static int CreateTime { get; private set; }
        /// <summary>
        /// 触发升级的时候先打个标记，之后再来弹升级界面。这个值在升级界面执行完成后修改为false
        /// </summary>
        public static bool NewLevelUp { get; set; }

        private static long battleTimeStamp;
        private static bool _inbattleani;
        /// <summary>
        /// 弹窗礼包往这个位置缩放
        /// </summary>
        public static Transform TimeGiftTrans;
        /// <summary>
        /// 是否处于战斗动画中, 设定战斗动画30秒过期，避免网络中断了以后一直处于战斗动画状态；正常情况应该在战斗动画之后解除这个状态；
        /// </summary>
        public static bool InBattleAni
        {
            get
            {
                if (!_inbattleani) return _inbattleani;

                if ((Utils.DataConvUtil.ServerTime - battleTimeStamp) > 30)
                {
                    _inbattleani = true;
                    return _inbattleani;
                }
                else return _inbattleani;
            }
            set
            {
                if (_inbattleani)
                {
                    battleTimeStamp = Utils.DataConvUtil.ServerTime;
                }
                _inbattleani = value;
                GuideManager.UpdatePopwindowFlag();
            }
        }

        /// <summary>
        /// 重算单个卡牌的战力，会更新总战力
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="TipFightPoint">是否飘战力</param>
        /// <param name="_position">如果是0 则为默认位置的战力</param>
        /// <returns></returns>
        public static int CalFightPoint_Single(int cardid, bool TipFightPoint = false, int _position = 0)
        {
            PlayerCard card = CardManager.GetCard(cardid);
            int oldCardFightPoint = card.FightPoint;
            int newCardFightPoint = card.GetCombatEffectiveness(_position, false, true);
            int diffFightPoint = newCardFightPoint - oldCardFightPoint;
            if (TipFightPoint && diffFightPoint > 0)
            {
                FightPoint.PopTips(oldCardFightPoint, diffFightPoint);
            }

            if (diffFightPoint > 0)
            {
                //重排英雄的战力，但是不会重算其他英雄
                Player.CardManager.CardList.Sort((a, b) => -a.FightPoint.CompareTo(b.FightPoint));

                int _cardCount = Player.CardManager.CardList.Count;
                int _ftpt = 0;
                for (int i = 0; i < _cardCount; i++)
                {
                    if (i >= 12) break;
                    if (Player.CardManager.CardList[i] == card)
                    {
                        if (diffFightPoint > 0) _strength += diffFightPoint;
                    }
                    _ftpt += Player.CardManager.CardList[i].FightPoint;
                }
                if (_ftpt > Strength) Strength = _ftpt;
            }
            return newCardFightPoint;
        }

        /// <summary>
        /// 登录之后调用这个方法计算战力，会重算所有英雄的战力，并取前12个作为球队战力
        /// </summary>
        /// <param name="TipFightPoint">是否飘战力</param>
        public static void CalFightPoint(bool TipFightPoint = false)
        {
            int scoreSum = 0; //实力
            List<int> scoreList = new List<int>();
            foreach (var card in Player.CardManager.CardList)
            {
                scoreList.Add(card.GetCombatEffectiveness(0, false, true));
            }

            scoreList.Sort((x, y) => -x.CompareTo(y));

            //取前12个人
            for (int i = 0; i < scoreList.Count && i < 12; i++)
            {
                scoreSum += scoreList[i];
            }

            if (TipFightPoint && _strength != 0 && _strength < scoreSum)
            {
                FightPoint.PopTips(_strength, scoreSum - _strength);
                EventManager.Instance.Dispatch(EventID.OnPlayerHeadChange, null);
            }
            Strength = scoreSum;
        }

        private static int _strength;
        public static int Strength
        {
            get
            {
                return _strength;
            }
            set
            {
                //最大战力只增不减
                if (value > _strength)
                {
                    _strength = value;
                }
            }
        }
        public static void ResetStrength()
        {
            _strength = 0;
        }

        /// <summary>
        /// 这个-1不能动，用这个来判断是不是被首次赋值的。
        /// </summary>
        private static int _level = -1;
        public static int Level
        {
            get
            {
                return _level;
            }
            private set
            {
                _level = value;
            }
        }
        private static int levelBeforeReset = 1;
        public static void ResetLevel()
        {
            if (_level != -1) levelBeforeReset = _level;
            _level = -1;
        }

        public static int Exp { get; private set; }

        public static float ExpProgress
        {
            get
            {
                return GetExpProgress(Level, Exp);
            }
        }
        public static float GetExpProgress(int level, int exp)
        {
            UserLevelConfig userLevelConfig = Configs.UserLevel.GetConfig(level);
            if (userLevelConfig == null)
            {
                userLevelConfig = Configs.UserLevel.GetConfig(levelBeforeReset);
            }
            if (userLevelConfig == null)
            {
                userLevelConfig = Configs.UserLevel.GetConfig(1);
            }
            int maxExp = userLevelConfig.Exp;
            int nowExp = exp - userLevelConfig.ExpTotal;
            float progress = nowExp / (float)maxExp;
            return progress;
        }
        public static int[] GetExpNum(int level, int exp)
        {
            UserLevelConfig userLevelConfig = Configs.UserLevel.GetConfig(level);
            if (userLevelConfig == null)
            {
                userLevelConfig = Configs.UserLevel.GetConfig(levelBeforeReset);
            }
            if (userLevelConfig == null)
            {
                userLevelConfig = Configs.UserLevel.GetConfig(1);
            }
            int maxExp = userLevelConfig.Exp;
            int nowExp = exp - userLevelConfig.ExpTotal;

            return new int[] { nowExp, maxExp };
        }

        public static PlayerAchievementManager AchievementManager { get; set; }
        public static PlayerTrainManager TrainManager { get; set; }
        public static PlayerCardManager CardManager { get; set; }
        public static PlayerPackageManager PackageManager { get; set; }
        public static PlayerFightManager FightManager { get; set; }
        public static PlayerEmailManager EmailManager { get; set; }
        public static PlayerChallengeManager ChallengeManager { get; set; }
        public static PlayerShopManager ShopManager { get; set; }
        public static PlayerTaskManager TaskManager { get; set; }
        public static ActivityManager ActivityManager { get; set; }
        public static PlayerOnoffManager OnoffManager { get; set; }
        public static PlayerPVPManager PVPManager { get; set; }
        public static BattleManager BattleManager { get; set; }

        public static NoviceTaskManager NoviceTaskManager { get; set; }
        public static ServerData ServerData { get; set; }


        public Player()
        {
        }

        public static void LoginSuccess()
        {
            Player.CalFightPoint(false);
            FightManager.LoginSuccess();

            //检查主线和剧情推图的小红点
            ClassicManager.Instance.CheckRedDot();
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_ClassicPVE, 0);

            HeroManager.Instance.CheckRedDot();
            //爬塔小红点
            FBTowerController.Instance.CheckRedDot();
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_FBClassicHero, 1);
            //竞技场小红点
            BattleManager.CheckArenaRedDot(() =>
            {
                //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady,
                //PanelNodePath.Home_ClassicArena, 2);
            });

            //联赛小红点不用主动检测，靠服务端推回来的信息更新小红点
            //Player.PVPManager.CheckRedData(() =>
            //{
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_ClassicPVP, 3);
            //});

            //训练小红点
            Player.TrainManager.CheckRedDot();
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_Train, 4);

            //特技小红点-只检查有没有可解锁的技能
            Player.CardManager.SkillController.CheckRedDot();
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_SkillTrain, 5);

            //任务小红点
            Player.TaskManager.CheckTaskRedDot(TaskType.Daily);
            Player.TaskManager.CheckTaskRedDot(TaskType.Weekly);
            BountyTaskManager.Instance.CheckRedDot();
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_Task, 6);

            //小游戏的小红点，改在首页去刷了，这里不用提前刷。
            //Player.ActivityManager.RefreshRedDot();
            //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_Games, 7);

            //生涯小红点
            Player.TaskManager.CheckTaskRedDot_Normal(TaskType.Normal, () =>
            {
                //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady,
                //PanelNodePath.Home_Career, 8);
            });

            //招募小红点
            Player.CardManager.RecruitController.CheckRedData(1);

            //成就小红点
            Player.AchievementManager.CheckAchievementRedDot();
            //荣誉小红点
            Player.AchievementManager.CheckHonourRedDot();
            
            //卡牌小红点
            Player.CardManager.CheckRedDot(0, true);

            //商店小红点
            Player.ShopManager.CheckRedDot();

            //EventManager.Instance.Dispatch(EventID.OnRefreshNavigationUIRedDot);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public static void UnPack(BasicPlayerInfoNotify data)
        {
            BasicPlayerInfoNotify basicPlayerInfoNotify = data;
            Player.isDeveloper = basicPlayerInfoNotify.Developer == 1;
            Player.GbId = basicPlayerInfoNotify.Gbid;
            Player.Name = basicPlayerInfoNotify.Name;
            Player.UpLevel = basicPlayerInfoNotify.Level;
            Player.UpStrengh = basicPlayerInfoNotify.Strength;
            Player.UpCreateTime = basicPlayerInfoNotify.CreateTime;

            GbId = data.Gbid;
            Name = data.Name;
            Icon = data.Icon;
            HomeJersey = data.HomeJersey;
            AwayJersey = data.AwayJersey;
            AlternativeJersey = data.AlternativeJersey;
            CreateTime = data.CreateTime;
            //Strength = data.Strength;
            if (Level != -1 && data.Level > Level)
            {
                NewLevelUp = true;
            }

            Level = data.Level;
            Exp = data.Exp;
            // 完成引导
            GuideManager.ProcessServerGuide(data.GuideId.Select(item => (GuideID)item).ToArray());
            DispatchLevelUp();
            EventManager.Instance.Dispatch(EventID.OnPlayerHeadChange, null);
        }

        /// <summary>
        /// 触发升级面板；这个事件遇到强制引导会丢弃，遇到战斗会延后。
        /// 平时都在通知经验变化后调用
        /// </summary>
        public static void DispatchLevelUp()
        {
            if (!NewLevelUp) return;

            EventManager.Instance.Dispatch(EventID.OnTeamlevelUp);
            HundredManager.Instance.CheckHundredRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);

            if (InBattleAni) return;
            if (GuideManager.InForceGuide)
            {
                //强制引导中不弹升级，且丢弃本次升级展示。
                NewLevelUp = false;
            }
            else
            {
                UIController.Instance.CloseWindow<EquipRouteUI>();
                UIController.Instance.OpenWindow<TeamLvUpUI>();
            }

            ByteDanceManager.Instance.ReportLevelUp(Player.UpLevel);
        }

        public static void ReviseName(string name)
        {
            Name = name;
        }
    }
}
