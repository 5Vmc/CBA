using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayoffFinalsGuessMVPPlayerItem : MonoBehaviour
{
    [SerializeField] private BabuButton playerItem = null;
    [SerializeField] private Image playerImage = null;
    [SerializeField] private TMP_Text playerNameText = null;
    [SerializeField] private Image lightImage = null;

    private void OnEnable()
    {
        playerItem.OnClick += OnClickPlayerItem;
    }
    private void OnDisable()
    {
        playerItem.OnClick -= OnClickPlayerItem;
    }
    public FinalsGuessPlayerConfig finalsGuessPlayerConfig = null;
    public async void SetData(FinalsGuessPlayerConfig finalsGuessPlayerConfig)
    {
        this.finalsGuessPlayerConfig = finalsGuessPlayerConfig;
        playerNameText.text = finalsGuessPlayerConfig.Name;
        playerImage.sprite = await SpriteProxy.GetPlayoffFinalsGuessMVPPlayerSprite(finalsGuessPlayerConfig.Icon);
    }
    public void SetLight(bool isLight)
    {
        lightImage.gameObject.SetActive(isLight);
    }
    private void OnClickPlayerItem(BabuButton _)
    {
        EventManager.Instance.Dispatch(EventID.OnSelectPlayoffFinalsGuessMVPPlayerItem, this);
    }
}
