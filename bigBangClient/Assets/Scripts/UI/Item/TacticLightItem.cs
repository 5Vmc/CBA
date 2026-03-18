using System.Collections.Generic;
using BigBang;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TacticLightItem : MonoBehaviour
{
    [SerializeField] private List<Image> NormalPosImageList;
    [SerializeField] private List<Image> SpecialPosImageList;
    [SerializeField] private TMP_Text TacticNameText;
    [SerializeField] private TMP_Text TacticLevelText;

    public TacticsConfig tacticCfg = null;
    public void SetConfig(TacticsConfig tacticCfg)
    {
        this.tacticCfg = tacticCfg;

        TacticNameText.text = tacticCfg.Name;
        if (TacticLevelText != null) TacticLevelText.text = "1级";

        for (int i = 0; i < 5; i++)
        {
            NormalPosImageList[i].gameObject.SetActive(true);
            SpecialPosImageList[i].gameObject.SetActive(false);
        }
        foreach (int pos in tacticCfg.MainPosition)
        {
            NormalPosImageList[pos - 1].gameObject.SetActive(false);
            SpecialPosImageList[pos - 1].gameObject.SetActive(true);
        }

    }

    public int level = 0;
    public void SetLevel(int level)
    {
        if (TacticLevelText != null) TacticLevelText.text = string.Format("{0}级", level);
    }
}
