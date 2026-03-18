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
    public class PlayoffFinalsGuessMVPUIProperties : WindowProperties
    {
        public PlayoffFinalsGuessManager.Team team;
        public PlayoffFinalsGuessMVPUIProperties(PlayoffFinalsGuessManager.Team team)
        {
            this.team = team;
        }
    }
    public class PlayoffFinalsGuessMVPUI : AWindowController<PlayoffFinalsGuessMVPUIProperties>
    {
        #region 初始化与监听
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
            EventManager.Instance.Register(EventID.OnSelectPlayoffFinalsGuessMVPPlayerItem, OnSelectPlayoffFinalsGuessMVPPlayerItem);
            confirmGuessButton.OnClick += OnClickConfirmGuessButton;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
            EventManager.Instance.Unregister(EventID.OnSelectPlayoffFinalsGuessMVPPlayerItem, OnSelectPlayoffFinalsGuessMVPPlayerItem);
            confirmGuessButton.OnClick -= OnClickConfirmGuessButton;
        }
        protected override void OnPropertiesSet()
        {
            scrollView.enabled = false;
            SetPlayerData();
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                scrollView.verticalNormalizedPosition = 1f;
                scrollView.enabled = true;
            });
        }
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<PlayoffFinalsGuessMVPUI>();
        }
        #endregion

        #region 设置球员

        [SerializeField] private GameObject playerItemPrefab = null;
        [SerializeField] private RectTransform contentPanel = null;
        private List<PlayoffFinalsGuessMVPPlayerItem> playoffFinalsGuessMVPPlayerItemList = new List<PlayoffFinalsGuessMVPPlayerItem>();
        [SerializeField] private TMP_Text teamNameText = null;
        [SerializeField] private ScrollRect scrollView = null;
        private void SetPlayerData()
        {
            teamNameText.text = Configs.FinalsGuessTeam.GetConfig((int)Properties.team).Name;

            List<FinalsGuessPlayerConfig> finalsGuessPlayerConfigList = Configs.FinalsGuessPlayer.GetConfigList().Where((p) => p.Team == (int)Properties.team).ToList();
            int playerCount = finalsGuessPlayerConfigList.Count;
            int itemCount = playoffFinalsGuessMVPPlayerItemList.Count;

            if (itemCount < playerCount)
            {
                for (int i = 0; i < playerCount - itemCount; i++)
                {
                    GameObject playerItemGo = Instantiate(playerItemPrefab, contentPanel);
                    playerItemGo.gameObject.SetActive(true);
                    PlayoffFinalsGuessMVPPlayerItem playoffFinalsGuessMVPPlayerItem = playerItemGo.GetComponent<PlayoffFinalsGuessMVPPlayerItem>();
                    playoffFinalsGuessMVPPlayerItemList.Add(playoffFinalsGuessMVPPlayerItem);
                }
            }

            for (int i = 0; i < playoffFinalsGuessMVPPlayerItemList.Count; i++)
            {
                if (i < playerCount)
                {
                    playoffFinalsGuessMVPPlayerItemList[i].gameObject.SetActive(true);
                    playoffFinalsGuessMVPPlayerItemList[i].SetData(finalsGuessPlayerConfigList[i]);
                }
                else
                {
                    playoffFinalsGuessMVPPlayerItemList[i].gameObject.SetActive(false);
                }
            }
            selectedPlayerItem = playoffFinalsGuessMVPPlayerItemList[0];
            RefreshSelectLight();
        }

        #endregion

        #region 选择球员

        private PlayoffFinalsGuessMVPPlayerItem selectedPlayerItem = null;
        private void OnSelectPlayoffFinalsGuessMVPPlayerItem(object[] objs)
        {
            PlayoffFinalsGuessMVPPlayerItem playoffFinalsGuessMVPPlayerItem = objs[0] as PlayoffFinalsGuessMVPPlayerItem;
            selectedPlayerItem = playoffFinalsGuessMVPPlayerItem;
            RefreshSelectLight();
        }
        private void RefreshSelectLight()
        {
            foreach (PlayoffFinalsGuessMVPPlayerItem item in playoffFinalsGuessMVPPlayerItemList)
            {
                item.SetLight(item == selectedPlayerItem);
            }
        }
        [SerializeField] private BabuButton confirmGuessButton = null;
        public void OnClickConfirmGuessButton(BabuButton _)
        {
            int selectStopTime = Configs.FinalsGuessCourse.GetConfig(PlayoffFinalsGuessManager.Instance.selectStopCourseId).MatchTime;
            long leftTime = selectStopTime - Utils.DataConvUtil.ServerTime;
            if (leftTime <= 0)
            {
                Tips.PopError("已过可预测时间");
                UIController.Instance.CloseWindow<PlayoffFinalsGuessMVPUI>();
                return;
            }
            PlayoffFinalsGuessManager.Instance.GuessMVP(selectedPlayerItem.finalsGuessPlayerConfig.Id, () =>
            {
                UIController.Instance.CloseWindow<PlayoffFinalsGuessMVPUI>();
            });
        }

        #endregion

    }
}