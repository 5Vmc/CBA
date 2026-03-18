using Babu;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class LineupPad : MonoBehaviour
    {
        [SerializeField] private LineupCardListAdapter osa;
        [SerializeField] private Button dataBtn;
        [SerializeField] private Button stateBtn;

        [SerializeField] private Image stateImage;
        [SerializeField] private Image dataImage;

        [SerializeField] private Image stateTitle;
        [SerializeField] private Image dataTitle;

        [SerializeField] private RectTransform bottom;
        [SerializeField] private RectTransform top;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private TMP_Text normalTips;
        [SerializeField] private TMP_Text selectTips;

        [SerializeField] private HorizontalLayoutGroup topLayout = null;

        private void Awake()
        {
#if UNITY_WEBGL
            normalTips.transform.SetAsLastSibling();
            topLayout.childAlignment = TextAnchor.LowerLeft;
#endif
        }

        private FormationBase _formation;

        private void OnEnable()
        {
            dataBtn.onClick.AddListener(OnData);
            stateBtn.onClick.AddListener(OnState);

            cancelBtn.onClick.AddListener(OnCancelSelectModel);
            EventManager.Instance.Register(EventID.OnLineupChangeSelectModel, OnLineupChangeSelectModel);
        }

        private void OnDisable()
        {
            dataBtn.onClick.RemoveListener(OnData);
            stateBtn.onClick.RemoveListener(OnState);
            cancelBtn.onClick.RemoveListener(OnCancelSelectModel);
            EventManager.Instance.Unregister(EventID.OnLineupChangeSelectModel, OnLineupChangeSelectModel);

            _formation.UpdateLineupShowTime();
        }

        public void OnShow(FormationBase formation)
        {
            _formation = formation;
            Player.FightManager.FormationController.SaveFormationToServer(_formation);
            osa.BindFormation(_formation);
            osa.SetItems(GetShowCardList());
            osa.ResetViewPortPos();
            SetSelectModel(false);

        }
        private List<PlayerCard> GetShowCardList()
        {
            List<PlayerCard> starterList = new List<PlayerCard>();
            List<PlayerCard> substituteList = new List<PlayerCard>();
            List<PlayerCard> reserveList = new List<PlayerCard>();

            foreach (var card in Player.CardManager.CardList)
            {
                FormationCardState formationState;
                if (!_formation.IsFightFormation())
                    formationState = card.FormationDataDic[_formation.FormationId].State;
                else
                    formationState = card.FightFormationData.State;

                if (formationState == FormationCardState.Starter) starterList.Add(card);
                else if (formationState == FormationCardState.Substitute) substituteList.Add(card);
                else
                {
                    reserveList.Add(card);
                }
            }

            starterList.Sort(CmpStarterList);
            substituteList.Sort(CmpSubstituteList);
            reserveList.Sort(CmpReserveList);

            return starterList.Union(substituteList).Union(reserveList).ToList();
        }

        private int CmpStarterList(PlayerCard x, PlayerCard y)
        {
            return x.FormationDataDic[_formation.FormationId].GetPositionId().CompareTo(y.FormationDataDic[_formation.FormationId].GetPositionId());
        }

        private int CmpSubstituteList(PlayerCard x, PlayerCard y)
        {
            return x.FormationDataDic[_formation.FormationId].SubstituteIndex.CompareTo(y.FormationDataDic[_formation.FormationId].SubstituteIndex);
        }

        private int CmpReserveList(PlayerCard x, PlayerCard y)
        {
            return -(x.FightPoint.CompareTo(y.FightPoint));
        }



        private void OnData()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH);
            ChangeShowType(LineupCardAdapterShowType.Data);
        }

        private void OnState()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH);
            ChangeShowType(LineupCardAdapterShowType.State);
        }

        private void ChangeShowType(LineupCardAdapterShowType showType)
        {
            osa.ChangeShowType(showType);
            dataTitle.gameObject.SetActive(showType == LineupCardAdapterShowType.Data);
            stateTitle.gameObject.SetActive(showType == LineupCardAdapterShowType.State);
            stateImage.gameObject.SetActive(showType == LineupCardAdapterShowType.State);
            dataImage.gameObject.SetActive(showType == LineupCardAdapterShowType.Data);
        }

        private void OnCancelSelectModel()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CANCEL);
            osa.CancelSelectModel();
        }

        private void OnLineupChangeSelectModel(object[] args)
        {
            var flag = (bool)args[0];
            SetSelectModel(flag);
            stateImage.gameObject.SetActive(!flag);
            dataImage.gameObject.SetActive(!flag);
            if (flag)
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_PRESSACTV);
                BottomMoveUp();
                TopMoveDown();
            }
            else
            {
                BottomMoveDown();
                TopMoveUp();
            }
        }

        private void SetSelectModel(bool flag)
        {
            cancelBtn.gameObject.SetActive(flag);
            normalTips.gameObject.SetActive(!flag);
            selectTips.gameObject.SetActive(flag);
        }

        private void BottomMoveUp()
        {
            bottom.DOAnchorPosY(0, 0.25f);
        }

        private void TopMoveDown()
        {
            top.DOAnchorPosY(0, 0.25f);
        }

        private void BottomMoveDown()
        {
            bottom.DOAnchorPosY(-146, 0.25f);
        }

        private void TopMoveUp()
        {
            top.DOAnchorPosY(120, 0.25f);
        }
    }
}
