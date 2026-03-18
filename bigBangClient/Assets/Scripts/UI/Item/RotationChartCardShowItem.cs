using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using GameConfig;
using UnityEngine;
using UnityEngine.UI;

public class RotationChartCardShowItem : MonoBehaviour
{
    [SerializeField] private Image bgImage = null;
    [SerializeField] private Image cardImage = null;
    [SerializeField] private BabuButton btn = null;

    private void Awake()
    {
        btn.OnClick += OnClick;
    }
    private void OnClick(BabuButton _)
    {
        UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(cardIdList[dataIndex % cardIdList.Count]));
    }

    public int bgIndex = 0;
    public int dataIndex = 0;
    public List<int> cardIdList = null;
    public int startIndex = 0;
    public float startX = 0;

    public int circleCount = 0;
    public void SetData(int startIndex, float startX, int bgIndex, int dataIndex, List<int> cardIdList)
    {
        circleCount = 0;
        this.startIndex = startIndex;
        this.startX = startX;
        this.bgIndex = bgIndex;
        this.dataIndex = dataIndex;
        this.cardIdList = cardIdList;
        RefreshShow();
    }
    public void RefreshData(int bgIndex, int dataIndex)
    {
        this.bgIndex = bgIndex;
        this.dataIndex = dataIndex;
        RefreshShow();
    }
    private async void RefreshShow()
    {
        bgImage.sprite = await SpriteProxy.GetActivityRecruitSprite("CardBg" + (bgIndex + 1));
        int cardId = cardIdList[dataIndex % cardIdList.Count];
        cardImage.sprite = await SpriteProxy.GetPlayerPortraitYellow(cardId);
        var cardCfg = Configs.CardModel.GetConfig(cardId);
        if (cardCfg == null || string.IsNullOrWhiteSpace(cardCfg.Param)) return;
        string[] args = cardCfg.Param.Split("|");
        if (args.Length <= 2) return;
        float scale = int.Parse(args[2]) / 100f;
        cardImage.rectTransform.anchoredPosition = new Vector2(float.Parse(args[0]) / scale - 50f, float.Parse(args[1]) / scale - 200f);
    }
}
