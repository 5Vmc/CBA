using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class CardEquipStatus {
    public List<EquipStatus> PartStatus = new();
    public bool IsMaxLevel = false;
    public EquipStatus CanTuPo;
}
public enum EquipStatus
{
    /// <summary>
    /// 材料不足：显示材料数量
    /// </summary>
    LackOfMaterial = 0,
    /// <summary>
    /// 球员等级不足：显示材料数量，显示等级限制
    /// </summary>
    LackOfLevel = 1,
    /// <summary>
    /// 在部位上，是已经升级完，等突破后才能升级；在突破上，是部位还没有升级好，不能突破。
    /// </summary>
    LackOfUpgrade = 2, 
    /// <summary>
    /// 突破状态：突破上限
    /// </summary>
    MaxLevel = 3,
    /// <summary>
    /// 啥都满足，坐等突破
    /// </summary>
    Ready = 4,               
}

public class cardEquipItem : MonoBehaviour
{
    [SerializeField] public TMP_Text propName1;
    [SerializeField] public TMP_Text propValue1;
    [SerializeField] public TMP_Text propName2;
    [SerializeField] public TMP_Text propValue2;
    [SerializeField] public TMP_Text txtBtn;
    [SerializeField] public InventoryItem icon;
    [SerializeField] public Button btnLvUp;
    [SerializeField] public TMP_Text equipName;
    [SerializeField] public TMP_Text txtLackLv;

    // Start is called before the first frame update

    private PlayerCard card;
    private int partIndex;
    private JerseyUpgradeConfig cfg;
    private GameItem costItem;
    public EquipStatus Status;



    public void OnEnable()
    {
        btnLvUp.onClick.AddListener(EquipLevelUp);
    }

    public void OnDisable()
    {
        btnLvUp.onClick.RemoveListener(EquipLevelUp);
    }

    public void EquipLevelUp()
    {
        if (Status == EquipStatus.Ready)
        {
            //这里先预扣，同步道具的通知可能会晚到
            Player.PackageManager.GetGoods(costItem.Id).DelCount(costItem.Count);
            Player.CardManager.CardEquipLevelUp(card, partIndex, onEquipPartLevelUp);
        }
        else if (Status == EquipStatus.LackOfMaterial)
        {
            UIController.Instance.OpenWindow<EquipRouteUI>(new EquipRouteUIProperties(costItem, cfg.Level, partIndex, card.CardId));
        }
    }

    private void onEquipPartLevelUp()
    {
        var nextCfg = Configs.JerseyUpgrade.GetConfig(cfg.Id + 1);
        SetData(card, partIndex, nextCfg);
        //todo:   注意，玩家进阶是有选择性的，所以只对战力前5的提示小红点就可以了， 这里还没有处理。
        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
    }

    private void setEquipName(int position)
    {
        int currentLv = cfg.Level;
        if (currentLv <= 0)
        {
            equipName.text = "";
        }
        else
        {
            equipName.text = costItem.GetName().Replace("碎片", "");
            JerseyBreakConfig bconfig = Configs.JerseyBreak.GetConfig(position * 1000 + currentLv);
            int quality = bconfig.Quality;
            equipName.text += bconfig.CardNameSuffix;
            equipName.color = CBAColorUtil.Instance.GetColor(quality);
        }
    }

    public async void SetData(PlayerCard _card, int _index, JerseyUpgradeConfig _cfg)
    {
        clear();
        partIndex = _index;
        card = _card;
        cfg = _cfg;
        Status = card.EquipStatus.PartStatus[_index];
        if (Status == EquipStatus.MaxLevel)
        {
            //满级
            clear();
            showIcon(false, false);
            return;
        }
        costItem = GameItemUtils.CreateGameItem(cfg.Cost);
        //不能挪位置，依赖costItem
        setEquipName(card.DefaultPosition);
        showProp();

        txtLackLv.gameObject.SetActive(false);
        btnLvUp.gameObject.SetActive(true);
        if (Status == EquipStatus.LackOfUpgrade)
        {
            btnLvUp.gameObject.SetActive(false);
            showIcon(false, false);
        }
        else if (Status == EquipStatus.LackOfLevel)
        {
            showIcon(true, false);
            btnLvUp.gameObject.SetActive(false);
            txtLackLv.gameObject.SetActive(true);
            txtLackLv.text = string.Format("球员Lv.{0}", cfg.CardLevel);
        }
        else if (Status == EquipStatus.LackOfMaterial)
        {
            txtBtn.text = "获取材料";
            showIcon(true, true);
            SpriteManager.GetSprite(AtlasNames.Public, "btn_9", s => btnLvUp.image.sprite = s);
        }
        else
        {
            btnLvUp.image.sprite = await SpriteProxy.YellowBtnEnable;
            txtBtn.text = cfg.Level == 1 ? "合成" : "升级";
            showIcon(true, false);
        }
    }

    private void showProp()
    {
        List<Props> plist = Utils.CBAUtils.CreateProps(cfg.Ability);

        propName1.text = plist[0].PropName;
        propValue1.text = "+" + plist[0].PropValue.ToString();
        propName2.text = plist[1].PropName;
        propValue2.text = "+" + plist[1].PropValue.ToString();
    }

    /// <summary>
    /// 展示icon
    /// </summary>
    /// <param name="showMaterial">是否展示材料</param>
    /// <param name="showblack">是否遮黑</param>
    private void showIcon(bool showMaterial, bool showblack)
    {
        var myItem = Player.PackageManager.GetGoods(costItem.Id);
        var mycount = myItem == null ? 0 : myItem.Count;
        icon.SetData(costItem, true);
        icon.blackImg.gameObject.SetActive(showblack);
        if (showMaterial)
        {
            icon.SetCount(string.Format("<color={0}>{1}</color>/{2}",
                    mycount >= costItem.Count ? CBAColorUtil.Instance.GetHexColor(CBAColor.Green) : CBAColorUtil.Instance.GetHexColor(CBAColor.Red),
                    mycount, costItem.Count));
        }
        else
        {
            icon.SetCount("");
        }
    }

    private void clear()
    {
        btnLvUp.gameObject.SetActive(false);
    }

    #region 详情界面使用

    public void SetCardDetailData(JerseyUpgradeConfig jerseyUpgradeConfig, PlayerCard card)
    {
        cfg = jerseyUpgradeConfig;
        costItem = GameItemUtils.CreateGameItem(cfg.Cost);
        setEquipName(card.DefaultPosition);
        showProp();
        txtLackLv.gameObject.SetActive(false);
        btnLvUp.gameObject.SetActive(false);
        showIcon(false, false);
    }

    #endregion
}
