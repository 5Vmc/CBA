using System.Collections;
using System.Collections.Generic;
using BigBang;
using GameConfig.Config;
using UnityEngine;
using UnityEngine.UI;

public class PeakImage : MonoBehaviour
{
    [SerializeField] public Image peakImage = null;

    public void SetHide()
    {
        peakImage.gameObject.SetActive(false);
    }
    public void SetData(PlayerCard playerCard)
    {
        if (playerCard == null)
        {
            peakImage.gameObject.SetActive(false);
        }
        else
        {
            SetData(playerCard.Config);
        }
    }
    public void SetData(CardModelConfig cardModelConfig)
    {
        if (cardModelConfig == null)
        {
            peakImage.gameObject.SetActive(false);
        }
        else
        {
            SetData(cardModelConfig.PeakLogo);
        }
    }
    public async void SetData(string peakLogoName)
    {
        if (string.IsNullOrWhiteSpace(peakLogoName))
        {
            peakImage.gameObject.SetActive(false);
        }
        else
        {
            peakImage.sprite = await SpriteProxy.GetPeakImage(peakLogoName);
            peakImage.gameObject.SetActive(true);
        }
    }
}
