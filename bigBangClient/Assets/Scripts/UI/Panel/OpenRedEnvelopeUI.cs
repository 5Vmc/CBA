using System;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class OpenRedEnvelopeUIProperties : WindowProperties
    {
        public ActivityData activityData { get; private set; }
        public SnatchRedPacketResponse snatchRedPacketResponse = null;

        public OpenRedEnvelopeUIProperties(ActivityData activityData, SnatchRedPacketResponse snatchRedPacketResponse)
        {
            this.activityData = activityData;
            this.snatchRedPacketResponse = snatchRedPacketResponse;
        }
    }

    public class OpenRedEnvelopeUI : AWindowController<OpenRedEnvelopeUIProperties>
    {
        [SerializeField] private RectTransform closePanel = null;
        [SerializeField] private BabuButton redEnvelopeImage = null;
        [SerializeField] private TMP_Text formText1 = null;

        [SerializeField] private RectTransform openPanel = null;
        [SerializeField] private RectTransform redEnvelopeContent = null;
        [SerializeField] private TMP_Text formText2 = null;
        [SerializeField] private InventoryItem inventoryItem = null;
        [SerializeField] private BabuButton upButton = null;
        [SerializeField] private BabuButton upedButton = null;
        [SerializeField] private TMP_Text closeTipText = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            redEnvelopeImage.OnClick += OnClickOpenButton;
            upButton.OnClick += OnClickUpButton;
            upedButton.OnClick += OnClickUpedButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            redEnvelopeImage.OnClick -= OnClickOpenButton;
            upButton.OnClick -= OnClickUpButton;
            upedButton.OnClick -= OnClickUpedButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
        }

        GameItem gameItem = null;
        [SerializeField] private Image blackImage = null;
        [SerializeField] private RectTransform background = null;
        private Sequence closeSeq = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            AudioManager.Instance.PlaySound(AudioNames.LOADING_IN);
            isTimeGo = false;
            openSeq?.Kill();
            openSeq = null;
            closePanel.gameObject.SetActive(true);
            closePanel.SetLocalScaleX(1f);
            openPanel.gameObject.SetActive(false);
            openPanel.SetLocalScaleX(0f);

            closeSeq?.Kill();
            closeSeq = null;
            closeSeq = DOTween.Sequence();
            closeSeq.SetTarget(redEnvelopeContent);
            closeSeq.AddTo(this.gameObject);
            redEnvelopeImage.enabled = false;
            background.gameObject.SetAlpha(0);
            background.localScale = Vector3.zero;
            background.SetAnchoredPositionY(-200);
            blackImage.color = new Color(0, 0, 0, 0);
            closeSeq.Append(background.DOAnchorPosY(350f, 0.4f).SetEase(Ease.OutQuad));
            closeSeq.Append(background.DOAnchorPosY(0f, 0.6f).SetEase(Ease.InQuad));
            closeSeq.Insert(0f, background.DOScale(1, 1.0f).SetEase(Ease.Linear));
            closeSeq.Insert(0f, background.gameObject.DOFade(1f, 0.5f));
            closeSeq.AppendCallback(() => { redEnvelopeImage.enabled = true; });
            closeSeq.Insert(0f, blackImage.DOFade(0.5f, 1.0f));

            gameItem = GameItemUtils.UnPack(Properties.snatchRedPacketResponse.Item);
            inventoryItem.SetData(gameItem);
            bool isFormDeveloper = Properties.snatchRedPacketResponse.Packet == null;
            if (isFormDeveloper)
            {
                formText1.text = "来自 <color=#ffe84f>CBA全力以赴 制作组</color>";
                formText2.text = "来自<color=#922C2F>CBA全力以赴 制作组</color>的红包";
            }
            else
            {
                formText1.text = "来自<color=#ffe84f>[{0}区]{1}</color>".SafeFormat(Properties.snatchRedPacketResponse.Packet.ServerId, Properties.snatchRedPacketResponse.Packet.Name);
                formText2.text = "来自<color=#922C2F>【{0}区】{1}</color>的红包".SafeFormat(Properties.snatchRedPacketResponse.Packet.ServerId, Properties.snatchRedPacketResponse.Packet.Name);
            }
        }

        private readonly int leftTimeMax = 3;
        private int leftTime = 3;
        private bool isTimeGo = false;
        private Sequence openSeq = null;
        private readonly float contentHideY = -500f;
        private readonly float contentShowY = -92f;

        private void OnClickOpenButton(BabuButton _)
        {
            redEnvelopeImage.enabled = false;
            leftTime = leftTimeMax;

            upButton.gameObject.SetActive(false);
            upedButton.gameObject.SetActive(false);

            openSeq?.Kill();
            openSeq = null;

            redEnvelopeContent.SetAnchoredPositionY(contentHideY);
            upButton.gameObject.SetAlpha(0);
            closeTipText.gameObject.SetAlpha(0);

            closeTipText.text = "{0}s后自动关闭".SafeFormat(Utility.KeepInRange(leftTime, 0, 3));

            openSeq = DOTween.Sequence();
            openSeq.SetTarget(redEnvelopeContent);
            openSeq.AddTo(this.gameObject);
            openSeq.Append(closePanel.DOScaleX(0, 0.2f));
            openSeq.AppendCallback(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.OPENREDENVELOPE);
                closePanel.gameObject.SetActive(false);
                openPanel.gameObject.SetActive(true);
            });
            openSeq.Append(openPanel.DOScaleX(1, 0.2f));
            openSeq.Append(redEnvelopeContent.DOAnchorPosY(contentShowY, 1.5f).SetEase(Ease.OutBack));
            openSeq.AppendCallback(() => { upButton.gameObject.SetActive(true); });
            openSeq.Append(upButton.gameObject.DOFade(1.0f, 0.5f));
            openSeq.Append(closeTipText.gameObject.DOFade(1.0f, 0.5f));
            openSeq.AppendCallback(() => { isTimeGo = true; });
        }

        private void RefreshLeftTime()
        {
            if (isTimeGo == false) return;
            leftTime--;
            closeTipText.text = "{0}s后自动关闭".SafeFormat(Utility.KeepInRange(leftTime, 0, 3));
            if (leftTime < 0)
            {
                OnClose();
            }
        }

        private Sequence likeSeq = null;
        [SerializeField] private Image upImage = null;
        private void OnClickUpButton(BabuButton _)
        {
            upButton.gameObject.SetActive(false);
            upedButton.gameObject.SetActive(true);

            likeSeq?.Kill();
            likeSeq = null;

            upImage.transform.SetLocalScale(0f);

            likeSeq = DOTween.Sequence();
            likeSeq.SetTarget(upedButton);
            likeSeq.AddTo(upedButton.gameObject);

            likeSeq.Append(upImage.transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack));

            if (Properties.snatchRedPacketResponse.Packet != null)
            {
                NetworkManager.Instance.LikeRedPacket(Properties.activityData.cfg.Id, Properties.snatchRedPacketResponse.Packet.Gbid, (LikeRedPacketResponse likeRedPacketResponse) => { });
                RedEnvlopeManager.Instance.AddLike(Properties.snatchRedPacketResponse.Packet.Gbid);
                RedEnvlopeManager.Instance.ResetRankDataByUp();
                EventManager.Instance.Dispatch(EventID.OnAfterOpenRedEnvlope);
            }
        }
        private void OnClickUpedButton(BabuButton _)
        {
            Tips.PopTips("您已经给该玩家点过赞啦");
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_HOME);
            UIController.Instance.CloseWindow<OpenRedEnvelopeUI>();
        }
    }
}