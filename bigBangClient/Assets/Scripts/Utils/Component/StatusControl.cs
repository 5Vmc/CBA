using DG.Tweening;
using UnityEngine;

namespace Utils
{
    public class StatusControl : MonoBehaviour
    {
        [SerializeField] private GameObject enableStatus;
        [SerializeField] private GameObject disableStatus;

        private bool initialize = false;

        private Tween t1;
        private Tween t2;

        /// <summary>
        /// 如果搭配BabuToggle使用
        /// BabuToggle中的OnValueChanged会自动调用同游戏对象下的StatusControl的SetStatus
        /// 不要重复调用，防止出动画 bug
        /// </summary>
        public void SetStatus(bool status)
        {
            //Debug.Log("StatusControl , SetStatus , name = " + this.gameObject.name + " , status = " + status);
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
    }
}