using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using Utils;

namespace BigBang.Battle
{
    /**
    战斗卡牌*
    */
    public class BattleCardItem : MonoBehaviour
    {
        [HideInInspector] public Protocol.FightCard fightCard;
        [HideInInspector] public int index = 0;
        [HideInInspector] public bool isRed;
        [HideInInspector] public bool isFirst;
        public void SetData(Protocol.FightCard fightCard, int index, bool isRed, bool isFirst, bool refreshNow = true)
        {
            this.fightCard = fightCard;
            this.index = index;
            this.isRed = isRed;
            this.isFirst = isFirst;

            ClearFireOnCardAni();
            if (refreshNow) RefreshCardInfo();
        }

        [HideInInspector] public BattleUI2 battleUI2 = null;
        public void SetBattleUI2(BattleUI2 battleUI2)
        {
            this.battleUI2 = battleUI2;
        }

        [SerializeField] public Transform MoveParent;
        [SerializeField] public SpriteRenderer BattleCardBg;
        [SerializeField] public SpriteRenderer PlayerImg;
        [SerializeField] public TMP_Text CombatEffectivenessText;
        [SerializeField] public TMP_Text PositionText;
        [SerializeField] public TMP_Text NameText;
        [SerializeField] public List<SpriteRenderer> starImageList = new();
        [SerializeField] public SpriteRenderer BattleCardBall;
        [SerializeField] public SpriteRenderer peakLogoImage = null;
        [SerializeField] public MeshRenderer CardHighLightImage;
        [SerializeField] public SpriteRenderer CardFireImage;

        public async void RefreshCardInfo()
        {
            BattleCardBg.sprite = fightCard.Star > 0 ? battleUI2.bgImageList[fightCard.Quality - 1] : battleUI2.bgNoStarImageList[fightCard.Quality - 1];
            if (isFirst == true)
            {
                BattleCardBall.sprite = battleUI2.ballImageList[fightCard.Quality - 1];
            }
            BattleCardBall.gameObject.SetActive(isFirst);
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(fightCard.CardId);
            bool isPeak = PlayerCard.IsPeak(cardModelConfig);
            peakLogoImage.gameObject.SetActive(isPeak);
            if(isPeak)
            {
                peakLogoImage.sprite = await SpriteProxy.GetPeakImage(cardModelConfig.PeakLogo);
            }
            if (fightCard.Portrait.ToString().Length > 6)
            {
                PlayerImg.sprite = await SpriteProxy.GetNpcPortrait(fightCard.Portrait);
            }
            else
            {
                PlayerImg.sprite = await SpriteProxy.GetPlayerPortrait(fightCard.Portrait); //cardModelConfig.Portrait);
            }
            CombatEffectivenessText.text = fightCard.Number.ToString();
            PositionText.text = GetPositionSeparatedShortName(fightCard.AdaptPosition[0]);
            NameText.text = fightCard.Name;
            NameText.color = battleUI2.textColorList[fightCard.Quality - 1];

            SetStar(fightCard.Star);
        }

        private void SetStar(int star)// 星级大于5之后换成彩色
        {
            bool isStarSpecial = star > 5;
            int showStar = isStarSpecial ? star - 5 : star;
            Sprite showSprite = isStarSpecial ? battleUI2.colorfulStar : battleUI2.normalStar;
            List<Transform> lightStarList = new();
            for (int i = 0; i < 5; i++)
            {
                if (i < showStar)
                {
                    SpriteRenderer starImage = starImageList[i];
                    starImage.gameObject.SetActive(true);
                    starImage.sprite = showSprite;
                    lightStarList.Add(starImage.transform);
                }
                else
                {
                    SpriteRenderer starImage = starImageList[i];
                    starImage.gameObject.SetActive(false);
                }
            }
            if (lightStarList.Count > 0)//设置星星居中
            {
                float oneDistance = 0.15f;
                float halfDistance = oneDistance / 2;
                float leftDistance = -halfDistance * (lightStarList.Count - 1);
                for (int i = 0; i < lightStarList.Count; i++)
                {
                    Vector3 oldPos = lightStarList[i].localPosition;
                    oldPos.x = leftDistance + i * oneDistance;
                    lightStarList[i].localPosition = oldPos;
                }
            }
        }

        private string GetPositionSeparatedShortName(int adaptPosition0)
        {
            var cfg = Configs.SeparatedPosition.GetConfig(adaptPosition0);
            if (cfg == null) return "";
            return cfg.Abbreviation;
        }

        #region 卡牌着火

        [HideInInspector] private Sequence fireOnCardSeq = null;
        [HideInInspector] private int fireIndex = 1;
        public async void PlayFireOnCardAni()
        {
            fireOnCardSeq?.Kill();

            if (CardFireImage == null) return;
            if (fightCard == null) return;

            CardFireImage.gameObject.SetActive(true);
            fireIndex = 1;
            CardFireImage.sprite = await SpriteProxy.GetBattle2CardFire(fightCard.Quality, fireIndex);

            fireOnCardSeq = DOTween.Sequence();
            fireOnCardSeq.AppendInterval(0.1f);
            fireOnCardSeq.AppendCallback(async () =>
            {
                fireIndex++;
                if (fireIndex > 7) fireIndex -= 7;
                if (CardFireImage == null) return;
                if (fightCard == null) return;
                CardFireImage.sprite = await SpriteProxy.GetBattle2CardFire(fightCard.Quality, fireIndex);
            });
            fireOnCardSeq.SetLoops(-1);
        }
        public void ClearFireOnCardAni()
        {
            fireOnCardSeq?.Kill();
            fireOnCardSeq = null;
            fireIndex = 1;
            if (CardFireImage == null) return;
            CardFireImage.gameObject.SetActive(false);
        }

        #endregion

    }
}