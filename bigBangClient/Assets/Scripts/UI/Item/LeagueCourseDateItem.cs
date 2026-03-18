using Babu;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using Utils;
using static BigBang.BattleManager;

namespace BigBang.UI
{
    public class LeagueCourseDateItem : MonoBehaviour
    {
        [SerializeField] public TMP_Text normalDateText = null;
        [SerializeField] public TMP_Text todayDateText = null;
        [SerializeField] public TMP_Text todayTitleText = null;
    }
}
