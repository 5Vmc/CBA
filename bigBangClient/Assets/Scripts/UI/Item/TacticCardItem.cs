using System.Linq;
using BigBang;
using BigBang.UI;
using GameConfig.Config;
using Google.Protobuf.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TacticCardItem : MonoBehaviour
{
    [SerializeField] private TacticLightItem LightItem;
    [SerializeField] private TacticLightItem DarkItem;
    [SerializeField] private Image UseImage;
    [SerializeField] private Image SelectImage;
    [SerializeField] private Button SelectButton;


    private TacticsSetPad tacticsSetPad = null;
    public TacticsConfig tacticCfg = null;
    public void SetConfig(TacticsConfig tacticCfg, TacticsSetPad tacticsSetPad = null)
    {
        this.tacticCfg = tacticCfg;
        this.tacticsSetPad = tacticsSetPad;
        LightItem.SetConfig(tacticCfg);
        DarkItem.SetConfig(tacticCfg);
        SelectButton.onClick.RemoveAllListeners();
        SelectButton.onClick.AddListener(OnClick);
    }

    public bool isClickToOptnTip = false;
    private void OnClick()
    {
        if (isClickToOptnTip)
        {
            UIController.Instance.OpenWindow<TacticHelpUI>(new TacticHelpUIProperties(tacticCfg.Id));
            return;
        }
        tacticsSetPad?.OnClickTacticCardItem(this);
    }

    public int level = 0;
    public void SetData(FormationBase formation)
    {
        if (formation == null || Player.FightManager.FormationController.TacticsLevelDic == null || Player.FightManager.FormationController.TacticsLevelDic.Count == 0 || Player.FightManager.FormationController.TacticsLevelDic.ContainsKey(tacticCfg.Id) == false)
        {
            level = 0;
        }
        else
        {
            level = Player.FightManager.FormationController.TacticsLevelDic[tacticCfg.Id];
        }
        LightItem.SetLevel(level);
        DarkItem.SetLevel(level);
        LightItem.gameObject.SetActive(level != 0);
        DarkItem.gameObject.SetActive(level == 0);

        bool isFind = false;
        foreach (int TacticsId in formation.TacticsIdList)
        {
            if (TacticsId == tacticCfg.Id)
            {
                isFind = true;
                break;
            }
        }
        SetUse(isFind);
    }

    public void SetData(RepeatedField<int> usingTactics, MapField<int, int> levelMap)
    {
        level = levelMap.ContainsKey(tacticCfg.Id) ? levelMap[tacticCfg.Id] : 0;
        LightItem.SetLevel(level);
        DarkItem.SetLevel(level);
        LightItem.gameObject.SetActive(level != 0);
        DarkItem.gameObject.SetActive(level == 0);
        bool isFind = usingTactics.Any(usingId => usingId == tacticCfg.Id);
        SetUse(isFind);
        SetSelect(false);
    }

    public bool isUse = false;
    public void SetUse(bool isUse)
    {
        this.isUse = isUse;
        UseImage.gameObject.SetActive(isUse);
    }

    public bool isSelect = false;
    public void SetSelect(bool isSelect)
    {
        this.isSelect = isSelect;
        SelectImage.gameObject.SetActive(isSelect);
    }

}
