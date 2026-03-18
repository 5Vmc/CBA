using deVoid.UIFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BigBang.UI
{
    public enum PrivacyType
    {
        Privacy,
        Agreement,
        Age
    }
    [Serializable]
    public class UserPrivacyProperties : WindowProperties
    {
        public PrivacyType privacyType;
        public UserPrivacyProperties(PrivacyType privacyType)
        {
            this.privacyType = privacyType;
        }
    }
    public class UserPrivacyDetailUI : AWindowController<UserPrivacyProperties>
    {
        [SerializeField] Button closeBtn;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text desText;
        [SerializeField] ScrollRect scroll;

        [SerializeField] TextAsset agreenmentText;
        [SerializeField] TextAsset privacyText;
        [SerializeField] TextAsset ageText;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            switch (Properties.privacyType)
            {
                case PrivacyType.Privacy:
                    //titleText.text = Lang.Get(LangID.PrivacyPolicy);
                    //desText.text = privacyText.text;
                    break;
                case PrivacyType.Agreement:
                    //titleText.text = Lang.Get(LangID.UserAgreement);
                    //desText.text = agreenmentText.text;
                    break;
                case PrivacyType.Age:
                    titleText.text = "适龄提示";//Lang.Get(LangID.AgeAppropriateTips);
                    desText.text = ageText.text;
                    break;
            }
            scroll.normalizedPosition = new Vector2(0, 1);
        }

        private void OnClose()
        {
            //AudioManager.Instance.PlaySound(SoundId.NORMAL_BTN_CLICK);
            UIController.Instance.CloseWindow<UserPrivacyDetailUI>();
        }
    }
}
