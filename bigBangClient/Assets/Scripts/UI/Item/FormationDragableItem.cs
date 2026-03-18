using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class FormationDragableItem : DragableItem
    {

        [SerializeField] protected UIEffect uiEffect;
        [SerializeField] protected UIEffect portraitEffect;
        [SerializeField] protected Image background;
        [SerializeField] protected Image portrait;
        [SerializeField] protected Image card;
        [SerializeField] protected Image hurt;
        [SerializeField] protected TMP_Text txtPipei;
        [SerializeField] private PeakImage peakImage = null;

        [Header("StateA")]
        [SerializeField] protected GameObject stateA;
        [SerializeField] protected TMP_Text scoreText;
        [SerializeField] protected TMP_Text scoreShadowText;
        [SerializeField] protected TMP_Text nameText;
        [SerializeField] protected TMP_Text positionText;
        [Header("StateB")]
        [SerializeField] protected GameObject stateB;
        [SerializeField] protected Image status;
        [SerializeField] protected Image energy;

        protected RectTransform parentRT;
        protected PlayerCard data;
        protected FormationDragableItemManager itemManager;
        protected bool isCardBanned = false;
        public int BoardId { get; private set; }
        public override void Init()
        {
            base.Init();
            pointerDownAction += OnPointerDownAction;
            pointerUpAction += OnPointerUpAction;
            pointerClickAction += OnPointerClickAction;
            dragAction += OnDragAction;
        }

        public virtual void Clear()
        {
            pointerDownAction -= OnPointerDownAction;
            pointerUpAction -= OnPointerUpAction;
            pointerClickAction -= OnPointerClickAction;
            dragAction -= OnDragAction;
        }

        public virtual void OnPointerDownAction(PointerEventData eventData)
        {
            if (!CanDrag())
                return;
            root.localScale = Vector2.one * 1.15f;
            uiEffect.colorFactor = 0.2f;
            itemManager.PickItem(eventData, this);
        }

        public virtual void OnPointerUpAction(PointerEventData eventData)
        {
            if (!CanDrop())
                return;
            itemManager.DropItem(eventData, this);
            root.localScale = Vector2.one;
            uiEffect.colorFactor = 0;
        }

        private void OnDragAction(PointerEventData eventData)
        {
            if (isCardBanned)
                return;
            if (!CanDrop())
            {
                Reset();
                return;
            }
            txtPipei?.gameObject.SetActive(false);
            root.pivot = new Vector2(0.5f, 0.5f);
            itemManager.DragItem(eventData, this);
        }

        private void OnPointerClickAction()
        {
            PlayerCard card = data;
            CardUpUIProperties cardUpUIProperties = new CardUpUIProperties(card);
            UIController.Instance.ShowPanel<CardUpUI>(cardUpUIProperties);
            // UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(data));
        }

        public virtual async void InitData(PlayerCard cardData, int formationId, int state)
        {
            if (cardData == null)
            {
                Debug.LogError("FormationDragableItem , InitData , cardData == null");
            }
            this.data = cardData;
            peakImage.SetData(cardData.Config);
            nameText.text = cardData.Config.Name;
            positionText.text = cardData.GetAdaptPositionAbbreviation();
            portrait.sprite = await SpriteProxy.GetPlayerPortrait(cardData.Config.Portrait);
            SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)cardData.Status], s => status.sprite = s);
            RefreshCombatEffectivenessNormal();
            hurt.gameObject.SetActive(cardData.IsHurt());
            energy.fillAmount = cardData.TotalEnergyRatio * 1.0f / 100;
            //设置红黄牌
            if (formationId == FormationID.PVE)
            {
                card.gameObject.SetActive(false);
                isCardBanned = false;
            }
            else
            {
                var nextCompitionId = Player.PVPManager.NextCompitionType;
                var isBanned = cardData.BanCountDic.ContainsKey(nextCompitionId) ? cardData.BanCountDic[nextCompitionId] > 0 : false;
                var yellowCardCount = cardData.YellowCardDic.ContainsKey(nextCompitionId) ? cardData.YellowCardDic[nextCompitionId] : 0;

                if (isBanned)
                {
                    SpriteManager.GetSprite(AtlasNames.Formation, SpriteNames.Player.RedCard, s => card.sprite = s);
                    card.gameObject.SetActive(true);
                    isCardBanned = true;
                }
                else if (yellowCardCount > 0)
                {
                    SpriteManager.GetSprite(AtlasNames.Formation, SpriteNames.Player.YelloCard, s => card.sprite = s);
                    card.gameObject.SetActive(true);
                    isCardBanned = false;
                }
                else
                {
                    card.gameObject.SetActive(false);
                    isCardBanned = false;
                }
            }
            ChangeState(state);
        }

        public void RefreshCombatEffectivenessInMain()
        {
            int separatedPosition = Configs.FormationBoard.GetDataDictionary()[BoardId].SeparatedPosition;
            int CombatEffectiveness = Mathf.RoundToInt(this.data.FightPoint * (((float)this.data.Config.PositionRatio[separatedPosition]) / 100));
            scoreText.text = CombatEffectiveness.ToString();
            scoreShadowText.text = CombatEffectiveness.ToString();
            txtPipei.gameObject.SetActive(true);
            txtPipei.text = "位置匹配:" + this.data.Config.PositionRatio[separatedPosition] + "%";
        }
        public void RefreshCombatEffectivenessNormal()
        {
            int CombatEffectiveness = this.data.FightPoint;
            scoreText.text = CombatEffectiveness.ToString();
            scoreShadowText.text = CombatEffectiveness.ToString();
            txtPipei?.gameObject.SetActive(false);
        }

        public virtual void SetBoardId(int boardId)
        {
            BoardId = boardId;
        }

        public void SetParent(RectTransform parent)
        {
            parentRT = parent;
            root.SetParent(parent);
        }

        public void SetManager(FormationDragableItemManager itemManager)
        {
            this.itemManager = itemManager;
        }

        public virtual void Reset()
        {
            root.SetParent(parentRT);
            root.DOAnchorPos(itemManager.GetPosByBoardId(BoardId), 0.2f).AddTo(this.gameObject);
            root.anchoredPosition = itemManager.GetPosByBoardId(BoardId);
        }

        public virtual void ChangeState(int state = 0)
        {
            stateA.SetActive(state == 0);
            stateB.SetActive(state != 0);
        }

        public virtual bool CanDrag()
        {
            return true;
        }

        public virtual bool CanDrop()
        {
            return true;
        }

        public virtual void Drop()
        {
            return;
        }

        public PlayerCard GetData()
        {
            return data;
        }

        public virtual void StartBreath()
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_HOVER);
        }

        public virtual void StopBreath()
        {

        }

        #region 状态标志

        [SerializeField] protected Image notMatchImage;
        [SerializeField] protected Image dispatchedImage;
        [SerializeField] protected Image canNotUseImage;

        public enum StateSign
        {
            Normal,
            NotMatch,
            Dispatched,
            CanNotUse,
        }
        public void SetStateSign(StateSign stateSign = StateSign.Normal)
        {
            notMatchImage.gameObject.SetActive(false);
            dispatchedImage.gameObject.SetActive(false);
            canNotUseImage.gameObject.SetActive(false);
            switch (stateSign)
            {
                case StateSign.NotMatch: notMatchImage.gameObject.SetActive(true); break;
                case StateSign.Dispatched: dispatchedImage.gameObject.SetActive(true); break;
                case StateSign.CanNotUse: canNotUseImage.gameObject.SetActive(true); break;
            }
        }

        #endregion

    }
}
