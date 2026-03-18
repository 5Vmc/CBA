using System;
using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using Babu.BigNumber;
using System.Threading.Tasks;

namespace BigBang.UI
{
    public class InviteMatchResultProperties : WindowProperties
    {
        public InviteMatch Match { get; set; }

        public InviteMatchResultProperties(InviteMatch match)
        {
            Match = match;
        }
    }

    public class InviteMatchResultUI : AWindowController<InviteMatchResultProperties>
    {
        [SerializeField] private TMP_Text resultContentText;
        [SerializeField] private TMP_Text rewardText;

        [SerializeField] private Button videoBtn;
        [SerializeField] private Button confirmBtn;
    
        [SerializeField] private TMP_Text diamondCostText;
        [SerializeField] private Button diamondBtn;


        [SerializeField] private Image lightImg;

        [SerializeField] private List<GameObject> activityGroup;

        [SerializeField] private InviteMatchResultUIAnim anim;
        [SerializeField] private GameObject particle;
        [SerializeField] private List<GameObject> blueParticles;
        [SerializeField] private List<GameObject> yellowParticles;

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
            // var clubConfig = Configs.InviteClub.GetConfig(Properties.Match.OpponentClubId);
            resultContentText.text = Lang.Get(LangID.InviteMatchResultSuccessText, "{ClubName}", Properties.Match.OpponentName);
            rewardText.text = $"+{Properties.Match.BaseReward.ToFormatString()}";

            //check channel setting
            if(ChannelManager.Instance.EnableAds){
                videoBtn.gameObject.SetActive(true);
                diamondBtn.gameObject.SetActive(false);
            }else{
                videoBtn.gameObject.SetActive(false);
                diamondBtn.gameObject.SetActive(true);
                diamondCostText.text = GameConst.VIDEO_DIAMOND.ToString();
            }


            anim.Play();
        }

        private void OnConfirm()
        {
            if (Properties.Match != null)
            {
                // Properties.Match.DoReward()
                Close(OfflineExpConfirmType.Noraml);
            }
        }

        private void OnVideo()
        {
            if (Properties.Match != null)
            {
                videoBtn.gameObject.SetActive(false);
                confirmBtn.gameObject.SetActive(false);
                OnVideoCompleted(() =>
                {
                    Close(OfflineExpConfirmType.Video);
                });
            }
        }

        private void OnDiamond()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            if (!Player.PackageManager.IsResourceEnough(ResourceId.Diamond, GameConst.VIDEO_DIAMOND))
            {
                Tips.PopError(ErrorID.DiamondNotEnough);
                return;
            }

            diamondBtn.gameObject.SetActive(false);
            confirmBtn.gameObject.SetActive(false);

            Close(OfflineExpConfirmType.Diamond);
        }

        private void Close(OfflineExpConfirmType type)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            activityGroup.ForEach(item => item.SetActive(false));
            particle.SetActive(true);
            // 播放经验移动动效
            anim.PlayDiamondMove(() =>
            {
                particle.SetActive(false);
                Player.TrainManager.InviteMatchController.DoReward(Properties.Match.Id, type);
                activityGroup.ForEach(item => item.SetActive(true));
                UIController.Instance.CloseWindow<InviteMatchResultUI>();
                videoBtn.gameObject.SetActive(true);
                confirmBtn.gameObject.SetActive(true);
            });
        }

        private void OnVideoCompleted(Action callback)
        {
            var start = Properties.Match.BaseReward.Clone();
            var end = Properties.Match.BaseReward.Clone() * 10;
            var sub = end - start;
            PlaySound();
            DOTween.To(value => rewardText.text = $"+{(start + sub * value).ToFormatString()}", 0, 1, 1f).OnComplete(() =>
            {
                Babu.DelayTaskService.Instance.Run(this.gameObject, 0.5f, () =>
                {
                    callback?.Invoke();
                });
            });
        }

        private async void PlaySound()
        {
            for (int i = 0; i < 10; i++)
            {
                AudioManager.Instance.PlaySound(AudioNames.EVENT_CDEND);
                await Task.Delay(100);
            }
        }
    }
}