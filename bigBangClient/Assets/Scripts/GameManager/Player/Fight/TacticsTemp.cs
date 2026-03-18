using System.Collections.Generic;
using System.Linq;
using GameConfig;
using GameConfig.Config;
using Protocol;

namespace BigBang
{
    public class TacticsTemp : FormationTempBase
    {
        public List<int> TacticsIdList { get; set; }

        public TacticsTemp(TacticsTempInfo data)
        {
            UnPack(data);
        }

        public TacticsTemp()
        {
            
        }

        public void UnPack(TacticsTempInfo data)
        {
            Type = FormationTempType.Custom;
            TempId = data.TacticsTempId;
            Name = data.Name;
            TacticsIdList = data.TacticsIdList.ToList();
            CreateTime = data.CreateTime;
        }

        public void InitFromConfig(SysTacticsConfig config)
        {
            if (config == null) return;
            TempId = config.Id;
            Type = FormationTempType.System;
            Name = config.Name;
            TacticsIdList = config.TacticsIdList.ToList();
            CreateTime = 0;
        }

        public bool CheckSame(List<int> tacticsIdList)
        {
            if (tacticsIdList.Count != TacticsIdList.Count) return false;
            TacticsIdList.Sort();

            for (int i = 0; i < tacticsIdList.Count; i++)
            {
                if (tacticsIdList[i] != TacticsIdList[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}