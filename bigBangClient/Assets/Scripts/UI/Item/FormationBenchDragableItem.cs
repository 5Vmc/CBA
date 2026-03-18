using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine;
using BigBang.Animation;

namespace BigBang.UI
{
    public class FormationBenchDragableItem : FormationDragableItem
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
            if (isCardBanned) return;
            if (itemManager.IsBackupWindowOpening())
            {
                PointerUp_DuringBackupWindow(eventData);
            }
            else
            {
                PointerUp_WithoutBackupWindow(eventData);
            }
        }

        private void PointerUp_WithoutBackupWindow(PointerEventData eventData)
        {
            int mainBoardId = itemManager.GetMainBoardId(eventData);
            int benchBoardId = itemManager.GetBenchBoardId(eventData);
            if (itemManager.MainGrids.ContainsKey(mainBoardId))
            {
                if (itemManager.MainPlayerCards.TryGetValue(mainBoardId, out FormationDragableItem targetMainItem))
                {
                    itemManager.Swap(FormationSwapType.BenchToMan, this, targetMainItem);
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                    Reset();
                    itemManager.CheckAndStopHighLightItem();
                }
                return;
            }

            if (itemManager.BenchGrids.ContainsKey(benchBoardId))
            {
                if (itemManager.BenchPlayerCards.TryGetValue(benchBoardId, out FormationDragableItem targetBenchItem) && benchBoardId != BoardId)
                {
                    itemManager.Swap(FormationSwapType.BenchToBench, this, targetBenchItem);
                }
                else
                {
                    if (benchBoardId < 8 && benchBoardId != BoardId)
                    {
                        SetParent(itemManager.benchContainer);
                        root.anchoredPosition = itemManager.BenchGrids[benchBoardId];
                        itemManager.ChangeBenchPlayerCardPos(this, benchBoardId);
                        itemManager.CheckAndStopHighLightItem();
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                        Reset();
                    }
                }
                return;
            }
            AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
            Reset();
        }

        private void PointerUp_DuringBackupWindow(PointerEventData eventData)
        {
            int benchBoardId = itemManager.GetBenchBoardId(eventData);
            int backupBoardId = itemManager.GetReserveBoardId(eventData);
            if (itemManager.BenchGrids.ContainsKey(benchBoardId))
            {
                if (itemManager.BenchPlayerCards.TryGetValue(benchBoardId, out FormationDragableItem targetBenchItem) && benchBoardId != BoardId)
                {
                    itemManager.Swap(FormationSwapType.BenchToBench, this, targetBenchItem);
                }
                else
                {
                    if (benchBoardId < 8 && benchBoardId != BoardId)
                    {
                        SetParent(itemManager.benchContainer);
                        root.anchoredPosition = itemManager.BenchGrids[benchBoardId];
                        itemManager.ChangeBenchPlayerCardPos(this, benchBoardId);
                        itemManager.CheckAndStopHighLightItem();
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(AudioNames.ANI_PICKRELEASE);
                        Reset();
                    }
                }
                return;
            }
            else if (itemManager.ReserveGrids.ContainsKey(backupBoardId))
            {
                if (itemManager.ReservePlayerCards.TryGetValue(backupBoardId, out FormationDragableItem targetReserveItem))
                {
                    itemManager.Swap(FormationSwapType.BenchToBackup, this, targetReserveItem);
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
        }

        public override bool CanDrag()
        {
            //if (data.IsBanned())
            //    return false;
            //if (data.IsHurt())
            //    return false;
            if (isCardBanned)
                return false;
            return true;
        }

        public override bool CanDrop()
        {
            return true;
        }

        public void PlayFadeIn(float duration, float delay)
        {
            tweener.Kill(); //这是为了防止快速切换界面，动画播放不完导致的效果错误
            gameObject.GetComponent<RectTransform>().DOScale(0.8f, duration).From().SetDelay(delay).AddTo(this.gameObject);
            gameObject.DOFade(0, 0).OnComplete(() =>
             {
                 tweener = gameObject.DOFade(1, duration).SetDelay(delay).AddTo(this.gameObject);
             }).AddTo(this.gameObject);
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
            root.SetSiblingIndex(11);
        }
    }
}
