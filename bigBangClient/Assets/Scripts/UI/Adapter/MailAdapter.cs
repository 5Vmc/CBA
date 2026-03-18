using Babu;
using BigBang.Animation;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;
using Task = System.Threading.Tasks.Task;

namespace BigBang.UI
{
    public class MailAdapter : OSA<MailParams, MailItemViewsHolder>
    {
        public SimpleDataHelper<MailInfo> Data { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<MailInfo>(this);
        }
        protected override MailItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new MailItemViewsHolder();
            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
            return instance;
        }

        protected override void UpdateViewsHolder(MailItemViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
# if UNITY_WEBGL
        protected override bool IsRecyclable(MailItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetItems(List<MailInfo> items)
        {
            if (!IsInitialized)
                Init();
            Data.ResetItems(items);
        }

        public void InitAnim()
        {
            for (int i = 0; i < Data.Count; i++)
            {
                var visibleItem = GetItemViewsHolderIfVisible(i);
                if (visibleItem != null)
                {
                    CanvasGroup canvasGrup = visibleItem.root.GetComponent<CanvasGroup>();
                    canvasGrup.alpha = 0f;
                }
            }
        }

        public void AnimIn()
        {
            for (int i = 0; i < Data.Count; i++)
            {
                var visibleItem = GetItemViewsHolderIfVisible(i);
                if (visibleItem != null)
                {
                    CanvasGroup canvasGrup = visibleItem.root.GetComponent<CanvasGroup>();
                    canvasGrup.alpha = 0;
                    canvasGrup.DOFade(1, 0.3f).SetDelay(i * 0.05f);
                }
            }
        }

        public void AnimOut()
        {
            for (int i = 0; i < Data.Count; i++)
            {
                var visibleItem = GetItemViewsHolderIfVisible(i);
                if (visibleItem != null)
                {
                    CanvasGroup canvasGrup = visibleItem.root.GetComponent<CanvasGroup>();
                    canvasGrup.alpha = 1;
                    canvasGrup.DOFade(0, 0.3f).SetDelay(i * 0.05f);
                }
            }
        }

        public void PlayDeleteAnim(Action callback)
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).PlayDeleteAnim(i * 0.1f);
            }
            Timer.Register(this.gameObject, VisibleItemsCount * 0.1f + 0.01f + 0.25f, callback);
        }
    }

    [Serializable]
    public class MailParams : BaseParamsWithPrefab { }

    public class MailItemViewsHolder : BaseItemViewsHolder
    {
        private Image icon;
        private Image background;
        private TMP_Text titleText;
        private TMP_Text sendTimeText;
        private TMP_Text overDueText;
        private Button btn;
        private List<RectTransform> InventoryItemList = new List<RectTransform>();
        private MailInfo _data;
        private Tween iconTween;

        public EmailItem emailItem;

        public override void CollectViews()
        {
            btn = root.GetComponent<Button>();
            background = root.GetComponent<Image>();
            btn.onClick.AddListener(OnClick);
            root.GetComponentAtPath("Icon", out icon);
            root.GetComponentAtPath("TitleText", out titleText);
            root.GetComponentAtPath("SendTimeText", out sendTimeText);
            root.GetComponentAtPath("OverDueText", out overDueText);
            emailItem = root.GetComponent<EmailItem>();
            for (int i = 1; i <= 5; i++)
            {
                root.GetComponentAtPath("InventoryItemLayout/InventoryItem" + i.ToString(), out RectTransform inventoryItem);
                InventoryItemList.Add(inventoryItem);
            }
            iconTween = icon.rectTransform.DOAnchorPosY(10, 1).SetLoops(-1).Pause();
        }

        public void PlayDeleteAnim(float delay)
        {
            root.DoRelativeAnchorPosX(-1000, 0.25f).SetDelay(delay);
            //root.gameObject.DOFade(0, 0.15f).SetDelay(delay);
        }

        public void UpdateViews(MailInfo data)
        {
            root.SetAnchoredPositionX(0);
            root.gameObject.SetAlpha(1);
            _data = data;
            titleText.text = data.title;

            UpdateShowState();

            sendTimeText.text = _data.GetSendTime();
            overDueText.text = _data.GetOverdueTime();

            for (int i = 0; i < InventoryItemList.Count; i++)
            {
                var item = InventoryItemList[i];
                if (i < _data.attachment.Count)
                {
                    item.gameObject.SetActive(true);
                    item.GetComponent<InventoryItem>().SetData(GameItemUtils.UnPack(_data.attachment[i]));
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateShowState()
        {
            switch (_data.state)
            {
                case (int)EmailState.New:
                    SpriteManager.GetSprite(AtlasNames.Email, "New", s => icon.sprite = s);
                    iconTween.Restart();
                    break;
                case (int)EmailState.READED:
                    SpriteManager.GetSprite(AtlasNames.Email, "Readed", s => icon.sprite = s);
                    iconTween.Pause();
                    break;
                case (int)EmailState.CANDELETE:
                    SpriteManager.GetSprite(AtlasNames.Email, "CanDelete", s => icon.sprite = s);
                    iconTween.Pause();
                    break;
            }

            //if (_data.state == (int)EmailState.New || (_data.state == (int)EmailState.READED && _data.attachment.Count > 0))
            //    background.sprite = SpriteManager.GetSprite(AtlasNames.Email, "Light");
            //else
            //    background.sprite = SpriteManager.GetSprite(AtlasNames.Email, "Dark");

            if (_data.CanDelete())
            {
                SpriteManager.GetSprite(AtlasNames.Email, "Dark", s => background.sprite = s);
            }
            else
            {
                SpriteManager.GetSprite(AtlasNames.Email, "Light", s => background.sprite = s);
            }
        }

        private void OnClick()
        {
            emailItem.PlayLightAnim(PlayOpenAnim);
        }

        private async void PlayOpenAnim()
        {
            if (_data.state == (int)EmailState.New)
            {
                for (int i = 1; i <= 6; i++)
                {
                    SpriteManager.GetSprite(AtlasNames.Email, "email" + i, s => icon.sprite = s);
                    await Task.Delay(100);
                }
                UIController.Instance.OpenWindow<EmailDetailWindow>(new EmailDetailWindowProperties(_data));
                EventManager.Instance.Dispatch(EventID.OnClickMailBoxUIMail);
                await Task.Delay(1000);
                Player.EmailManager.ReadEmail(_data.id);
            }
            else
            {
                UIController.Instance.OpenWindow<EmailDetailWindow>(new EmailDetailWindowProperties(_data));
                EventManager.Instance.Dispatch(EventID.OnClickMailBoxUIMail);
            }
        }
    }
}
