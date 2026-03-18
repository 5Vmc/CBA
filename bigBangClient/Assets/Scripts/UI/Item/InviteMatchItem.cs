using Babu;
using BigBang.Animation;
using GameConfig;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class InviteMatchItem : MonoBehaviour
    {
        [SerializeField] private Button startBtn;
        [SerializeField] private GameObject cdCanvas;
        [SerializeField] private GameObject matchCanvas;
        [SerializeField] private Image organizerImage;
        [SerializeField] private TMP_Text clubNameText;
        [SerializeField] private TMP_Text placeText;
        [SerializeField] private TMP_Text cdContentText;
        [SerializeField] private TMP_Text inviteContentText;
        [SerializeField] private TMP_Text inviteTitleText;

        

       // [SerializeField] private List<Button> btnList=new List<Button>();

        [SerializeField] public InviteMatchItemAnim Anim;

        private InviteMatch _match;
        private int _tick = 0;
        [HideInInspector] public bool? CurrentState = null;

        public int Index;

       // public static System.Action toResetBtn;

        private void Awake()
        {
            //Anim = GetComponent<InviteMatchItemAnim>();
           // toResetBtn += ResetButton;
        }

        private void OnEnable()
        {
           
            startBtn.onClick.AddListener(OnClickStartMatch);
            
            // foreach(var btns in btnList)
            // {
            //     btns.onClick.AddListener(() => { CanOnClickStartMatch(btns); });//检查事件
            // }
            // // currentState = (_match.State != InviteMatchState.Rewarded);
        }

        private void OnDisable()
        {
            
            startBtn.onClick.RemoveListener(OnClickStartMatch);
        }
        

        private void Update()
        {
            SetCdTime();
        }

        // private void ResetButton()
        // {
        //     foreach(var btns in btnList)
        //     {
        //         btns.GetComponent<Button>().enabled = true;
        //     }    
        // }

        private void OnStateChanged()
        {
            if (!CurrentState.Value)
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_INVITREF);
            }
            Anim.PlayClose(() =>
            {
                Anim.Play(0, !CurrentState.Value);
                matchCanvas.SetActive(CurrentState.Value);
                cdCanvas.SetActive(!CurrentState.Value);
                if (CurrentState.Value)
                {
                    AudioManager.Instance.PlaySound(AudioNames.ANI_BBBOARDREF);
                }
            });
        }

        public void SetData(InviteMatch match)
        {
            _match = match;
            if (match == null)
            {
                Debug.LogError("InviteMatch = null");
                matchCanvas.SetActive(false);
                cdCanvas.SetActive(false);
                return;
            }

            if (CurrentState != null && CurrentState != (_match.State != InviteMatchState.Rewarded))
            {
                CurrentState = (_match.State != InviteMatchState.Rewarded);
                OnStateChanged();
            }
            else
            {
                CurrentState = (_match.State != InviteMatchState.Rewarded);
                matchCanvas.SetActive(CurrentState.Value);
                cdCanvas.SetActive(!CurrentState.Value);
            }
            SetCdTime();
            SetMatchData();
        }

        private async void SetMatchData()
        {
            if (_match != null && _match.State != InviteMatchState.Rewarded)
            {
                // 设置队徽
                organizerImage.sprite = await SpriteProxy.GetInviteOrganizerSprite(_match.OrganizerIcon);
                inviteContentText.text = _match.Content;
                inviteTitleText.text = Lang.Get(LangID.InviteTitleText, "{ClubName}", _match.Organizer);
                clubNameText.text = _match.OpponentName;
                placeText.text = _match.Place;
            }
        }

        private void SetCdTime()
        {
            if (_match is { State: InviteMatchState.Rewarded })
            {
                int cdTime = (int)((_match.CdEndTime - Utils.DataConvUtil.ServerTimeEx) / 1000);
                if (cdTime == _tick) return;

                _tick = cdTime;
                cdContentText.text = TimeUtils.GetTimeString(cdTime > 0 ? cdTime : 0);
                if (cdTime == 0 || cdTime == -1)
                {
                    EventManager.Instance.Dispatch(EventID.OnInviteMatchRefresh);
                }
            }
        }

        private void OnClickStartMatch()
        {
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.BTN_CDACC);
            Anim.PlayAccept(() =>
            {
                TouchManager.Instance.EnableTouch();
                var flag = Player.TrainManager.InviteMatchController.DoMatch(_match.Id);
                if (!flag)
                {
                    Debug.LogError("OnClickStartMatch ，!flag ， _match.Id = " + _match.Id);
                    //todo error tips
                }
            });
        }

        //检查方法
        // private void CanOnClickStartMatch(Button btn)
        // {
        //     foreach(var item in btnList)
        //     {
        //         if(item!=btn)
        //         {
        //             item.GetComponent<Button>().enabled = false;
        //             StartCoroutine("StandTime", item);
        //         }
        //     }
        // }

        // IEnumerator StandTime(Button btn)
        // {
        //     yield return new WaitForSecondsRealtime(2);
        //     btn.GetComponent<Button>().enabled = true;
        // }
    }
}