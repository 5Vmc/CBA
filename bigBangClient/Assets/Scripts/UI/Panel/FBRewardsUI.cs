using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Globalization;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Babu;
using GameConfig;
using System.Linq;
using GameConfig.Config;

namespace BigBang.UI
{
    public class FBRewardsUIProperties : PanelProperties
    {
        public FBRewardsUIProperties()
        {
        }
    }


    public class FBRewardsUI : APanelController<FBRewardsUIProperties>
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private FBRewardsItemAdapter itemAdapter;
        [SerializeField] private FBRewardsUIAnim anim;

        protected override void Awake()
        {
        }

        public void OnClickFBTowerGetStarReward(object[] args)
        {
            FBRewardsItem FBRewardsItem = args[0] as FBRewardsItem;
            FBTowerController.Instance.GetRewards(FBRewardsItem.data.cfg.Id, () =>
            {
                RefreshData();
            });
        }

        private void RefreshData()
        {
            itemAdapter.PlayExit();
            var listData = Configs.TowerStarReward.GetConfigList().ConvertAll<FBRewardsItemData>(p => new FBRewardsItemData(p));
            SetStatus(listData);

            listData = listData.OrderByDescending(p => p.Status).ToList<FBRewardsItemData>();
            itemAdapter.SetData(listData);
            itemAdapter.PlayAnim();
        }

        private void SetStatus(List<FBRewardsItemData> list)
        {
            foreach (var data in list)
            {
                var got = FBTowerController.Instance.FBData.getRewardsId >= data.cfg.Id;
                if (got)
                {
                    data.Status = -1;
                }
                else
                {
                    if (FBTowerController.Instance.FBData.totalStar >= data.cfg.Number)
                    {
                        data.Status = 1;
                    }
                    else
                    {
                        data.Status = 0;
                    }
                }
            }
        }

        protected override void AddListeners()
        {
            closeButton.onClick.AddListener(OnClickCloseBtn);
            EventManager.Instance.Register(EventID.OnClickFBTowerGetStarReward, OnClickFBTowerGetStarReward);
        }

        protected override void RemoveListeners()
        {
            closeButton.onClick.RemoveListener(OnClickCloseBtn);
            EventManager.Instance.Unregister(EventID.OnClickFBTowerGetStarReward, OnClickFBTowerGetStarReward);
        }

        protected override void OnPropertiesSet()
        {
            RefreshData();
            anim.PlayEnter();
        }

        private void OnClickCloseBtn()
        {
            itemAdapter.PlayExit();
            anim.PlayExit(() =>
            {
                UIController.Instance.HidePanel<FBRewardsUI>();
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            });
        }
    }
}