using System;
using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class CardUpLevelGoodsUseItem : MonoBehaviour
{
    [SerializeField] public BabuLongPressButton button = null;
    [SerializeField] public InventoryItem inventoryItem = null;
    [SerializeField] private TMP_Text itemNameText = null;
    [SerializeField] private TMP_Text expAddText = null;

    public GoodsConfig goodsConfig;
    int goodsCount;
    Func<CardUpLevelGoodsUseItem, bool> clickCallBack;
    Action<CardUpLevelGoodsUseItem> clickEndCallBack;
    public void SetData(int goodsId, Func<CardUpLevelGoodsUseItem, bool> clickCallBack, Action<CardUpLevelGoodsUseItem> clickEndCallBack)
    {
        this.clickCallBack = clickCallBack;
        this.clickEndCallBack = clickEndCallBack;
        goodsConfig = Configs.Goods.GetConfig(goodsId);
        goodsCount = Player.PackageManager.GetGoodsNumber(goodsId);
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, goodsId, goodsCount);
        inventoryItem.SetData(gameItem, false);
        itemNameText.text = goodsConfig.Name;
        itemNameText.color = CBAColorUtil.Instance.GetColor(goodsConfig.Quality);
        expAddText.text = "+{0}".SafeFormat(goodsConfig.Param1);
    }

    private void OnEnable()
    {
        button.onClick += OnButtonClick;
        button.onClickEnd += OnButtonClickEnd;
    }
    private void OnDisable()
    {
        button.onClick -= OnButtonClick;
        button.onClickEnd -= OnButtonClickEnd;
    }
    public bool OnButtonClick(BabuLongPressButton levelItem)
    {
        bool? isCanTrigNext = clickCallBack?.Invoke(this);
        return !isCanTrigNext.HasValue || (bool)(isCanTrigNext) == true;
    }
    public void OnButtonClickEnd(BabuLongPressButton levelItem)
    {
        clickEndCallBack?.Invoke(this);
    }
}
