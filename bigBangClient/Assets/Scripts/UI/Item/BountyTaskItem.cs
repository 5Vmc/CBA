using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.BountyTaskManager;

public class BountyTaskItem : MonoBehaviour
{
    [SerializeField] private RectTransform doingPanel = null;
    [SerializeField] private HorizontalLayoutGroup playerItemLayout = null;
    [SerializeField] private Image itemBgImage = null;
    [SerializeField] private RectTransform normalPanel = null;
    [SerializeField] private Image lineImage = null;
    [SerializeField] private Image flagBgImage = null;
    [SerializeField] private HorizontalLayoutGroup doingInventoryLayout = null;
    [SerializeField] private BabuButton startButton = null;
    [SerializeField] private RectTransform lockPanel = null;
    [SerializeField] private Image lockImage = null;
    [SerializeField] private TMP_Text lockText = null;
    [SerializeField] private HorizontalLayoutGroup startInventoryLayout = null;
    [SerializeField] private Image flagImage = null;
    [SerializeField] private TMP_Text taskNameText = null;
    [SerializeField] private RectTransform canStartPanel = null;
    [SerializeField] private TMP_Text taskDescText = null;
    [SerializeField] private TMP_Text doubelDescText = null;
    [SerializeField] private RectTransform doingTextPanel = null;
    [SerializeField] private TMP_Text doingLeftTimeText = null;
    [SerializeField] private TMP_Text doingTipText = null;
    [SerializeField] private BabuButton getButton = null;
    [SerializeField] private TMP_Text doubleText = null;


    protected void OnEnable()
    {
        startButton.OnClick += OnClickStartButton;
        getButton.OnClick += OnClickGetButton;
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
    }
    protected void OnDisable()
    {
        startButton.OnClick -= OnClickStartButton;
        getButton.OnClick -= OnClickGetButton;
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
    }

    BountyTaskData bountyTaskData;
    public void SetData(BountyTaskData bountyTaskData)
    {
        this.bountyTaskData = bountyTaskData;

        normalPanel.gameObject.SetActive(!bountyTaskData.isLock);
        lockPanel.gameObject.SetActive(bountyTaskData.isLock);

        isNeedUpdateTime = false;

        if (bountyTaskData.isLock)
        {
            RefreshLock();
        }
        else
        {
            RefreshNormal();
        }
    }

    private void RefreshLock()
    {
        if (bountyTaskData.isLock && bountyTaskData.userLevelConfig != null)
        {
            lockText.text = "{0}级开启第{1}个任务".SafeFormat(bountyTaskData.userLevelConfig.Id, bountyTaskData.userLevelConfig.BountyTaskCount);
        }
    }

    private async void RefreshNormal()
    {
        flagImage.sprite = await SpriteProxy.GetBountyTaskBadge(bountyTaskData.bountyTaskConfig.Icon);
        taskNameText.text = bountyTaskData.bountyTaskConfig.Name;

        doingPanel.gameObject.SetActive(bountyTaskData.IsStart);
        canStartPanel.gameObject.SetActive(!bountyTaskData.IsStart);
        if (bountyTaskData.IsStart)
        {
            RefreshDoing();
        }
        else
        {
            RefreshCanStart();
        }
    }

    [SerializeField] private List<InventoryItem> startInventoryList = new();
    private void RefreshCanStart()
    {
        taskDescText.text = bountyTaskData.bountyTaskConfig.Desc;
        doubelDescText.text = "双倍奖励：{0}".SafeFormat(bountyTaskData.bountyTaskConfig.Option);
        GameItemUtils.SetRewards(startInventoryList, bountyTaskData.bountyTaskConfig.Rewards);
    }
    public class TempFormation : FormationBase
    {
        public override bool IsFightFormation()
        {
            return false;
        }

        public override void SaveToServer()
        {
            return;
        }

        public override void UpdateCardFormationInfo()
        {
            return;
        }

