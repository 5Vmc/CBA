using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SkillTrainSelectCardGridAdapter : GridAdapter<GridParams, SkillTrainSelectCardGridViewsHolder>
    {
        public SimpleDataHelper<PlayerCard> Data { get; private set; }

        private List<PlayerCard> _cardList;

        private int _selectCardId;
        private int _selectSkillId;
        private Action<PlayerCard> _selectAction;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<PlayerCard>(this);
        }

        protected override void Start()
        {
            base.Start();
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<SkillTrainSelectCardGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetData(List<PlayerCard> cardList, int selectCardId = 0, int selectSkillId = 0)
        {
            if (!IsInitialized)
            {
                Init();
            }

            _cardList = cardList;
            _selectCardId = selectCardId;
            _selectSkillId = selectSkillId;
            cardList ??= new List<PlayerCard>();

            Data.ResetItems(cardList);
        }

        protected override void UpdateCellViewsHolder(SkillTrainSelectCardGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            var isSelect = _selectCardId == model.Config.Id;
            var showState = GetShowStateType(model.CardId);
            newOrRecycled.UpdateViews(model, isSelect, showState, OnSelectCard);
        }

        public void SelectActionRegister(Action<PlayerCard> selectAction)
        {
            _selectAction = selectAction;
        }

        private void OnSelectCard(PlayerCard selectCard)
        {
            _selectCardId = selectCard.Config.Id;
            Refresh();
            _selectAction(selectCard);
        }

        private SkillTrainSelectCardState GetShowStateType(int cardId)
        {

            var list = Player.CardManager.SkillController.GetUnlockSkillList();
            bool canTrain = false;
            //只要不在训练中，有任意技能可以训练，这个英雄都可以训练。
            foreach (var _skill in list)
            {
                SkillTrainSelectCardState state = Player.CardManager.SkillController.GetSelectCardShowState(cardId, _skill.Id);
                if (state == SkillTrainSelectCardState.DoTraining)
                {
                    canTrain = false;
                    return state;
                }
                else if (state == SkillTrainSelectCardState.Normal)
                {
                    canTrain |= true;
                }
            }

            if (canTrain)
            {
                return SkillTrainSelectCardState.Normal;
            }
            else
            {
                return SkillTrainSelectCardState.CanNotTrain;
            }

            //return Player.CardManager.SkillController.GetSelectCardShowState(cardId, _selectSkillId);
        }
    }

    public class SkillTrainSelectCardGridViewsHolder : CellViewsHolder
    {
        //点击按钮
        private Button _btn;
        private PlayerCard _card;
        private SkillTrainRoomCardSelectIcon cardItem;
        private Action<PlayerCard> _selectAction;
        private SkillTrainSelectCardState _showState;

        public override void CollectViews()
        {
            base.CollectViews();
            cardItem = root.GetComponent<SkillTrainRoomCardSelectIcon>();

            _btn = root.GetComponent<Button>();
            _btn.onClick.AddListener(OnClickCard);
        }

        private void OnClickCard()
        {
            showState();
        }

        private void showState()
        {
            switch (_showState)
            {
                case SkillTrainSelectCardState.Normal:
                    PlayMoveAnim(() => _selectAction?.Invoke(_card));
                    break;
                case SkillTrainSelectCardState.DoTraining:
                    Tips.PopError(ErrorID.SkillTrainSelectCardDoTraining);
                    break;
                case SkillTrainSelectCardState.CanNotTrain:
                    Tips.PopError("没有该球员可以学习的特技");//ErrorID.SkillTrainSelectCardCanNotTrain 除了品种不足，还有已学习了其他所有已解锁技能的情况
                    break;
                case SkillTrainSelectCardState.HaveBeenTrain:
                    Tips.PopError(ErrorID.SkillTrainSelectCardHaveBeenTrain);
                    break;
            }
        }

        // 球员卡牌移动到球员空位
        private void PlayMoveAnim(Action callback)
        {
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT_LOAD);
            // 显示勾勾
            cardItem.ShowSelectAnim();

            var playerRect = cardItem.PlayerImgRect;
            var playerClone = SkillTrainRoomSelectUI.PlayerCloneStatic;
            var targetPos = SkillTrainRoomSelectUI.PlayerTargetPosStatic.position;
            // 初始化起始位置
            playerClone.position = playerRect.position;
            // 初始化起始缩放
            playerClone.localScale = Vector3.one * 0.8f;
            // 启用组件
            playerClone.gameObject.SetActive(true);
            // 设置球员图片
            playerClone.GetComponentInChildren<Image>().sprite = playerRect.GetComponent<Image>().sprite;
            playerClone.DOMove(targetPos, 0.15f).OnComplete(() =>
            {
                TouchManager.Instance.EnableTouch();

                playerClone.gameObject.SetActive(false);
                callback?.Invoke();
            });
            playerClone.DOScale(1, 0.15f);
        }

        public void UpdateViews(PlayerCard card, bool isSelect, SkillTrainSelectCardState showState, Action<PlayerCard> selectAction)
        {
            _card = card;
            _showState = showState;
            cardItem.SetData(_card.Config);
            cardItem.SetQuality(_card.Quality);
            cardItem.SetState(showState);
            _selectAction = selectAction;
            cardItem.SetSelect(isSelect);
        }
    }
}