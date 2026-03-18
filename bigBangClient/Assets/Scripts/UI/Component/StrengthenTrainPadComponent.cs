using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class StrengthenTrainPadComponent : MonoBehaviour
    {
        public Transform Content;
        //一键强化按钮
        public Button TrainAllBtn;
        public TMP_Text TrainAllBtnText;
        public List<PlayerStrengthenItem> ItemList = null;
        public RectTransform Scroll;
        public RectTransform ViewPort;
    }
}
