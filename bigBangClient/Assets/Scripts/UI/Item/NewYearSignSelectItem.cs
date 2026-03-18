using System;
using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class NewYearSignSelectItem : MonoBehaviour
{
    [SerializeField] private BabuButton newYearSignSelectItem = null;
    [SerializeField] private InventoryItem inventoryItem = null;
    [SerializeField] private Image darkImage = null;
    [SerializeField] private Image selectImage = null;
    [SerializeField] private TMP_Text nameText = null;

    private void OnEnable()
    {
        newYearSignSelectItem.OnClick += OnClickNewYearSignSelectItem;
    }
    private void OnDisable()
    {
        newYearSignSelectItem.OnClick -= OnClickNewYearSignSelectItem;
    }

    private void OnClickNewYearSignSelectItem(BabuButton button)
    {
        if(isDark)
        {
            Tips.PopTips("该物品已设置到其它许愿签中");
            return;
        }
        clickCallBack?.Invoke(this);
    }

    public GameItem gameItem = null;
    public int itemIndex = 0;
    public Action<NewYearSignSelectItem> clickCallBack = null;
    public void SetDataOnce(GameItem gameItem, int itemIndex, Action<NewYearSignSelectItem> clickCallBack)
    {
        this.gameItem = gameItem;
        this.itemIndex = itemIndex;
        this.clickCallBack = clickCallBack;

        inventoryItem.SetData(gameItem);
        nameText.text = gameItem.GetName();
        nameText.color = CBAColorUtil.Instance.GetColor(gameItem.GetQuality());
    }

    public bool isDark = false;
    public void RefreshDark()
    {
        isDark = ActivityController.Instance.wishSigns.Contains(itemIndex);
        darkImage.gameObject.SetActive(isDark);
    }

    public bool isSelect = false;
    public void SetSelect(bool isSelect)
    {
        this.isSelect = isSelect;
        selectImage.gameObject.SetActive(isSelect);
    }


}
