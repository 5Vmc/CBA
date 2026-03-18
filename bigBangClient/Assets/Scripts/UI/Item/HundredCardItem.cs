using System;
using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    public class HundredCardData
    {
        public bool isCanMove = false;//允许调整位置、
        public PlayerCard playerCard = null;//游戏卡牌默认结构，名字，品质，星级等
        public int orderNumber = 0;//出战次序 1-5 ， 0 代表不出战
    }

    public class HundredCardItem : MonoBehaviour
    {
        [SerializeField] private BabuButton hundredCardItem = null;
        [SerializeField] private List<Image> qualityImageList = new();
        [SerializeField] private Image strengthBgImage = null;
        [SerializeField] private Image playerIconImage = null;
        [SerializeField] private Image progressbarBgImage = null;
        [SerializeField] private Image playerIconFgImage = null;
        [SerializeField] private Image nameBgImage = null;
        [SerializeField] private TMP_Text strengthNumText = null;
        [SerializeField] private Image stateImage = null;
        [SerializeField] private Image medicalImage = null;
        [SerializeField] private Image progressbarFgImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text progressbarPercentText = null;
        [SerializeField] private Image darkImage = null;
        [SerializeField] private Image orderBgImage = null;
        [SerializeField] private TMP_Text orderText = null;
        [SerializeField] private Image medicalUpImage = null;
        [SerializeField] private Image selectImage = null;
        [SerializeField] private DragActionComponent dragActionComponent = null;
        [SerializeField] private PeakImage peakImage = null;

        private void OnEnable()
        {
            hundredCardItem.OnClick += OnClickHundredCardItemDown;
        }

        private void OnDisable()
        {
            hundredCardItem.OnClick -= OnClickHundredCardItemDown;
        }

        private void OnClickHundredCardItemDown(BabuButton _)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);

            if (hundredCardData.isCanMove == false) return;
            if (isShowUp == true) return;
            if (hundredCardData.orderNumber > 0)
            {
                Tips.PopTips("该球员已出战");
                return;
            }

            Babu.EventManager.Instance.Dispatch(EventID.OnClickHundredCardItemDown, this);
        }

        public bool isShowUp = false;//是在布阵界面上面的卡牌
        public HundredCardData hundredCardData = null;
        public async void SetData(HundredCardData hundredCardData, bool isShowUp = false)
        {
            this.hundredCardData = hundredCardData;
            this.isShowUp = isShowUp;

            SetBg(hundredCardData.playerCard.Quality);
            strengthBgImage.gameObject.SetActive(!isShowUp);
            strengthNumText.gameObject.SetActive(!isShowUp);
            if (!isShowUp) strengthNumText.text = hundredCardData.playerCard.FightPoint.ToString("N0");
            bool isHurt = hundredCardData.playerCard.IsHurt();
            medicalImage.gameObject.SetActive(!isShowUp && isHurt);
            medicalUpImage.gameObject.SetActive(isShowUp && isHurt);
            progressbarFgImage.fillAmount = hundredCardData.playerCard.TotalEnergyRatio / 100;
            nameText.text = hundredCardData.playerCard.Config.Name;
            progressbarPercentText.text = "{0}%".SafeFormat(Mathf.FloorToInt(hundredCardData.playerCard.TotalEnergyRatio));
            darkImage.gameObject.SetActive(!isShowUp && hundredCardData.orderNumber > 0);
            orderBgImage.gameObject.SetActive(!isShowUp && hundredCardData.orderNumber > 0);
            orderText.gameObject.SetActive(!isShowUp && hundredCardData.orderNumber > 0);
            if (!isShowUp && hundredCardData.orderNumber > 0) orderText.text = "{0}号出战".SafeFormat(hundredCardData.orderNumber);
            peakImage.SetData(hundredCardData.playerCard);

            hundredCardItem.enabled = (!isShowUp && hundredCardData.isCanMove);
            dragActionComponent.enabled = (isShowUp && hundredCardData.isCanMove);
            if (isShowUp && hundredCardData.isCanMove)
            {
                dragActionComponent.DragBeginAction = DragBeginAction;
                dragActionComponent.DragMoveAction = DragMoveAction;
                dragActionComponent.DragEndAction = DragEndAction;
                dragActionComponent.PointerDownAction = PointerDownAction;
            }

            stateImage.sprite = await hundredCardData.playerCard.GetPlayerCardStatusSprite();
            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(hundredCardData.playerCard.Config.Portrait);
        }

        private void SetBg(int quality)
        {
            for (int i = 0; i < qualityImageList.Count; i++)
            {
                qualityImageList[i].gameObject.SetActive(i == quality - 1);
            }
        }

        public bool isSelect = false;//被选中
        public void SetSelect(bool isSelect)
        {
            this.isSelect = isSelect;
            selectImage.gameObject.SetActive(hundredCardData.isCanMove && isShowUp && isSelect);
            if (isSelect) PlayHighlightAnim();
        }

        public Action<PointerEventData, HundredCardItem> DragBeginCardItem = null;
        public Action<PointerEventData, HundredCardItem> DragMoveCardItem = null;
        public Action<PointerEventData, HundredCardItem> DragEndCardItem = null;
        private void DragBeginAction(PointerEventData data)
        {
            Debug.Log("DragBeginAction:" + data.pointerCurrentRaycast.screenPosition);
            if (hundredCardData.isCanMove == false) return;
            if (isShowUp == false) return;
            DragBeginCardItem?.Invoke(data, this);
        }
        private void DragMoveAction(PointerEventData data)
        {
            Debug.Log("DragMoveAction:" + data.pointerCurrentRaycast.screenPosition);
            if (hundredCardData.isCanMove == false) return;
            if (isShowUp == false) return;
            DragMoveCardItem?.Invoke(data, this);
        }
        private void DragEndAction(PointerEventData data)
        {
            Debug.Log("DragEndAction:" + data.pointerCurrentRaycast.screenPosition);
            if (hundredCardData.isCanMove == false) return;
            if (isShowUp == false) return;
            DragEndCardItem?.Invoke(data, this);
        }

        private void PointerDownAction(PointerEventData data)
        {
            Debug.Log("PointerDownAction:" + data.pointerCurrentRaycast.screenPosition);
            if (hundredCardData.isCanMove == false) return;
            if (isShowUp == false) return;
            Babu.EventManager.Instance.Dispatch(EventID.OnClickHundredCardItemUp, this);
        }

        private void PlayHighlightAnim()
        {
            selectImage.DOKill();
            selectImage.transform.DOKill();
            selectImage.SetAlpha(0);
            selectImage.transform.localScale = Vector3.one * 1.5f;
            selectImage.DOFade(1, 0.2f).AddTo(this.gameObject);
            selectImage.transform.DOScale(1.25f, 0.2f).AddTo(this.gameObject);
        }
    }
}
