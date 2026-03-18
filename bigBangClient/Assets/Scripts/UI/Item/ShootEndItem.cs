using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BigBang.UI;
using GameConfig.Config;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

public class ShootEndItem : MonoBehaviour
{
    [SerializeField] private RectTransform shootEndItem = null;
    [SerializeField] private Image lightBgImage = null;
    [SerializeField] private Image darkBgImage = null;
    [SerializeField] private Image levelTitleImage = null;
    [SerializeField] private ImageFont levelNumImageFont = null;
    [SerializeField] private InventoryItem inventoryItem = null;
    [SerializeField] private Image darkImage = null;
    [SerializeField] private Image hasGetImage = null;
    [SerializeField] private Image canNotGetImage = null;

    public ShootGameStageConfig shootGameStageConfig = null;
    public void SetData(ShootGameStageConfig shootGameStageConfig)
    {
        this.shootGameStageConfig = shootGameStageConfig;
        bool isLight = shootGameStageConfig.Id % 2 == 0;
        lightBgImage.gameObject.SetActive(isLight);
        darkBgImage.gameObject.SetActive(!isLight);
        levelNumImageFont.text = shootGameStageConfig.Id.ToString();
        if (string.IsNullOrWhiteSpace(shootGameStageConfig.Reward) == false)
        {
            inventoryItem.SetData(GameItemUtils.CreateGameItems(shootGameStageConfig.Reward).ToList()[0]);
        }
    }

    public void RefreshInfo(int oldLevel, int newLevel)
    {
        if (string.IsNullOrWhiteSpace(shootGameStageConfig.Reward) == true) return;
        if (newLevel <= oldLevel)
        {
            darkImage.gameObject.SetActive(true);
            hasGetImage.gameObject.SetActive(shootGameStageConfig.Id <= oldLevel);
            canNotGetImage.gameObject.SetActive(shootGameStageConfig.Id > oldLevel);
        }
        else
        {
            darkImage.gameObject.SetActive(shootGameStageConfig.Id <= oldLevel || shootGameStageConfig.Id > newLevel);
            hasGetImage.gameObject.SetActive(shootGameStageConfig.Id <= oldLevel);
            canNotGetImage.gameObject.SetActive(shootGameStageConfig.Id > newLevel);
        }
    }

}
