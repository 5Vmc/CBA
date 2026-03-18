using Babu;
using GameConfig.Config;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class ActivityToggle : MonoBehaviour
    {
        public ActivityConfig cfg;

        [SerializeField] public TMP_Text txt1;
        [SerializeField] public TMP_Text txt2;
        [SerializeField] public Image reddot;
        [SerializeField] Image bgImg;

        public async void RefreshShow()
        {
            txt1.text = cfg.Name;
            txt2.text = cfg.Name;
            bgImg.sprite = await SpriteProxy.GetActivityImage(cfg.ToggleIcon);
            RefreshRedDot(null);
        }

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        }
        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            RefreshRedDot(null);
        }

        public void RefreshRedDot(object[] _)
        {
            bool isRed = ActivityController.Instance.HasRedDot(cfg.Id);
            reddot.transform.gameObject.SetActive(isRed);
        }
    }
}