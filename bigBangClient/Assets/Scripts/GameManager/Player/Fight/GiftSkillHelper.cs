using BigBang;
using GameConfig.Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBA
{
    public abstract class FActionResult
    {
        public FActionResultType resultType;
        public int value;
        public int rateValue;
    }

    #region 只是为了写代码可读性更强的设定

    public static class FActionObjectType
    {
        public const string Position = "position";
        public const string Pointer = "pointer";
        public const string Defender = "defender";
        public const string Self = "self";
        public const string Ran = "ran";
        public const string Target = "target";
    }

    public enum FActionObjectSide
    {
        Our = 0,
        They = 1
    }


    public class FActionTimeType
    {
        /// <summary>
        /// 战斗开始时
        /// </summary>
        public static string OnBattle = "onbattle";
        /// <summary>
        /// 与某人一起出战时
        /// </summary>
        public static string OnBattleWith = "onbattlewith";
        /// <summary>
        /// 下场时
        /// </summary>
        public static string OnOff = "onff";
        /// <summary>
        /// 得分达到多少时
        /// </summary>
        public static string OnPoint = "onpoint";
        /// <summary>
        /// 落后多少分时
        /// </summary>
        public static string OnPointBehind = "onpointBehind";
        /// <summary>
        /// 每多少回合
        /// </summary>
        public static string OnRoundReq = "onroundfreq";
        /// <summary>
        /// 在某1节开始时
        /// </summary>
        public static string OnSection = "onsection";
    }

    public class FActionType
    {
        public static string AddBuff = "addbuff";
        public static string AddProp = "addprop";
        public static string ForceOff = "forceoff";
    }

    public enum FActionResultType
    {

        /// <summary>
        /// 身体
        /// </summary>
        BODY = 1,
        /// <summary>
        /// 传球
        /// </summary>
        PASS = 2,
        /// <summary>
        /// 防守
        /// </summary>
        DEF = 3,
        /// <summary>
        /// 扣篮
        /// </summary>
        DUNK = 4,
        /// <summary>
        /// 篮板
        /// </summary>
        REB = 5,
        /// <summary>
        /// 得分
        /// </summary>
        PT = 6,
        /// <summary>
        /// 抢断
        /// </summary>
        STEAL = 7,
        /// <summary>
        /// 控球
        /// </summary>
        Handle = 8,
        /// <summary>
        /// 盖帽
        /// </summary>
        BLOCK = 9,
        /// <summary>
        /// 稳定
        /// </summary>
        STAB = 10,
        /// <summary>
        /// 体力消耗速度
        /// </summary>
        ENERGYSPD = 101
    }


    #endregion


    public class FActionTime
    {
        public string key;
        public int value;

        public FActionTime(string when, int wparam2)
        {
            this.key = when;
            this.value = wparam2;
        }
    }

    #region ActionResult的三种结果
    public class FBuff : FActionResult
    {
        public static FBuff Create(int[] _params)
        {
            FBuff model = new();
            model.resultType = (FActionResultType)_params[0];
            model.value = _params[1];
            model.rateValue = _params[2];
            return model;
        }
    }

    public class FProp : FActionResult
    {
        public static FProp Create(int[] _params)
        {
            FProp model = new();
            model.resultType = (FActionResultType)_params[0];
            model.value = _params[1];
            model.rateValue = _params[2];
            return model;
        }
    }

    /// <summary>
    /// 强制立场的结果
    /// </summary>
    public class FForceOff : FActionResult
    {
        public static FForceOff Create(int[] _params)
        {
            FForceOff buff = new();
            buff.rateValue = _params[0];
            return buff;
        }
    }
    #endregion

    public class FActionObject
    {
        /// <summary>
        /// 类型, 
        /// </summary>
        public string type;
        /// <summary>
        /// 哪方的
        /// </summary>
        public int side;
        /// <summary>
        /// 
        /// </summary>
        public List<int> valueList;

        public FActionObject(string[] actobj, int _side)
        {
            side = _side;
            if (actobj[0] != FActionObjectType.Defender &&
                actobj[0] != FActionObjectType.Pointer &&
                actobj[0] != FActionObjectType.Self &&
                actobj[0] != FActionObjectType.Ran)
            {
                valueList = actobj.ToList().ConvertAll(p => int.Parse(p));
                type = "position";
            }
            else
            {
                type = actobj[0];
            }
        }
    }

    public class FGiftSkill
    {
        /// <summary>
        /// 行为时机，什么时候发生
        /// </summary>
        public FActionTime ActionTime;
        /// <summary>
        /// 行为类型，做什么事
        /// </summary>
        public string ActionType;
        /// <summary>
        /// 行为对象，对谁做
        /// </summary>
        public FActionObject ActionTarget;
        /// <summary>
        /// 行为结果
        /// </summary>
        public FActionResult ActionResult;

        public FGiftSkill(GiftSkillConfig cfg)
        {
            ActionTime = new FActionTime(cfg.When, cfg.Wparam2);
            ActionType = cfg.Action;
            ActionTarget = new FActionObject(cfg.Actobj, cfg.Side);

            if (cfg.Action == FActionType.AddBuff)
            {
                ActionResult = FBuff.Create(cfg.Actparam);
            }
            else if (cfg.Action == FActionType.AddProp)
            {
                ActionResult = FProp.Create(cfg.Actparam);
            }
            else if (cfg.Action == FActionType.ForceOff)
            {
                ActionResult = FForceOff.Create(cfg.Actparam);
            }
        }
    }

    public class FGiftSkillResult
    {
        public int cardId_sender;

        public FActionResult result;

        public List<int> cardId_receiver;

        public FGiftSkillResult()
        {
            cardId_receiver = new List<int>();
        }
    }


    public class FActionPlayer
    {
        public static void PlayAction(PlayerCard card, FGiftSkill skill, List<PlayerCard> cards, Dictionary<int, int> positionAndIdDic)
        {
            if (skill.ActionType == FActionType.AddBuff)
            {
                AddBuff(card, skill, cards, positionAndIdDic);
            }
        }

        /// <summary>
        /// 没写完
        /// </summary>
        /// <param name="card"></param>
        /// <param name="skill"></param>
        /// <param name="cards"></param>
        /// <param name="positionAndIdDic"></param>
        public static void AddBuff(PlayerCard card, FGiftSkill skill, List<PlayerCard> cards, Dictionary<int, int> positionAndIdDic)
        {
            FGiftSkillResult result = new FGiftSkillResult();
            result.cardId_sender = card.CardId;
            //现在只是做给布阵用，所以只考虑给我方加的
            if (skill.ActionTarget.side == (int)FActionObjectSide.Our)
            {
                if (skill.ActionTarget.type == FActionObjectType.Self)
                {
                    result.cardId_receiver = new List<int>() { card.CardId };
                }
                else if (skill.ActionTarget.type == FActionObjectType.Position)
                {
                    skill.ActionTarget.valueList.ForEach((p) =>
                    {
                        //p>5,就是给新上阵的人了。
                        if (p <= 5)
                        {
                            var boardId = Player.FightManager.FormationController.GetBoardIdFromPositionId(p);
                            var targetPlayerId = positionAndIdDic[boardId];
                            result.cardId_receiver.Add(targetPlayerId);
                        }
                    });
                }
                else if (skill.ActionTarget.type == FActionObjectType.Pointer)
                {

                }
            }

        }
    }
}