using System.Collections.Generic;
using System.Linq;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using UnityEngine;

namespace BigBang
{
    public static class GuideManager
    {
        /// <summary>
        /// 是否在强制引导中；后面的代码如果做强制引导要把这个设置为true来屏蔽一些系统弹窗，例如：升级。
        /// </summary>
        public static bool InForceGuide
        {
            get
            {
                if (IsStarterGuide) return true;

                foreach (GuideID guideID in doingGuideSet)
                {
                    if (trigSet.Contains(guideID)) continue;
                    return true;
                }

                return false;
            }
        }

        public static readonly int startGuidePassLevel = 5;
        /// <summary>
        /// 是否在新手引导引导中
        /// </summary>
        public static bool IsStarterGuide
        {
            get
            {
                if (Player.Level >= startGuidePassLevel) return false;
                return IsFinished(GuideID.starterGuide) == false;
            }
        }

        private static HashSet<GuideID> doingGuideSet = new();
        public static void DoGuide(GuideID guideID)
        {
            if (doingGuideSet.Contains(guideID) == true) return;
            doingGuideSet.Add(guideID);
            GuideManager.UpdatePopwindowFlag();
        }
        public static bool IsGuideDoing(GuideID guideID)
        {
            return doingGuideSet.Contains(guideID);
        }

        // 引导是否完成
        public static bool IsFinished(GuideID guideID)
        {
            //return true;//关闭所有引导
            if (Player.Level >= startGuidePassLevel && (int)guideID <= (int)GuideID.starterGuide) return true;
            if (guideFinishSet.Contains(GuideID.starterGuide) && (int)guideID <= (int)GuideID.starterGuide) return true;
            bool isFinished = guideFinishSet.Contains(guideID);
            return isFinished;
        }

        ///// <summary>一组引导是否都完成</summary>
        //public static bool IsFinished(IList<GuideID> guideIDs)
        //{
        //    for (int i = 0; i < guideIDs.Count; i++)
        //    {
        //        if (IsFinished(guideIDs[i]))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        /// <summary>完成的引导集合</summary>
        private static HashSet<GuideID> guideFinishSet = new();

        /// <summary>完成引导</summary>
        public static void Finish(GuideID guideID, bool network = true)
        {
            if (doingGuideSet.Contains(guideID)) doingGuideSet.Remove(guideID);
            GuideManager.UpdatePopwindowFlag();
            if (IsFinished(guideID)) return;

            guideFinishSet.Add(guideID);

            if (guideID == GuideID.starterGuide)
            {
                if (UIController.Instance.PopwindowFlag) UIController.Instance.OpenAllHideScreens();
            }

            if (!network) return;

            NetworkManager.Instance.FinishGuide(guideID);
        }

        /// <summary>成组出现的引导，最后一个没完成就会下次重启游戏登录进主界面时全部重置</summary>
        public static GuideID[][] guideGroupList = new GuideID[][]
        {
            new GuideID[]{ GuideID.guideGetProgressBox3Tip, GuideID.guideGetProgressBox3 },
            new GuideID[]{ GuideID.guideGetNewPlayerTip, GuideID.guideGetNewPlayer },
            new GuideID[]{ GuideID.guideUpLevelPlayerTip, GuideID.guideUpLevelPlayer },
        };
        /// <summary>非强制的引导</summary>
        public static HashSet<GuideID> trigSet = new() { };

        public static void ResetGuideSign()
        {
            doingGuideSet.Clear();
            guideFinishSet.Clear();
            isFirstGetServerGuide = true;
            GuideManager.UpdatePopwindowFlag();
        }
        private static bool isFirstGetServerGuide = true;
        public static void ProcessServerGuide(IList<GuideID> guideIDs)
        {
            //foreach (var item in guideIDs)
            //{
            //    Debug.LogWarningFormat("ProcessServerGuide , guideID = {0}", item);
            //}
            if (isFirstGetServerGuide == false)
            {
                //Finish(guideIDs, false);//防止后续更新Player中其他内容时错误的覆盖了内存中的引导数据
                return;
            }
            isFirstGetServerGuide = false;
            HashSet<GuideID> finishGuiIdServerSet = new();
            foreach (GuideID guideID in guideIDs)
            {
                //Debug.Log("finishGuiId = " + (int)guideID);
                if (finishGuiIdServerSet.Contains(guideID))
                {
                    Debug.LogWarningFormat("GuideManager , ProcessServerGuide , finishGuiIdServerSet.Contains(guideID) , guideID = {0}", guideID.ToString());
                    continue;
                }
                if (guideFinishSet.Contains(guideID) == false) guideFinishSet.Add(guideID);
                finishGuiIdServerSet.Add(guideID);
            }
            foreach (GuideID[] guideGroup in guideGroupList)
            {
                if (finishGuiIdServerSet.Contains(guideGroup[^1]) == false)
                {
                    foreach (GuideID guideID in guideGroup)
                    {
                        if (guideFinishSet.Contains(guideID) == true) guideFinishSet.Remove(guideID);
                    }
                }
                else
                {
                    Finish(guideGroup, false);
                }
            }
            if (Player.Level >= startGuidePassLevel && !IsFinished(GuideID.starterGuide))
            {
                Finish(GuideID.starterGuide);
            }
            GuideManager.UpdatePopwindowFlag();
        }
        // 完成多个引导
        public static void Finish(IList<GuideID> guideIDs, bool network = true)
        {
            List<GuideID> result = new List<GuideID>();
            for (int i = 0; i < guideIDs.Count; i++)
            {
                if (doingGuideSet.Contains(guideIDs[i])) doingGuideSet.Remove(guideIDs[i]);
                // 只将未完成的添加到列表
                if (!IsFinished(guideIDs[i]))
                {
                    result.Add(guideIDs[i]);
                }
                if (guideFinishSet.Contains(guideIDs[i]) == false) guideFinishSet.Add(guideIDs[i]);
            }

            GuideManager.UpdatePopwindowFlag();

            if (guideIDs.Any(item => item == GuideID.starterGuide))
            {
                if (UIController.Instance.PopwindowFlag) UIController.Instance.OpenAllHideScreens();
            }

            if (!network) return;
            // 如果都完成了
            if (result.Count <= 0) return;

            NetworkManager.Instance.FinishGuide(result.ToArray());
        }

