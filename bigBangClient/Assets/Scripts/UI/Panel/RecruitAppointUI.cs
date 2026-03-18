using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class RecruitAppointProperties : WindowProperties
    {
        public int PoolId { get; set; }

        public RecruitAppointProperties(int poolId)
        {
            PoolId = poolId;
        }
    }
    public class RecruitAppointUI : AWindowController<RecruitAppointProperties>
    {
        [SerializeField] private BabuButton closeBtn;

        [SerializeField] private List<AppointCardIcon> appointList;
        [SerializeField] private List<BabuButton> appointBtnList;

        [SerializeField] private PoolCardGridAdapter poolCardGridAdapter;

        [SerializeField] private Toggle QFBtn;
        [SerializeField] private Toggle ZFBtn;
        [SerializeField] private Toggle HWBtn;
        [SerializeField] private Toggle ALLBtn;

        // [SerializeField] private List<ParticleSystem> particles;

        [SerializeField] private GameObject paper;

        private RecruitPool _pool;

        public RecruitAppointUIAnim Anim;

        [SerializeField] private BabuToggleGroup toggleGroup = null;

        protected override void Awake()
        {
            base.Awake();
            poolCardGridAdapter.SelectActionRegister(SelectCard);
        }
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            appointBtnList[0].OnClick += OnClickCancelAppoint1;
            appointBtnList[1].OnClick += OnClickCancelAppoint2;
            appointBtnList[2].OnClick += OnClickCancelAppoint3;

            toggleGroup.OnValueChanged += OnToggleChanged;

            EventManager.Instance.Register(EventID.OnRecruitPoolRefresh, RefreshPoolInfo);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            appointBtnList[0].OnClick -= OnClickCancelAppoint1;
            appointBtnList[1].OnClick -= OnClickCancelAppoint2;
            appointBtnList[2].OnClick -= OnClickCancelAppoint3;

            toggleGroup.OnValueChanged -= OnToggleChanged;

            EventManager.Instance.Unregister(EventID.OnRecruitPoolRefresh, RefreshPoolInfo);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            _pool = Player.CardManager.RecruitController.GetPool(Properties.PoolId);
            if (_pool == null) return;

            UpdateAppointCardInfo();
            // poolCardGridAdapter.SetData(_pool.GetPoolCardList(), _pool.PoolId);

            gameObject.SetAlpha(0);
            gameObject.DOFade(1, 0.3f);

            toggleGroup.Switch((int)SubUIID.ALL);

            // 播放进入动画
            Anim.PlayEnter();
        }

        private void UpdateAppointCardInfo()
        {
            foreach (var info in _pool.AppointCardDic)
            {
                var index = info.Key;
                var appointCard = info.Value;
                var cardId = appointCard.CardId;
                var cardConfig = Configs.CardModel.GetConfig(cardId);
                appointList[index - 1].SetData(cardConfig, appointCard.State == RecruitAppointCardState.Hit);
            }
        }

        private void OnClose(BabuButton _)
        {
            // 初始化RecruitUI模型动画
            EventManager.Instance.Dispatch(EventID.InitRecruitUIModelAnim);
            EventManager.Instance.Dispatch(EventID.OnRecruitPoolRefresh);
            // 关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            AudioManager.Instance.PlaySound(AudioNames.BACK_TLIST);
            // 纸张淡出
            paper.DOFade(0, 0.3f);
            // 纸张缩放
            paper.transform.DOScale(0.9f, 0.3f).OnComplete(() =>
            {
                paper.transform.localScale = Vector3.one;
                paper.SetAlpha(1);
                UIController.Instance.CloseWindow<RecruitAppointUI>();
            });
            EventManager.Instance.Dispatch(EventID.RecruitUIShowInfo);
        }

        public enum SubUIID
        {
            HW = 0,
            QF = 1,
            ZF = 2,
            ALL = 3,
        }
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = toggleGroup.EnableIndex;
            ShowPad((SubUIID)selectedIndex);
        }
        private void ShowPad(SubUIID padIndex)
        {
            List<CardModelConfig> list = new List<CardModelConfig>();
            switch (padIndex)
            {
                case SubUIID.HW: list = _pool.GetPoolCardList(QualityType.Orange, (int)PositionType.HouWei); break;
                case SubUIID.QF: list = _pool.GetPoolCardList(QualityType.Orange, (int)PositionType.QianFeng); break;
                case SubUIID.ZF: list = _pool.GetPoolCardList(QualityType.Orange, (int)PositionType.ZhongFeng); break;
                case SubUIID.ALL: list = _pool.GetPoolCardList(QualityType.Orange, (int)PositionType.All); break;
            }
            SetDataAndPlayAnim(list);
        }

        private void OnClickCancelAppoint1(BabuButton _)
        {
            DoCancelAppoint(1);
        }

        private void OnClickCancelAppoint2(BabuButton _)
        {
            DoCancelAppoint(2);
        }

        private void OnClickCancelAppoint3(BabuButton _)
        {
            DoCancelAppoint(3);
        }

        private void DoCancelAppoint(int index)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CANCEL);
            // particles[index - 1].Stop();
            Player.CardManager.RecruitController.DoCancelAppoint(_pool.PoolId, index, CancelAppointSuccess);
        }

        private void CancelAppointSuccess(int index)
        {
            toggleGroup.Switch(index - 1);
        }

        private void RefreshPoolInfo(object obj)
        {
            UpdateAppointCardInfo();
        }


        private void SetDataAndPlayAnim(List<CardModelConfig> list)
        {
            poolCardGridAdapter.SetData(list, _pool.PoolId);
        }

        private void SelectCard(CardModelConfig selectCard)
        {

        }
    }
}