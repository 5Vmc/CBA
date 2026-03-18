using Babu;
using Babu.BigNumber;
using Protocol;
using Utils;

namespace BigBang
{
    public class InviteMatch
    {
        public int Id { get; set; }

        // public int OpponentClubId { get; set; }
        public string OpponentName { get; set; }

        public string OrganizerIcon { get; set; }

        public string Organizer { get; set; }

        public string Content { get; set; }
        public string Place { get; set; }

        public int MineScore { get; set; }
        public int OpponentScore { get; set; }
        public InviteMatchState State { get; set; } = InviteMatchState.Init;
        public long CdEndTime { get; set; }
        public BigNumber BaseReward { get; set; }

        public InviteMatch(InviteMatchInfo data)
        {
            Id = data.Id;
            // OpponentName = data.OpponentClubId;
            MineScore = data.MineScore;
            OpponentScore = data.OpponentScore;
            State = (InviteMatchState) data.State;
            CdEndTime = data.CdEndTime;
            BaseReward = data.BaseReward.ToBigNumber();

            OpponentName = data.OpponentName;
            Place = data.Place;
            Organizer = data.Organizer;
            OrganizerIcon = data.OrganizerIcon;
            Content = data.Content;

            Content = Content.Replace("{Organizer}", Organizer);
            Content = Content.Replace("{OpponentName}", OpponentName);
            Content = Content.Replace("{Place}", Place);
        }

        public bool IsCdEnd()
        {
            return !(State == InviteMatchState.Rewarded && Utils.DataConvUtil.ServerTimeEx < CdEndTime);
        }
    }
}