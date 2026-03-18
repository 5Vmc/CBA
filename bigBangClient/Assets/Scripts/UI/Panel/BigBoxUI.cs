using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Utils.GameItem;
using System;
using DG.Tweening;
using Babu;
using Utils;
using GameConfig.Config;
using GameConfig;
using System.Linq;
using TMPro;
using BigBang.Animation;

namespace BigBang.UI
{
    public class BigBoxUIProperties : WindowProperties
    {
        public Action callback { get; private set; }
        public string titleStr = "获得宝箱";

        public BigBoxUIProperties(Action callback = null, string titleStr = "获得宝箱")
        {
            this.callback = callback;
            this.titleStr = titleStr;
        }
    }

    public class BigBoxUI : AWindowController<BigBoxUIProperties>
    {

        protected override void AddListeners()
        {
            base.AddListeners();
            bigBoxOpenButton.OnClick += OnClickBigBoxOpenButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            bigBoxOpenButton.OnClick -= OnClickBigBoxOpenButton;
        }


        private bool isButtonClicked = false;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            isButtonClicked = false;

            bigBoxTipText.text = Properties.titleStr;

            bigBoxPanel.gameObject.SetActive(true);
            bigBoxCloseImage.gameObject.SetActive(true);
            bigBoxOpenImage.gameObject.SetActive(false);

            Anim.PlayBigBoxAnim();

        }

        [SerializeField] private RectTransform bigBoxPanel = null;
        [SerializeField] private BabuButton bigBoxOpenButton = null;
        [SerializeField] private Image bigBoxCloseImage = null;
        [SerializeField] private Image bigBoxOpenImage = null;
        [SerializeField] private BigBoxUIAnim Anim = null;
        [SerializeField] private TMP_Text bigBoxTipText = null;

        private void OnClickBigBoxOpenButton(BabuButton _)
        {
            if (isButtonClicked)
            {
                return;
            }
            isButtonClicked = true;

            bigBoxCloseImage.gameObject.SetActive(false);
            bigBoxOpenImage.gameObject.SetActive(true);

            Anim.HideBigBoxAnim(() =>
            {
                UIController.Instance.CloseWindow<BigBoxUI>();
            });

            Properties.callback?.Invoke();
        }

    }
}