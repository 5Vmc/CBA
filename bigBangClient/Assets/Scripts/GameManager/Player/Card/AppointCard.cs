using Protocol;

namespace BigBang
{
    public class AppointCard
    {
        public int Index { get; set; }
        public int CardId { get; set; }
        public RecruitAppointCardState State { get; set; }

        public AppointCard(int index)
        {
            Index = index;
        }

        public void UnPack(AppointCardInfo data)
        {
            CardId = data.CardId;
            State = (RecruitAppointCardState) data.State;
        }
    }
}