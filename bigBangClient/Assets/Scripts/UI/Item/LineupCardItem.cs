using System;
using Babu;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class LineupCardItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text specialtyText;
        [SerializeField] private TMP_Text abilityText;
        [SerializeField] private TMP_Text energyText;
        [SerializeField] private Image stateImg;

        [SerializeField] private TMP_Text playingCountText;
        [SerializeField] private TMP_Text goalText; //场均得分
        [SerializeField] private TMP_Text assistText; //场均助攻
        [SerializeField] private TMP_Text reboundText; //场均篮板

        [SerializeField] private GameObject stateCanvas;
        [SerializeField] private GameObject dataCanvas;

        [SerializeField] private Button detailButton;
        [SerializeField] private RepeatButton pressButton;
        [SerializeField] private Button exchangeButton;

        [SerializeField] private Image newCardImage;
        [SerializeField] private Image selectImage;

        [SerializeField] private Image darkImage;
        [SerializeField] private Image lineImage;

        public ISelectListener SelectListener { get; set; }

        public PlayerCard Card => _card;
        private PlayerCard _card;

        private int _formationId;

        private LineupCardAdapterShowType _showType;

        public RectTransform RT => _RT;
        private RectTransform _RT;

        private bool _isPress = false;

        private bool _selectModel = false;

        void Start()
        {
            _RT = transform as RectTransform;
            exchangeButton.interactable = false;
        }

        private void OnEnable()
        {
            detailButton.onClick.AddListener(OnClickDetail);
            pressButton.onPress.AddListener(OnPress);
            exchangeButton.onClick.AddListener(OnClickExchange);
        }

        private void OnDisable()
        {
            detailButton.onClick.RemoveListener(OnClickDetail);
            pressButton.onPress.RemoveListener(OnPress);
            exchangeButton.onClick.RemoveListener(OnClickExchange);
        }

        public void SetData(PlayerCard card, FormationBase formation, int ItemIndex)
        {
            _card = card;
            _formationId = formation.FormationId;
            specialtyText.text = card.GetAdaptPositionAbbreviation();
            nameText.text = PlayerCard.GetFullName(card.Config);
            abilityText.text = card.FightPoint.ToString();
            energyText.text = Math.Floor(card.Energy).ToString();
            SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)card.Status], s => stateImg.sprite = s);

            SetFormationInfo();
            SetPerformanceInfo();
            // var formation = Player.FightManager.FormationController.GetDefaultFormation(formationId);
            // newCardImage.gameObject.SetActive(formation.LineupShowTime < card.CTime);

            if (ItemIndex == 4 || ItemIndex == 11)
            {
                lineImage.gameObject.SetActive(true);
            }
            else
            {
                lineImage.gameObject.SetActive(false);
            }

            if (ItemIndex == 5 || ItemIndex == 12)
            {
                darkImage.gameObject.SetActive(true);
            }
            else
            {
                darkImage.gameObject.SetActive(false);
            }

        }

        private void SetFormationInfo()
        {
            var formationData = _card.FormationDataDic[_formationId];
            if (formationData == null) return;

            string showString = "";
            switch (formationData.State)
            {
                case FormationCardState.Starter:
                    {
                        showString = formationData.GetPositionName();
                        break;
                    }
                case FormationCardState.Substitute:
                    {
                        showString = $"替补{formationData.SubstituteIndex}";
                        break;
                    }
                case FormationCardState.Reserve:
                    {
                        showString = "休息";
                        break;
                    }
            }

            positionText.text = showString;
        }

        private void SetPerformanceInfo()
        {
            var performanceData = _card.PerformanceData;

            if (performanceData == null) return;
            playingCountText.text = performanceData.PlayingCount.ToString();
            goalText.text = performanceData.ScoreAverage.ToString();
            assistText.text = performanceData.AssistsAverage.ToString();
            reboundText.text = performanceData.ReboundAverage.ToString();  //$"{performanceData.YellowCard}/{performanceData.RedCard}";
        }

        public void SetShowType(LineupCardAdapterShowType showType)
        {
            _showType = showType;
            stateCanvas.SetActive(showType == LineupCardAdapterShowType.State);
            dataCanvas.SetActive(showType == LineupCardAdapterShowType.Data);
        }

        //长按
        private void OnPress()
        {
            _isPress = true;
            Debug.Log($"On press card {_card.CardId}");
            SelectListener.OnSelectToExchange(this);
            EventManager.Instance.Dispatch(EventID.OnLineupChangeSelectModel, true);
        }

        //点击交换
        private void OnClickExchange()
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_SETSQUAD);
            if (!_card.CanFight())
            {
                Tips.PopError(ErrorID.LineupCardCanNotFight);
                return;
            }
            SelectListener.OnExchangeCard(this);
        }

        //点击详情
        private void OnClickDetail()
        {
            if (_isPress)
            {
                _isPress = false;
                return;
            }

            if (_selectModel)
            {
                return;
            }

            UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(_card));
        }

        public void SetSelectModel(bool selectModel, PlayerCard card)
        {
            if (selectModel == true && card != null)
            {
                _selectModel = true;
                if (card.CardId == _card.CardId)
                {
                    selectImage.gameObject.SetActive(true);
                    exchangeButton.gameObject.SetActive(false);
                    BlinkSelectImage();
                }
                else
                {
                    selectImage.gameObject.SetActive(false);
                    exchangeButton.gameObject.SetActive(true);
                    FadeInExchangeImage();
                }
            }
            else
            {
                _selectModel = false;
                selectImage.gameObject.SetActive(false);
                HideSelectImage();
                FadeOutExchangeImage();
            }
        }

        public interface ISelectListener
        {
            void OnSelectToExchange(LineupCardItem item);

            void OnExchangeCard(LineupCardItem item);
        }

        #region Animation
        private void BlinkSelectImage()
        {
            selectImage.SetAlpha(1);
            selectImage.DOFade(0.6f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }

        private void HideSelectImage()
        {
            selectImage.DOKill();
        }

        private void FadeInExchangeImage()
        {
            Image exchangeImg = exchangeButton.GetComponent<Image>();
            exchangeImg.SetAlpha(0);
            exchangeButton.interactable = false;
            exchangeImg.DOFade(1, 0.25f).OnComplete(() => { exchangeButton.interactable = true; });
        }

        private void FadeOutExchangeImage()
        {
            Image exchangeImg = exchangeButton.GetComponent<Image>();
            exchangeButton.interactable = false;
            exchangeImg.DOFade(0, 0.25f).OnComplete(() => { exchangeButton.gameObject.SetActive(false); });
        }

        #endregion
    }
}