        public override void UseFormationTemp(FormationTemp temp)
        {
            return;
        }
    }
    private void OnClickStartButton(BabuButton sender)
    {
        //BountyTaskManager.Instance.StartBountyTask();

        TempFormation tempFormation = new();
        tempFormation.FormationName = "";
        tempFormation.BaseFormationName = "";
        tempFormation.FormationId = FormationID.Bounty;
        tempFormation.TacticsIdList = new() { 101, 201 };
        UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(tempFormation, false, FormationUI.FormationShowType.Formation, FormationID.Bounty, AfterFormationCallBack, bountyTaskData.bountyTaskConfig.OnBattle));
    }
    private void AfterFormationCallBack(FormationBase formation, bool isAllLimitPass)
    {
        BountyTaskManager.Instance.StartBountyTask(bountyTaskData.bountyTaskInfo.Id, formation.Pack(), isAllLimitPass, null);
    }

    [SerializeField] private List<BountyTaskDoingPlayerItem> playerItemList = new();
    [SerializeField] private List<InventoryItem> doingInventoryList = new();
    private void RefreshDoing()
    {
        doubleText.gameObject.SetActive(bountyTaskData.bountyTaskInfo.Twice);
        for (int i = 0; i < playerItemList.Count; i++)
        {
            BountyTaskDoingPlayerItem playerItem = playerItemList[i];
            if (i < bountyTaskData.bountyTaskInfo.CardIds.Count)
            {
                playerItem.gameObject.SetActive(true);
                int cardId = bountyTaskData.bountyTaskInfo.CardIds[i];

                var cardConfig = Configs.CardModel.GetConfig(cardId);

                //PlayerCard playerCard = Player.CardManager.GetCard(cardId);
                playerItem.SetData(cardConfig);
            }
            else
            {
                playerItem.gameObject.SetActive(false);
            }
        }
        GameItemUtils.SetRewards(doingInventoryList, bountyTaskData.bountyTaskConfig.Rewards);

        RefreshGetButton();
        if (!bountyTaskData.IsFinish)
        {
            isNeedUpdateTime = true;
            RefreshLeftTime();
        }
    }
    private void RefreshGetButton()
    {
        doingTextPanel.gameObject.SetActive(!bountyTaskData.IsFinish);
        getButton.gameObject.SetActive(bountyTaskData.IsFinish);
    }

    private bool isNeedUpdateTime = false;
    private void RefreshLeftTime()
    {
        if (isNeedUpdateTime == false) return;
        if (bountyTaskData == null) return;
        if (bountyTaskData.bountyTaskInfo == null) return;
        if (bountyTaskData.bountyTaskConfig == null) return;
        int needTime = bountyTaskData.bountyTaskConfig.Time;
        int startTime = bountyTaskData.bountyTaskInfo.StartTime;
        int nowTime = (int)DataConvUtil.ServerTime;
        int leftSec = (startTime + needTime) - nowTime;
        if (leftSec <= 0)
        {
            RefreshGetButton();
        }
        else
        {
            doingLeftTimeText.text = TimeUtils.FormatLeftTimeWithHour(leftSec);
        }
    }

    private void OnClickGetButton(BabuButton sender)
    {
        BountyTaskData bountyTaskDataGet = bountyTaskData;
        string rewards = bountyTaskDataGet.bountyTaskConfig.Rewards;
        NetworkManager.Instance.CollectBountyTaskReward(bountyTaskDataGet.bountyTaskInfo.Id, (resp) =>
        {
            BountyTaskManager.Instance.completedCount++;
            if (bountyTaskDataGet.bountyTaskInfo.Twice)
            {
                rewards = rewards + "|" + rewards;
            }
            var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(rewards).ToList(), null, "获得悬赏任务奖励");
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            EventManager.Instance.Dispatch(EventID.OnBountyTaskDataRefreshTopBox);
        });
    }

}
