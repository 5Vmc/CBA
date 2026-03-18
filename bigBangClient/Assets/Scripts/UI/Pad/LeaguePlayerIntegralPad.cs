using System.Linq;
using Protocol;
using TMPro;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class LeaguePlayerIntegralPad : MonoBehaviour
    {
        [SerializeField] private TMP_Text leagueNameText;
        [SerializeField] private LeaguePlayerIntegralAdapter adapter;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle scoreToggle;
        [SerializeField] private BabuToggle assistToggle;
        [SerializeField] private BabuToggle reboundToggle; //篮板
        [SerializeField] private BabuToggle stealToggle; // 抢断
        [SerializeField] private BabuToggle blockToggle; // 盖帽
        [SerializeField] private TMP_Text valueTitle;

        private GetLeagueCardRankResponse data;

        private void OnEnable()
        {
            scoreToggle.OnSelect += OnScoreSelect;
            assistToggle.OnSelect += OnAssistSelect;
            stealToggle.OnSelect += OnStealSelect;
            reboundToggle.OnSelect += OnReboundSelect;
            blockToggle.OnSelect += OnBlockSelect;
        }

        private void OnDisable()
        {
            scoreToggle.OnSelect -= OnScoreSelect;
            assistToggle.OnSelect -= OnAssistSelect;
            stealToggle.OnSelect -= OnStealSelect;
            reboundToggle.OnSelect -= OnReboundSelect;
            blockToggle.OnSelect -= OnBlockSelect;
        }

        public void InitAnim()
        {
            adapter.InitAnim();
        }

        // 设置联赛数据
        public void SetData(GetLeagueCardRankResponse data, string leagueName)
        {
            this.data = data;
            // 设置联赛名称
            leagueNameText.text = leagueName;
            // 默认显示得分榜
            toggleGroup.Switch(scoreToggle);
            var list = data.GoalsScoredRank.Where(item => item.Point > 0).OrderByDescending(item => item.Point).ToList();
            adapter.SetData(list);
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示得分榜
        private void OnScoreSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Score;
            var list = data.GoalsScoredRank.Where(item => item.Point > 0).OrderByDescending(item => item.Point).ToList();
            adapter.SetData(list);
            valueTitle.text = "得分";//Lang.Get(LangID.GoalsScoredText);
                                   //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示助攻榜
        private void OnAssistSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Assists;
            var list = data.AssistsRank.Where(item => item.Assist > 0).OrderByDescending(item => item.Assist).ToList();
            adapter.SetData(list);
            valueTitle.text = Lang.Get(LangID.AssistsNumberText);
            //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示抢断榜
        private void OnStealSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Steal;
            var list = data.StealRank.Where(item => item.Steal > 0).OrderByDescending(item => item.Steal).ToList();
            adapter.SetData(list);
            valueTitle.text = Lang.Get(LangID.StealCountText);
            //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示篮板榜
        private void OnReboundSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Rebound;
            var list = data.ReboundRank.Where(item => item.Rebound > 0).OrderByDescending(item => item.Rebound).ToList();
          
            adapter.SetData(list);
            valueTitle.text =  "篮板数";//Lang.Get(LangID.ZeroKeeperText);
                                     //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        //显示盖帽榜
        private void OnBlockSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Block;
            var list = data.BlockRank.Where(item => item.Block > 0).OrderByDescending(item => item.Block).ToList();
            adapter.SetData(list);
            valueTitle.text =  "盖帽数";
            //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }
    }
}