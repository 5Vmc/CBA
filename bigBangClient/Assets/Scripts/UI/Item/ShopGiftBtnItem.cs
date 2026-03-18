using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

public class ShopGiftBtnItem : MonoBehaviour
{
    [SerializeField] public RectTransform rect = null;
    [SerializeField] public Button shopGiftBtnItem = null;
    [SerializeField] public Image goodsIcon = null;
    [SerializeField] public TMP_Text goodsCount = null;
    [HideInInspector] public GameItem gameItem = null;

    public async void SetData(GameItem gameItem)
    {
        this.gameItem = gameItem;
        goodsIcon.sprite = await gameItem.GetIcon();
        goodsCount.text = gameItem.CountString();
    }
    private void OnEnable()
    {
        shopGiftBtnItem.onClick.AddListener(OnClick);
    }
    private void OnDisable()
    {
        shopGiftBtnItem.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
    }
}
