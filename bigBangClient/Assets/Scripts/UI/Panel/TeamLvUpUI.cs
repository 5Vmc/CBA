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

    public class TeamLvUpUI : AWindowController
    {
        [SerializeField] private GameObject allPanel;
        [SerializeField] private GameObject bg;
        [SerializeField] private GameObject rewards;
        [SerializeField] private SkeletonGraphic spinAni;
        [SerializeField] private List<InventoryItem> list;
        [SerializeField] private TMP_Text txtContinue;
        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private Image bgImg;

        private Sequence seq;
        protected override void Awake()
        {
            PlayInit();
        }



        public void OnClose(BabuButton sender)
        {
            PlayExit();
            TouchManager.Instance.EnableTouch();
        }

        private void PlayInit()
        {
            seq = DOTween.Sequence();
            txtContinue.SetAlpha(0f);
            spinAni.gameObject.SetActive(false);
            spinAni.Initialize(true);
            bg.SetAlpha(0f);
            bgImg.transform.localScale = new Vector3(1.0f, 0.2f, 1.0f);

            UserLevelConfig config = Configs.UserLevel.GetConfig(Player.Level);

            List<GameItem> rewardsList = GameItemUtils.CreateGameItems(config.Rewards).ToList();
            int count = Math.Max(rewardsList.Count, list.Count);
            for (var i = 0; i < count; i++)
            {
                if (i > list.Count - 1)
                {
                    InventoryItem item = Instantiate(list[0], rewards.transform);
                    list.Add(item);
                }

                list[i].gameObject.SetAlpha(0f);
                if (i > rewardsList.Count - 1)
                {
                    list[i].gameObject.SetActive(false);
                }
                else
                {
                    list[i].canShowTip = true;
                    list[i].gameObject.SetActive(true);
                    list[i].SetData(rewardsList[i]);
                }
            }
        }

        private void PlayEnter()
        {

            seq.Append(bg.DOFade(1f, .3f));
            seq.Append(bgImg.transform.DOScaleY(1.0f, 0.2f));
            seq.AppendCallback(() =>
            {
                spinAni.gameObject.SetActive(true);
                Debug.Log("incallback");
                spinAni.AnimationState.SetAnimation(0, "play", false);
            });
            for (var i = 0; i < list.Count; i++)
            {
                //seq.Append(list[i].gameObject.DOFade(1.0f, 0.5f));
                seq.Insert(1.5f + 0.2f * i, list[i].gameObject.DOFade(1.0f, 0.5f));
            }

            //rewards.DOFade(1f, 1f).SetDelay(1.5f);
            seq.Append(txtContinue.DOFade(1.0f, 0.2f).SetDelay(2.5f));
        }

        void PlayExit()
        {
            seq.Kill(true);
            Player.NewLevelUp = false;
            UIController.Instance.CloseWindow<TeamLvUpUI>();
            if (TriggerManager.Instance.IsNewModuleOpen(Player.Level) > 0)
            {
                UIController.Instance.OpenWindow<NewFunctionOpen>();
            }
        }
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClose;
        }

        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClose;
        }

        protected override void OnPropertiesSet()
        {
            PlayInit();
            PlayEnter();
            TouchManager.Instance.DisableTouch();
            DOTween.Sequence().AppendInterval(1).AppendCallback(() => { TouchManager.Instance.EnableTouch(); });
        }
    }
}