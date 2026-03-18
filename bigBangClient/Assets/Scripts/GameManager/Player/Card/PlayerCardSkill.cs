using GameConfig;
using GameConfig.Config;
using System.Threading.Tasks;
using UnityEngine;

namespace BigBang
{
    public class PlayerCardSkill
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public SkillConfig Config { get; set; }

        public PlayerCardSkill(int id, int level)
        {
            Id = id;
            Level = level;
            Config = Configs.Skill.GetConfig(id);
        }

        public Task<Sprite> GetIcon()
        {
            if (Config == null) return null;
            return SpriteProxy.GetSkillIcon(Config.Icon);
        }

        public int GetEffectaddValue()
        {
            if (Config == null) return 0;
            return Config.EffectaddValue[Level-1];
        }
    }
}