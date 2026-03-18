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
    public class RecruitRewardsUIProperties : PanelProperties
    {
        public int PoolId;
        public bool isFestival;
        public RecruitRewardsUIProperties(int poolid = 1, bool isFestival = false)
        {
            PoolId = poolid;
            this.isFestival = isFestival;
        }
    }


    public class RecruitRewardsUI : APanelController<RecruitRewardsUIProperties>
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private RecruitRewardsItemAdapter shopItemAdapter;

        //private ArenaExShopUIAnim Anim; 
        public RecruitRewardsUIAnim Anim;

        [SerializeField] private ResourceTitle resourceTitle = null;

        protected override void Awake()
        {

        }

        public void OnBuyItem(object[] args)
        {
            SetDatas();
        }

        private void SetDatas()
        {
            shopItemAdapter.PlayExit();

            resourceTitle.FieldRecruitItem = !Properties.isFestival;
            resourceTitle.FieldRecruitItem1 = Properties.isFestival;

            var listData = Player.CardManager.RecruitController.GetRewardsData(Properties.PoolId);
            SetStatus(listData);
            Player.CardManager.RecruitController.CheckRedData(Properties.PoolId);

            listData = listData.OrderByDescending(p => p.Status).ToList<RecruitRewardItemData>();
            shopItemAdapter.SetData(listData);
            shopItemAdapter.PlayAnim();
        }

        private void SetStatus(List<RecruitRewardItemData> list)
        {
            var pool = Player.CardManager.RecruitController.GetPool(Properties.PoolId);
            foreach (var data in list)
            {
                var got = pool.GotRewardsCount.Exists(itemid => itemid == data.cfg.Id);
                if (got)
                {
                    data.Status = -1;
                }
                else
                {
                    if (pool.TotalCount >= data.cfg.Option)
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
            EventManager.Instance.Register(EventID.ClassicShopUIItemBuy, OnBuyItem);
        }

        protected override void RemoveListeners()
        {
            closeButton.onClick.RemoveListener(OnClickCloseBtn);
            EventManager.Instance.Unregister(EventID.ClassicShopUIItemBuy, OnBuyItem);
        }



        protected override void OnPropertiesSet()
        {
            SetDatas();
        }

        private void OnClickCloseBtn()
        {
            shopItemAdapter.PlayExit();
            UIController.Instance.HidePanel<RecruitRewardsUI>();
            Player.CardManager.RecruitController.CheckRedData(Properties.PoolId);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
    }
}