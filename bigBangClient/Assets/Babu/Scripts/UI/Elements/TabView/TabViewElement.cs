using UnityEngine;

namespace Babu.UI
{
    public class TabViewElement : MonoBehaviour
    {
        private TransAnimation _transAnim;

        void Awake()
        {
            _transAnim = GetComponent<TransAnimation>();
        }

        void OnEnable()
        {
            OnAddListeners();
        }

        void OnDisable()
        {
            OnRemoveListeners();
        }

        public void Show(params object[] args)
        {
            gameObject.SetActive(true);
            OnShow(args);
            _transAnim?.In(null);
        }

        public void Hide()
        {
            if (_transAnim != null)
            {
                _transAnim.Out(() =>
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual void OnAddListeners()
        {

        }

        protected virtual void OnRemoveListeners()
        {

        }

        protected virtual void OnShow(params object[] args)
        {

        }

        protected virtual void OnHide()
        {

        }

        protected void RegisterEvent(string eve, EventManager.EventCallback onEvent)
        {
            EventManager.Instance.Register(eve, onEvent);
        }

        protected void UnregisterEvent(string eve, EventManager.EventCallback onEvent)
        {
            EventManager.Instance.Unregister(eve, onEvent);
        }
    }
}
