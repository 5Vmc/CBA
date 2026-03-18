using Babu.BigNumber;
using BigBang.Animation;
using deVoid.UIFramework;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class BigBangResultProperties : WindowProperties
    {
        public BigNumber AddForce { get; private set; }
        public BigNumber IncomeForceAdd { get; private set; }

        public BigBangResultProperties(BigNumber addForce, BigNumber incomeForceAdd)
        {
            AddForce = addForce;
            IncomeForceAdd = incomeForceAdd;
        }
    }

    public class BigBangResultUI : AWindowController<BigBangResultProperties>
    {
        [SerializeField] private BigBangResultUIComponent com;
        [SerializeField] private BigBangResultAnim anim;

        protected override void AddListeners()
        {
            com.CloseBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            com.CloseBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            com.AddForceText.text = $"+{Properties.AddForce.ToFormatString()}";
            com.IncomeText.text = $"×{Properties.IncomeForceAdd.ToFormatString()}";
            AudioManager.Instance.PlaySound(AudioNames.EVENT_BBSUCCESS);
            anim.Play();
        }

        private void OnClose()
        {
            Babu.EventManager.Instance.Dispatch(EventID.OnBigBangResultClose);
            UIController.Instance.CloseWindow<BigBangResultUI>();
        }
    }
}