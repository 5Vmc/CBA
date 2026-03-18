using System.Linq;

using GameConfig;
using Protocol;

namespace BigBang
{
    public class FightMessageTrigger
    {
        private FightData _fight;
        private System.Random _random;

        public void Init(FightData fight)
        {
            _fight = fight;

            // 保证同一个fightID随机种子一样
            int seed = _fight.FightId.Sum(item => item);
            _random = new System.Random(seed);
        }

        public void OnTrigger(FightFrameEvent data)
        {
        }
    }
}