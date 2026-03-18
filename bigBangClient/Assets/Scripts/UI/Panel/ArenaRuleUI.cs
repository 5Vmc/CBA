using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;

namespace BigBang.UI
{
    public class ArenaRuleUIProperties : WindowProperties
    {
        public int txtType = 0;
        public ArenaRuleUIProperties(int _txtType = 0)
        {
            txtType = _txtType;
        }
    }
    public class ArenaRuleUI : AWindowController<ArenaRuleUIProperties>
    {

        [SerializeField] private Button closeBtn;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private ConfirmationBoxUIAnim anim;
        [SerializeField] private TMP_Text ContentText;

        private List<string> helpText = new();
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            anim.PlayEnter();
            helpText.Clear();
            helpText.Add("1.竞技场分为9个阶，每个阶段需要的积分不同\n2.当积分足够，球队晋级到下一阶，并且获得晋阶奖励\n3.球队到S阶以后，进入服务器排行榜\n4.球队到S阶以后，可以查看挑战记录\n5.竞技场积分和排行每2周重置\n6.D, C, B, A阶段战斗失败不扣积分，S阶段战斗失败会置换排名");
            helpText.Add("1.排行每小时刷新\n2.奖励将于活动结束后通过邮件发放\n3.每个玩家在每个排行榜中只能有1名球星上榜");

            ContentText.text = helpText[Properties.txtType];
        }

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            confirmBtn.onClick.AddListener(onConfirm);
           
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            confirmBtn.onClick.RemoveListener(onConfirm);
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<ArenaRuleUI>();
                TouchManager.Instance.EnableTouch();
            });
        }

        private void onConfirm()
        {
            this.OnClose();
        }
    }
}