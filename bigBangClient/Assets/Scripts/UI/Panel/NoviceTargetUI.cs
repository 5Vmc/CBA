using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using GameConfig;
using System.Linq;
using TMPro;
using Utils.GameItem;
using Utils;
using System.Collections.Generic;
using System;
using UnityTimer;
using BigBang.Animation;
using DG.Tweening;
using Coffee.UIEffects;

namespace BigBang.UI
{
    public class NoviceTargetUI : APanelController
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private NoviceTargetItem[] items;
        //[SerializeField] private NoviceTargetItem item2;
        //[SerializeField] private NoviceTargetItem item3;
        //[SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private TMP_Text remainingTime;
        // [SerializeField] private TMP_Text cardName;
        // [SerializeField] private TMP_Text combatEffectiveness;
        // [SerializeField] private TMP_Text positionTxt;
        [SerializeField] private TMP_Text progressTxt;
        //[SerializeField] private Image playerImg;
        [SerializeField] private Image progressValue;
        [SerializeField] private Button getBtn;
        [SerializeField] private List<NoviceTargetSwitchButton> days;
        [SerializeField] private TMP_Text finishDesText;
        // [SerializeField] private Sprite enableSprite;
        // [SerializeField] private Sprite disableSprite;

        private string remainingTimeString = null;
        private string progressString = null;

        private int selectDay = 1;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            //toggleGroup.OnValueChanged += OnToggleGroupChanged;
            // toggleGroup.OnHandle += OnHandle;
            getBtn.onClick.AddListener(OnGet);
            Player.NoviceTaskManager.OnUpdateData += OnPropertiesSet;
            //InitToggle();

            for (int i = 0; i < days.Count; i++)
            {
                var i1 = i + 1;
                days[i].EnableButton.onClick.AddListener(() => OnClickDayBtn(i1, true));
            }
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            // toggleGroup.OnValueChanged -= OnToggleGroupChanged;
            //toggleGroup.OnHandle -= OnHandle;
            getBtn.onClick.RemoveListener(OnGet);
            Player.NoviceTaskManager.OnUpdateData -= OnPropertiesSet;


