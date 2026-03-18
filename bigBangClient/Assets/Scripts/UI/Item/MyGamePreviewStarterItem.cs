using BigBang.Animation;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class MyGamePreviewStarterItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text positionText;

        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text scoreShadowText;
         
        [SerializeField] private TMP_Text positiomMarkText;

        [SerializeField] private Image portrait;

        [SerializeField] private Image background;

        [SerializeField] private PositionSeparatedType posType;

        private int strength;

        private void Awake() {
            SeparatedPositionConfig cfg = Configs.SeparatedPosition.GetConfig((int)posType);
            if(cfg == null){
                Debug.LogError("配置错误");
                return;
            }
            positiomMarkText.text = cfg.Abbreviation;
        }


        public PositionSeparatedType PosType {
            get { return this.posType; }
            private set {}
        }

        public async void SetData(PlayerCardMiniInfo miniInfo)
        {
           // nameText = data.
            int cardId = miniInfo.CardId; 
            this.strength = miniInfo.CombatEffectiveness;
            
            CardModelConfig cardConf = Configs.CardModel.GetDataDictionary()[cardId];


            background.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.FormationMain, cardConf.Quality);
            this.InitData(cardConf);
        }

        private async void InitData(CardModelConfig cardData)
        {
           // this.data = cardData;
            nameText.text = PlayerCard.GetFullName(cardData);
            positionText.text = this.GetAdaptPositionAbbreviation(cardData.AdaptPosition[0]);
            portrait.sprite = await SpriteProxy.GetPlayerPortrait(cardData.Portrait);
           
            scoreText.text = this.strength.ToString();
            scoreShadowText.text = this.strength.ToString();
            
            //ChangeState();
        }

        public string GetAdaptPositionAbbreviation(int pos)
        {
            var cfg = Configs.SeparatedPosition.GetConfig(pos);
            if (cfg == null) return "";
            return cfg.Abbreviation;
        }

    }
}