using BigBang;
using BigBang.UI;
using GameConfig.Config;

public class LiXiaoXuMainPagePad : MainPagePadBase, IActivity
{
    public void LoadActivity(ActivityData ActivityData)
    {
        base.OnShow();
    }
    protected override int GetCardId()
    {
        return CardId.LiXiaoXu;
    }
    protected override void OnClickGoto()
    {
        UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.SpringFestivalTask, new() { ActivityClientType.LiXiaoXuMainPage, ActivityClientType.SpringFestivalTask, ActivityClientType.SpringFestivalGift, ActivityClientType.DragonYearRedEnvelope }));
    }
}
