using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class BigBangConfirmUIComponent : MonoBehaviour
    {
        public Button CloseBtn;
        public Button BigBangBtn;
        public Button SuperBigBangBtn;
        public RectTransform Pad;
        public TMP_Text TitleText;
        public Image SuperImg;

        public GameObject ImageAd;
        public GameObject DiamondPanel;

        public void UpdateUI()
        {
            if(ChannelManager.Instance.EnableAds){
                ImageAd.SetActive(true);
                DiamondPanel.SetActive(false);
            }
            else{
                ImageAd.SetActive(false);
                DiamondPanel.SetActive(true);
            }
        }
    }
}

