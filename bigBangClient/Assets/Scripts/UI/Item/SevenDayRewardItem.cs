using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Utils.GameItem;
using DG.Tweening;
using Utils;
using Coffee.UIEffects;
using UnityTimer;

namespace BigBang.UI
{
    public class SevenDayRewardItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text countTxt;
        [SerializeField] private TMP_Text dayTxt;
        [SerializeField] private BabuButton tipBtn;
        [SerializeField] public Image iconImg;
        [SerializeField] private Image completedImg;
        [SerializeField] private Image lightImg;
        [SerializeField] private Image bgImg;
        [SerializeField] private RectMask2D mask;
        [SerializeField] private UIShiny lineShiny;

        [IgnoreNullWarning] [SerializeField] private TMP_Text nameTxt;

        [SerializeField] private CardItem cardItem;


        private UIShiny shiny;

        private GameItem reward;

        private Color grayColor = new Color(100 / 255f, 100 / 255f, 100 / 255f, 1);

        public bool IsCompleted = false;

        private void Awake()
        {
            if (iconImg != null) shiny = iconImg.GetComponent<UIShiny>();
        }

        private void OnEnable()
        {
            tipBtn.OnClick += OnRewardTip;
        }

        private void OnDisable()
        {
            tipBtn.OnClick -= OnRewardTip;
        }

        private void OnRewardTip(BabuButton sender)
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);
            if (reward == null) return;
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(reward));
        }

        // 设置为可领取
        public void SetAsObtainable()
        {
            if (iconImg != null) iconImg.color = Color.white;
            completedImg.gameObject.SetActive(false);
            lightImg.gameObject.SetActive(true);
            lightImg.SetAlpha(0);
            lightImg.DOFade(1, 0.3f).SetDelay(1);
        }

        // 设置为已完成
        public void SetAsCompleted()
        {
            if (iconImg != null) iconImg.color = grayColor;
            completedImg.gameObject.SetActive(true);
            lightImg.gameObject.SetActive(false);
        }

        // 设置为未完成
        public void SetAsNormal()
        {
            if (iconImg != null) iconImg.color = Color.white;
            completedImg.gameObject.SetActive(false);
            lightImg.gameObject.SetActive(false);
        }

        public async void SetData(GameItem gameItem, ActivityClientType activityType, int activityId, int index, bool isCard = false)
        {
            setImg(gameItem, activityType, activityId, index);
            reward = gameItem;
            if (iconImg != null) iconImg.sprite = await reward.GetIcon();
            if (countTxt != null)
            {
                countTxt.text = reward.CountString();
            }
            if (nameTxt != null)
            {
                nameTxt.text = gameItem.GetName();
            }

            if (isCard)
            {
                PlayerCard playerCard = PlayerCard.GetEmptyCard(reward.Id);
                cardItem.SetData(playerCard);
            }
        }

        private async void setImg(GameItem gameItem, ActivityClientType activityType, int activityId, int index)
        {
            var colorHex = activityType == ActivityClientType.Sign7Day ? "#75B2D5" : "#ffe0d3";
            int skinInt = activityType == ActivityClientType.Sign7Day ? 1 : 2;
            ColorUtility.TryParseHtmlString(colorHex, out Color color);
            dayTxt.color = color;
            lightImg.sprite = await SpriteProxy.GetFestivalImg(skinInt, "img_687");
            completedImg.sprite = await SpriteProxy.GetFestivalImg(skinInt, "img_517_4");
            if (index == 7)
            {
                bgImg.sprite = await SpriteProxy.GetFestivalImg(skinInt, "img_688");
            }
            else
            {
                bgImg.sprite = await SpriteProxy.GetFestivalImg(skinInt, "img_686");
            }
        }

        public void PlayAnim()
        {
            if (iconImg != null) iconImg.color = Color.white;
            // 黑幕淡入
            if (iconImg != null) iconImg.DOColor(grayColor, 0.3f);
            // 盖章动画
            mask.enabled = false;
            completedImg.rectTransform.localScale = Vector3.one * 1.5f;
            AudioManager.Instance.PlaySound(AudioNames.BTN_STREN);
            completedImg.rectTransform.DOScale(1, 0.3f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                mask.enabled = true;
            });
        }

        public void PlayShiny(float delay, bool loop)
        {
            Timer.Register(this.gameObject, delay, () =>
            {
                if (shiny != null) shiny.effectPlayer.loop = loop;
                if (shiny != null) shiny.Play();
                lineShiny.effectPlayer.loop = loop;
                lineShiny.Play();
            });
        }
    }
}