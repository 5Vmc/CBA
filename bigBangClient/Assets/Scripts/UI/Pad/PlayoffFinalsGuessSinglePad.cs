using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.Animation;
using BigBang.UI;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.PlayoffFinalsGuessManager;
using static BigBang.SpriteNames;

public class PlayoffFinalsGuessSinglePad : MonoBehaviour, IActivity
{
    #region 初始化

    private void OnEnable()
    {
        helpButton.OnClick += OnClickHelpButton;
        EventManager.Instance.Register(EventID.RefreshPlayoffFinalsGuessUI, RefreshPlayoffFinalsGuessUI);
    }

    private void OnDisable()
    {
        helpButton.OnClick -= OnClickHelpButton;
        EventManager.Instance.Unregister(EventID.RefreshPlayoffFinalsGuessUI, RefreshPlayoffFinalsGuessUI);
    }
    private ActivityData activityData = null;
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        loadingDataAnim.Init();
        playoffFinalsGuessSinglePadAnim.Init();
        loadingDataAnim.PlayEnter();
        PlayoffFinalsGuessManager.Instance.GetCourseData(() =>
        {
            RefreshUI();
            playoffFinalsGuessSingleAdapter.SetNormalizedPosition(0);
            loadingDataAnim.PlayExit();
            playoffFinalsGuessSinglePadAnim.PlayEnter(() =>
            {

            });
        });
    }
    private void RefreshPlayoffFinalsGuessUI(object[] _)
    {
        RefreshUI();
    }
    #endregion

    #region 按钮回调

    [SerializeField] private BabuButton helpButton = null;
    private void OnClickHelpButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<PlayoffFinalsGuessHelpUI>();
    }

    #endregion

    #region 界面刷新
    Stage stage = Stage.NotOpen;
    [SerializeField] private PlayoffFinalsGuessSinglePadAnim playoffFinalsGuessSinglePadAnim = null;
    [SerializeField] private LoadingDataAnim loadingDataAnim = null;
    private void RefreshUI()
    {
        stage = PlayoffFinalsGuessManager.Instance.GetStage();
        switch (stage)
        {
            case Stage.CanSelectTeam:
            case Stage.CanSelectMVP:
            case Stage.NormalPlaying:
            case Stage.Ending:
                SetList();
                break;
            default:
                playoffFinalsGuessSingleAdapter.SetData(new());
                break;
        }
    }

    [SerializeField] private PlayoffFinalsGuessSingleAdapter playoffFinalsGuessSingleAdapter = null;
    private void SetList()
    {
        playoffFinalsGuessSingleAdapter.SetData(PlayoffFinalsGuessManager.Instance.GetCanShowCourse());
    }


    #endregion


}
