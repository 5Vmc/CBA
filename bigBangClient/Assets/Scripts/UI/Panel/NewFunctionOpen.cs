using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class NewFunctionOpen : AWindowController
    {
        [SerializeField] private BabuButton btnGo;
        [SerializeField] private Image imgBg;
        [SerializeField] private Image imgTitle;
        [SerializeField] private Image imgTitle1;
        [SerializeField] private Image imgIconBg;
        [SerializeField] private Image imgIcon;
        [SerializeField] private TMP_Text txtFunName;


        private int moduleId = 0;
        protected override void Awake()
        {
        }



        public void OnClose(BabuButton sender)
        {
            PlayExit();
        }

        private void clear()
        {
            imgTitle.SetAlpha(0f);
            imgTitle1.SetAlpha(0f);
            btnGo.gameObject.SetAlpha(0f);
        }

        private void PlayEnter()
        {
            clear();
            Sequence seq = DOTween.Sequence();
            //底条出现
            seq.Insert(0f, imgBg.transform.DOScaleY(0f, 0.25f).From());
            seq.Insert(0.25f, imgIconBg.transform.DOScale(0.2f, 0.25f).From().SetEase(Ease.OutBack));
            //文字初始化
            seq.Insert(0.5f, imgTitle.DOFade(1f, 0f));
            seq.Insert(0.5f, imgTitle.transform.DOScale(1.5f, 0f));
            //文字盖章
            seq.Insert(0.5f, imgTitle.transform.DOScale(1f, 0.25f));
            //虚影初始化
            seq.Insert(0.75f, imgTitle1.DOFade(1f, 0f));
            //虚影反弹
            seq.Insert(0.8f, imgTitle1.transform.DOScale(1.5f, 0.5f));
            seq.Insert(0.8f, imgTitle1.DOFade(0f, 0.5f));
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() =>
            {
                btnGo.gameObject.SetAlpha(1f);
            });
        }

        void PlayExit()
        {
            Player.NewLevelUp = false;
            UIController.Instance.CloseWindow<NewFunctionOpen>();
        }
        protected override void AddListeners()
        {
            btnGo.OnClick += jumpPanel;
        }

        protected override void RemoveListeners()
        {
            btnGo.OnClick -= jumpPanel;
        }

        private void jumpPanel(BabuButton obj)
        {
            UIController.Instance.CloseWindow<NewFunctionOpen>();
            TriggerManager.Instance.JumpPanel(moduleId, true);
        }

        protected override async void OnPropertiesSet()
        {
            var newOpenModule = TriggerManager.Instance.IsNewModuleOpen(Player.Level);
            //var newOpenModule = TriggerManager.Instance.IsNewModuleOpen(5);
            if (newOpenModule != 0)
            {
                var config = Configs.ModuleDefine.GetConfig(newOpenModule);
                moduleId = newOpenModule;
                txtFunName.text = string.Format("<color=#ffff00>[{0}]</font>功能开启了", config.Name);
                imgIcon.sprite = await SpriteProxy.GetHomeIcon(config.Icon);
            }

            PlayEnter();
        }
    }
}