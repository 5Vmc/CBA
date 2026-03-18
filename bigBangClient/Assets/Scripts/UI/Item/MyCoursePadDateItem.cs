using Babu;
using TMPro;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class MyCoursePadDateItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;

        public void SetData(long time)
        {
            timeText.text = TimeUtils.GetUnixTimeString(time, Lang.Get(LangID.DateString2));
        }
    }
}
