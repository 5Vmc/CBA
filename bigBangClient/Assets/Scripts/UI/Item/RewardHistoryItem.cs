using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class RewardHistoryItem : MonoBehaviour
{
    [SerializeField] private BabuButton rewardHistoryItem = null;
    [SerializeField] private Image hasGetBgImage = null;
    [SerializeField] private Image notGetBgImage = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text hasGetNumText = null;
    [SerializeField] private TMP_Text notGetTipText = null;

    public GameItem gameItem = null;
    public async void SetData(GameItem gameItem)
    {
        this.gameItem = gameItem;
        bool isGet = gameItem.Count > 0;
        hasGetBgImage.gameObject.SetActive(isGet);
        notGetBgImage.gameObject.SetActive(!isGet);
        hasGetNumText.gameObject.SetActive(isGet);
        if (isGet) hasGetNumText.text = "X{0}".SafeFormat(gameItem.Count);
        notGetTipText.gameObject.SetActive(!isGet);
        iconImage.sprite = await gameItem.GetIcon();
    }

    private void OnEnable()
    {
        rewardHistoryItem.OnClick += OnClick;
    }
    private void OnDisable()
    {
        rewardHistoryItem.OnClick -= OnClick;
    }

    private void OnClick(BabuButton _)
    {
        gameItem.ShowTip();
    }
}
