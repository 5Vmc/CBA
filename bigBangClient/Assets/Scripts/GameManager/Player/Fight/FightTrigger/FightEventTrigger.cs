using Babu;
using Protocol;
using UnityEngine;
using GameConfig;
using DG.Tweening;

namespace BigBang
{
    public class FightEventTrigger
    {
        private FightData _fight;

        public void Init(FightData fight)
        {
            _fight = fight;
        }

        public void OnTrigger(int frame, FightFrameEvent eventData)
        {
            // 射门、射正、角球、任意球、犯规、红牌、黄牌、控球率、传球成功率
            // 进球
            switch (eventData.EventId)
            {
                // 开球 1
                default:
                    break;
            }
        }

        // 上半场开始
        private void OnTriggerFirstHalf(int frame, FightFrameEvent eventData)
        {
            AudioManager.Instance.PlaySound(AudioNames.LONG_WHISTLE);
            //var s1 = AudioManager.Instance.PlaySound(AudioNames.BG_MATCHCHEER_01);
            //var s2 = AudioManager.Instance.PlaySound(AudioNames.BG_MATCHCHORUS);
            //if (s1 != null || s2 != null)
            //{
            //    DOTween.To(value => s1.volume = s2.volume = value, 1, 0, 3).SetDelay(2f);
            //}
            EventManager.Instance.Dispatch(EventID.OnFirstHalfStart, frame);
        }

        //上半场伤停补时预告
        private void OnTriggerFirstHalfInjuryForecast(int frame, FightFrameEvent eventData)
        {
            EventManager.Instance.Dispatch(EventID.OnFirstHalfInjuryForecast, eventData.Arg1);
        }

        // 上半场伤停补时
        private void OnTriggerFirstHalfInjuryTime(int frame, FightFrameEvent eventData)
        {
            Debug.Log("上半场伤停补时frame=" + frame);
            //EventManager.Instance.Dispatch(EventID.OnBreaktimeStart, frame);
        }

        // 中场休息
        private void OnTriggerHalfTime(int frame, FightFrameEvent eventData)
        {
            AudioManager.Instance.PlaySound(AudioNames.A_SHORT_A_LONG);
            EventManager.Instance.Dispatch(EventID.OnHalfTimeBreak);
            // 时间暂停
            EventManager.Instance.Dispatch(EventID.OnTimeStop, frame);
        }

        // 下半场开始
        private void OnTriggerSecondHalf(int frame, FightFrameEvent eventData)
        {
            AudioManager.Instance.PlaySound(AudioNames.LONG_WHISTLE);
            EventManager.Instance.Dispatch(EventID.OnSecondHalfStart, frame);
            // 时间开始
            EventManager.Instance.Dispatch(EventID.OnTimeStart, frame);
        }

        //下半场伤停补时预告
        private void OnTriggerSecondHalfInjuryForecast(int frame, FightFrameEvent eventData)
        {
            EventManager.Instance.Dispatch(EventID.OnSecondHalfInjuryForecast, eventData.Arg1);
        }

        private void OnTriggerSecondHalfInjuryTime(int frame, FightFrameEvent eventData)
        {
            //EventManager.Instance.Dispatch(EventID.OnBreaktimeStart, frame);
            Debug.Log("下半场伤停补时frame=" + frame);
        }

        #region 加时赛赛程相关
        private void OnTriggerExtra(int frame, FightFrameEvent eventData)
        {
            EventManager.Instance.Dispatch(EventID.OnExtraPrepareToStart);
            // 时间暂停
            EventManager.Instance.Dispatch(EventID.OnTimeStop, frame);
        }

        private void OnTriggerExtraFirstHalf(int frame, FightFrameEvent eventData)
        {
            //EventManager.Instance.Dispatch(EventID.OnBreaktimeEnd, frame);
            EventManager.Instance.Dispatch(EventID.OnTimeStop);
            // 时间开始
            EventManager.Instance.Dispatch(EventID.OnTimeStart, frame);
        }

        //加时赛上半场伤停补时预报
        private void OnTriggerExtraFirstHalfInjuryForecast(int frame, FightFrameEvent eventData)
        {
            EventManager.Instance.Dispatch(EventID.OnExtraFirstHalfInjuryForecast, eventData.Arg1);
        }

        private void OnTriggerExtraFirstHalfInjuryTime(int frame, FightFrameEvent eventData)
        {
            //EventManager.Instance.Dispatch(EventID.OnBreaktimeStart, frame);
            Debug.Log("加时赛上半场场伤停补时frame=" + frame);
        }

