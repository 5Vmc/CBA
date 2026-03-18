using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utils
{
    public class TouchManager : Babu.BabuSingleton<TouchManager>
    {
        /*
        //原有的这种方式练unity发的消息也一起屏蔽掉了，比如按钮按下后变灰，之后禁用了消息系统，按钮释放后仍然会是灰色
        private GameObject eventSystem;

        /// <summary>
        /// 启用触摸
        /// </summary>
        public void EnableTouch()
        {
            if (eventSystem == null)
            {
                eventSystem = FindObjectOfType<EventSystem>().gameObject;
            }
            eventSystem.SetActive(true);
        }

        /// <summary>
        /// 禁用触摸
        /// </summary>
        public void DisableTouch()
        {
            if (eventSystem == null)
            {
                eventSystem = FindObjectOfType<EventSystem>().gameObject;
            }
            eventSystem.SetActive(false);
        }
        */


        /// <summary>
        /// 启用触摸
        /// </summary>
        public void EnableTouch()
        {
            Debug.Log("EnableTouch");
            UIController.Instance.IsTouchMaskShow = false;
        }

        /// <summary>
        /// 禁用触摸
        /// </summary>
        public void DisableTouch()
        {
            Debug.Log("DisableTouch");
            UIController.Instance.IsTouchMaskShow = true;
        }


    }
}