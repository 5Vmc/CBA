using GameConfig;
using GameConfig.Config;
using Protocol;
using System.Collections.Generic;
using System.Linq;

namespace BigBang
{
    public class FormationTemp : FormationTempBase
    {
        public List<int> BoardIdList { get; set; } = new List<int>();

        public FormationTemp(FormationTempInfo data)
        {
            UnPack(data);
        }

        public FormationTemp()
        {
        }

        public void UnPack(FormationTempInfo data)
        {
            Type = FormationTempType.Custom;
            TempId = data.FormationTempId;
            Name = data.Name;
            CreateTime = data.CreateTime;

            BoardIdList = data.BoardIdList.ToList();
        }

        public void InitFromConfig(SysFormationConfig config)
        {
            if (config == null) return;
            TempId = config.Id;
            Type = FormationTempType.System;
            Name = config.Name;
            BoardIdList = config.BoardIdList.ToList();
        }

        public bool CheckSame(List<int> boardIdList)
        {
            if (boardIdList.Count != BoardIdList.Count) return false;
            BoardIdList.Sort();

            for (int i = 0; i < boardIdList.Count; i++)
            {
                if (boardIdList[i] != BoardIdList[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetPositionCount(PositionType posType)
        {
            int count = 0;
            for (int i = 0; i < BoardIdList.Count; i++)
            {
                var cfg = Configs.FormationBoard.GetConfig(BoardIdList[i]);
                var cfg1 = Configs.SeparatedPosition.GetConfig(cfg.SeparatedPosition);
                if (cfg1.Position == (int)posType)
                    count++;
            }
            return count;
        }
    }


}
