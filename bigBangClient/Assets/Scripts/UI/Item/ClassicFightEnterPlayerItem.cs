using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BigBang.Animation;
using Utils;
using TMPro;
using GameConfig;
using BigBang;
using GameConfig.Config;

namespace BigBang.UI
{
    public class ClassicFightEnterPlayerItem : MonoBehaviour
    {
        // 球员位置
        [SerializeField] private TMP_Text positionText;
        // 球员姓名
        [SerializeField] private TMP_Text nameText;
        // 球员品质
        [SerializeField] private Image qualityImg;
        // 球员头像
        [SerializeField] protected Image playerImg;

        [SerializeField] private Image upImage = null;
        [SerializeField] private Image downImage = null;
        [SerializeField] private TMP_Text fightPointText = null;

        [HideInInspector] public int fightPoint = 0;

        [SerializeField] private HorizontalLayoutGroup nameLayout = null;
        [SerializeField] private PeakImage peakImage = null;

        [SerializeField] private ClassicFightEnterPlayerItemAnim anim;
        public async void SetData(ChallengePlayerConfig config)
        {
            // 设置球员位置
            SeparatedPositionConfig positionCfg = null;
            if (config.AdaptPosition == null || config.AdaptPosition.Length <= 0)
            {
                Debug.LogWarning("ClassicFightEnterPlayerItem , config.AdaptPosition == null || config.AdaptPosition.Length <= 0 , ChallengePlayerConfig.id = " + config.Id);
                positionCfg = Configs.SeparatedPosition.GetConfig(1);
            }
            if (positionCfg == null)
            {
                positionCfg = Configs.SeparatedPosition.GetConfig(config.AdaptPosition[0]);
            }
            if (positionCfg == null)
            {
                Debug.LogWarning("ClassicFightEnterPlayerItem , positionCfg == null , config.AdaptPosition[0] = " + config.AdaptPosition[0] + " ,  ChallengePlayerConfig.id = " + config.Id);
                positionCfg = Configs.SeparatedPosition.GetConfig(1);
            }
            positionText.text = positionCfg.Abbreviation;
            // 设置球员姓名
            nameText.text = config.Name;
            peakImage.SetHide();
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameLayout.transform as RectTransform);
            // 设置品质
            qualityImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Icon, config.Quality);
            // 设置球员头像
            playerImg.sprite = await SpriteProxy.GetPortrait(config.Portrait);
            fightPoint = PlayerCard.GetNpcPlayerCombat(config, positionCfg);
            fightPointText.text = fightPoint.ToString();
            anim.Init();
        }

        public async void SetData(PlayerCard playerCard)
        {
            // 设置球员位置
            positionText.text = playerCard.GetAdaptPositionAbbreviation();
            // 设置球员姓名
            nameText.text = playerCard.Config.Name;
            peakImage.SetData(playerCard);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameLayout.transform as RectTransform);
            // 设置品质
            qualityImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Icon, playerCard.Quality);
            // 设置球员头像
            playerImg.sprite = await SpriteProxy.GetPortrait(playerCard.Config.Portrait);
            // 设置球员战力
            fightPoint = playerCard.FightPoint;
            fightPointText.text = fightPoint.ToString();
            anim.Init();
        }

        public void SetUpImage(bool isUp)
        {
            // 设置球员强弱标识
            upImage.gameObject.SetActive(isUp);
            downImage.gameObject.SetActive(false);
        }

        public void PlayInit()
        {
            anim.Init();
        }
        public void PlayEnter()
        {
            anim.PlayEnter(fightPoint);
        }
    }
}