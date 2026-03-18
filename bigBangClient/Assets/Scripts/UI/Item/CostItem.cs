using BigBang;
using BigBang.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class CostItem : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public Image IconImg;
    [SerializeField] public TMP_Text IconText;
    [SerializeField] public Button iconButton;
    [HideInInspector] public GameItem gameItem;

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        iconButton.onClick.AddListener(OnClickIconButton);
    }
    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        iconButton.onClick.RemoveListener(OnClickIconButton);
    }
    public void OnClickIconButton()
    {
        Debug.Log("OnClickIconButton1");
        if (gameItem == null) return;
        Debug.Log("OnClickIconButton2");
        UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="num">要对比的数量</param> 
    public async void SetData(GameItem item, int num, int needCount = 1)
    {
        gameItem = item;
        IconImg.sprite = await item.GetIcon();
        IconText.text = CBAUtils.GetCompareColorStr(num, item.Count * needCount, "/");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="itemid"></param>
    /// <param name="itemcount"></param>
    /// <param name="num">要对比的数量</param>
    public void SetData(BigBang.GameItemType itemType, int itemid, int itemcount, int num)
    {
        GameItem item = GameItemUtils.CreateGameItem(itemType, itemid, itemcount);
        SetData(item, num);
    }

    /// <summary>
    /// 使用玩家自己的数据来对比
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="itemid"></param>
    /// <param name="itemcount"></param>
    /// <param name="usePlayersData">使用玩家背包物品来对比，如果找不到就是0</param>
    public void SetData(BigBang.GameItemType itemType, int itemid, int itemcount, bool usePlayersData = true)
    {
        int num = 0;
        if (itemType == BigBang.GameItemType.Goods)
        {
            num = Player.PackageManager.GetGoodsNumber(itemid);
        }
        else if (itemType == GameItemType.Resource)
        {
            num = Player.PackageManager.GetResourceCount(itemid);
        }

        GameItem item = GameItemUtils.CreateGameItem(itemType, itemid, itemcount);
        SetData(item, num);
    }

    public void SetData(GameItem item, bool usePlayersData = true, int needCount = 1)
    {
        int num = 0;
        if (item.Type == BigBang.GameItemType.Goods)
        {
            num = Player.PackageManager.GetGoodsNumber(item.Id);
        }
        else if (item.Type == GameItemType.Resource)
        {
            num = Player.PackageManager.GetResourceCount(item.Id);
        }
        SetData(item, num, needCount);
    }

    public void ForceRebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(IconText.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}
