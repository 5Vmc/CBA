using DG.Tweening;
using UnityEngine;

namespace Utils
{
    public class StatusRedDotControl : MonoBehaviour
    {
        [SerializeField] private GameObject enableStatus;
        [SerializeField] private GameObject disableStatus;

        [SerializeField] private GameObject redDot; // 红点

        private bool initialize = false;

        private Tween t1;
        private Tween t2;


        public void SetStatus(bool status)
        {
            
            if (status)
            {
                t1?.Kill();
                t2?.Kill();
                enableStatus.SetAlpha(1);
                disableStatus.SetAlpha(0);
            }
            else
            {
                if (!initialize)
                {
                    t1?.Kill();
                    t2?.Kill();
                    enableStatus.SetAlpha(0);
                    disableStatus.SetAlpha(1);
                    initialize = true;
                }
                else
                {
                    t1 = DOTween.To(value => enableStatus.SetAlpha(value), 1, 0, 0.25f);
                    t2 = DOTween.To(value => disableStatus.SetAlpha(value), 0, 1, 0.25f);
                }
            }
        }

        private void OnDisable()
        {
            initialize = false;
        }

        public void ShowRedDot(bool show)
        {
            redDot.SetActive(show);
        }
    }
}