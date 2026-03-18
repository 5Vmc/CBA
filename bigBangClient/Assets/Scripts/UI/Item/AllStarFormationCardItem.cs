using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.Config;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using static BigBang.AllStarManager;

namespace BigBang.UI
{
    public class AllStarFormationCardItem : MonoBehaviour
    {
        private void OnEnable()
        {
            normalCardPanel.OnClick += OnClick;
            emptyCardPanel.OnClick += OnClick;
        }
        private void OnDisable()
        {
            normalCardPanel.OnClick -= OnClick;
            emptyCardPanel.OnClick -= OnClick;
        }
        private void OnClick(BabuButton _)
        {
            if (!isShowUp)
            {
                bool isOtherArea = allStarAdditionConfig.Area != AllStarManager.Instance.serverData.Area;
                if (isOtherArea)
                {
                    Tips.PopTips("无法上阵不同阵营的球员");
                    return;
                }
                bool isUsing = false;
                foreach (var item in AllStarManager.Instance.usingCardPositionIdDic)
                {
                    if (item.Value == playerCard && item.Key == selectPosition)
                    {
                        isUsing = true;
                        break;
                    }
                }
                if (isUsing)
                {
                    Tips.PopTips("该球员已经上阵");
                    return;
                }
            }
            EventManager.Instance.Dispatch(EventID.OnClickAllStarFormationCardItem, this);
        }

        [SerializeField] private BabuButton normalCardPanel = null;
        [SerializeField] private BabuButton emptyCardPanel = null;
        [SerializeField] private RectTransform regionBgPanel = null;
        [SerializeField] private RectTransform typeTextPanel = null;
        [HideInInspector] public PlayerCard playerCard = null;
        [HideInInspector] public bool isShowUp = false;
        [HideInInspector] public bool isEmpty = false;
        public PositionSeparatedType selectPosition = PositionSeparatedType.All;
        public void SetData(PlayerCard playerCard, bool isShowUp, PositionSeparatedType selectPosition)
        {
            this.selectPosition = selectPosition;
            this.playerCard = playerCard;
            this.isShowUp = isShowUp;
            this.isEmpty = playerCard == null;
            normalCardPanel.gameObject.SetActive(!isEmpty);
            emptyCardPanel.gameObject.SetActive(isEmpty);
            regionBgPanel.gameObject.SetActive(!isEmpty);
            typeTextPanel.gameObject.SetActive(!isEmpty);
            additionText.gameObject.SetActive(false);
            if (isEmpty)
            {
                SetEmptyCardData();
            }
            else
            {
                SetNormalCardData(selectPosition);
            }
            if (isShowUp == false) SetSelect(false);
        }

        [SerializeField] public int upPosition = 0;
        [SerializeField] private IllusionAnim emptyAddImage = null;
        [SerializeField] private TMP_Text emptyPositionText = null;
        private void SetEmptyCardData()
        {
            emptyAddImage.PlayLoop(2f, 0, 1f, 0.5f);
            emptyPositionText.text = PlayerCard.GetAdaptPositionAbbreviation((PositionSeparatedType)upPosition);
        }

