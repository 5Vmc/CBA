using Babu.BigNumber;
using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;

namespace BigBang.UI
{
    public class ExpRewardProperties : WindowProperties
    {
        public string Content { get; set; }
        public BigNumber RewardExp { get; set; }

        public ExpRewardProperties(BigNumber exp, string content = "")
        {
            RewardExp = exp;
            Content = content;
        }
    }

    public class ExpRewardUI : AWindowController<ExpRewardProperties>
    {
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text rewardText;

        [SerializeField] private Button confirmBtn;

        [SerializeField] public ExpRewardUIAnim Anim;
        [SerializeField] private List<GameObject> blueParticles;
        [SerializeField] private List<GameObject> yellowParticles;

        protected override void AddListeners()
        {
            confirmBtn.onClick.AddListener(OnConfirm);
        }

        protected override void RemoveListeners()
        {
            confirmBtn.onClick.RemoveListener(OnConfirm);
        }

        protected override void OnPropertiesSet()
        {
            contentText.text = Properties.Content;
            rewardText.text = $"+{Properties.RewardExp.ToFormatString()}";
            Anim.Play();
        }

        private void OnConfirm()
        {
            confirmBtn.GetComponent<ButtonAnim>().Play(Close);
        }

        private void Close()
        {
            Anim.PlayDiamondMove(() =>
            {
                UIController.Instance.CloseWindow<ExpRewardUI>();
            });
        }
    }
}