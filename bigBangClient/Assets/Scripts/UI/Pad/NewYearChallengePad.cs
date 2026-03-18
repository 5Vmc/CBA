using BigBang;

public class NewYearChallengePad : MainPagePadBase
{
    protected override int GetCardId()
    {
        return CardId.ChenGuoHao;
    }
    protected override void OnClickGoto()
    {
        if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Games, true))
        {
            TriggerManager.Instance.JumpPanel((int)TriggerModuleType.Games, false, 1);
        }
    }
    protected override void AfterShow()
    {
        RefreshBtnState();
    }
    private void RefreshBtnState()
    {
        challengeBtnUIShiny.enabled = false;
        if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Games, false))
        {
            bool challengeNotPlay = Player.ActivityManager.ShootGameTimesLeft >= GameConst.ChallengeTimes;
            challengeBtnUIShiny.enabled = challengeNotPlay;
        }
    }
}
