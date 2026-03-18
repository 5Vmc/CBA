using System;
using Babu;
using DG.Tweening;

namespace BigBang
{
    public class SecondUpdateManager : BabuSingleton<SecondUpdateManager>
    {
        
        private Sequence secondUpdateSequence = null;
        public override void Awake()
        {
            base.Awake();

            secondUpdateSequence = DOTween.Sequence();
            secondUpdateSequence.AppendInterval(1.0f);
            secondUpdateSequence.AppendCallback(CallUpdateAction);
            secondUpdateSequence.SetLoops(-1);
        }

        private Action secondUpdateAction;
        public void RegistAction(Action act)
        {
            secondUpdateAction += act;
        }
        public void UnRegistAction(Action act)
        {
            secondUpdateAction -= act;
        }
        public void CallUpdateAction()
        {
            secondUpdateAction?.Invoke();
        }


    }
}