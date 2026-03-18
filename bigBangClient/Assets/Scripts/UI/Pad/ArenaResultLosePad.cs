using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class ArenaResultLosePad : MonoBehaviour
    {
        private void OnEnable()
        {
            gotoButton1.onClick.AddListener(OnClickGotoButton1);
            gotoButton2.onClick.AddListener(OnClickGotoButton2);
            gotoButton3.onClick.AddListener(OnClickGotoButton3);
            SetData();
            anim.PlayEnter();
        }
        private void OnDisable()
        {
            gotoButton1.onClick.RemoveListener(OnClickGotoButton1);
            gotoButton2.onClick.RemoveListener(OnClickGotoButton2);
            gotoButton3.onClick.RemoveListener(OnClickGotoButton3);
        }

        [SerializeField] public ArenaResultLosePadAnim anim;
        [SerializeField] private GameObject rankNotChangePanel;
        [SerializeField] private GameObject rankDownPanel;
        [SerializeField] private TMP_Text rankOldNumText;
        [SerializeField] private TMP_Text rankNewNumText;
        [SerializeField] private Button gotoButton1;
        [SerializeField] private Button gotoButton2;
        [SerializeField] private Button gotoButton3;
        private void SetData()
        {
            int oldRank = Player.BattleManager.oldArenaInfo.ArenaRank;
            int newRank = Player.BattleManager.newArenaInfo.ArenaRank;
            bool isSameRank = oldRank == newRank;
            rankNotChangePanel.SetActive(isSameRank);
            rankDownPanel.SetActive(!isSameRank);
            if (!isSameRank)
            {
                rankOldNumText.text = oldRank.ToString();
                rankNewNumText.text = newRank.ToString();
            }
        }

        private void OnClickGotoButton1()
        {
            UIController.Instance.HidePanel<ArenaEndRewardUI>();
            if (Player.ChallengeManager.ChallengeId <= 103)
            {
                Tips.PopTips(LangID.NeedToUnlockRecruit);
                return;
            }
            UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.Auto));
        }
        private void OnClickGotoButton2()
        {
            UIController.Instance.HidePanel<ArenaEndRewardUI>();
            UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.Card));
        }
        private void OnClickGotoButton3()
        {
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.ARENA, formation =>
            {
                UIController.Instance.HidePanel<ArenaEndRewardUI>();
                UIController.Instance.ShowPanel<ArenaUI>(new ArenaUIProperties(ArenaUI.SubUIID.Tactic));
            });
        }
    }
}
