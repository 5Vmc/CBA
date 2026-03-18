using System;
using Babu;
using BigBang.Animation;
using BigBang.Battle;
using DG.Tweening;
using GameConfig;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class ClassicEnterFightUIGuide : MonoBehaviour
    {
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private BabuButton formationButtonBlue = null;
        [SerializeField] private BabuButton fightBtn = null;

        private void OnEnable()
        {
            fightBtn.OnClick += OnClickFightBtn;
        }
        private void OnDisable()
        {
            fightBtn.OnClick -= OnClickFightBtn;
        }

        public void CheckGuide()
        {
            if (!GuideManager.IsFinished(GuideID.guidePass13))
            {
                OnGuideClickFightStart();
                return;
            }
        }
        private bool isGuideClickFightDoing = false;
        public bool IsGuideClickFightDoing
        {
            get
            {
                return isGuideClickFightDoing;
            }
        }
        [SerializeField] private RectTransform fingerPanel = null;
        //对手预览界面点开战
        public void OnGuideClickFightStart()
        {
            if (GuideManager.IsFinished(GuideID.guidePass13)) return;
            isGuideClickFightDoing = true;
            closeBtn.gameObject.SetActive(false);
            formationButtonBlue.gameObject.SetActive(false);
            fingerPanel.gameObject.SetActive(true);
            fingerPanel.gameObject.SetAlpha(0);
            fingerPanel.gameObject.DOFade(1, 0.8f);
        }
        private void OnClickFightBtn(BabuButton _)
        {
            OnGuideClickFightEnd();
        }
        public void OnGuideClickFightEnd()
        {
            if (GuideManager.IsFinished(GuideID.guidePass13)) return;
            if (isGuideClickFightDoing == false) return;
            fingerPanel.gameObject.SetActive(false);
            isGuideClickFightDoing = false;
        }

        public void CheckFinishPass13Guide()
        {
            if (!GuideManager.IsFinished(GuideID.guidePass13))
            {
                if (Player.BattleManager.classicTeamData == null) return;
                bool isAwayWin = !Player.BattleManager.fightInfo.Result.Win;
                bool isWin = isAwayWin;
                if (isWin == false) return;
                if (Player.BattleManager.classicTeamData.challengeClubConfig.Id >= 1010103)
                {
                    GuideManager.Finish(GuideID.guidePass13);
                }
                return;
            }
        }
    }
}