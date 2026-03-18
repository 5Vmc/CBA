using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    /// <summary>
    /// 翻页钟形状的倒计时组件
    /// </summary>
    public class LeftTimeComponent : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeHourText = null;
        [SerializeField] private TMP_Text timeMinuteText = null;
        [SerializeField] private TMP_Text timeSecondText = null;

        /// <summary>
        /// 设置剩余时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位是秒</param>
        public void SetLeftTimeText(int leftTime)
        {
            List<string> timeStrList = Utility.FormatLeftTimeWithList(leftTime);
            timeHourText.text = timeStrList[0];
            timeMinuteText.text = timeStrList[1];
            timeSecondText.text = timeStrList[2];
        }
    }
}
