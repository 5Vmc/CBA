using System.Collections.Generic;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using GameConfig.Config;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CardConfigGridAdapter : GridAdapter<CardConfigInfoParams, CardConfigGridViewsHolder>
    {
        public SimpleDataHelper<CardModelConfig> Data { get; private set; }

        private List<CardModelConfig> _cardList;
        protected override void Start()
        {
            Data = new SimpleDataHelper<CardModelConfig>(this);
            base.Start();
            SetData(_cardList);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<CardConfigGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetData(List<CardModelConfig> cardList)
        {
            _cardList = cardList;
            if (!IsInitialized) return;
            if (cardList is null)
            {
                cardList = new List<CardModelConfig>();
            }
            Data.ResetItems(cardList);
        }

        protected override void UpdateCellViewsHolder(CardConfigGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
    }

    [System.Serializable]
    public class CardConfigInfoParams : GridParams { }

    public class CardConfigGridViewsHolder : CellViewsHolder
    {
        // //球员姓名
        // private TMP_Text nameText;
        // //球员位置
        // private TMP_Text positionText;
        // //球员号码
        // private TMP_Text numberText;
        // //球员头像
        // private Image playerImg;
        // //卡片图片
        // private Image cardImg;
        // //旗帜图片
        // private Image flagImg;
        // //足球颜色
        // private Image ballImg;
        // //边框颜色
        // private Image borderImg;
        // //点击按钮
        // private Button btn;
        // private PlayerCard _card;
        // private List<GameObject> stars = new List<GameObject>();
        private CardModelConfig _config;
        private CardItem cardItem;
        private Button btn;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<CardItem>();
            btn = root.GetComponentAtPath<Button>("Button");
            btn.onClick.AddListener(OnClick);

            // btn = views.GetComponent<Button>();
            // btn.onClick.AddListener(OnClickCard);
        }

        // private void OnClickCard()
        // {
        //     UIController.Instance.ShowPanel<CardDetailUI>(new CardDetailProperties(_card));
        // }

        private void OnClick()
        {
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(_config));
        }

        public void UpdateViews(CardModelConfig config)
        {
            _config = config;

            cardItem.SetConfigShow(_config);
            cardItem.transform.localScale = Vector3.one * 0.8f;
        }
    }
}