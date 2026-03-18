using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using BigBang.Animation;
using Utils;

namespace BigBang.UI
{
    public class FormationReserveDragableItem : FormationDragableItem
    {
        Tweener tweener;
        private bool isBreathing = false;
        private Tween lightTween;

        public override async void InitData(PlayerCard cardData, int formationId, int state)
        {
            base.InitData(cardData, formationId, state);
            background.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.FormationBench, cardData.Quality);
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
                PointerUp_WithMainWindow(eventData);
            }
        }

        private void PointerUp_WithBenchWindow(PointerEventData eventData)
        {
            int benchBoardId = itemManager.GetBenchBoardId(eventData);
            int backupBoardId = itemManager.GetReserveBoardId(eventData);
            if (itemManager.BenchGrids.ContainsKey(benchBoardId))
            {
                if (itemManager.BenchPlayerCards.TryGetValue(benchBoardId, out FormationDragableItem targetBenchItem))
                {
                    itemManager.Swap(FormationSwapType.BackupToBench, this, targetBenchItem);
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                    itemManager.CheckAndStopHighLightItem();
                    Reset();
                }
                return;
            }
            else if (itemManager.ReserveGrids.ContainsKey(backupBoardId))
            {
                if (itemManager.ReservePlayerCards.TryGetValue(backupBoardId, out FormationDragableItem targetReserveItem) && backupBoardId != BoardId)
                {
                    Tips.PopError(ErrorID.NoNeedToExchangeBackup);
                    Reset();
                    targetReserveItem.Reset();
                    itemManager.CheckAndStopHighLightItem();
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                }
                else
                {
                    Reset();
                    itemManager.CheckAndStopHighLightItem();
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                }
                return;
            }
            AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
            Reset();
        }

        private void PointerUp_WithMainWindow(PointerEventData eventData)
        {
            int mainBoardId = itemManager.GetMainBoardId(eventData);
            int backupBoardId = itemManager.GetReserveBoardId(eventData);
            if (itemManager.MainGrids.ContainsKey(mainBoardId))
            {
                if (itemManager.MainPlayerCards.TryGetValue(mainBoardId, out FormationDragableItem targetMainItem))
                {
                    itemManager.Swap(FormationSwapType.BackupToMain, this, targetMainItem);
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                    Reset();
                    itemManager.CheckAndStopHighLightItem();
                }
                return;
            }
            else if (itemManager.ReserveGrids.ContainsKey(backupBoardId))
            {
                if (itemManager.ReservePlayerCards.TryGetValue(backupBoardId, out FormationDragableItem targetReserveItem) && backupBoardId != BoardId)
                {
                    //Tips.PopError(ErrorID.NoNeedToExchangeBackup);
                    Reset();
                    targetReserveItem.Reset();
                    itemManager.CheckAndStopHighLightItem();
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                }
                else
                {
                    Reset();
                    itemManager.CheckAndStopHighLightItem();
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                }
                return;
            }
            AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
            Reset();
        }

        public override bool CanDrag()
        {
            return true;
        }

        public override bool CanDrop()
        {
            return true;
        }

        public void PlayFadeIn(float duration, float delay)
        {

        }

        public override void StartBreath()
        {
            if (!isBreathing)
            {
                isBreathing = true;
                uiEffect.colorFactor = 0.5f;
                portraitEffect.colorFactor = 0.5f;
                //lightTween = DOTween.To(value =>
                //{
                //    uiEffect.colorFactor = value;
                //    portraitEffect.colorFactor = value;
                //}, 0, 0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
            root.position = new Vector3(root.position.x, root.position.y, -1);
        }

        public override void StopBreath()
        {
            isBreathing = false;
            lightTween?.Kill();
            uiEffect.colorFactor = 0;
            portraitEffect.colorFactor = 0;
            root.position = new Vector3(root.position.x, root.position.y, 0);
        }

        public override void Drop()
        {
            StopBreath();
            LightOnce(0.1f);
            root.DOScale(0.9f, 0.1f)
                .OnComplete(() =>
                {
                    root.DOScale(1.0f, 0.1f).AddTo(this.gameObject);
                }).AddTo(this.gameObject);
        }

        private void LightOnce(float duration)
        {
            DOTween.To(value =>
            {
                uiEffect.colorFactor = value;
                portraitEffect.colorFactor = value;
            }, 0, 0.5f, duration).SetLoops(2, LoopType.Yoyo).AddTo(this.gameObject);
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}