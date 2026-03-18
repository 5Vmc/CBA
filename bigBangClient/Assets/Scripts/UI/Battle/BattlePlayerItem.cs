using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;

namespace BigBang
{
    public class BattlePlayerItem : MonoBehaviour
    {
        [SerializeField] public Transform thisTrans;
        [SerializeField] public MeshRenderer HeadBg;
        [SerializeField] public MeshRenderer HeadZeroPos;
        [SerializeField] public MeshRenderer Head;
        [SerializeField] public TMP_Text NumberText;

        public Protocol.FightCard fightCard;
        public CardModelConfig cardModelConfig;
        public void SetData(Protocol.FightCard fightCard, int headIndex = -1)
        {
            this.fightCard = fightCard;
            cardModelConfig = Configs.CardModel.GetConfig(fightCard.CardId);

            string HeadTexturePath = "";

            if (headIndex == -1)
            {
                if (fightCard.Portrait == 0)
                {
                    HeadTexturePath = $"{ResourcePath.BattleRealPlayerHead}{cardModelConfig.Portrait}.png";
                }
                else
                {
                    HeadTexturePath = $"{ResourcePath.BattleNpcPlayerHead}{fightCard.Portrait}.png";
                }
            }
            else
            {
                if (fightCard.Portrait == 0)
                {
                    HeadTexturePath = $"{ResourcePath.BattleRealPlayerHead}{headIndex}.png";
                }
                else
                {
                    HeadTexturePath = $"{ResourcePath.BattleNpcPlayerHead}{headIndex}.png";
                }
            }

            //Texture HeadBgRedTexture = Addressables.LoadAssetAsync<Texture>(HeadTexturePath).WaitForCompletion();
            //if (HeadBgRedTexture == null)
            //{
            //    HeadTexturePath = $"{ResourcePath.BattleRealPlayerHead}Touxiang.png";
            //    HeadBgRedTexture = Addressables.LoadAssetAsync<Texture>(HeadTexturePath).WaitForCompletion();
            //}

            //Head.material.mainTexture = HeadBgRedTexture;

            NumberText.text = fightCard.Number.ToString();
        }
    }
}
