using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using System.Linq;
using BigBang.Animation;
using DG.Tweening;
using Utils;
using UnityTimer;
using GameConfig.Config;
using Babu;
using static BigBang.ClassicManager;

namespace BigBang.UI
{
    public class SeasonPassItemAdapter : OSA<SeasonPassItemParams, SeasonPassItemHolder>
    {
        public SimpleDataHelper<ActivityBattlePassRewardConfig> Data { get; private set; }

        private List<ActivityBattlePassRewardConfig> ActivityBattlePassRewardConfigList;
        private ActivityData activityData;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ActivityBattlePassRewardConfig>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(SeasonPassItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        protected override void UpdateViewsHolder(SeasonPassItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, activityData, newOrRecycled.ItemIndex);
        }

        public void SetData(List<ActivityBattlePassRewardConfig> ActivityBattlePassRewardConfigList, ActivityData activityData)
        {
            this.ActivityBattlePassRewardConfigList = ActivityBattlePassRewardConfigList;
            this.activityData = activityData;
            if (!IsInitialized) Init();
            Data.ResetItems(this.ActivityBattlePassRewardConfigList);
            RefreshInfo(activityData);
        }
        public void RefreshInfo(ActivityData activityData)
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).item.RefreshState(activityData);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override SeasonPassItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new SeasonPassItemHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    if (i < 3)
            //    {
            //        Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
            //    }
            //    GetItemViewsHolder(i).PlayAnim(i * 0.02f);
            //}
        }

        public void InitAnim()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    GetItemViewsHolder(i).InitAnim();
            //}
        }

        public void PlayExit()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    GetItemViewsHolder(i).PlayExit();
            //}
        }
    }

    [Serializable]
    public class SeasonPassItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class SeasonPassItemHolder : BaseItemViewsHolder
    {
        public SeasonPassItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<SeasonPassItem>();
        }

        public void UpdateViews(ActivityBattlePassRewardConfig data, ActivityData activityData, int index)
        {
            item.SetData(data, index);
            item.RefreshState(activityData);
        }

        // 播放动画
        public void PlayAnim(float delay)
        {
            InitAnim();
            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            StopAnim();
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            StopAnim();
            root.gameObject.DOFade(0, 0.3f);
        }

        public void StopAnim()
        {
            root.gameObject.DOKill();
            root.DOKill();
        }
        public void SetStateNormal()
        {
            StopAnim();
            root.gameObject.SetAlpha(1);
            root.localScale = Vector3.one;
        }
    }
}