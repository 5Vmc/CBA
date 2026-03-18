using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class HonourItem : MonoBehaviour
{
    [SerializeField] private BabuButton honourItem = null;
    [SerializeField] private RectTransform cupImageRoot = null;
    [SerializeField] public Image cupImage = null;
    [SerializeField] private RectTransform titlePanel = null;
    [SerializeField] private Image bgImage = null;
    [SerializeField] private Image dotNodeImg = null;
    [SerializeField] private TMP_Text titleText = null;
    [SerializeField] private TMP_Text countText = null;

    private void OnEnable()
    {
        honourItem.OnClick += OnClickHonourItem;
    }
    private void OnDisable()
    {
        honourItem.OnClick -= OnClickHonourItem;
    }
    private void OnClickHonourItem(BabuButton _)
    {
        UIController.Instance.OpenWindow<HonourDetailUI>(new HonourDetailUIProperties(this, honourGroupData, achievementData));
    }

    private HonourGroupData honourGroupData = null;
    private AchievementData achievementData = null;
    public async void SetData(HonourGroupData honourGroupData)
    {
        this.honourGroupData = honourGroupData;
        if (honourGroupData == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);

        achievementData = honourGroupData.list[0];
        foreach (AchievementData achievementDataI in honourGroupData.list)
        {
            if (achievementDataI.Current >= achievementDataI.Config.Target[0])
            {
                achievementData = achievementDataI;
            }
        }
        bool isFinish = achievementData.IsComplete || achievementData.Received == 1;
        cupImage.sprite = await SpriteProxy.GetHonourCup(achievementData.Config.Icon);
        cupImage.SetGray(!isFinish);
        cupImage.SetAlpha(isFinish ? 1 : 0.5f);
        // 列表中的奖杯不再需要红点
        // bool isNew = achievementData.Received == 0;
        // dotNodeImg.gameObject.SetActive(isNew);
        titleText.text = achievementData.Config.Name;
        if (achievementData.Current <= 1)
        {
            countText.text = "";
        }
        else
        {
            countText.text = "x{0}".SafeFormat(achievementData.HonourCurrentShow);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(titleText.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(countText.transform as RectTransform);
        (countText.transform as RectTransform).SetAnchoredPositionX(titleText.rectTransform.sizeDelta.x / 2 + 5);
    }
}