            for (int i = 0; i < days.Count; i++)
            {
                days[i].EnableButton.onClick.RemoveAllListeners();
            }
        }

        protected override void OnPropertiesSet()
        {
            remainingTimeString ??= remainingTime.text;
            progressString ??= progressTxt.text;
            base.OnPropertiesSet();
            var finishCount = Player.NoviceTaskManager.GetFinishedCount();
            progressTxt.text = progressString.Replace("{value}", finishCount.ToString());

            remainingTime.text = remainingTimeString.Replace("{value}", Mathf.Max(0, 14 - Player.NoviceTaskManager.Days).ToString());
            // var reward = GameItemUtils.CreateGameItem(Configs.NoviceTargetTask.GetConfigList().Last().Reward);

            var card = Configs.NoviceTargetTask.GetConfigList().Find((item) => { return item.Type == 3; });
            if (card != null)
            {
                progressValue.fillAmount = finishCount * 1.0f / card.Target;
                GameItem cardGameItem = GameItemUtils.CreateGameItem(card.Reward);
                var playCard = new PlayerCard(cardGameItem.Id);
                finishDesText.text = finishDesText.text.Replace("{PlayerName}", PlayerCard.GetFullName(playCard.Config));
            }

            for (int i = 0; i < days.Count; i++)
            {
                days[i].SetDay(i + 1);
                days[i].ShowRedDot(Player.NoviceTaskManager.HasDayRedDot((i + 1)));
            }
            // int index = 1;
            // index = toggleGroup.Index;
            var cfgList = Configs.NoviceTargetTask.GetConfigList().FindAll((item) => { return item.Day == Player.NoviceTaskManager.Days; }); //.ElementAtOrDefault(index);
            if (cfgList == null) return;
            int index = 0;
            for (index = 0; index < cfgList.Count && index < items.Length; index++)
            {
                items[index].gameObject.SetActive(true);
                items[index].SetData(cfgList[index].Id);
            }

            for (int j = index; j < items.Length; j++)
            {
                items[j].gameObject.SetActive(false);
            }

            OnClickDayBtn(this.selectDay, false);
            RefreshGetButtonState();
        }

        private void OnGet()
        {
            if (Player.NoviceTaskManager.Days > GameConst.NOVICE_TASK_END_DADYS)
            {
                Tips.PopTips("活动结束");
                UIController.Instance.HidePanel<NoviceTargetUI>();
                return;
            }

            int finishTarge = 20;
            var card = Configs.NoviceTargetTask.GetConfigList().Find((item) => { return item.Type == 3; });
            if (card != null) finishTarge = card.Target;
            if (Player.NoviceTaskManager.GetFinishedCount() >= finishTarge)
            {
                var cfg = Configs.NoviceTargetTask.GetConfigList().Last();
                NetworkManager.Instance.GetNoviceTaskReward(cfg.Id, response =>
                {
                    if (response.Succeed)
                    {
                        var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(cfg.Reward).ToList());
                        UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                        RefreshGetButtonState();
                    }
                });
            }
        }
        [SerializeField] private UIEffect GetButtonGray;
        private void RefreshGetButtonState()
        {
            if (Player.NoviceTaskManager.Days > GameConst.NOVICE_TASK_END_DADYS)//活动结束
            {
                getBtn.interactable = false;
                GetButtonGray.enabled = true;
                return;
            }

            int finishTarge = 20;
            var card = Configs.NoviceTargetTask.GetConfigList().Find((item) => { return item.Type == 3; });
            if (card != null) finishTarge = card.Target;
            if (Player.NoviceTaskManager.GetFinishedCount() < finishTarge)//未达成活动要求的次数
            {
                getBtn.interactable = false;
                GetButtonGray.enabled = true;
                return;
            }

            var cfg = Configs.NoviceTargetTask.GetConfigList().Last();
            if (Player.NoviceTaskManager.IsFinished(cfg.Id) == true)//已领取过
            {
                getBtn.interactable = false;
                GetButtonGray.enabled = true;
                return;
            }

            getBtn.interactable = true;
            GetButtonGray.enabled = false;
        }


        private void OnClickDayBtn(int clickDay, bool playAnim)
        {
            this.selectDay = clickDay;
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            if (clickDay > Player.NoviceTaskManager.Days)
            {
                return;
            }

            var cfgList = Configs.NoviceTargetTask.GetConfigList().FindAll((item) => { return item.Day == clickDay; }); //.ElementAtOrDefault(index);
            if (cfgList == null) return;

            for (int i = 0; i < days.Count; i++)
            {
                if (Player.NoviceTaskManager.Days < i + 1)
                {
                    days[i].SetStatus(NoviceTargetSwitchButtonStatus.Disable);
                }
                else
                {
                    if (i + 1 == clickDay)
                    {
                        days[i].SetStatus(NoviceTargetSwitchButtonStatus.EnableSelect);
                    }
                    else
                    {
                        days[i].SetStatus(NoviceTargetSwitchButtonStatus.EnableNormal);
                    }
                }
                days[i].ShowRedDot(Player.NoviceTaskManager.HasDayRedDot((i + 1)));
            }

            int index = 0;
            for (index = 0; index < cfgList.Count && index < items.Length; index++)
            {
                items[index].gameObject.SetActive(true);
                items[index].SetData(cfgList[index].Id);
            }

            for (int j = index; j < items.Length; j++)
            {
                items[j].gameObject.SetActive(false);
            }

            this.PlayItemsAnimAfterClick(playAnim);
        }

        private void PlayItemsAnimAfterClick(bool playAnim)
        {
            if (playAnim == false) return;
            for (int i = 0; i < items.Length; i++)
            {
                RectTransform rectTransform = items[i].gameObject.GetComponent<RectTransform>();
                rectTransform.gameObject.SetAlpha(0);
                rectTransform.localScale = Vector3.one * 0.8f;
            }
            for (int i = 0; i < items.Length; i++)
            {
                if (i < 3)
                {
                    Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }

                RectTransform rectTransform = items[i].gameObject.GetComponent<RectTransform>();
                rectTransform.gameObject.DOFade(1, 0.3f).SetDelay(0.1f * i);
                rectTransform.DOScale(1, 0.3f).SetDelay(0.1f * i);
            }
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.HidePanel<NoviceTargetUI>();
        }
    }
}