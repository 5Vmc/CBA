using Babu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BigBang
{
    public class PanelNodePath
    {
        public static string Home = "Home";
        /// <summary>
        /// 新活动集合页
        /// </summary>
        public static string Activity = "Home/Activity";
        public static string Home_ClassicPVE = "Home/ClassicPVE";
        public static string Home_ClassicPVE_Level1 = "Home/ClassicPVE/Level1";
        public static string Home_ClassicPVE_Level2 = "Home/ClassicPVE/Level2";
        public static string Home_ClassicPVE_Level3 = "Home/ClassicPVE/Level3";
        public static string Home_FB = "Home/FB";
        public static string Home_FBTower = "Home/FB/Tower";
        public static string Home_FBClassicHero = "Home/FB/ClassicHero";
        public static string Home_ClassicArena = "Home/ClassicArena";
        public static string Home_ClassicPVP = "Home/ClassicPVP";
        public static string Home_Train = "Home/Train";
        public static string Home_SkillTrain = "Home/SkillTrain";
        public static string Home_Task = "Home/Task";
        public static string Home_Games = "Home/Games";
        public static string Home_Career = "Home/Career";
        public static string Home_Bag = "Home/Bag";
        public static string Home_Achieve = "Home/Honour/Achieve";
        public static string Home_PayFirst = "Home/PayFirst";
        public static string Home_Mail = "Home/Mail";
        public static string Home_Festival = "Home/Festival";
        public static string Home_FestivalLogin = "Home/Festival/Login";
        public static string Home_FestivalTotalPay = "Home/Festival/TotalPay";
        public static string Card = "Player";
        public static string Formation = "Formation";
        public static string Recruit = "Recruit";
        public static string Shop = "Shop";
        public static string GiftShop = "Shop/GiftShop";
        public static string Home_Christmas = "Home/Christmas";
        public static string Home_NewYear = "Home/NewYear";
        public static string Home_NewYearSign = "Home/NewYearSign";
        public static string Home_Hundred = "Home/Hundred";
        public static string Home_RedEnvlope = "Home/RedEnvlope";
        public static string Home_Honour = "Home/Honour/Honour";
    }

    public class RedDotManager
    {
        //public GameObject dotImg;
        private static RedDotManager _instance;

        public static RedDotManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RedDotManager();
                return _instance;
            }
        }

        public RedDotManager()
        {
            //dotImg = Resources.Load<GameObject>("DotNodeImg");
        }

        public RedDotNode rootNode;
        private Dictionary<string, RedDotNode> _allNodes;

        #region 模块
        #endregion



        //以"/"作为分隔符
        private bool isInitRedTree = false;
        public void InitRedTree()
        {
            isInitRedTree = true;
            _allNodes = new Dictionary<string, RedDotNode>();
            rootNode = new RedDotNode(null);
            foreach (var p in typeof(PanelNodePath).GetFields())
            {
                var fullpath = (string)p.GetValue(null);
                var splitKeys = fullpath.Split("/");
                RedDotNode curNode = rootNode;
                foreach (var splitKey in splitKeys)
                {
                    curNode = curNode.GetOrAddChild(splitKey);
                }
                _allNodes[fullpath] = curNode;
            }
        }

        private RedDotNode AddRedDotNode(string fullpath)
        {
            var splitKeys = fullpath.Split("/");
            RedDotNode curNode = rootNode;
            if (curNode == null)
            {
                Debug.LogWarning("RedDotManager , AddRedDotNode , curNode == null , fullpath = " + fullpath);
            }
            foreach (var splitKey in splitKeys)
            {
                curNode = curNode.GetOrAddChild(splitKey);
            }
            _allNodes[fullpath] = curNode;
            return curNode;
        }

        /// <summary>
        /// 确认红点，没有会自动添加
        /// </summary>
        /// <param name="parentpath">一般是面板id，如果有二级面板要自己拼接 a/b 例如：章节关系</param>
        /// <param name="prefix">前缀有必要，避免不同系统中id重复</param>
        /// <param name="nodeid">一般直接用id就可以了</param>
        public RedDotNode ConfirmNode(string parentpath, string nodeid)
        {
            if (parentpath == "")
            {
                throw new Exception("没有设置小红点的父节点路径");
            }

            return AddRedDotNode(parentpath + nodeid);
        }

        /// <summary>
        /// 获取Panel的最终节点，通常adapter在这个节点下直接添加子node，如果没有panel节点，就要到InitRedNode里去注册。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedDotNode GetPanelNode(string key)
        {
            return _allNodes[key];
        }
    }
}