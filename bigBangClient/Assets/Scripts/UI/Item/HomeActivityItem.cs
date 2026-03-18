using BigBang.Animation;
using BigBang.Battle;
using DG.Tweening;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class HomeActivityItem : MonoBehaviour
    {
        [SerializeField] public RectTransform rectTransform;
        [SerializeField] private Button wholeBtn;
        [SerializeField] private Image redDot;
        [SerializeField] private Image bgImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject tipGo;
        [SerializeField] private TMP_Text tipText;

        private void OnEnable()
        {
            UpdateUI();
        }
        public void UpdateUI()
        {
            if (activityConfig == null) return;
            int activityId = activityConfig.Id;
            bool hasRedDot = ActivityController.Instance.HasRedDot(activityId);
            redDot.gameObject.SetActive(hasRedDot);
        }

        private bool isListenersAdded = false;
        public void AddListeners()
        {
            if (isListenersAdded == true) return;
            isListenersAdded = true;
            wholeBtn.onClick.AddListener(OnClickWholeBtn);
        }

        public void RemoveListener()
        {
            if (isListenersAdded == false) return;
            isListenersAdded = false;
            wholeBtn.onClick.RemoveListener(OnClickWholeBtn);
        }

        private ActivityConfig activityConfig;
        public async void SetData(ActivityConfig activityConfig)
        {
            this.activityConfig = activityConfig;
            bgImage.sprite = await SpriteProxy.GetActivityImage(activityConfig.Pic);
            string title = activityConfig.Name;
            if (string.IsNullOrEmpty(activityConfig.HomeTitle) == false)
            {
                string[] homeTitleStrArr = activityConfig.HomeTitle.Split('|');
                title = "{0}<size=48>{1}</size>".SafeFormat(homeTitleStrArr.Length > 0 ? homeTitleStrArr[0] : "", homeTitleStrArr.Length > 1 ? homeTitleStrArr[1] : "");
            }
            titleText.text = title;
            bool hasSubTitle = !string.IsNullOrEmpty(activityConfig.HomeSubTitle);
            tipGo.SetActive(hasSubTitle);
            if (hasSubTitle) tipText.text = activityConfig.HomeSubTitle;
        }

        private void OnClickWholeBtn()
        {
            TriggerManager.Instance.JumpPanel(activityConfig.JumpGroup, false, activityConfig.Id);
        }

    }
}