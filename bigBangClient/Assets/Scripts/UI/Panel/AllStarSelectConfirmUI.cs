using System.Collections.Generic;
using Babu;
using deVoid.UIFramework;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.AllStarManager;

namespace BigBang.UI
{
    public class AllStarSelectConfirmUIProperties : WindowProperties
    {
        public Area area;

        public AllStarSelectConfirmUIProperties(Area area)
        {
            this.area = area;
        }
    }
    public class AllStarSelectConfirmUI : AWindowController<AllStarSelectConfirmUIProperties>
    {
        [SerializeField] private BabuButton closeButton = null;
        [SerializeField] private BabuButton confirmBtn = null;
        [SerializeField] private BabuButton cancelBtn = null;
        [SerializeField] private BabuButton waitBtn = null;
        [SerializeField] private TMP_Text waitText = null;
        [SerializeField] private RectTransform changeTipPanel = null;
        [SerializeField] private RectTransform confirmTipPanel = null;
        protected override void AddListeners()
        {
            closeButton.OnClick += OnClickCloseButton;
            confirmBtn.OnClick += OnClickConfirmBtn;
            cancelBtn.OnClick += OnClickCancelBtn;
        }
        protected override void RemoveListeners()
        {
            closeButton.OnClick -= OnClickCloseButton;
            confirmBtn.OnClick -= OnClickConfirmBtn;
            cancelBtn.OnClick -= OnClickCancelBtn;
            secondUpdateSequence?.Kill();
            secondUpdateSequence = null;
        }
        [SerializeField] private List<Image> northImageList;
        [SerializeField] private List<Image> southImageList;
        private bool needChange = false;
        private Sequence secondUpdateSequence = null;
        protected override void OnPropertiesSet()
        {
            foreach (var item in northImageList)
            {
                item.gameObject.SetActive(Properties.area == Area.North);
            }
            foreach (var item in southImageList)
            {
                item.gameObject.SetActive(Properties.area == Area.South);
            }
            Area bestArea = AllStarManager.Instance.GetBestArea();
            Area selectArea = Properties.area;
            needChange = bestArea != selectArea;
            changeTipPanel.gameObject.SetActive(needChange);
            confirmTipPanel.gameObject.SetActive(!needChange);
            leftTime = leftTimeMax;
            isTimeGo = true;
            waitBtn.gameObject.SetActive(true);
            confirmBtn.gameObject.SetActive(false);
            waitText.text = "{0}s".SafeFormat(Utility.KeepInRange(leftTime, 0, leftTimeMax));
            secondUpdateSequence?.Kill();
            secondUpdateSequence = DOTween.Sequence();
            secondUpdateSequence.AppendInterval(1.0f);
            secondUpdateSequence.AppendCallback(RefreshLeftTime);
            secondUpdateSequence.SetLoops(leftTimeMax);
        }
        private readonly int leftTimeMax = 5;
        private int leftTime = 5;
        private bool isTimeGo = false;
        private void RefreshLeftTime()
        {
            if (isTimeGo == false) return;
            leftTime--;
            waitText.text = "{0}s".SafeFormat(Utility.KeepInRange(leftTime, 0, leftTimeMax));
            if (leftTime <= 0)
            {
                isTimeGo = false;
                waitBtn.gameObject.SetActive(false);
                confirmBtn.gameObject.SetActive(true);

                confirmBtn?.DOKill();
                confirmBtn.transform.SetLocalScale(0);
                confirmBtn.transform.DOScale(1,0.5f).SetEase(Ease.OutBack).SetTarget(confirmBtn).AddTo(this.gameObject);

                secondUpdateSequence?.Kill();
                secondUpdateSequence = null;
            }
        }

        private void OnClickCloseButton(BabuButton _)
        {
            UIController.Instance.CloseWindow<AllStarSelectConfirmUI>();
        }
        private void OnClickCancelBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<AllStarSelectConfirmUI>();
        }
        private void OnClickConfirmBtn(BabuButton _)
        {
            if (isTimeGo) return;
            NetworkManager.Instance.PickAllStarArea((int)Properties.area, (PickAllStarAreaResponse pickAllStarAreaResponse) =>
            {
                if (pickAllStarAreaResponse.Success)
                {
                    Tips.PopTips("您加入了{0}阵营".SafeFormat(AllStarManager.Instance.GetAreaName(Properties.area)));
                    AllStarManager.Instance.serverData.Area = (int)Properties.area;
                    EventManager.Instance.Dispatch(EventID.RefreshAllStarHomePad);
                    UIController.Instance.CloseWindow<AllStarSelectConfirmUI>();
                }
                else
                {
                    AllStarManager.Instance.GetServerData(() =>
                    {
                        EventManager.Instance.Dispatch(EventID.RefreshAllStarHomePad);
                        UIController.Instance.CloseWindow<AllStarSelectConfirmUI>();
                    });
                }
            });
        }
    }
}