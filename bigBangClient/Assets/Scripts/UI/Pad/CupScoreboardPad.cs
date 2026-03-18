using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CupScoreboardPad : MonoBehaviour
    {
        [SerializeField] private Button rewardBtn;
        [SerializeField] private Button introduceBtn;
        [SerializeField] private SpiderMap64 spiderMap64;

        [SerializeField] private HorizontalLayoutGroup giftLayout = null;

        public int CupLevel { get; set; }

        private void Awake()
        {
#if UNITY_WEBGL
            giftLayout.childAlignment = TextAnchor.MiddleLeft;
#endif
        }

        private void OnEnable()
        {
            rewardBtn.onClick.AddListener(OnReward);
            introduceBtn.onClick.AddListener(OnIntroduce);
            spiderMap64.OnClickItem += OnClickSpiderItem;
        }

        private void OnDisable()
        {
            rewardBtn.onClick.RemoveListener(OnReward);
            introduceBtn.onClick.RemoveListener(OnIntroduce);
            spiderMap64.OnClickItem -= OnClickSpiderItem;
        }

        private void OnClickSpiderItem(CupScoreboardPadItem item)
        {
            if (item == null || item.dataProvider == null || item.dataProvider.Score == -1) return;

            NetworkManager.Instance.GetFightReport(item.dataProvider.FightID, response =>
            {
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.CupUI_Integral;
                Player.BattleManager.SetFightInfo(FightType.Cup, response);
                Player.BattleManager.StartPlayFight();
            });
        }

        private void OnReward()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_TARGET);
            // 打开奖励窗口
            UIController.Instance.OpenWindow<LeagueRewardsUI>(new LeagueRewardsUIProperties(CompitionID.Cup, CupLevel));
        }

        private void OnIntroduce()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            // 打开介绍窗口
            UIController.Instance.OpenWindow<LeagueIntroductionUI>(new LeagueIntroductionUIProperties(CompitionID.Cup));
        }

        public void SetData(SpiderMap64Data provider)
        {
            spiderMap64.SetShowDetailButton(true);
            spiderMap64.SetData(provider);
        }
    }
}