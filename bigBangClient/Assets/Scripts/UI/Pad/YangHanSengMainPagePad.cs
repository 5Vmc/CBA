using BigBang;
using BigBang.UI;
using GameConfig.Config;

public class YangHanSengMainPagePad : MainPagePadBase, IActivity
{
    public void LoadActivity(ActivityData ActivityData)
    {
        base.OnShow();
    }
    protected override int GetCardId()
    {
        return CardId.YangHanSen;
    }
    protected override void OnClickGoto()
    {
        UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.SpringFestivalWish, new() { ActivityClientType.YangHanSenMainPage, ActivityClientType.SpringFestivalWish }));
    }
}
