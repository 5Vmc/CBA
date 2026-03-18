using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarUpPad : MonoBehaviour
{
    [SerializeField] public CardUpStarAdapter OSA;

    protected void OnEnable()
    {

    }

    protected void OnDisable()
    {

    }

    public void SetData(PlayerCard card, (CardUpgradeConfig, CardUpgradeConfig) cfgs)
    {
        ShowAbilityData(cfgs.Item1, cfgs.Item2);
    }

    private void ShowAbilityData(CardUpgradeConfig cfg, CardUpgradeConfig cfgNext)
    {
        List<CardUpStarItemData1> list = new();
        Dictionary<int, int> keyValuesDict = new();
        if (cfg != null)
        {
            keyValuesDict = cfg.AbilityRatio;
        }
        else {
            //0星的时候，cfg是null,读模型的abilityratio
            var _modulecfg = Configs.CardModel.GetConfig(cfgNext.CardId);
            keyValuesDict = _modulecfg.AbilityRatio;
        }

        foreach (var abilityKey in keyValuesDict.Keys)
        {
            string name = Configs.CardAbility.GetConfig(abilityKey).Name;
            //cfgNext=null是升满了
            int nextValue = cfgNext == null ? 0 : cfgNext.AbilityRatio[abilityKey];

            CardUpStarItemData1 item = new CardUpStarItemData1(name, keyValuesDict[abilityKey], nextValue, cfgNext == null ? true : false);
            list.Add(item);
            //只展示属性有提高的部分
            //if (nextValue == 0 || nextValue > cfg.AbilityRatio[abilityKey]) {
            //    CardUpStarItemData1 item = new CardUpStarItemData1(name, cfg.AbilityRatio[abilityKey], nextValue, cfgNext == null ? true : false);
            //    list.Add(item);
            //}
        }
        list = list.OrderByDescending(p => p.abilityToValue > p.abilityValue).ToList();
        OSA.SetData(list);
        OSA.ScrollTo(0);
    }
}