        private void OnTriggerExtraHalfTime(int frame, FightFrameEvent eventData)
        {
            EventManager.Instance.Dispatch(EventID.OnExtraHalfBreak);
            // 时间暂停
            EventManager.Instance.Dispatch(EventID.OnTimeStop, frame);
        }

        private void OnTriggerExtraSecondHalf(int frame, FightFrameEvent eventData)
        {
            //EventManager.Instance.Dispatch(EventID.OnBreaktimeEnd, frame);
            // 时间开始
            EventManager.Instance.Dispatch(EventID.OnTimeStart, frame);
        }

        //加时赛下半场伤停补时预报
        private void OnTriggerExtraSecondHalfInjuryForecast(int frame, FightFrameEvent eventData)
        {
            EventManager.Instance.Dispatch(EventID.OnExtraSecondHalfInjuryForecast, eventData.Arg1);
        }

        private void OnTriggerExtraSecondHalfInjuryTime(int frame, FightFrameEvent eventData)
        {
            //EventManager.Instance.Dispatch(EventID.OnBreaktimeStart, frame);
            Debug.Log("加时赛下半场场伤停补时frame=" + frame);
        }

        #endregion

        private void OnTriggerFightEnd(int frame, FightFrameEvent eventData)
        {
            AudioManager.Instance.PlaySound(AudioNames.TWO_SHORT_A_LONG);
        }

        // 点球准备
        private void OnTriggerShootOutPrepare(FightFrameEvent eventData)
        {
            Debug.Log("点球准备");
            // 派发点球准备事件
            EventManager.Instance.Dispatch(EventID.OnShootOutPrepare, eventData.Arg1);
        }

        //点球大战开始
        private void OnTriggerShootOut(FightFrameEvent eventData)
        {
            Debug.Log("点球开始");
            EventManager.Instance.Dispatch(EventID.OnShootOut, eventData.Arg1);
            EventManager.Instance.Dispatch(EventID.OnTimeStop);
        }

        private void OnTriggerSubstitutionUp(FightFrameEvent data)
        {
            Debug.Log("substitution up");
            // var boardId = data.Arg1 / 1000;
            // var roleId = data.Arg1 % 1000;
            // var cardId = data.Arg2;
            // var team = _fight.GetTeamBySide(roleId / 100);
            // team.ReplaceCardBoard(cardId, boardId);
            // team.ReplaceFightRoleCard(roleId, cardId);
        }

        private void OnTriggerSubstitutionDown(FightFrameEvent data)
        {
            Debug.Log("substitution down");
            // var roleId = data.Arg1;
            // var boardId = data.Arg2;
            // var team = _fight.GetTeamBySide(roleId / 100);
            // var card = _fight.GetCardByRoleID(roleId);
            // if (card == null) return;
            // team.ReplaceCardBoard(card.CardId, boardId);
        }

        private void OnTriggerSubstitutionReplaceBoard(FightFrameEvent data)
        {
            Debug.Log("substitution replace board");
            // var boardId = data.Arg1 / 1000;
            // var roleId = data.Arg1 % 1000;
            // var cardId = data.Arg2;
            // var team = _fight.GetTeamBySide(roleId / 100);
            // team.ReplaceCardBoard(cardId, boardId);
        }

        private void OnTriggerYellowCard(FightFrameEvent data)
        {

        }

        private void OnTriggerRedCard(FightFrameEvent data)
        {

        }

        private void OnTriggerYellowToRedCard(FightFrameEvent data)
        {

        }

        // // 开球：1
        // private void OnTriggerKickOff(FightFrameEvent data)
        // {
        //     OnTriggerMessage(data);
        // }

        private void OnTriggerFoul(FightFrameEvent data)
        {
            AudioManager.Instance.PlaySound(AudioNames.SHORT_WHISTLE);
            _fight.GetCardByRoleID(data.Arg1).Team.FoulCount++;
        }

        // 射门：101
        private void OnTriggerShoot(FightFrameEvent data)
        {
            var shootRole = _fight.GetCardByRoleID(data.Arg1);
            shootRole.ShootCount++;
            shootRole.Team.ShootCount++;
            EventManager.Instance.Dispatch(EventID.OnShoot, data.Arg1, data.Arg2); //Arg1：roleId；Arg2：shoot_result
        }