        // 完成所有引导
        public static void FinishAll(bool network = true)
        {
            var type = typeof(GuideID);
            List<GuideID> guideID = new List<GuideID>();
            foreach (var item in type.GetEnumValues())
            {
                guideID.Add((GuideID)item);
            }
            Finish(guideID, network);
        }

        ////已全部改为服务器记录
        //// 清除所有新手引导的完成状态
        //public static void Clear()
        //{
        //    var type = typeof(GuideID);
        //    foreach (var item in type.GetEnumNames())
        //    {
        //        PlayerPrefs.DeleteKey(nameof(GuideID) + item);
        //    }
        //}

        //// 发送比赛胜利邮件
        //public static void SendWinEmail(string name)
        //{
        //    if (IsFinished(GuideID.GUIDE3_1)) return;

        //    NetworkManager.Instance.GuideEmail(GuideEmailID.WinEmail, name);
        //}

        //// 发送比赛平局邮件
        //public static void SendDeuceEmail(string name)
        //{
        //    if (IsFinished(GuideID.GUIDE3_1)) return;

        //    NetworkManager.Instance.GuideEmail(GuideEmailID.DeuceEmail, name);
        //}

        //// 发送比赛失败邮件
        //public static void SendFailEmail(string name)
        //{
        //    if (IsFinished(GuideID.GUIDE3_1)) return;

        //    NetworkManager.Instance.GuideEmail(GuideEmailID.FailEmail, name);
        //}

        // 发送引导结束邮件
        public static void SendGuideEndEmail()
        {
            ChallengeClubConfig challengeClubConfigGuide = Configs.ChallengeClub.GetConfig(0);//0	训练俱乐部
            NetworkManager.Instance.GuideEmail(GuideEmailID.GuideEndEmail, challengeClubConfigGuide.Name);
        }

        public static readonly int UpLevelCardID = 103001;
        public static CardItem UpLevelCardItem = null;

        /// <summary>
        /// 刷新是否可以弹出一系列的隐藏的窗口的状态（触发礼包和活动之类的）
        /// </summary>
        public static void UpdatePopwindowFlag()
        {
            bool flag = true;
            if (flag == true && Player.InBattleAni == true) flag = false;//在战斗动画中
            if (flag == true && GuideManager.InForceGuide == true) flag = false;//在强制引导中
            if (flag == true && LoginManager.Instance.isBeforeLoadingEnd == true) flag = false;//还没登陆游戏
            if (flag == true && LoginManager.Instance.isDoingSilenceReLogin == true) flag = false;//正在默默重连
            if (flag == true && LoginManager.Instance.isCheckingSilenceReLoginHeart == true) flag = false;//正在检测是否断线
            if (flag == true && LoginManager.Instance.isNeedCloseClientAfterChangeAccount == true) flag = false;//切换账号后需要关闭游戏
            UIController.Instance.PopwindowFlag = flag;

            //#if DEBUG
            //            Debug.LogFormat("Player.InBattleAni = {0} , GuideManager.InForceGuide = {1} , LoginManager.Instance.isBeforeLoadingEnd = {2} , LoginManager.Instance.isDoingSilenceReLogin = {3} , LoginManager.Instance.isCheckingSilenceReLoginHeart = {4} , LoginManager.Instance.isNeedCloseClientAfterChangeAccount = {5} , UIController.Instance.PopwindowFlag = {6}", Player.InBattleAni, GuideManager.InForceGuide, LoginManager.Instance.isBeforeLoadingEnd, LoginManager.Instance.isDoingSilenceReLogin, LoginManager.Instance.isCheckingSilenceReLoginHeart, LoginManager.Instance.isNeedCloseClientAfterChangeAccount, UIController.Instance.PopwindowFlag);
            //#endif

        }
    }
}