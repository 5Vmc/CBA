using System;
using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using Coffee.UIEffects;
using GameConfig.Config;
using Babu.Config;
using Babu;
using static BigBang.SpriteNames;
using static BigBang.AllStarManager;
using Protocol;

namespace BigBang.UI
{
    public class AllStarPosterUIProperties : PanelProperties
    {
        public Area area;

        public AllStarPosterUIProperties(Area area)
        {
            this.area = area;
        }
    }
    public class AllStarPosterUI : APanelController<AllStarPosterUIProperties>
    {
        #region 初始化
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
        }
        private Area area = Area.South;
        [SerializeField] private List<Image> northImageList;
        [SerializeField] private List<Image> southImageList;
        protected override void OnPropertiesSet()
        {
            area = (Area)Properties.area;
            foreach (var item in northImageList)
            {
                item.gameObject.SetActive(area == Area.North);
            }
            foreach (var item in southImageList)
            {
                item.gameObject.SetActive(area == Area.South);
            }
        }
        #endregion

        #region 按钮回调
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.HidePanel<AllStarPosterUI>();
        }
        #endregion
    }
}