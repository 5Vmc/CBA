using System.Collections.Generic;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using BigBang.Animation;

namespace BigBang.UI
{

    public class LeagueScoreboardPad : MonoBehaviour
    {
        [SerializeField] private TMP_Text leagueNameText;
        [SerializeField] private LeagueScoreboardAdapter adapter;
        [SerializeField] private Button giftBtn;
        [SerializeField] private Button introduceBtn;

        [SerializeField] private HorizontalLayoutGroup giftLayout = null;

        private void Awake()
        {
#if UNITY_WEBGL
            giftLayout.childAlignment = TextAnchor.MiddleLeft;
#endif
        }

        public LeagueScoreboardPadAnim Anim;
        public int LeagueLevel { get; set; }

        private void OnEnable()
        {
            giftBtn.onClick.AddListener(OnGift);
            introduceBtn.onClick.AddListener(OnIntroduce);
        }

        private void OnDisable()
        {
            giftBtn.onClick.RemoveListener(OnGift);
            introduceBtn.onClick.RemoveListener(OnIntroduce);
        }

        public void SetData(List<LeagueScorebarTeam> data, string leagueName)
        {
            // 设置联赛名称
            leagueNameText.text = leagueName;
            // 积分榜按累计积分从高到低排列：积分相同的，先看场次，场次少在前；场次相同再看净胜球，多的在前；净胜球相同，再看进球数，多的在前；进球数相同，则ID大的在前
            var result = data.OrderByDescending(item => item.Win * 3 + item.Deuce)              // 积分
                             .ThenByDescending(item => item.Win + item.Deuce + item.Failed)     // 场次
                             .ThenByDescending(item => item.Obtain - item.Lost)                 // 净胜球
                             .ThenByDescending(item => item.Obtain)                             // 进球数
                             .ThenByDescending(item => item.BaseData.TeamId);
            adapter.SetData(result.ToList());
            Anim.PlayEnter();
        }


        private void OnGift()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_TARGET);
            UIController.Instance.OpenWindow<LeagueRewardsUI>(new LeagueRewardsUIProperties(CompitionID.League, LeagueLevel));
        }

        private void OnIntroduce()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            UIController.Instance.OpenWindow<LeagueIntroductionUI>(new LeagueIntroductionUIProperties(CompitionID.League));
        }
    }
}