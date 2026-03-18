using System.Collections;
using System.Collections.Generic;
using BigBang;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllStarCombatStageItem : MonoBehaviour
{
    [SerializeField] public Image lightBgImage = null;
    [SerializeField] public Image darkBgImage = null;
    [SerializeField] public TMP_Text targetCombatText = null;

    public AllStarRewardConfig allStarRewardConfig = null;
    public void SetData(AllStarRewardConfig allStarRewardConfig)
    {
        this.allStarRewardConfig = allStarRewardConfig;

        targetCombatText.text = allStarRewardConfig.Option.ToString("N0");
        bool isAchieveTheGoal = allStarRewardConfig.Option <= AllStarManager.Instance.savedTotalMaxCombatInServer;
        lightBgImage.gameObject.SetActive(isAchieveTheGoal);
        darkBgImage.gameObject.SetActive(!isAchieveTheGoal);
    }
}
