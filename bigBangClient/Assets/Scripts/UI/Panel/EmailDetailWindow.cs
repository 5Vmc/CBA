using System;
using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    [Serializable]
    public class EmailDetailWindowProperties : WindowProperties
    {
        public MailInfo data;
        public EmailDetailWindowProperties(MailInfo info)
        {
            data = info;
        }
    }

    public class EmailDetailWindow : AWindowController<EmailDetailWindowProperties>
    {
        [SerializeField] private Image blackImg;
        [SerializeField] private RectTransform paper;
        [SerializeField] private TMP_Text receiverText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text senderText;
        [SerializeField] private TMP_Text overdueText;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private RectTransform attachmentRT;
        [SerializeField] private RectTransform collectRT;
        [SerializeField] public EmailDetailWindowAnim Anim;

        private MailInfo _data;
        private Vector2 envelopeOriginPos;

        protected override void AddListeners()
        {
            base.AddListeners();
            confirmBtn.onClick.AddListener(OnConfirm);
            closeBtn.onClick.AddListener(OnClose);

            EventManager.Instance.Register(EventID.OnReceiveEmailDetail, OnReceiveEmail);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            confirmBtn.onClick.RemoveListener(OnConfirm);
            closeBtn.onClick.RemoveListener(OnClose);

            EventManager.Instance.Unregister(EventID.OnReceiveEmailDetail, OnReceiveEmail);
        }

        [SerializeField] private ScrollRect scrollView = null;
        [SerializeField] private EmailDetailWindowGuide emailDetailWindowGuide = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            _data = Properties.data;
            receiverText.text = _data.receiverTitle; //收件人标题
            // <tab>替换为2个中文空格
            contentText.text = _data.content.Replace("<tab>", "　　");
            senderText.text = _data.sender;
            overdueText.text = _data.GetOverdueTime();
            LayoutRebuilder.ForceRebuildLayoutImmediate(receiverText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(senderText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(receiverText.transform.parent as RectTransform);

            if (_data.CanReceive())
            {
                confirmText.text = Lang.Get(LangID.ReceiveEmail);
            }
            else
            {
                confirmText.text = Lang.Get(LangID.DeleteEmail);
            }
            bool hasAttachment = _data.HasAttachment();
            attachmentRT.gameObject.SetActive(hasAttachment);
            (scrollView.transform as RectTransform).SetSizeDeltaHeight(hasAttachment ? 475 : 670);
            scrollView.ScroolToTop(0);

            collectRT.gameObject.SetActive(_data.state == (int)EmailState.CANDELETE && _data.attachment.Count > 0);

            SetRewards();

            Anim.PlayEnter();

            emailDetailWindowGuide.CheckGuide();
        }

        private void SetRewards()
        {
            List<GameItem> gameItemList = new();
            for (int i = 0; i < _data.attachment.Count; i++)
            {
                GameItem gameItem = GameItemUtils.UnPack(_data.attachment[i]);
                gameItemList.Add(gameItem);
            }
            SetRewards(gameItemList);
        }

        private void OnConfirm()
        {
            if (_data.CanReceive())
            {
                Player.EmailManager.ReceiveEmail(_data.id);
            }
            else
            {
                Player.EmailManager.DeleteEmail(_data.id);
                float duration = 0.3f;
                blackImg.DOFade(0, duration);
                paper.DORotate(new Vector3(0, 0, -15), duration);
                paper.DOAnchorPosX(envelopeOriginPos.x + 100, duration);
                paper.gameObject.DOFade(0, duration).OnComplete(() =>
                {
                    UIController.Instance.CloseWindow<EmailDetailWindow>();
                });
            }
        }

        private void OnClose()
        {
            Anim.PlayExit(() => UIController.Instance.CloseWindow<EmailDetailWindow>());
        }

        private void OnReceiveEmail(object[] args)
        {
            Timer.Register(this.gameObject, 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT));
            confirmText.text = Lang.Get(LangID.DeleteEmail);
            collectRT.gameObject.SetActive(true);
            collectRT.localScale = Vector3.one * 5f;
            collectRT.DOScale(1, 0.3f);
        }

        [SerializeField] private HorizontalLayoutGroup layout;
        [SerializeField] private GameObject itemPrefab;
        private void SetRewards(List<GameItem> gameItemList)
        {
            Transform layoutTrans = layout.transform;
            while (layoutTrans.childCount < gameItemList.Count) Instantiate(itemPrefab, layoutTrans);
            for (int i = 0; i < layoutTrans.childCount; i++)
            {
                if (i < gameItemList.Count)
                {
                    var reward = gameItemList[i];
                    var child = layoutTrans.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.GetComponent<InventoryItem>().SetData(reward);
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }
}
