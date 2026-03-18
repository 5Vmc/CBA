using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public enum OfflineExpConfirmType
    {
        Noraml=0,
        Video=1,
        Diamond=2
    }
    public class OfflineUI : AWindowController
    {
        [SerializeField] private TMP_Text offlineTimeText;
        [SerializeField] private TMP_Text offlineExpText;

        [SerializeField] private Button videoBtn;
        [SerializeField] private Button confirmBtn;

        [SerializeField] public OfflineUIAnim Anim;

        [SerializeField] private TMP_Text diamondCostText;
        [SerializeField] private Button diamondBtn;

        protected override void AddListeners()
        {
            videoBtn.onClick.AddListener(OnVideo);
            confirmBtn.onClick.AddListener(OnConfirm);
            diamondBtn.onClick.AddListener(OnDiamond);
        }

        protected override void RemoveListeners()
        {
            videoBtn.onClick.RemoveListener(OnVideo);
            confirmBtn.onClick.RemoveListener(OnConfirm);
            diamondBtn.onClick.RemoveListener(OnDiamond);
        }

        protected override void OnPropertiesSet()
        {
            var offlineTime = (Utils.DataConvUtil.ServerTimeEx - Player.TrainManager.OfflineExpBeginTime) / 1000;
            if (offlineTime < 0) offlineTime = 0;
            offlineTimeText.text = TimeUtils.GetTimeString(offlineTime);
            offlineExpText.text = $"+{Player.TrainManager.OfflineExp.ToFormatString()}";

            //check channel setting
            if(ChannelManager.Instance.EnableAds){
                videoBtn.gameObject.SetActive(true);
                diamondBtn.gameObject.SetActive(false);
            }else{
                videoBtn.gameObject.SetActive(false);
                diamondBtn.gameObject.SetActive(true);
                diamondCostText.text = GameConst.VIDEO_DIAMOND.ToString();
            }

            // 播放动效
            Anim.PlayEnter();
        }

        private void OnConfirm()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            confirmBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                Anim.PlayDiamondMove(() =>
                {
                    Player.TrainManager.DoOfflineReward(OfflineExpConfirmType.Noraml);
                    Close();
                });
                //UIController.Instance.OpenWindow<ExpRewardUI>(new ExpRewardProperties(100, Lang.Get(LangID.OfflineVideoReward)));
            }, playAudio: false);
        }

        private void OnVideo()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            videoBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                Close();
                Player.TrainManager.DoOfflineReward(OfflineExpConfirmType.Video);
                // Properties.Match.DoReward()
            }, playAudio: false);
        }

        private void OnDiamond()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            if (!Player.PackageManager.IsResourceEnough(ResourceId.Diamond, GameConst.VIDEO_DIAMOND))
            {
                Tips.PopError(ErrorID.DiamondNotEnough);
                return;
            }

            diamondBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                Close();
                Player.TrainManager.DoOfflineReward(OfflineExpConfirmType.Diamond);
            }, playAudio: false);
        }

        private void Close()
        {
            
            UIController.Instance.CloseWindow<OfflineUI>();
        }
    }
}