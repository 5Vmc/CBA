using System.Diagnostics.Tracing;
using System;
using Babu.Config;
using BigBang.Animation;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Babu;
using Utils;

namespace BigBang.UI
{
    public class SmallCardItem : MonoBehaviour
    {
        [SerializeField] private Button myBtn;
        // 球员姓名
        [SerializeField] private TMP_Text nameText;
       
        // 球员头像
        [SerializeField] private Image playerImg;
        // 卡片图片
        [SerializeField] private Image cardImg;

        [SerializeField] private Image ball;

        [SerializeField] private Image bounty;

        [SerializeField] private Image starMask;
       
  
       
        // 星级
        [SerializeField] private StarListItem starListItem;
        // 流光效果
        [SerializeField] private UIShiny shiny;

        [SerializeField] private GameObject sub;

        [SerializeField] private PeakImage peakImage = null;

        private PlayerCard cardData;
        public void SetData(PlayerCard card)
        {
            this.cardData = card;
            // 首发标记
            ball.gameObject.SetActive(card.IsStarter1() || card.IsStarter() || card.IsStarter2() || card.IsStarter3() || card.IsStarter4());

            bounty.gameObject.SetActive(card.IsUsingInBounty && !ball.gameObject.activeSelf);

            SetBaseData(card.Config);
            SetStars(card.Star);

            if(CardFirePad.Inst.IsSelectedCard(this.cardData.CardId))
            {
                ShowSubButton();
            }else{
                HideSubButton();
            }
        }

        private void OnEnable() {

            myBtn.onClick.AddListener(OnClickMe);
            
        }

        private void OnDisable(){
            myBtn.onClick.RemoveListener(OnClickMe);
        }

        private void SetStars(int star)
        {
            if(star == 0){
                starMask.gameObject.SetActive(false);
            }
            else{
                starMask.gameObject.SetActive(true);
            }
            this.starListItem.SetLevel(star, true);
        }

        private async void SetBaseData(CardModelConfig config)
        {
            // 设置球员头像
            playerImg.sprite = await SpriteProxy.GetPlayerPortrait(config.Portrait);

            // 设置球员姓名
            nameText.text = config.Name;
            // 设置球员位置

            // 设置卡片颜色
            cardImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Icon, cardData.Quality);
            
           // ball.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Ball, config.Quality);
          
            // 只有金色和红色牌才有流光效果
            if (shiny != null)
            {
                shiny.enabled = config.Quality >= QualityType.Orange;
                shiny.Play();
            }

            peakImage.SetData(config);


        }

        private string GetPositionName(CardModelConfig config)
        {
            var cfg = Configs.Position.GetConfig(config.Position);
            if (cfg == null) return "";
            return cfg.Name;
        }

        private string GetPositionSeparatedShortName(CardModelConfig config)
        {
            var cfg = Configs.SeparatedPosition.GetConfig(config.AdaptPosition[0]);
            if (cfg == null) return "";
            return cfg.Abbreviation;
        }

        private void OnClickMe()
        {
            if(CardFirePad.Inst.IsSelectedCard(this.cardData.CardId))
            {
                HideSubButton();
                CardFirePad.Inst.UnselectCard(this.cardData.CardId);
            }else{
                if(CardFirePad.Inst.CheckSelectedCount() == false){
                    Tips.PopTips("回收选择已满");
                    return;
                }
                ShowSubButton();
                CardFirePad.Inst.SelectCard(this.cardData.CardId);
            }

            EventManager.Instance.Dispatch(EventID.OnClickWillFireMe);
        }

        public void ShowSubButton()
        {
            sub.SetActive(true);
        }

        public void HideSubButton()
        {
            sub.SetActive(false);
        }
    }
}