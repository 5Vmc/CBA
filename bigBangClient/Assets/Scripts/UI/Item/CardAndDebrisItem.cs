using BigBang.Animation;
using GameConfig.Config;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class CardAndDebrisItem : MonoBehaviour
    {
        public CardItem CardItem;
        public DebrisItem DebrisItem;

        // 是否是碎片
        public bool IsDebris { get; private set; }
        public CardAndDebrisItemAnim Anim;

        // 卡片
        public void SetData(CardModelConfig cardCfg)
        {
            IsDebris = false;
            CardItem.SetConfigShow(cardCfg);
            CardItem.SetPlayerEffect(Player.CardManager.GetCard(cardCfg.Id));
        }

        // 碎片
        public void SetData(CardModelConfig cardCfg, int count)
        {
            IsDebris = true;
            CardItem.SetConfigShow(cardCfg);
            DebrisItem.SetData(cardCfg, count);
            CardItem.SetPlayerEffect(Player.CardManager.GetCard(cardCfg.Id));
        }
    }
}