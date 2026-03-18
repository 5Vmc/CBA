using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Protocol;
using BigBang.Animation;
using TMPro;
using Utils;
using GameConfig;
using DG.Tweening;
using System;

namespace BigBang.UI
{
    public class HundredBattleEndUIProperties : WindowProperties
    {
        public Protocol.FightCard leftFightCard = null;
        public Protocol.FightCard rightFightCard = null;
        public int leftScore = 0;
        public int rightScore = 0;
        public Action ClickCloseCallBack;
        public HundredBattleEndUIProperties(Protocol.FightCard leftFightCard, Protocol.FightCard rightFightCard, int leftScore, int rightScore, Action ClickCloseCallBack)
        {
            this.leftFightCard = leftFightCard;
            this.rightFightCard = rightFightCard;
            this.leftScore = leftScore;
            this.rightScore = rightScore;
            this.ClickCloseCallBack = ClickCloseCallBack;
        }
    }
    public class HundredBattleEndUI : AWindowController<HundredBattleEndUIProperties>
    {
        #region 初始化与监听
        [SerializeField] private Button closeButton = null;
        [SerializeField] private Image blackBg = null;
        [SerializeField] private Image bgLightImage = null;
        [SerializeField] private RectTransform leftPanel = null;
        [SerializeField] private Image leftBgImage = null;
        [SerializeField] private RectTransform scalePanel = null;
        [SerializeField] private HundredTeamDetailCardItem leftHundredTeamDetailCardItem = null;
        [SerializeField] private Image leftWinImage = null;
        [SerializeField] private Image leftLoseImage = null;
        [SerializeField] private TMP_Text leftScoreText = null;
        [SerializeField] private RectTransform rightPanel = null;
        [SerializeField] private Image rightBgImage = null;
        [SerializeField] private HundredTeamDetailCardItem rightHundredTeamDetailCardItem = null;
        [SerializeField] private Image rightWinImage = null;
        [SerializeField] private Image rightLoseImage = null;
        [SerializeField] private TMP_Text rightScoreText = null;
        [SerializeField] private Image vsImage = null;
        [SerializeField] private Image midLineImage = null;
        [SerializeField] private TMP_Text clickTipText = null;

        [SerializeField] private float midY = 6f;
        [SerializeField] private Vector3 leftWinPos = new();
        [SerializeField] private Vector3 leftLosePos = new();
        [SerializeField] private Vector3 rightWinPos = new();
        [SerializeField] private Vector3 rightLosePos = new();

        protected override void AddListeners()
        {
            base.AddListeners();
            closeButton.onClick.AddListener(OnClickCloseBtn);
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeButton.onClick.RemoveListener(OnClickCloseBtn);
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn()
        {
            if (canPass == false) return;
            UIController.Instance.CloseWindow<HundredBattleEndUI>();
            Properties.ClickCloseCallBack?.Invoke();
        }
        #endregion

        #region 数据刷新与显示刷新
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            Refresh();
            PrepareEnterAnim();
            PlayEnterAnim();
        }
        private void Refresh()
        {
            if (Properties.leftFightCard != null) leftHundredTeamDetailCardItem.SetData(Properties.leftFightCard);
            if (Properties.rightFightCard != null) rightHundredTeamDetailCardItem.SetData(Properties.rightFightCard);
            //leftScoreText.text = Properties.leftScore.ToString();//在动画中设置
            //rightScoreText.text = Properties.rightScore.ToString();
        }

        #endregion

        #region 动画

