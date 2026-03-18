
namespace BigBang
{
    public class RecruitLogic
    {
        public static int GetCostDiamond(RecruitCountType recruitCountType)
        {
            return recruitCountType == RecruitCountType.Once ? 300 : 2700;
        }

        public static int GetRecruitCount(RecruitCountType recruitCountType)
        {
            if (recruitCountType == RecruitCountType.Once) return 1;
            else if (recruitCountType == RecruitCountType.Ten) return 10;
            return 0;
        }
    }
}