using UnityEngine;

namespace BigBang.Battle
{
    public sealed class BattleUtil
    {
        private static readonly BattleUtil instance = new BattleUtil();
        static BattleUtil()
        {
        }
        private BattleUtil()
        {
        }
        public static BattleUtil Instance
        {
            get
            {
                return instance;
            }
        }

        public static GameObject  CloneBall(GameObject ball)
        {
            return UnityEngine.GameObject.Instantiate(ball);
        }
    }
}