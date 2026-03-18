using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;

namespace BigBang.UI
{
    public class DragonBoatFestivalEndUI : AWindowController
    {
        [SerializeField] private BabuButton closeButton = null;
        [SerializeField] private TMP_Text leaderText = null;
        [SerializeField] private BabuButton confirmButton = null;

        protected override void AddListeners()
        {
            base.AddListeners();

            closeButton.OnClick += OnClickCloseButton;
            confirmButton.OnClick += OnClickCloseButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            closeButton.OnClick -= OnClickCloseButton;
            confirmButton.OnClick -= OnClickCloseButton;
        }

        [SerializeField] private Image leftWinImage = null;
        [SerializeField] private Image rightWinImage = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            bool hasLeader = DragonBoatFestivalManager.Instance.courseData.Leaders.Count == 2;
            bool hasMeters = DragonBoatFestivalManager.Instance.courseData.Meters.Count == 2;
            bool leftWin = hasMeters && DragonBoatFestivalManager.Instance.courseData.Meters[0] > DragonBoatFestivalManager.Instance.courseData.Meters[1];
            bool rightWin = hasMeters && DragonBoatFestivalManager.Instance.courseData.Meters[0] < DragonBoatFestivalManager.Instance.courseData.Meters[1];
            leftWinImage.gameObject.SetActive(leftWin);
            rightWinImage.gameObject.SetActive(rightWin);
            leaderText.text = "";
            if (hasLeader)
            {
                if (leftWin) leaderText.text = "在 <size=36>{0}区-{1}</size> 的带领下 \n大家赢得了胜利".SafeFormat(DragonBoatFestivalManager.Instance.courseData.Leaders[0].ServerId, DragonBoatFestivalManager.Instance.courseData.Leaders[0].Name);
                if (rightWin) leaderText.text = "在 <size=36>{0}区-{1}</size> 的带领下 \n大家赢得了胜利".SafeFormat(DragonBoatFestivalManager.Instance.courseData.Leaders[1].ServerId, DragonBoatFestivalManager.Instance.courseData.Leaders[1].Name);
                if (hasMeters && DragonBoatFestivalManager.Instance.courseData.Meters[0] == DragonBoatFestivalManager.Instance.courseData.Meters[1]) leaderText.text = "在 <size=36>{0}区-{1}</size> 和 size=36>{2}区-{3}</size> 的带领下 \n大家赢得了胜利".SafeFormat(DragonBoatFestivalManager.Instance.courseData.Leaders[0].ServerId, DragonBoatFestivalManager.Instance.courseData.Leaders[0].Name, DragonBoatFestivalManager.Instance.courseData.Leaders[1].ServerId, DragonBoatFestivalManager.Instance.courseData.Leaders[1].Name);
            }
            UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKeys.DragonBoatFestival2024ShowEnd + Player.GbId, 1);
        }

        private void OnClickCloseButton(BabuButton _)
        {
            UIController.Instance.CloseWindow<DragonBoatFestivalEndUI>();
        }
    }
}