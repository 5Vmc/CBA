using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Protocol;
using BigBang.Animation;
using TMPro;
using Babu;
using Utils;
using GameConfig.Config;
using GameConfig;

namespace BigBang.UI
{
    public class PlayoffFinalsGuessHomeHistoryUI : AWindowController
    {
        #region 初始化与监听
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuButton confirmBtn = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickCloseBtn;
            confirmBtn.OnClick += OnClickCloseBtn;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickCloseBtn;
            confirmBtn.OnClick -= OnClickCloseBtn;
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<PlayoffFinalsGuessHomeHistoryUI>();
        }
        #endregion

        #region 数据刷新与显示刷新
        [SerializeField] private Image teamIconImage = null;
        [SerializeField] private TMP_Text teamNameText = null;
        [SerializeField] private Image playerImage = null;
        [SerializeField] private TMP_Text mVPNameText = null;
        [SerializeField] private RectTransform playerItem = null;
        [SerializeField] private TMP_Text noMvpTipText = null;
        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            FinalsGuessTeamConfig finalsGuessTeamConfig = Configs.FinalsGuessTeam.GetConfig(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess);
            teamIconImage.sprite = await SpriteProxy.GetPlayoffFinalsGuessMVPTeamSprite(finalsGuessTeamConfig.Icon);
            teamNameText.text = finalsGuessTeamConfig.Name;
            if (PlayoffFinalsGuessManager.Instance.isMVPSelected)
            {
                FinalsGuessPlayerConfig finalsGuessPlayerConfig = Configs.FinalsGuessPlayer.GetConfig(PlayoffFinalsGuessManager.Instance.mvpGuessData.Guess);
                playerImage.sprite = await SpriteProxy.GetPlayoffFinalsGuessMVPPlayerSprite(finalsGuessPlayerConfig.Icon);
                mVPNameText.text = finalsGuessPlayerConfig.Name;
            }
            playerItem.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.isMVPSelected);
            noMvpTipText.gameObject.SetActive(!PlayoffFinalsGuessManager.Instance.isMVPSelected);
        }

        #endregion

    }
}