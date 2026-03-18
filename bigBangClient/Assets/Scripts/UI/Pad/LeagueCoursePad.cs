using BigBang.Animation;
using Protocol;
using TMPro;
using UnityEngine;
using Utils;
using static BigBang.BattleManager;

namespace BigBang.UI
{

    public class LeagueCoursePad : MonoBehaviour
    {
        [SerializeField] private TMP_Text leagueNameText;
        [SerializeField] private LeagueCourseAdapter adapter;

        public LeagueCoursePadAnim Anim;

        public void SetData(GetLeagueCourseResponse data, string leagueName, int compitionID, BattleEnterType battleEnterType)
        {
            if (compitionID == CompitionID.League)
            {
                // 设置联赛名称
                leagueNameText.text = leagueName;
            }
            if (compitionID == CompitionID.Cup)
            {
                // 设置杯赛名称
                leagueNameText.text = Lang.Get(LangID.CupNameText);
            }
            adapter.SetData(data, compitionID, battleEnterType);
            Anim.PlayEnter();
        }
    }
}