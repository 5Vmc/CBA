using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.SpriteNames;

public class DragonYearGiftPad : MonoBehaviour, IActivity
{
    [SerializeField] private ScrollRect giftPanelScrollView = null;
    [SerializeField] private GridLayoutGroup content = null;
    [SerializeField] private GameObject newYearGiftItemPrefab = null;
    [SerializeField] private TMP_Text leftTimeText = null;
    private List<NewYearGiftItem> itemList = new();

    private void OnEnable()
    {
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
    }
    private void OnDisable()
    {
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
    }
    private ActivityData activityData = null;
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        RefreshLeftTimeOneSec();
        RefreshGiftList();
    }
    [SerializeField] private string leftTimeStr = "活动剩余时间：{0}";
    private void RefreshLeftTimeOneSec()
    {
        if (activityData == null) return;
        long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
        leftTimeText.text = leftTimeStr.SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
    }
    private void RefreshGiftList()
    {
        List<GiftShopConfig> giftShopConfigList = Configs.GiftShop.GetConfigList().Where(g => g.Type == activityData.cfg.Id).ToList();
        giftShopConfigList.Insert(0, null);
        int maxCount = Mathf.Max(giftShopConfigList.Count, itemList.Count);
        for (int i = 0; i < maxCount; i++)
        {
            if (i < giftShopConfigList.Count && i >= itemList.Count)
            {
                GameObject newYearGiftItemGameObject = GameObject.Instantiate(newYearGiftItemPrefab, content.transform);
                NewYearGiftItem newYearGiftItemAdd = newYearGiftItemGameObject.GetComponent<NewYearGiftItem>();
                itemList.Add(newYearGiftItemAdd);
            }
            NewYearGiftItem newYearGiftItem = itemList[i];
            if (i >= giftShopConfigList.Count)
            {
                newYearGiftItem.gameObject.SetActive(false);
            }
            else
            {
                GiftShopConfig giftShopConfig = giftShopConfigList[i];
                if (giftShopConfig == null)
                {
                    newYearGiftItem.SetData(activityData);
                }
                else
                {
                    newYearGiftItem.SetData(giftShopConfig);
                }
                newYearGiftItem.gameObject.SetActive(true);
            }
        }
        giftPanelScrollView.verticalNormalizedPosition = 1f;
    }
}
