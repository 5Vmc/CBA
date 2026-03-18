using System.Collections;
using System.Collections.Generic;
using BigBang;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BountyTaskDoingPlayerItem : MonoBehaviour
{
    [SerializeField] private Image qualityBgImage = null;
    [SerializeField] private Image portraitIconImage = null;
    [SerializeField] private TMP_Text scoreText = null;
    [SerializeField] private TMP_Text positionText = null;
    [SerializeField] private TMP_Text scoreShadowText = null;
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private PeakImage peakImage = null;

    public async void SetData(CardModelConfig cardModel)
    {
        qualityBgImage.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.FormationBench, cardModel.Quality);
        portraitIconImage.sprite = await SpriteProxy.GetPlayerPortrait(cardModel.Portrait);
        //int CombatEffectiveness = playerCard.GetCombatEffectiveness();

        PlayerCard card = Player.CardManager.GetCard(cardModel.Id);
        if (card != null)
        {
            scoreText.text = card.FightPoint.ToString();
            scoreShadowText.text = card.FightPoint.ToString();
        }
        else
        {
            scoreText.text = "--";
            scoreShadowText.text = "--";
        }
        peakImage.SetData(card);


        nameText.text = cardModel.Name;
        positionText.text = Configs.SeparatedPosition.GetConfig(cardModel.AdaptPosition[0]).Abbreviation;
    }

}