        [SerializeField] private List<Image> qualityImageList = new();
        [SerializeField] private Image playerIconImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text positionText = null;
        [SerializeField] private TMP_Text strengthNumText = null;
        [SerializeField] private Image lockDarkImage = null;
        [SerializeField] private Image useDarkImage = null;
        [SerializeField] private PeakImage peakImage = null;
        [SerializeField] private TMP_Text lockText = null;
        [SerializeField] private Image useBgImage = null;
        [HideInInspector] public AllStarAdditionConfig allStarAdditionConfig = null;
        private async void SetNormalCardData(PositionSeparatedType selectPosition)
        {
            CardModelConfig cardModelConfig = playerCard.Config;
            SetBg(playerCard.Quality);
            SetStar(playerCard.Star);
            nameText.text = cardModelConfig.Name;
            positionText.gameObject.SetActive(isShowUp);
            if (isShowUp) positionText.text = PlayerCard.GetAdaptPositionAbbreviation((PositionSeparatedType)upPosition);

            peakImage.SetData(cardModelConfig);

            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(cardModelConfig.Portrait);

            allStarAdditionConfig = Configs.AllStarAddition.GetConfig(cardModelConfig.Id);
            if (allStarAdditionConfig == null)
            {
                Debug.LogWarning("AllStarFormationCardItem , SetNormalCardData , allStarAdditionConfig == null , cardModelConfig.Id = " + cardModelConfig.Id);
                return;
            }
            strengthNumText.text = Mathf.RoundToInt(playerCard.FightPoint * allStarAdditionConfig.Addition).ToString("N0");
            SetType((Type)allStarAdditionConfig.Type, (Area)allStarAdditionConfig.Area);

            bool isOtherArea = allStarAdditionConfig.Area != AllStarManager.Instance.serverData.Area;
            bool isUsing = false;
            foreach (var item in AllStarManager.Instance.usingCardPositionIdDic)
            {
                if(item.Value == playerCard && item.Key == selectPosition)
                {
                    isUsing = true;
                    break;
                }
            }
            useDarkImage.gameObject.SetActive(!isShowUp && isUsing);
            lockDarkImage.gameObject.SetActive(!isShowUp && isOtherArea);
            lockText.gameObject.SetActive(!isShowUp && isOtherArea);
            if (!isShowUp && isOtherArea) lockText.text = "{0}球员".SafeFormat(AllStarManager.Instance.GetAreaName((Area)allStarAdditionConfig.Area));
            useBgImage.gameObject.SetActive(!isShowUp && isUsing);
        }

        private void SetBg(int quality)// 设置品质
        {
            for (int i = 0; i < qualityImageList.Count; i++)
            {
                qualityImageList[i].gameObject.SetActive(i == quality - 1);
            }
        }
        [SerializeField] private List<GameObject> stars;
        private async void SetStar(int star)// 设置星级
        {
            if (star > 5)
            {
                int showStar = star - 5;
                for (int i = 0; i < stars.Count; i++)
                {
                    stars[i].SetActive(true);
                    if (i + 1 <= showStar)
                        stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetColorfulStar();
                    else
                        stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
            else
            {
                for (int i = 0; i < stars.Count; i++)
                {
                    stars[i].SetActive(i + 1 <= star);
                    stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
        }
        [SerializeField] private List<Image> areaImageList = new();
        [SerializeField] private List<Image> typeImageList = new();
        [SerializeField] private Color otherColor = new();
        [SerializeField] private Color addColor = new();
        [SerializeField] private TMP_Text additionText = null;
        private void SetType(Type type, Area area)
        {
            bool isOtherArea = allStarAdditionConfig.Area != AllStarManager.Instance.serverData.Area;
            bool isNeedLabel = type != Type.Other && !isOtherArea;
            int typeInt = (int)(type) - 1;
            for (int i = 0; i < typeImageList.Count; i++)
            {
                typeImageList[i].gameObject.SetActive(typeInt == i && isNeedLabel);
            }
            int areaInt = (int)(area) - 1;
            for (int i = 0; i < areaImageList.Count; i++)
            {
                areaImageList[i].gameObject.SetActive(areaInt == i && isNeedLabel);
            }
            if (!isOtherArea) additionText.text = "战力<color=#FFFC00>x{0}</color>".SafeFormat(allStarAdditionConfig.Addition);
            additionText.gameObject.SetActive(isNeedLabel);
            strengthNumText.color = isNeedLabel ? addColor : otherColor;
        }

        [SerializeField] private Image selectImage = null;
        [HideInInspector] public bool isSelect = false;//被选中
        public void SetSelect(bool isSelect)
        {
            this.isSelect = isSelect;
            selectImage.gameObject.SetActive(isShowUp && isSelect);
            if (isSelect) PlayHighlightAnim();
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
