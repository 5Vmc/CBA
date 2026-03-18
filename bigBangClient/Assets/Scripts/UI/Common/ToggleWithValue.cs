using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;

namespace BigBang.UI
{
    public class ToggleWithValue : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        public Toggle Toggle => _toggle;
        [SerializeField] private GameObject enableStatus;
        [SerializeField] private GameObject disableStatus;
        [SerializeField] private int value;
        public int Value => value;

        private bool initialize = false;

        public void SetStatus(bool status)
        {
            if (status)
            {
                enableStatus.SetAlpha(1);
                disableStatus.SetAlpha(0);
            }
            else
            {
                if (!initialize)
                {
                    enableStatus.SetAlpha(0);
                    disableStatus.SetAlpha(1);
                    initialize = true;
                }
                else
                {
                    DOTween.To(value => enableStatus.SetAlpha(value), 1, 0, 0.25f);
                    DOTween.To(value => disableStatus.SetAlpha(value), 0, 1, 0.25f);
                }
            }
        }

        private void OnDisable()
        {
            initialize = false;
        }
    }
}