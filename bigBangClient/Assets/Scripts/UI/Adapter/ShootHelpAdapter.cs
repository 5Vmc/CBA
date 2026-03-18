using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using GameConfig;
using GameConfig.Config;
using System.Linq;

namespace BigBang.UI
{
    public class ShootHelpAdapter : OSA<ShootHelpParams, ShootHelpViewsHolder>
    {
        public SimpleDataHelper<ShootGameStageConfig> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ShootGameStageConfig>(this);
        }

        protected override void UpdateViewsHolder(ShootHelpViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(ShootHelpViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif

        protected override void Start()
        {
            base.Start();
            SetData();
        }
        public void SetData()
        {
            if (!IsInitialized) Init();
            List<ShootGameStageConfig> result = Configs.ShootGameStage.GetConfigList().Where((item) => string.IsNullOrWhiteSpace(item.Reward) == false).ToList();
            Data.ResetItems(result);
        }

        protected override ShootHelpViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ShootHelpViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class ShootHelpParams : BaseParams
    {
        public GameObject prefab;
    }

    public class ShootHelpViewsHolder : BaseItemViewsHolder
    {
        private ShootHelpItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<ShootHelpItem>();
        }

        public void UpdateViews(ShootGameStageConfig shootGameStageConfig, int index)
        {
            item.SetData(shootGameStageConfig, index);
        }
    }
}