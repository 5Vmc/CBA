using Babu;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{

    public class TimeGiftItemPad : MonoBehaviour
    {
        [SerializeField] private PageViewVirtual pageView;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_list"></param>
        /// <param name="_giftid">如果传值，则选中这个id</param>
        public void SetData(List<GiftItemData> _list, bool tolast = true)
        {
            pageView.SetData(_list);
            if (tolast)
            {
                pageView.MoveTo(_list.Count - 1);
            }
        }
    }
}