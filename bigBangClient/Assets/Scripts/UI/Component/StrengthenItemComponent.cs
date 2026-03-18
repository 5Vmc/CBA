using System.Collections;
using System.Collections.Generic;
using BigBang.Animation;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BigBang.UI
{
    public class StrengthenItemComponent : MonoBehaviour
    {
        //强化按钮
        public Button StrengthenBtn;
        //科技描述
        public TMP_Text DescriptionText;
        //科技消耗
        public TMP_Text CostText;
        //科技图片
        public Image Icon;
        public ButtonAnim BtnAnim;
        //闪烁
        public Image FlashImg;
        public UIEffect IconEffect;
        //用这个来记录变化，变化后才推送小红点变动
        public bool comEnabled = false;
    }
}