        private bool canPass = false;
        private void PrepareEnterAnim()
        {
            seq?.Kill();
            canPass = false;
            blackBg.SetAlpha(0);
            bgLightImage.SetAlpha(0);
            leftPanel.SetLocalPositionY(midY);
            leftPanel.SetAnchoredPositionX(-400f);
            rightPanel.SetLocalPositionY(midY);
            rightPanel.SetAnchoredPositionX(400f);
            midLineImage.SetAlpha(0);
            vsImage.transform.SetLocalScale(0);
            leftScoreText.text = "0";
            leftScoreText.SetAlpha(1);
            rightScoreText.text = "0";
            rightScoreText.SetAlpha(1);
            leftWinImage.SetAlpha(0);
            leftLoseImage.SetAlpha(0);
            rightWinImage.SetAlpha(0);
            rightLoseImage.SetAlpha(0);
            leftWinImage.transform.SetLocalScale(10);
            leftLoseImage.transform.SetLocalScale(10);
            rightWinImage.transform.SetLocalScale(10);
            rightLoseImage.transform.SetLocalScale(10);
            clickTipText.SetAlpha(0);
            scalePanel.SetLocalScale(1);
            leftHundredTeamDetailCardItem.darkImage.gameObject.SetActive(true);
            rightHundredTeamDetailCardItem.darkImage.gameObject.SetActive(true);
            leftHundredTeamDetailCardItem.darkImage.SetAlpha(0);
            rightHundredTeamDetailCardItem.darkImage.SetAlpha(0);
        }

        private Sequence seq = null;
        private void PlayEnterAnim()
        {
            seq = DOTween.Sequence();
            seq.AddTo(this.gameObject);
            //背景变黑
            seq.Append(blackBg.DOFade(0.9f, 0.2f));
            seq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUT_DING); });// 对撞音效
            //左右板子来到中间
            seq.Append(leftPanel.DOAnchorPosX(0f, 0.3f));
            seq.Join(rightPanel.DOAnchorPosX(0f, 0.3f));
            //出现 VS
            seq.Append(midLineImage.DOFade(1f, 0.3f));
            seq.Join(vsImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            //左右板子交错开
            seq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ANI_QUICKCD); });// 数字滚动音效
            bool isLeftWin = Properties.leftScore > Properties.rightScore;
            seq.Append(leftPanel.DOAnchorPos(isLeftWin ? leftWinPos : leftLosePos, 1.5f));
            seq.Join(rightPanel.DOAnchorPos(isLeftWin ? rightLosePos : rightWinPos, 1.5f));
            //分数滚动
            seq.Join(leftScoreText.DOChangeNumber(Properties.leftScore, 1.5f, 0));
            seq.Join(rightScoreText.DOChangeNumber(Properties.rightScore, 1.5f, 0));
            //低分数的降透明度
            seq.Join((isLeftWin ? rightScoreText : leftScoreText).DOFade(0.6f, 1.5f));
            //失败的卡牌变暗
            seq.Join((isLeftWin ? rightHundredTeamDetailCardItem : leftHundredTeamDetailCardItem).darkImage.DOFade(1f, 1.5f));
            seq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.RSLT_INFO); });// 卡戳音效
            //出现胜利失败
            Image leftImage = isLeftWin ? leftWinImage : leftLoseImage;
            Image rightImage = isLeftWin ? rightLoseImage : rightWinImage;
            seq.Append(leftImage.DOFade(1f, 0.3f));
            seq.Join(leftImage.transform.DOScale(1f, 0.3f));
            seq.Join(rightImage.DOFade(1f, 0.3f));
            seq.Join(rightImage.transform.DOScale(1f, 0.3f));
            seq.Append(leftImage.transform.DOScale(0.9f, 0.05f));
            seq.Join(rightImage.transform.DOScale(0.9f, 0.05f));
            seq.Join(scalePanel.DOScale(0.9f, 0.05f));
            seq.Append(leftImage.transform.DOScale(1f, 0.1f));
            seq.Join(rightImage.transform.DOScale(1f, 0.1f));
            seq.Join(scalePanel.DOScale(1f, 0.1f));
            //出现点击 Tip
            seq.AppendCallback(() => { canPass = true; });
            seq.AppendInterval(0.3f);
            seq.Append(clickTipText.DOFade(1f, 0.3f));
            //背景光
            seq.Insert(0, bgLightImage.DOFade(1f, 3.0f));
        }

        #endregion

    }
}