using BigBang.Animation;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
namespace BigBang.UI
{
    public class TrainUIComponent : MonoBehaviour
    {
        // 金钱
        public TMP_Text SpeedText;
        // 经验(需要逐帧刷新)
        public TMP_Text ExpText;
        // 倍率切换按钮
        public Button SpeedBtn;
        // 倍率切换图片
        public Transform SpeedTransform;

        public BabuToggleGroup toggleGroup;
        public TrainUIAnim TrainAnim;
        public RectTransform TopItem;
        public RectTransform BottomItem;
        public RectTransform SpeedDiamond;
        public GameObject padContainer;

        [NonSerialized] public List<GameObject> padList;

        [NonSerialized]
        public Dictionary<int, GameObject> padState = new();
        [NonSerialized]
        public List<string> padPathList = new List<string>(){
            "Prefabs/Pad/AsyncPad/RegularTrainPad.prefab",
            "Prefabs/Pad/AsyncPad/StrengthenTrainPad.prefab",
            "Prefabs/Pad/AsyncPad/BigBangPad.prefab",
            "Prefabs/Pad/AsyncPad/InviteMatchPad.prefab"
        };
    }
}
