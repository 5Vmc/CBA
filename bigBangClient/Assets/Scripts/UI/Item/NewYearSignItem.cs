using System;
using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using DG.Tweening;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class NewYearSignItem : MonoBehaviour
{
    [SerializeField] private RectTransform lightPanel = null;
    [SerializeField] private RectTransform normalPanel = null;
    [SerializeField] private RectTransform addPanel = null;
    [SerializeField] private BabuButton addSignButton = null;
    [SerializeField] private RectTransform timePanel = null;
    [SerializeField] private TMP_Text timeText = null;
    [SerializeField] private InventoryItem inventoryItem = null;
    [SerializeField] private RectTransform hasGetPanel = null;
    [SerializeField] private RectTransform newYearSignItem = null;
    [SerializeField] private Image lineImage = null;

    private void OnEnable()
    {
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
        addSignButton.OnClick += OnClickAddSignButton;
        isLeft = Utility.GetRandomBool();
        if (isLeft)
            this.transform.SetLocalRotationZ(Utility.GetRandomFloat(-3, 0));
        else
            this.transform.SetLocalRotationZ(Utility.GetRandomFloat(0, 3));
        StartMove();
    }
    private void OnDisable()
    {
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
        addSignButton.OnClick -= OnClickAddSignButton;
        StopMove();
    }
    private void RefreshLeftTime()
    {
        if (activityData == null) return;
        if (timePanel.gameObject.activeSelf == false) return;
        DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
        DateTime startDT = DateTimeOffset.FromUnixTimeSeconds(activityData.StartTime).DateTime.ToLocalTime();
        TimeSpan timeSpan = serverDT - startDT;
        int openDayCount = timeSpan.Days;
        int needOpenDay = itemIndex - 1;
        bool isMoreThenDay = needOpenDay - openDayCount > 1;
        if (isMoreThenDay)
        {
            timeText.text = "{0}天".SafeFormat(needOpenDay - openDayCount - 1);
        }
        else
        {
            DateTime itemStartDT = startDT.AddDays(itemIndex - 1);
            int leftSec = (int)(itemStartDT - serverDT).TotalSeconds;
            if (leftSec < 0)
            {
                RefreshData();
                return;
            }
            timeText.text = "{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn(leftSec));
        }
    }

    public ActivityData activityData = null;
    public WishSignConfig wishSignConfig = null;
    public int itemIndex = 0;
    /// <param name="itemIndex">从 1 开始</param>
    public void SetData(ActivityData activityData, WishSignConfig wishSignConfig, int itemIndex)
    {
        this.activityData = activityData;
        this.wishSignConfig = wishSignConfig;
        this.itemIndex = itemIndex;

        RefreshData();
        RefreshLeftTime();
    }

    public bool isOpen = false;
    public bool isSetReward = false;
    public bool isHasGet = false;
    public bool isLight = false;
    public GameItem gameItem = null;
    public int wishIndex = 0;
    public void RefreshData()
    {
        isLight = ActivityController.Instance.wishSignRewards.Count == itemIndex - 1 && Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100) >= 100;
        lightPanel.gameObject.SetActive(isLight);
        normalPanel.gameObject.SetActive(!isLight);

        DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
        DateTime startDT = DateTimeOffset.FromUnixTimeSeconds(activityData.StartTime).DateTime.ToLocalTime();
        TimeSpan timeSpan = serverDT - startDT;
        int openDayCount = timeSpan.Days;

        timePanel.gameObject.SetActive(false);
        addPanel.gameObject.SetActive(false);
        hasGetPanel.gameObject.SetActive(false);

        isOpen = itemIndex <= openDayCount + 1;
        isSetReward = ActivityController.Instance.wishSigns.Count >= itemIndex;
        isHasGet = ActivityController.Instance.wishSignRewards.Count >= itemIndex;
        if (!isOpen)
        {
            timePanel.gameObject.SetActive(true);
        }
        else if (!isSetReward)
        {
            addPanel.gameObject.SetActive(true);
        }
        else if (isHasGet)
        {
            hasGetPanel.gameObject.SetActive(true);
        }
        if (isSetReward)
        {
            wishIndex = ActivityController.Instance.wishSigns[itemIndex - 1];
            string wishRewardStr = wishSignConfig.Rewards.Split('|')[wishIndex - 1];
            gameItem = GameItemUtils.CreateGameItem(wishRewardStr);
            inventoryItem.SetData(gameItem);
        }
        inventoryItem.gameObject.SetActive(isSetReward);
    }

    private void OnClickAddSignButton(BabuButton _)
    {
        if (addPanel.gameObject.activeSelf == false) return;
        if (ActivityController.Instance.wishSigns.Count < itemIndex - 1)
        {
            Tips.PopTips("请按顺序设置许愿签");
            return;
        }
        if (activityData.clientType == ActivityClientType.NewYearWish) UIController.Instance.OpenWindow<NewYearSignSelectUI>(new NewYearSignSelectUIProperties(this));
        if (activityData.clientType == ActivityClientType.SpringFestivalWish) UIController.Instance.OpenWindow<DragonYearSignSelectUI>(new DragonYearSignSelectUIProperties(this));
        if (activityData.clientType == ActivityClientType.LabourDaySign) UIController.Instance.OpenWindow<LabourDaySignSelectUI>(new LabourDaySignSelectUIProperties(this));
        if (activityData.clientType == ActivityClientType.DragonBoatFestivalSign) UIController.Instance.OpenWindow<DragonBoatFestivalSignSelectUI>(new DragonBoatFestivalSignSelectUIProperties(this));
        if (activityData.clientType == ActivityClientType.Olympics2024Sign) UIController.Instance.OpenWindow<Olympics2024SignSelectUI>(new Olympics2024SignSelectUIProperties(this));
    }

    //private Sequence seq = null;
    //public void StartMove()
    //{
    //    seq = DOTween.Sequence();
    //    seq.AddTo(this.gameObject);
    //    seq.Append(this.transform.DOLocalRotate(new Vector3(0, 0, 3), 5f));
    //    seq.AppendInterval(0.1f);
    //    seq.Append(this.transform.DOLocalRotate(new Vector3(0, 0, -3), 5f));
    //    seq.AppendInterval(0.1f);
    //    seq.SetLoops(-3);
    //}
    //public void StopMove()
    //{
    //    seq?.Kill();
    //    seq = null;
    //    this.transform.SetLocalRotationZ(-3);
    //}

    [SerializeField] private float moveAngle = 3.0f;
    private UnityTimer.Timer timer = null;
    private Sequence seq = null;
    private bool isLeft = false;
    public void StartMove()
    {
        seq = DOTween.Sequence();
        seq.AddTo(this.gameObject);
        if (isLeft)
            seq.Append(this.transform.DOLocalRotate(new Vector3(0, 0, Utility.GetRandomFloat(0, moveAngle)), Utility.GetRandomFloat(1.0f, 1.5f)));
        else
            seq.Append(this.transform.DOLocalRotate(new Vector3(0, 0, Utility.GetRandomFloat(-moveAngle, 0)), Utility.GetRandomFloat(1.0f, 1.5f)));
        isLeft = !isLeft;
        seq.AppendCallback(() =>
        {
            timer = UnityTimer.Timer.Register(this.gameObject, 0.1f, () =>
            {
                StartMove();
            });
        });
    }
    public void StopMove()
    {
        timer?.Cancel();
        timer = null;
        seq?.Kill();
        seq = null;
        this.transform.SetLocalRotationZ(-moveAngle);
    }


}
