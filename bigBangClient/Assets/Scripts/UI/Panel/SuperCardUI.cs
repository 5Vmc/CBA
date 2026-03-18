using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using BigBang.Animation;
using GameConfig.Config;
using System.Collections.Generic;
using System;

namespace BigBang.UI
{
    public class SuperCardUIProperties : WindowProperties
    {
        private List<CardModelConfig> cardList;
        public Action CallBack;
        public bool IsRecruit = false;
       // public bool isRecruit;
        public SuperCardUIProperties(bool isRecruit, Action a, List<CardModelConfig> list)
        {
            cardList = list;
            CallBack = a;
            IsRecruit = isRecruit;
        }

        public CardModelConfig GetCard()
        {
            if(cardList.Count > 0){
                CardModelConfig card = cardList[0];
                cardList.RemoveAt(0);
                return card;
            }
            return null;
        }
    }

    public class SuperCardUI : AWindowController<SuperCardUIProperties>
    {
        [SerializeField] private Image lightImg;
        [SerializeField] private Image borderImg;
        [SerializeField] private CardItem cardItem;
        [SerializeField] private Button closeBtn;

        public SuperCardUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            CardModelConfig cardModel = Properties.GetCard();
            
            // 设置边缘光图片
            SpriteProxy.GetCardQualitySprite(SpriteNames.Card.DebrisBorder, cardModel.Quality);
            // 设置背光图片
            SpriteProxy.GetCardQualitySprite(SpriteNames.Card.DebrisLight, cardModel.Quality);
            // 设置卡片数据
            cardItem.SetConfigShow(cardModel);
            // 播放超级牌动画
            Anim.PlayEnter();

            //todo 如果多张卡片的时候
        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<SuperCardUI>();

            if(Properties.CallBack != null){
                Properties.CallBack.Invoke();
            }
                // 显示招募结果界面
            if(Properties.IsRecruit)
                Babu.EventManager.Instance.Dispatch(EventID.ShowRecruitResult);
            
        }
    }
}