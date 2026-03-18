using BigBang;
using BigBang.UI;
using GameConfig;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class NoviceTargetItem : MonoBehaviour
{
    [SerializeField] private InventoryItem item;
    [SerializeField] private TMP_Text infoTxt;
    [SerializeField] private Button goBtn;
    [SerializeField] private Button getBtn;
    [SerializeField] private Image completedImg;

    [SerializeField] private TMP_Text progressText;

    private int taskID;

    private void OnEnable()
    {
        goBtn.onClick.AddListener(OnGo);
        getBtn.onClick.AddListener(OnGet);
    }

    private void OnDisable()
    {
        goBtn.onClick.RemoveListener(OnGo);
        getBtn.onClick.RemoveListener(OnGet);
    }

    public void SetData(int taskID)
    {
        this.taskID = taskID;
        var cfg = Configs.NoviceTargetTask.GetConfig(taskID);

        if (Player.NoviceTaskManager.IsFinished(taskID))
        {
            completedImg.gameObject.SetActive(Player.NoviceTaskManager.IsObtain(taskID));
            getBtn.gameObject.gameObject.SetActive(!Player.NoviceTaskManager.IsObtain(taskID));
            goBtn.gameObject.SetActive(false);

        }
        else
        {
            getBtn.gameObject.SetActive(false);
            completedImg.gameObject.SetActive(false);
            goBtn.gameObject.SetActive(cfg.ModuleId != 0);
        }

        progressText.gameObject.SetActive(!completedImg.gameObject.activeInHierarchy);

        int currentCount = Player.NoviceTaskManager.GetCurrnetCount(taskID);
        int targetCount = Player.NoviceTaskManager.GetTargetCount(taskID);

        if (cfg.TriggerId == 10005)
        {
            if (currentCount >= targetCount)
                progressText.text = "1/1";
            else
                progressText.text = "<color=red>0</color>/1";
        }
        else if (currentCount >= targetCount)
        {
            progressText.text = $"{currentCount}/{targetCount}";
        }
        else
        {
            progressText.text = $"<color=red>{currentCount}</color>/{targetCount}";
        }
        item.SetGameItemData(GameItemUtils.CreateGameItem(cfg.Reward));
        infoTxt.text = cfg.Name;
    }

    private void OnGo()//使用统一的跳转
    {
        if (Player.NoviceTaskManager.Days > 14)
        {
            Tips.PopTips("活动结束");
            UIController.Instance.HidePanel<NoviceTargetUI>();
            return;
        }
        var cfg = Configs.NoviceTargetTask.GetConfig(taskID);
        var moduleOpen = TriggerManager.Instance.CheckModuleOpen(cfg.ModuleId, true);
        if (moduleOpen)
        {
            TriggerManager.Instance.JumpPanel(cfg.ModuleId);
        }
    }

    //private void OnGo()//旧版本再每个界面单独做了跳转，现在，使用统一的跳转
    //{
    //    if (Player.NoviceTaskManager.Days > GameConst.NOVICE_TASK_END_DADYS)
    //    {
    //        Tips.PopTips("活动结束");
    //        UIController.Instance.HidePanel<NoviceTargetUI>();
    //        return;
    //    }
    //    var cfg = Configs.NoviceTargetTask.GetConfig(taskID);
    //    switch (cfg.Way)
    //    {
    //        case UIID.RegularUI:
    //            // 跳转到常规训练界面
    //            UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Regular));
    //            break;
    //        case UIID.StrengthUI:
    //            // 跳转到强化训练
    //            UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Strength));
    //            break;
    //        case UIID.InviteUI:
    //            // 跳转到邀请赛界面
    //            UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Invite));
    //            break;
    //        case UIID.BigbangUI:
    //            // 跳转到超能训练
    //            UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.BigBang));
    //            break;
    //        case UIID.RecruitUI:
    //            // 跳转到招募界面
    //            if (Player.ChallengeManager.ChallengeId <= 103)
    //            {
    //                Tips.PopTips(LangID.NeedToUnlockRecruit);
    //                return;
    //            }
    //            UIController.Instance.ShowPanel<RecruitUI>();
    //            break;
    //        case UIID.RecycleUI:
    //            // 跳转到回收界面
    //            UIController.Instance.ShowPanel<InventoryUI>(new InventoryUIProperties(InventoryUI.SubUIID.Recycle));
    //            break;
    //        case UIID.SkillTrainUI:
    //            // 跳转到特技学习界面
    //            UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.SkillTrain));
    //            break;
    //        case UIID.ChallengeUI:
    //            if (Player.TrainManager.GetUnlockCount() < 4)
    //            {
    //                Tips.PopTips(LangID.UnlockChallengeTxt);
    //                return;
    //            }
    //            Player.ChallengeManager.OpenChallengeUI();
    //            break;
    //        //case UIID.MyGameUI:
    //        //    //bool isUnlock = Player.TrainManager.GetUnlockCount() >= 10;
    //        //    //if (!isUnlock)
    //        //    //{
    //        //    //    Tips.PopError(ErrorID.NoTenTrainCanUnclockArena);
    //        //    //    return;
    //        //    //}
    //        //    if (!string.IsNullOrEmpty(HomeUI.FightID))
    //        //    {
    //        //        Player.FightManager.FightDataController.WatchFight(HomeUI.FightID);
    //        //        return;
    //        //    }
    //        //    if (HomeUI.HasCompition)
    //        //    {
    //        //        UIController.Instance.ShowPanel<MyGameUI>(new MyGameUIProperties(HomeUI.MainUIResponse.CompitionId, HomeUI.MainUIResponse.LeagueId, HomeUI.LeagueName));
    //        //    }
    //        //    else
    //        //    {
    //        //        // 赛季筹备中
    //        //        Tips.PopError(ErrorID.CompitionIsNotReady);
    //        //    }
    //        //    break;
    //        case UIID.DiamondShopUI:
    //            if (ServerConst.OPEN_BUY == false)
    //            {
    //                Tips.PopTips("测试期间不开放充值");
    //                break;
    //            }
    //            UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Diamond));
    //            break;
    //        case UIID.CardUI:
    //            UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.Card));
    //            break;
    //        case UIID.DailyTaskUI:
    //            UIController.Instance.ShowPanel<TaskUI>(new TaskUIProperties(TaskUI.SubUIID.Daily));
    //            break;
    //        case UIID.RecruitShopUI:
    //            UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Recruit));
    //            break;
    //        case UIID.PVPShopUI:
    //            throw new System.Exception("还没做");
    //            break;
    //        default:
    //            //Tips.PopTips("请配置前往界面");
    //            Debug.LogWarningFormat("NoviceTargetItem , OnGo , NoviceTargetTask way is not program , cfg.Way = {0}", cfg.Way);
    //            break;
    //    }

    //}

    private void OnGet()
    {
        if (Player.NoviceTaskManager.Days > GameConst.NOVICE_TASK_END_DADYS)
        {
            Tips.PopTips("活动结束");
            UIController.Instance.HidePanel<NoviceTargetUI>();
            return;
        }
        NetworkManager.Instance.GetNoviceTaskReward(taskID, response =>
        {
            if (response.Succeed)
            {
                var cfg = Configs.NoviceTargetTask.GetConfig(taskID);
                var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(cfg.Reward).ToList());
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            }
        });
    }
}
