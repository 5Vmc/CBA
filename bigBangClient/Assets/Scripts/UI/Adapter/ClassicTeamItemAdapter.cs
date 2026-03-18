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
    public class ClassicTeamItemAdapter : OSA<ClassicTeamItemParams, ClassicTeamItemHolder>
    {
        public SimpleDataHelper<ClassicTeamData> Data { get; private set; }

        private List<ClassicTeamData> ClassicTeamDataList;
        private int lookItemId;
        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ClassicTeamData>(this);
        }

        protected override void UpdateViewsHolder(ClassicTeamItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, lookItemId);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(ClassicTeamItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<ClassicTeamData> ClassicTeamDataList, int _lookItemId = 0)
        {
            lookItemId = _lookItemId;
            this.ClassicTeamDataList = ClassicTeamDataList.Where(p => p.isOpen == true).ToList();
            if (!IsInitialized) Init();
            Data.ResetItems(this.ClassicTeamDataList);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override ClassicTeamItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ClassicTeamItemHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    if (i < 4)
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
    public class ClassicTeamItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class ClassicTeamItemHolder : BaseItemViewsHolder
    {
        public ClassicTeamItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<ClassicTeamItem>();
        }

        public void UpdateViews(ClassicTeamData data, int _lookitemid = 0)
        {
            item.SetData(data, _lookitemid);
        }

        Tween fadeTween;
        Tween scaleTween;
        // 播放动画
        public void PlayAnim(float delay)
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            scaleTween = root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = root.gameObject.DOFade(0, 0.3f);
        }
    }
}