        private void OnTriggerPass(FightFrameEvent data)
        {
            var passRole = _fight.GetCardByRoleID(data.Arg1);
            passRole.PassCount++;
            passRole.Team.PassCount++;
        }

        private void OnTriggerPassSuccess(FightFrameEvent data)
        {
            var passRole = _fight.GetCardByRoleID(data.Arg1);
            passRole.PassSuccess++;
            passRole.Team.PassSuccessCount++;
        }

        // 射进：102
        private void OnTriggerEventShootIn(FightFrameEvent data)
        {
            //var names1 = new string[] { AudioNames.BG_MATCHCHEER_01, AudioNames.BG_MATCHCHEER_01 };
            //var names2 = new string[] { AudioNames.BG_MATCHCHORUS, AudioNames.BG_MATCHCHORUS_DRUM };
            //var s1 = AudioManager.Instance.PlaySound(names1[Random.Range(0, 2)]);
            //var s2 = AudioManager.Instance.PlaySound(names2[Random.Range(0, 2)]);
            //if (s1 != null) DOTween.To(value => s1.volume = value, 1, 0, 4).SetDelay(2);
            //if (s2 != null) DOTween.To(value => s2.volume = value, 1, 0, 4).SetDelay(2);
            //var shootRole = _fight.GetCardByRoleID(data.Arg1);
            //shootRole.Goal++;
            //var assistRole = _fight.GetCardByRoleID(data.Arg2);
            //if (assistRole != null)
            //{
            //    assistRole.Assist++;
            //}

            //var enemyGoalKeeper = shootRole.Team.GetEnemyTeam()?.GetPlayingGoalKeeper();
            //if (enemyGoalKeeper != null)
            //{
            //    enemyGoalKeeper.BeShootIn++;
            //}

            //string name;
            //if (shootRole.Team.TeamType != 3)
            //{
            //    name = Configs.CardModel.GetConfig(shootRole.CardId).Name;
            //}
            //else
            //{
            //    name = Configs.ChallengePlayer.GetConfig(shootRole.CardId).Name;
            //}
            //EventManager.Instance.Dispatch(EventID.OnGoal, data.Arg1, name);
        }

        // 射正
        private void OnTriggerEventShootHit(FightFrameEvent data)
        {
            var shootRole = _fight.GetCardByRoleID(data.Arg1);
            shootRole.Team.HitCount++;
        }

        private void OnTriggerPenaltyShootIn(FightFrameEvent data)
        {

        }

        private void OnTriggerPenaltyShootFailed(FightFrameEvent data)
        {

        }

        private void OnTriggerBreach(FightFrameEvent data)
        {
        }

        private void OnTriggerBreachSuccess(FightFrameEvent data)
        {
            var breachRole = _fight.GetCardByRoleID(data.Arg1);
            var beBreachRole = _fight.GetCardByRoleID(data.Arg2);
            breachRole.BreachSuccess++;
            beBreachRole.BeBreach++;
        }

        private void OnTriggerBreachFail(FightFrameEvent data)
        {
            // var breachRole = _fight.GetCardByRoleID(data.Arg1);
            var stealsStlsRole = _fight.GetCardByRoleID(data.Arg2);
            // breachRole.BreachSuccess++;
            stealsStlsRole.StealStls++;
        }

        private void OnTriggerBlock(FightFrameEvent data)
        {
            var interceptRole = _fight.GetCardByRoleID(data.Arg2);
            interceptRole.Intercept++;
        }

        private void OnTriggerPlugging(FightFrameEvent data)
        {
            var pluggingRole = _fight.GetCardByRoleID(data.Arg1);
            pluggingRole.Plugging++;
        }

        private void OnTriggerShootSave(FightFrameEvent data)
        {
            var shootSaveRole = _fight.GetCardByRoleID(data.Arg1);
            shootSaveRole.ShootSave++;
        }

        private void OnTriggerCornerKick(FightFrameEvent data)
        {
            _fight.GetCardByRoleID(data.Arg1).Team.CornerKick++;
        }

        private void OnTriggerFreeKick(FightFrameEvent data)
        {
            _fight.GetCardByRoleID(data.Arg1).Team.FreeKick++;
        }

        private void OnTriggerPenalty(FightFrameEvent data)
        {
            EventManager.Instance.Dispatch(EventID.OnPenalty, data.Arg1);//Arg1是roleId
        }
    }
}
