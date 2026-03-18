using BigBang;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Babu;

public class FBTowerSelectBuffItem : MonoBehaviour
{
    [SerializeField] private Image bgImage = null;
    [SerializeField] private Image iconBgImage = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text positionText = null;
    [SerializeField] private TMP_Text addText = null;
    [SerializeField] private TMP_Text conditionTipText = null;
    [SerializeField] private Image starImage = null;
    [SerializeField] private TMP_Text starCostText = null;
    [SerializeField] private BabuButton selectButton = null;
    [SerializeField] private TMP_Text selectText = null;
    [SerializeField] private BabuButton canNotSelectButton = null;
    [SerializeField] private TMP_Text canNotSelectText = null;

    [SerializeField] private Color orangeColor;
    [SerializeField] private Color redColor;

    void OnEnable()
    {
        selectButton.OnClick += OnSelectButtonClick;
        canNotSelectButton.OnClick += OnCanNotSelectButtonClick;
    }
    void OnDisable()
    {
        selectButton.OnClick -= OnSelectButtonClick;
        canNotSelectButton.OnClick -= OnCanNotSelectButtonClick;
    }

    public int buffPos = 0;
    public int buffType = 0;
    public int buffValue = 0;
    public int buffCost = 0;
    public int index = 0;
    public int configId = 0;
    public bool SetBuffStr(int configId, string buffStr, int index)
    {
        this.configId = configId;
        this.index = index;
        string[] buffStrItem = buffStr.Split(":");
        if (buffStrItem.Length != 4)
        {
            Debug.LogWarningFormat("FBTowerSelectBuffUI , Refresh , buffStrItem.Length != 4 , configId = {0}", configId);
            return false;
        }

        try
        {
            buffPos = int.Parse(buffStrItem[0]);
            buffType = int.Parse(buffStrItem[1]);
            buffValue = int.Parse(buffStrItem[2]);
            buffCost = int.Parse(buffStrItem[3]);
        }
        catch (System.Exception)
        {
            Debug.LogWarningFormat("FBTowerSelectBuffUI , Refresh , int.Parse error , configId = {0}", configId);
            return false;
        }

        SeparatedPositionConfig separatedPositionConfig = Configs.SeparatedPosition.GetConfig(buffPos);
        if (separatedPositionConfig == null)
        {
            Debug.LogWarningFormat("FBTowerSelectBuffUI , Refresh , Configs.SeparatedPosition.GetConfig(buffPos) == null , configId = {0}", configId);
            return false;
        }
        CardAbilityConfig cardAbilityConfig = Configs.CardAbility.GetConfig(buffType);
        if (cardAbilityConfig == null)
        {
            Debug.LogWarningFormat("FBTowerSelectBuffUI , Refresh , Configs.CardAbility.GetConfig(buffType) == null , configId = {0}", configId);
            return false;
        }

        positionText.text = separatedPositionConfig.Name;
        addText.text = "{0}<color=#fed701>+{1}%</color>".SafeFormat(cardAbilityConfig.Name, buffValue);
        starCostText.text = buffCost.ToString();

        return true;
    }

    public void RefreshShow()
    {
        starCostText.color = FBTowerController.Instance.FBData.currentStar >= buffCost ? orangeColor : redColor;
        selectButton.gameObject.SetActive(FBTowerController.Instance.FBData.currentStar >= buffCost);
        canNotSelectButton.gameObject.SetActive(FBTowerController.Instance.FBData.currentStar < buffCost);
    }

    private void OnSelectButtonClick(BabuButton _)
    {
        EventManager.Instance.Dispatch(EventID.OnClickFBTowerBuff, this);
    }
    private void OnCanNotSelectButtonClick(BabuButton _)
    {
        Tips.PopTips("星星数量不足");
    }

    public void SetButtonCanUse(bool isCanUse)
    {
        selectButton.interactable = isCanUse;
        canNotSelectButton.interactable = isCanUse;
    }

}
