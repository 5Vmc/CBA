using UnityEngine;

namespace Babu.UI
{
    // 状态管理，可以配合TransAnimation接口做动画效果
    public class StatusController : MonoBehaviour
    {
        [SerializeField] bool _defaultStatus = false;
        [SerializeField] GameObject _enableStatus;
        [SerializeField] GameObject _disableStatus;

        private TransAnimation _enableStatusTransAnimation;
        private TransAnimation _disableStatusTransAnimation;

        void Awake()
        {
            _enableStatusTransAnimation = _enableStatus?.GetComponent<TransAnimation>();
            _disableStatusTransAnimation = _disableStatus?.GetComponent<TransAnimation>();

            _enableStatus?.SetActive(_defaultStatus);
            _disableStatus?.SetActive(!_defaultStatus);
        }

        public void SetStatus(bool status)
        {
            if (status)
            {
                _enableStatus?.SetActive(true);
                _enableStatusTransAnimation?.In(null);
                
                if (_disableStatusTransAnimation != null)
                {
                    _disableStatusTransAnimation.Out(() =>
                    {
                        _disableStatus?.SetActive(false);
                    });
                }
                else
                {
                    _disableStatus?.SetActive(false);
                }
            }
            else
            {
                _disableStatus?.SetActive(true);
                _disableStatusTransAnimation?.In(null);

                if (_enableStatusTransAnimation != null)
                {
                    _enableStatusTransAnimation.Out(() =>
                    {
                        _enableStatus?.SetActive(false);
                    });
                }
                else
                {
                    _enableStatus?.SetActive(false);
                }
            }
        }
    }
}
