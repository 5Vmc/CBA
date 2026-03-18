using GameConfig;

namespace BigBang
{
    public class FormationData
    {
        public int BoardId { get; set; }
        public int SubstituteIndex { get; set; }
        public FormationCardState State { get; set; }

        public void Clear()
        {
            BoardId = 0;
            SubstituteIndex = 0;
            State = FormationCardState.Reserve;
        }

        public void SetData(FormationCardState state, int boardId, int substituteIndex)
        {
            State = state;
            BoardId = boardId;
            SubstituteIndex = substituteIndex;
        }

        public string GetPositionName()
        {
            var config = Configs.FormationBoard.GetConfig(BoardId);
            if (config == null) return "";
            var positionId = config.SeparatedPosition;
            var positionConfig = Configs.SeparatedPosition.GetConfig(positionId);
            if (positionConfig == null) return "";
            return positionConfig.Abbreviation;
        }

        public int GetPositionId()
        {
            var config = Configs.FormationBoard.GetConfig(BoardId);
            if (config == null) return -1;
            return config.SeparatedPosition;
        }
    }
}