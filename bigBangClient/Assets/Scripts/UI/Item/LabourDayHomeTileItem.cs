using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

public class LabourDayHomeTileItem : MonoBehaviour
{
    [SerializeField] private BabuButton tileItem = null;
    private void OnEnable()
    {
        tileItem.OnClick += OnClickTileItem;
    }
    private void OnDisable()
    {
        tileItem.OnClick -= OnClickTileItem;
    }
    private void OnClickTileItem(BabuButton _)
    {
        if (innerIndex == 0)
        {
            Tips.PopTips("这是起点");
            return;
        }
        if (innerIndex <= targetInnerIndex)
        {
            Tips.PopTips("该奖励已被领取");
            return;
        }
        UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
    }

    [SerializeField] private TMP_Text posText = null;

    public int innerIndex;
    public Vector2Int pos;
    [SerializeField] private Image emptyImage = null;
    [SerializeField] private Image startImage = null;
    [SerializeField] private List<Image> qualityImageList = new();
    [SerializeField] public Image iconImage = null;
    [SerializeField] private TMP_Text numText = null;

    private ActivityData activityData = null;
    public GameItem gameItem = null;
    public void SetData(int innerIndex, Vector2Int pos)
    {
        this.innerIndex = innerIndex;
        this.pos = pos;
    }
    public void SetActivityData(ActivityData activityData)
    {
        this.activityData = activityData;
    }
    private int targetInnerIndex = 0;
    public async void RefreshInfo(int targetInnerIndex)
    {
        this.targetInnerIndex = targetInnerIndex;
        posText.text = pos.ToString();
        emptyImage.gameObject.SetActive(false);
        startImage.gameObject.SetActive(false);
        SetBg(-1);
        iconImage.gameObject.SetActive(false);
        numText.gameObject.SetActive(false);

        if (innerIndex == 0)
        {
            startImage.gameObject.SetActive(true);
            return;
        }
        if (innerIndex <= targetInnerIndex)
        {
            emptyImage.gameObject.SetActive(true);
            return;
        }
        iconImage.gameObject.SetActive(true);
        numText.gameObject.SetActive(true);
        int festivalTrivalId = activityData.cfg.Id * 1000 + LabourDayManager.Instance.mapIndex * 30 + innerIndex;
        FestivalTravelConfig festivalTravelConfig = Configs.FestivalTravel.GetConfig(festivalTrivalId);
        if (festivalTravelConfig == null)
        {
            Debug.LogWarning("LabourDayHomeTileItem , festivalTravelConfig == null , festivalTrivalId = " + festivalTrivalId);
            return;
        }
        gameItem = GameItemUtils.CreateGameItem(festivalTravelConfig.Reward);
        numText.text = gameItem.Count.ToString();
        SetBg(gameItem.GetQuality());
        iconImage.sprite = await gameItem.GetIcon();
    }

    private void SetBg(int quality)// 设置品质
    {
        for (int i = 0; i < qualityImageList.Count; i++)
        {
            qualityImageList[i].gameObject.SetActive(i == quality - 1);
        }
    }
}
