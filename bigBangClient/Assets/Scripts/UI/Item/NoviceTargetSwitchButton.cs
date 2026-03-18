using BigBang.Animation;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public enum NoviceTargetSwitchButtonStatus
    {
        Disable,
        EnableNormal,
        EnableSelect,
    }

    public class NoviceTargetSwitchButton : MonoBehaviour
    {

        [SerializeField] public Button EnableButton;
        [SerializeField] private Image NormalImage;
        [SerializeField] private Image SelectImage;
        [SerializeField] private TMP_Text EnableDayText;
        [SerializeField] private Image RedDot;

        [SerializeField] private Image DisableImage;
        [SerializeField] private TMP_Text DisableText;

        public void ShowRedDot(bool show)
        {
            RedDot.gameObject.SetActive(show);
        }

        public void SetDay(int day)
        {
            string dayStr = "第<size=45>{0}</size>天".SafeFormat(day);
            EnableDayText.text = dayStr;
            DisableText.text = dayStr;
        }

        public void SetStatus(NoviceTargetSwitchButtonStatus status)
        {
            switch (status)
            {
                case NoviceTargetSwitchButtonStatus.Disable:
                    EnableButton.gameObject.SetActive(false);
                    DisableImage.gameObject.SetActive(true);
                    break;
                case NoviceTargetSwitchButtonStatus.EnableNormal:
                    EnableButton.gameObject.SetActive(true);
                    EnableButton.enabled = true;
                    DisableImage.gameObject.SetActive(false);
                    NormalImage.gameObject.SetActive(true);
                    SelectImage.gameObject.SetActive(false);
                    break;
                case NoviceTargetSwitchButtonStatus.EnableSelect:
                    EnableButton.gameObject.SetActive(true);
                    EnableButton.enabled = false;
                    DisableImage.gameObject.SetActive(false);
                    NormalImage.gameObject.SetActive(false);
                    SelectImage.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }


    }
}