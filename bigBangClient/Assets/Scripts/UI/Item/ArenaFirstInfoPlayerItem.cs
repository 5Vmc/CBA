using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    public class ArenaFirstInfoPlayerItem : MonoBehaviour
    {

        [SerializeField] private Image background;
        [SerializeField] private Image portrait;
        //[SerializeField] protected Image card;

        [Header("StateA")]
        [SerializeField] private GameObject stateA;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text scoreShadowText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text positionText;

        [SerializeField] private PeakImage peakImage = null;

        private CardModelConfig data;
        private int strength = 0;

        public void InitState()
        {
            nameText.text = "--";
            positionText.text = "--";
            //portrait.sprite = await SpriteProxy.GetPlayerPortrait(cardData.Portrait);

            scoreText.text = "--";
            scoreShadowText.text = "--";
        }
        public async void SetData(int cardId, int strength, int quality)
        {
            //PlayerCard card = Player.CardManager.GetCard(cardId);
            this.strength = strength;

            CardModelConfig cardConf = Configs.CardModel.GetDataDictionary()[cardId];


            background.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.FormationMain, quality);
            this.InitData(cardConf);
        }

        private async void InitData(CardModelConfig cardData)
        {
            this.data = cardData;
            nameText.text = cardData.Name;
            positionText.text = this.GetAdaptPositionAbbreviation(cardData.AdaptPosition[0]);
            portrait.sprite = await SpriteProxy.GetPlayerPortrait(cardData.Portrait);
            //status.sprite = SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)cardData.Status]);
            scoreText.text = this.strength.ToString();
            scoreShadowText.text = this.strength.ToString();

            peakImage.SetData(cardData);

            ///hurt.gameObject.SetActive(cardData.IsHurt());
            //energy.fillAmount = cardData.EnergyRatio * 1.0f / 100;
            ChangeState();
        }

        public string GetAdaptPositionAbbreviation(int pos)
        {
            var cfg = Configs.SeparatedPosition.GetConfig(pos);
            if (cfg == null) return "";
            return cfg.Abbreviation;
        }

        private void ChangeState(int state = 0)
        {
            stateA.SetActive(state == 0);
            //stateB.SetActive(state != 0);
        }
    }

}


