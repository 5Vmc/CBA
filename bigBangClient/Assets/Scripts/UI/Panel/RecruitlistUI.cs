using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace BigBang.UI
{
    public class RecruitlistUI : MonoBehaviour
    {
        [SerializeField] private List<WishItem> wishItems;

        public void SetData(Dictionary<int, AppointCard> dic)
        {
            foreach (var info in dic)
            {
                var index = info.Key;
                var appointCard = info.Value;
                var cardId = appointCard.CardId;
                if (cardId != 0)
                {
                    var cardConfig = Configs.CardModel.GetConfig(cardId);
                    wishItems[index - 1].SetData(cardConfig, appointCard.State == RecruitAppointCardState.Hit);
                    wishItems[index - 1].StopIllusion();
                }
                else
                {
                    wishItems[index - 1].SetEmpty();
                    wishItems[index - 1].PlayIllusion();
                }
            }
        }

        public void StopIllusion()
        {
            foreach (var item in wishItems)
            {
                item.StopIllusion();
            }
        }

        public void StopAni()
        {
            foreach (var item in wishItems)
            {
                item.StopAni();
            }
        }
    }
}