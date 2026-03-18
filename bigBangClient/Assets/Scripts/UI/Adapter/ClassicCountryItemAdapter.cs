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
    public class ClassicCountryItemAdapter : OSA<ClassicCountryItemParams, ClassicCountryItemHolder>
    {
        public SimpleDataHelper<ClassicCountryLevelData> Data { get; private set; }

        private List<ClassicCountryLevelData> ClassicCountryLevelDataList;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ClassicCountryLevelData>(this);
        }

        protected override void UpdateViewsHolder(ClassicCountryItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL 
        protected override bool IsRecyclable(ClassicCountryItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 

        public void SetData(List<ClassicCountryLevelData> ClassicCountryLevelDataList)
        {
            this.ClassicCountryLevelDataList = ClassicCountryLevelDataList;
            if (!IsInitialized) Init();
            Data.ResetItems(this.ClassicCountryLevelDataList);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.Register(EventID.ClassicCountryUIOnClickCountryButton, ClassicCountryUIOnClickCountryButton);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.Instance.Unregister(EventID.ClassicCountryUIOnClickCountryButton, ClassicCountryUIOnClickCountryButton);
        }
        public void ClassicCountryUIOnClickCountryButton(object[] args)
        {
            ClassicCountryLevelData data = (ClassicCountryLevelData)args[0];
            foreach (var item in Data)
            {
                item.isSelect = (item == data);
            }
            Refresh();
        }
        public void SetSelect(int countryId)
        {
            int index = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                if (item.challengeCountryConfig.Id == countryId) index = i;
                item.isSelect = (item.challengeCountryConfig.Id == countryId);
            }
            Refresh();
        }
        public void ScrollToSelect()
        {
            int index = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                if (item.isSelect) index = i;
            }
            if (Data.Count > 0) ScrollTo(index);
        }

        protected override ClassicCountryItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ClassicCountryItemHolder();
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
            //    GetItemViewsHolder(i).PlayAnim(i * 0.1f);
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
    public class ClassicCountryItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class ClassicCountryItemHolder : BaseItemViewsHolder
    {
        private ClassicCountryItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<ClassicCountryItem>();
        }

        public void UpdateViews(ClassicCountryLevelData data)
        {
            item.SetData(data);
        }

        // 播放动画
        public void PlayAnim(float delay)
        {
            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            root.gameObject.DOFade(0, 0.3f);
        }
    }
}