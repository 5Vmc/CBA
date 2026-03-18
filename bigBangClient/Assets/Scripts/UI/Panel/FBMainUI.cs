using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    [System.Serializable]
    public class FBMainUIProperties : PanelProperties
    {
        public FBMainUIProperties()
        {
        }
    }

    public class FBMainUI : APanelController<FBMainUIProperties>
    {

        [SerializeField] private GameObject padContainer;
        [SerializeField] private List<TinyFunItem> TinyFunList;
        [SerializeField] private List<RedDotNode> reddotList;



        private Dictionary<int, GameObject> padState = new();
        #region 初始化

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.RefreshWindow, RefreshUI);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.RefreshWindow, RefreshUI);
        }

        protected override void OnPropertiesSet()
        {
            //在refreshUI里，根据Properties选过selectedIndex
            RefreshUI();
        }

        private void RefreshUI(object[] args = null)
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);

            List<ModuleDefineConfig> list = Configs.ModuleDefine.GetConfigList().FindAll(p => p.Id == 2900 || p.Id == 1200);

            TinyFunList[0].SetData(list[0]);
            TinyFunList[1].SetData(list[1]);
        }
        #endregion

        #region 关闭界面
        [SerializeField] private Button closeBtn;
        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            UIController.Instance.HidePanel<FBMainUI>();
        }
        #endregion

    }
}
