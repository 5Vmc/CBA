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
    public class WorldMapUIItemAdapter : OSA<WorldMapUIItemParams, WorldMapUIItemHolder>
    {
        public SimpleDataHelper<ClassicMapLevelData> Data { get; private set; }

        private List<ClassicMapLevelData> ClassicMapLevelDataList;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ClassicMapLevelData>(this);
        }

        protected override void UpdateViewsHolder(WorldMapUIItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(WorldMapUIItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<ClassicMapLevelData> ClassicMapLevelDataList, int selectMapId = 0)
        {
            this.ClassicMapLevelDataList = ClassicMapLevelDataList;
            if (selectMapId != 0)
            {
                foreach (var item in this.ClassicMapLevelDataList)
                {
                    item.isSelect = (item.challengeMapConfig.Id == selectMapId);
                }
            }
            if (!IsInitialized) Init();
            Data.ResetItems(this.ClassicMapLevelDataList);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.Register(EventID.OnClickWorldUIItem, OnClickWorldUIItem);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.Instance.Unregister(EventID.OnClickWorldUIItem, OnClickWorldUIItem);
        }
        public void OnClickWorldUIItem(object[] args)
        {
            WorldMapUIItem worldMapUIItem = (WorldMapUIItem)args[0];
            foreach (var item in Data)
            {
                item.isSelect = (item == worldMapUIItem.data);
            }
            Refresh();
        }

        protected override WorldMapUIItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new WorldMapUIItemHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 3)
                {
                    Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }
                GetItemViewsHolder(i).PlayAnim(i * 0.1f);
            }
        }

        public void InitAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).InitAnim();
            }
        }

        public void PlayExit()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).PlayExit();
            }
        }
    }

    [Serializable]
    public class WorldMapUIItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class WorldMapUIItemHolder : BaseItemViewsHolder
    {
        private WorldMapUIItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<WorldMapUIItem>();
        }

        public void UpdateViews(ClassicMapLevelData data)
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