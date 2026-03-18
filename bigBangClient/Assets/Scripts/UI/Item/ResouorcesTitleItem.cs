using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

public class ResouorcesTitleItem : MonoBehaviour
{
    [SerializeField] private BabuButton resouorcesTitleItem = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text countText = null;

    private void OnEnable()
    {
        EventManager.Instance.Register(EventID.OnRefreshGoods, RefreshCount);
        resouorcesTitleItem.OnClick += OnClick;
        RefreshCount();
    }
    private void OnDisable()
    {
        EventManager.Instance.Unregister(EventID.OnRefreshGoods, RefreshCount);
        resouorcesTitleItem.OnClick -= OnClick;
    }

    GameItem gameItem = null;
    public async void SetGoodsId(int goodsId)
    {
        gameItem = GameItemUtils.CreateGameItem(BigBang.GameItemType.Goods, goodsId, 0);
        if (gameItem == null)
        {
            Debug.LogWarning("ResouorcesTitleItem , SetGoodsId , gameItem == null , goodsId = " + goodsId);
            return;
        }
        iconImage.sprite = await gameItem.GetIcon();
        RefreshCount();
    }

    private void RefreshCount(object[] _ = null)
    {
        if (gameItem == null) return;
        countText.text = gameItem.GetPlayerCount().ToString();
    }

    private void OnClick(BabuButton _)
    {
        if (gameItem == null) return;
        ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties(gameItem);
        itemtipsUIProperties.SetPos(transform, new Vector3(0, -20f, 0));
        UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
    }

    //强制重建布局
    public void ForceRebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(countText.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(resouorcesTitleItem.transform as RectTransform);
    }

}
