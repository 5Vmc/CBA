using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;
using static BigBang.AllStarManager;
using Utils.GameItem;
using Babu;

namespace BigBang.UI
{
    public class PlayoffFinalsGuessNumberUIProperties : WindowProperties
    {
        public int courseID = 0;
        public PlayoffFinalsGuessNumberUIProperties(int courseID)
        {
            this.courseID = courseID;
        }
    }
    public class PlayoffFinalsGuessNumberUI : AWindowController<PlayoffFinalsGuessNumberUIProperties>
    {
        #region 初始化与监听
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
            EventManager.Instance.Register(EventID.OnSelectPlayoffFinalsGuessNumberBallItem, OnSelectPlayoffFinalsGuessNumberBallItem);
            confirmButton.OnClick += OnClickConfirm;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
            EventManager.Instance.Unregister(EventID.OnSelectPlayoffFinalsGuessNumberBallItem, OnSelectPlayoffFinalsGuessNumberBallItem);
            confirmButton.OnClick -= OnClickConfirm;
        }
        protected override void OnPropertiesSet()
        {
            InitBallItem();
        }

        #endregion

        #region 刷新UI
        private PlayoffFinalsGuessNumberBallItem selectBall = null;
        [SerializeField] private List<PlayoffFinalsGuessNumberBallItem> ballItemList = new();
        private bool isBallInited = false;
        private void InitBallItem()
        {
            if (isBallInited)
            {
                return;
            }
            isBallInited = true;
            for (int i = 0; i < ballItemList.Count; i++)
            {
                ballItemList[i].SetLuckyNumber((i + 1) % 10);
            }
            selectBall = ballItemList[0];
            RefreshLight();
        }
        private void RefreshLight()
        {
            for (int i = 0; i < ballItemList.Count; i++)
            {
                ballItemList[i].SetSelected(ballItemList[i] == selectBall);
            }
        }

        #endregion

        #region 按钮回调

        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<PlayoffFinalsGuessNumberUI>();
        }

        [SerializeField] private BabuButton confirmButton = null;
        private void OnClickConfirm(BabuButton _)
        {
            FinalsGuessCourseConfig finalsGuessCourseConfig = Configs.FinalsGuessCourse.GetConfig(Properties.courseID);
            if (finalsGuessCourseConfig == null)
            {
                Debug.LogWarning("PlayoffFinalsGuessNumberUI , OnClickConfirm , finalsGuessCourseConfig == null");
                UIController.Instance.CloseWindow<PlayoffFinalsGuessNumberUI>();
            }
            if (finalsGuessCourseConfig.MatchTime <= Utils.DataConvUtil.ServerTime)
            {
                Tips.PopError("已过可预测时间");
                UIController.Instance.CloseWindow<PlayoffFinalsGuessNumberUI>();
            }
            int selectedLuckyNumber = selectBall.luckyNumber;
            PlayoffFinalsGuessManager.Instance.GuessLuckyNumber(Properties.courseID, selectedLuckyNumber, () =>
            {
                UIController.Instance.CloseWindow<PlayoffFinalsGuessNumberUI>();
            });
        }

        private void OnSelectPlayoffFinalsGuessNumberBallItem(object[] param)
        {
            selectBall = param[0] as PlayoffFinalsGuessNumberBallItem;
            RefreshLight();
        }

        #endregion

    }
}