using System.Collections.Generic;
using System.Linq;
using Babu.Client.Fsm;
using BigBang.Animation;
using deVoid.UIFramework;
using Spine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ArenaEndRewardUI : APanelController
    {
        [SerializeField] private ArenaEndRewardUIAnim anim;
        [SerializeField] private ArenaResultLosePad arenaResultLosePad;
        [SerializeField] private ArenaResultWinPad arenaResultWinPad;
        [SerializeField] private Button closeButton;


        protected override void AddListeners()
        {
            base.AddListeners();
            closeButton.onClick.AddListener(OnClose);
            arenaResultLosePad.anim.onAniEnd += AfterWinLosePadAniEnd;
            arenaResultWinPad.anim.onAniEnd += AfterWinLosePadAniEnd;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeButton.onClick.RemoveListener(OnClose);
            arenaResultLosePad.anim.onAniEnd -= AfterWinLosePadAniEnd;
            arenaResultWinPad.anim.onAniEnd -= AfterWinLosePadAniEnd;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            anim.Init();
            bool isWin = Player.BattleManager.battleResponse.BattleWin;
            arenaResultWinPad.gameObject.SetActive(isWin);
            arenaResultLosePad.gameObject.SetActive(!isWin);
        }
        private void AfterWinLosePadAniEnd()
        {
            anim.PlayEnter();
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

            FsmManager.Instance.ChangeToState<StateBattle>(new StateCommonUserData()
            {
                OpenUIAction = async () =>
                {
                    UIController.Instance.HideTopestPanel();
                    await UIController.Instance.ShowPanel<ArenaUI>();
                }
            });

        }





    }
}