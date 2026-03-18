using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class FormationMainDragableItem : FormationDragableItem
    {
        private bool isBreathing = false;
        private Tween lightTween;
        private float baseMainItemWidth = 91f;
        private float baseMainItemHeight = 97f;

        [SerializeField] private Image arrowImage;
        [SerializeField] public List<Image> fireStarList;

        public override async void InitData(PlayerCard cardData, int formationId, int state)
        {
            base.InitData(cardData, formationId, state);
            background.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.FormationMain, cardData.Quality);
            arrowImage.sprite = await SpriteProxy.GetFormationMainArrpwSprite(cardData.Quality);
        }

        public void SetScale(float rate)
        {
            root.localScale = new Vector3(rate, rate, rate);
        }

        public override void OnPointerDownAction(PointerEventData eventData)
        {
            base.OnPointerDownAction(eventData);
            AudioManager.Instance.PlaySound(AudioNames.ANI_PICKUP);
        }

        public override void OnPointerUpAction(PointerEventData eventData)
        {
            base.OnPointerUpAction(eventData);

            if (itemManager.IsBenchWindowOpening())
            {
                PointerUp_WithBenchWindow(eventData);
            }
            else
            {
                PointerUp_WithBackupWindow(eventData);
            }
        }

        public void PointerUp_WithBenchWindow(PointerEventData eventData)
        {
            int mainBoardId = itemManager.GetMainBoardId(eventData);
            int benchBoadId = itemManager.GetBenchBoardId(eventData);
            if (itemManager.MainGrids.ContainsKey(mainBoardId))
            {
                if (itemManager.MainPlayerCards.TryGetValue(mainBoardId, out FormationDragableItem targetMainItem) && mainBoardId != BoardId)
                {
                    itemManager.Swap(FormationSwapType.MainToMain, this, targetMainItem);
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                    SetParent(itemManager.mainContainer);
                    root.anchoredPosition = itemManager.MainGrids[mainBoardId];
                    if (mainBoardId != BoardId)
                    {
                        var flag = itemManager.ChangeMainPlayerCardPos(this, mainBoardId);
                        if (!flag)
                        {
                            itemManager.CheckAndStopHighLightItem();
                            return;
                        }
                    }
                    SetScaleByBoardId(mainBoardId);
                    itemManager.CheckAndStopHighLightItem();
                }
                root.pivot = new Vector2(0.5f, 0.02f);
                return;
            }

            if (itemManager.BenchGrids.ContainsKey(benchBoadId))
            {
                if (itemManager.BenchPlayerCards.TryGetValue(benchBoadId, out FormationDragableItem targetBenchItem))
                {
                    itemManager.Swap(FormationSwapType.MainToBench, this, targetBenchItem);
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                    itemManager.CheckAndStopHighLightItem();
                    Reset();
                }
                return;
            }
            AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
            Reset();
            itemManager.CheckAndStopHighLightItem();
        }
        public void PointerUp_WithBackupWindow(PointerEventData eventData)
        {
            int mainBoardId = itemManager.GetMainBoardId(eventData);
            int backupBoardId = itemManager.GetReserveBoardId(eventData);
            if (itemManager.MainGrids.ContainsKey(mainBoardId))
            {
                if (itemManager.MainPlayerCards.TryGetValue(mainBoardId, out FormationDragableItem targetMainItem) && mainBoardId != BoardId)
                {
                    itemManager.Swap(FormationSwapType.MainToMain, this, targetMainItem);
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                    SetParent(itemManager.mainContainer);
                    root.anchoredPosition = itemManager.MainGrids[mainBoardId];
                    if (mainBoardId != BoardId)
                    {
                        var flag = itemManager.ChangeMainPlayerCardPos(this, mainBoardId);
                        if (!flag)
                        {
                            itemManager.CheckAndStopHighLightItem();
                            return;
                        }
                    }
                    SetScaleByBoardId(mainBoardId);
                    itemManager.CheckAndStopHighLightItem();
                }
                root.pivot = new Vector2(0.5f, 0.02f);
                return;
            }
            else if (itemManager.ReserveGrids.ContainsKey(backupBoardId))
            {
                if (itemManager.ReservePlayerCards.TryGetValue(backupBoardId, out FormationDragableItem targetReserveItem))
                {
                    itemManager.Swap(FormationSwapType.MainToBackup, this, targetReserveItem);
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                    Reset();
                    itemManager.CheckAndStopHighLightItem();
                }
                return;
            }
            AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
            Reset();
            itemManager.CheckAndStopHighLightItem();
        }

        public override void StartBreath()
        {
            if (!isBreathing)
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_HOVER);
                isBreathing = true;
                SetHightLightScale();
                lightTween = DOTween.To(value =>
                {
                    uiEffect.colorFactor = value;
                    portraitEffect.colorFactor = value;
                }, 0, 0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo).AddTo(this.gameObject);
            }
        }

        public override void StopBreath()
        {
            isBreathing = false;
            lightTween?.Kill();
            SetScaleByBoardId();
            uiEffect.colorFactor = 0;
            portraitEffect.colorFactor = 0;
        }

        public void LightOnce(float duration)
        {
            DOTween.To(value =>
            {
                uiEffect.colorFactor = value;
                portraitEffect.colorFactor = value;
            }, 0, 0.5f, duration).SetLoops(2, LoopType.Yoyo).AddTo(this.gameObject);
        }
        //首发席卡牌长出
        public void GrowUp(float duration, float delay)
        {
            gameObject.GetComponent<RectTransform>().DOScaleY(0, duration).From().SetDelay(delay).AddTo(this.gameObject);
        }

        public override void Drop()
        {
            StopBreath();
            LightOnce(0.1f);
            SetScaleByBoardId();

            var targetScale = root.localScale.x;
            root.DOScale(targetScale * 0.9f, 0.1f)
                .OnComplete(() =>
                {
                    root.DOScale(targetScale, 0.1f).AddTo(this.gameObject);
                }).AddTo(this.gameObject);
        }

        private void SetScaleByBoardId()
        {
            int row = Mathf.FloorToInt(BoardId / 100);
            if (row > 2)
            {
                float rate = 1.0f - (row - 2) * 0.02f;
                SetScale(rate);
            }
            else
            {
                SetScale(1);
            }
        }

        private void SetScaleByBoardId(int boardId)
        {
            int row = Mathf.FloorToInt(boardId / 100);
            if (row > 2)
            {
                float rate = 1.0f - (row - 2) * 0.02f;
                SetScale(rate);
            }
            else
            {
                SetScale(1);
            }
        }

        private void SetHightLightScale()
        {
            root.localScale = root.localScale * 1.15f;
        }


        public override void Reset()
        {
            base.Reset();
            root.pivot = new Vector2(0.5f, 0.00f);
            StopBreath();
            SetScaleByBoardId();
        }
    }
}
