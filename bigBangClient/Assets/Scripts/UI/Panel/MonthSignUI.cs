using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using System;
using Utils;
using BigBang.Animation;
using Babu.Client.Fsm;
using GameConfig.Config;

namespace BigBang.UI
{
    public class MonthSignUI : MonoBehaviour, IActivityClient
    {
        //[SerializeField] private BabuButton closeBtn;
        [SerializeField] private TMP_Text monthTitleText;
        [SerializeField] private TMP_Text signCountTxt;
        [SerializeField] private MonthSignUIAdapter adapter;
        [SerializeField] private MonthSignAddedItem addedSiginItem;

        [SerializeField] public MonthSignUIAnim Anim;
        [SerializeField] private RectTransform moveLayer;

        public static RectTransform SignPos;
        public static RectTransform MoveLayer;

        private bool isInit = false;

        protected void Awake()
        {
            SignPos = signCountTxt.rectTransform;
            MoveLayer = moveLayer;
            //closeBtn.Sound = null;
        }

        private void OnEnable()
        {
            //closeBtn.OnClick += OnClose;
            Babu.EventManager.Instance.Register(EventID.OnRefreshMonthSiginUI, OnRefreshMonthSignUI);
        }

        private void OnDisable()
        {
            //closeBtn.OnClick -= OnClose;
            Babu.EventManager.Instance.Unregister(EventID.OnRefreshMonthSiginUI, OnRefreshMonthSignUI);
            isInit = false;
        }

        public void LoadActivityClient(ActivityConfig activityConfig)
        {
            SetData();
            Anim.PlayEnter();
            isInit = true;
        }

        //private void OnClose(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
        //    TouchManager.Instance.DisableTouch();
        //    Anim.PlayExit(() =>
        //    {
        //        TouchManager.Instance.EnableTouch();
                
        //        FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
        //        {
        //            OpenUIAction = async () =>
        //            {
        //                UIController.Instance.HidePanel<MonthSignUI>();
        //                //await UIController.Instance.ShowPanel<HomeUI>();
        //            }
        //        });
        //    });
        //}

        private void OnRefreshMonthSignUI(object[] args)
        {
            if (args != null && args.Length > 0)
            {
                int monthID = (int)args[0];
                Player.ActivityManager.UpdateMonthSign();
                adapter.RefreshData(monthID);

                RefreshDate();

                Player.ActivityManager.UpdateMonthSignAdded();
                addedSiginItem.SetData(Player.ActivityManager.AddedSignMonth);
            }
            else
            {
                SetData();
            }
        }

        private void SetData()
        {
            RefreshDate();
            RefreshMonthSign();
            RefreshMonthSignAdded();
        }

        private void RefreshDate()
        {
            monthTitleText.text = DateTime.Now.Month.ToString() + Lang.Get(LangID.MonthRewardTxt);
            if (isInit)
            {
                Anim.PlaySignCountTxt(Player.ActivityManager.SignDay.ToString());
            }
            else
            {
                signCountTxt.text = Player.ActivityManager.SignDay.ToString();
            }
        }

        //刷新每日月签
        private void RefreshMonthSign()
        {
            Player.ActivityManager.UpdateMonthSign();
            adapter.SetData(Player.ActivityManager.SignMonth);
        }

        //刷新累计月签
        private void RefreshMonthSignAdded()
        {
            Player.ActivityManager.UpdateMonthSignAdded();
            addedSiginItem.SetData(Player.ActivityManager.AddedSignMonth);
        }
    }
}