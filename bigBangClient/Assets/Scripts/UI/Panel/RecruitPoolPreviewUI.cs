using System.Collections.Generic;
using System.Linq;
using deVoid.UIFramework;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class RecruitPoolPreviewProperties : WindowProperties
    {
        public int PoolId { get; set; }

        public RecruitPoolPreviewProperties(int poolId)
        {
            PoolId = poolId;
        }
    }
    public class RecruitPoolPreviewUI : AWindowController<RecruitPoolPreviewProperties>
    {
        [SerializeField] private List<TMP_Text> ratioTextList;
        [SerializeField] private Button closeBtn;

        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown positionDropdown;

        [SerializeField] private CardConfigGridAdapter cardAdapter;


        private int _selectQuality = QualityType.All;
        private int _selectPosition = (int)PositionType.All;

        private RecruitPool _pool;

        protected override void Awake()
        {
            InitQualityDropdown();
            InitPositionDropdown();
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            closeBtn.onClick.AddListener(OnClose);

            qualityDropdown.onValueChanged.AddListener(OnSelectQuality);
            positionDropdown.onValueChanged.AddListener(OnSelectPosition);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);

            qualityDropdown.onValueChanged.RemoveListener(OnSelectQuality);
            positionDropdown.onValueChanged.RemoveListener(OnSelectPosition);
        }


        protected override void OnPropertiesSet()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            _pool = Player.CardManager.RecruitController.GetPool(Properties.PoolId);
            if (_pool == null) return;

            UpdatePoolRatioText();

            UpdateCardList();
        }


        private void UpdatePoolRatioText()
        {
            for (int i = 0; i < ratioTextList.Count; i++)
            {
                var quality = i + 1;
                var ratio = _pool.GetQualityRatio(quality);
                ratioTextList[i].text = ratio.ToString("P");
            }
        }

        private void InitPositionDropdown()
        {
            DropdownMaker.SetData(positionDropdown, OptionsType.Position);
        }

        private void InitQualityDropdown()
        {
            DropdownMaker.SetData(qualityDropdown, OptionsType.Quality, "<color=#CCCCCC>未开放</color>");
        }

        public void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<RecruitPoolPreviewUI>();
        }

        private void OnSelectPosition(int index)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            var value = DropdownMaker.GetOptionValueByType(OptionsType.Position, index);
            Debug.Log("OnSelectPosition value = " + value);
            _selectPosition = value;

            UpdateCardList();
        }

        private void OnSelectQuality(int index)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);

            var value = DropdownMaker.GetOptionValueByType(OptionsType.Quality, index);
            Debug.Log("OnSelectQuality value = " + value);
            _selectQuality = value;

            UpdateCardList();
        }

        public List<CardModelConfig> MakeCardList()
        {
            return _pool.GetPoolCardList(_selectQuality, _selectPosition);
        }

        private void UpdateCardList()
        {
            var cardList = MakeCardList()
                .OrderByDescending(cfg => cfg.Quality)
                .ThenBy(cfg=>string.IsNullOrWhiteSpace(cfg.PeakYear))
                .ThenByDescending(cfg=>cfg.Id)
                .ToList();
            cardAdapter.SetData(cardList);
        }
    }
}