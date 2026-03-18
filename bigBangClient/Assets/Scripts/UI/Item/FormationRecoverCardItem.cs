using System;
using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    public enum FormationRecoverCardItemType
    {
        State,
        Injury,
        Energy,
    }

    public class FormationRecoverCardItem : MonoBehaviour
    {
        [SerializeField] private BabuButton formationRecoverCardItem = null;
        [SerializeField] private List<Image> qualityImageList = new();
        [SerializeField] private Image playerIconImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private RectTransform statePanel = null;
        [SerializeField] private RectTransform injuryPanel = null;
        [SerializeField] private RectTransform energyPanel = null;
        [SerializeField] private Image selectImage = null;
        [SerializeField] private Image stateImage = null;
        [SerializeField] private Image medicalImage = null;
        [SerializeField] private TMP_Text seriousInjuryText = null;
        [SerializeField] private TMP_Text minorInjuryText = null;
        [SerializeField] private TMP_Text healthText = null;
        [SerializeField] private HorizontalLayoutGroup timeLayout = null;
        [SerializeField] private Image clockImage = null;
        [SerializeField] private TMP_Text startDateText = null;
        [SerializeField] private Image progressbarNotFullFgImage = null;
        [SerializeField] private Image progressbarFullFgImage = null;
        [SerializeField] private TMP_Text energyText = null;
        [SerializeField] private TMP_Text energyBackupText = null;

        private void OnEnable()
        {
            formationRecoverCardItem.OnClick += OnClickFormationRecoverCardItem;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        }

        private void OnDisable()
        {
            formationRecoverCardItem.OnClick -= OnClickFormationRecoverCardItem;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        }

        private void OnClickFormationRecoverCardItem(BabuButton _)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);

            Babu.EventManager.Instance.Dispatch(EventID.OnClickFormationRecoverCardItem, this);
        }

        public PlayerCard playerCard = null;
        [SerializeField] private PeakImage peakImage = null;
        public async void SetCardData(PlayerCard playerCard)
        {
            this.playerCard = playerCard;
            this.gameObject.SetActive(playerCard != null);
            if (playerCard == null) return;
            peakImage.SetData(playerCard);
            SetBg(playerCard.Quality);
            nameText.text = playerCard.Config.Name;
            stateImage.sprite = await playerCard.GetPlayerCardStatusSprite();
            playerIconImage.sprite = await SpriteProxy.GetPlayerPortrait(playerCard.Config.Portrait);
        }

        public FormationRecoverCardItemType type = FormationRecoverCardItemType.State;
        public async void SetType(FormationRecoverCardItemType type)
        {
            this.type = type;
            if (playerCard == null) return;
            switch (type)
            {
                case FormationRecoverCardItemType.State:
                    {
                        statePanel.gameObject.SetActive(true);
                        injuryPanel.gameObject.SetActive(false);
                        energyPanel.gameObject.SetActive(false);
                        stateImage.sprite = await playerCard.GetPlayerCardStatusSprite();
                    }
                    break;
                case FormationRecoverCardItemType.Injury:
                    {
                        statePanel.gameObject.SetActive(false);
                        injuryPanel.gameObject.SetActive(true);
                        energyPanel.gameObject.SetActive(false);
                        RefreshInjury();
                    }
                    break;
                case FormationRecoverCardItemType.Energy:
                    {
                        statePanel.gameObject.SetActive(false);
                        injuryPanel.gameObject.SetActive(false);
                        energyPanel.gameObject.SetActive(true);
                        bool isSingleWarning = playerCard.Energy < GameConst.CardSingleEnergyWarning;
                        progressbarNotFullFgImage.gameObject.SetActive(isSingleWarning);
                        progressbarFullFgImage.gameObject.SetActive(!isSingleWarning);
                        //energyText.text = "{0}%".SafeFormat(Mathf.FloorToInt(playerCard.SingleEnergyRatio));
                        energyText.text = "{0}%".SafeFormat(Mathf.FloorToInt(playerCard.TotalEnergyRatio));
                        //energyBackupText.text = "储备{0}%".SafeFormat(Mathf.FloorToInt(playerCard.BackupEnergyRatio));
                        if (isSingleWarning == false)
                        {
                            progressbarFullFgImage.fillAmount = playerCard.TotalEnergyRatio / 100;
                        }
                        else
                        {
                            progressbarNotFullFgImage.fillAmount = playerCard.TotalEnergyRatio / 100;
                        }
                    }
                    break;
            }
        }
        private void RefreshInjury()
        {
            bool isHurt = playerCard.IsHurt();
            medicalImage.gameObject.SetActive(isHurt);
            seriousInjuryText.gameObject.SetActive(playerCard.InjuryType == InjuryType.SeriousInjury);
            minorInjuryText.gameObject.SetActive(playerCard.InjuryType == InjuryType.MinorInjury);
            healthText.gameObject.SetActive(playerCard.InjuryType == InjuryType.None || playerCard.InjuryType == InjuryType.Health);
            clockImage.gameObject.SetActive(isHurt);
            timeLayout.gameObject.SetActive(isHurt);
            if (isHurt)
            {
                long endTime = playerCard.InjuryEndTime;
                int leftTime = (int)(endTime - Utils.DataConvUtil.ServerTime);
                startDateText.text = Utility.FormatLeftTimeMustHasHour(leftTime);
                LayoutRebuilder.ForceRebuildLayoutImmediate(startDateText.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(timeLayout.transform as RectTransform);
            }
        }
        private void RefreshLeftTimeOneSec()
        {
            if (playerCard == null) return;
            if (type != FormationRecoverCardItemType.Injury) return;
            bool isHurt = playerCard.IsHurt();
            if (healthText.gameObject.activeSelf) return;
            RefreshInjury();
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
            selectImage.gameObject.SetActive(isSelect);
            if (isSelect) PlayHighlightAnim();
        }

        private void PlayHighlightAnim()
        {
            selectImage.DOKill();
            selectImage.transform.DOKill();
            selectImage.SetAlpha(0);
            selectImage.transform.localScale = Vector3.one * 1.5f;
            selectImage.DOFade(1, 0.2f).AddTo(this.gameObject);
            selectImage.transform.DOScale(1.016f, 0.2f).AddTo(this.gameObject);
        }
    }
}
