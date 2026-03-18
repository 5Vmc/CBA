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
using Utils.GameItem;

namespace BigBang.UI
{
    public class HundredGuessExchangeAdapter : OSA<HundredGuessExchangeParams, HundredGuessExchangeHolder>
    {
        public SimpleDataHelper<ShopItemData> Data { get; private set; }

        private List<ShopItemData> DataSource;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ShopItemData>(this);
        }

        protected override void UpdateViewsHolder(HundredGuessExchangeHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(HundredGuessExchangeHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif

        protected override void Start()
        {
            base.Start();
            SetData(hundredGuessDataList, type);
        }
        private List<ShopItemData> hundredGuessDataList = null;
        private int type = 0;
        public void SetData(List<ShopItemData> hundredGuessDataList, int type)
        {
            this.hundredGuessDataList = hundredGuessDataList;
            this.type = type;
            if (!IsInitialized) return;
            DataSource = new List<ShopItemData>();
            foreach (ShopItemData item in hundredGuessDataList)
            {
                if (item.cfg.Type == type)
                {
                    DataSource.Add(item);
                }
            }
            if (!IsInitialized) Init();
            Data.ResetItems(this.DataSource);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override HundredGuessExchangeHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new HundredGuessExchangeHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        private UnityTimer.Timer nextFrameTimer = null;

        public void PlayAnim()
        {
            this.gameObject.SetAlpha(0);
            nextFrameTimer?.Cancel();
            nextFrameTimer = UnityTimer.Timer.Register(this.gameObject, 0.05f, () =>
            {
                this.InitAnimAll();
                this.gameObject.SetAlpha(1);
                this.PlayAnimAll();
            }).AddTo(this.gameObject);
        }

        private void PlayAnimAll()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 4)
                {
                    Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }
                GetItemViewsHolder(i).anim.PlayAnim(i * 0.1f);
            }
        }

        private void InitAnimAll()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).anim.InitAnim();
            }
        }
    }

    [Serializable]
    public class HundredGuessExchangeParams : BaseParams
    {
        public GameObject prefab;
    }

    public class HundredGuessExchangeHolder : BaseItemViewsHolder
    {
        private HundredGuessExchangeItem item;
        public AdapterItemAnim anim;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<HundredGuessExchangeItem>();
            anim = root.GetComponent<AdapterItemAnim>();
        }

        public void UpdateViews(ShopItemData data)
        {
            item.SetData(data);
            anim.InitAnim();
            anim.PlayAnim(0);
        }


    }
}