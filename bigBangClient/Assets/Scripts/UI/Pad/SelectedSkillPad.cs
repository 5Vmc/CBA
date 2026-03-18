using System;
using System.Collections.Generic;
using BigBang.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SelectedSkillPad : MonoBehaviour
    {
        [SerializeField] private Button unlockBtn;
        [SerializeField] private TMP_Text costText;//花费
        [SerializeField] private Button trainBtn;
        [SerializeField] private GameObject trainingInfo;
        [SerializeField] private Button trainingInfoBtn;

        [SerializeField] public SkillIcon skillIcon;

        [SerializeField] private GameObject unlockConditions;//解锁条件
        [SerializeField] private List<TMP_Text> conditionTextList;

        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text skillNameText;

        [SerializeField] private Image trainingCardIcon;
        [SerializeField] private TMP_Text trainingCardNameText;
        [SerializeField] private GameObject costObj;

        //[SerializeField] private SkillUI skillUI;

        private Skill _skill;
        private Color redColor = new Color(187 / 255f, 48 / 255f, 49 / 255f, 1);
        private Color normalColor = new Color(193 / 255f, 202 / 255f, 208 / 255f, 1);

        public SelectedSkillPadAnim Anim;
        public event Action OnUnlock;

        private void OnEnable()
        {
            unlockBtn.onClick.AddListener(OnUnlockBtn);
            trainBtn.onClick.AddListener(OnTrain);
            trainingInfoBtn.onClick.AddListener(OnTraining);
        }

        private void OnDisable()
        {
            unlockBtn.onClick.RemoveListener(OnUnlockBtn);
            trainBtn.onClick.RemoveListener(OnTrain);
            trainingInfoBtn.onClick.RemoveListener(OnTraining);
        }

        public void SetData(Skill skill)
        {
            _skill = skill;
            // 如果没有选中技能，隐藏选中栏
            if (_skill == null)
            {
                HidSelf();
                return;
            }
            ShowSelf();
            // 设置Icon
            skillIcon.SetData(_skill, false, true);
            // 设置技能名称
            skillNameText.text = skill.Config.Name;
            // 设置技能描述
            descText.text = skill.Config.Desc;
            int index = 0;
            foreach (var condition in _skill.Config.UnlockConditions)
            {
                ///Debug.Log(_skill.Config.UnlockConditions.Count);
                //Debug.Log(condition.Key + " KeyAndValue " + condition.Value);
                if (_skill.Config.UnlockConditions.Count == 1)
                {
                    conditionTextList[1].text = null;
                }
                conditionTextList[index++].text = MakeConditionString(condition.Key, condition.Value);
            }

            UpdateStateInfo();
        }

        // 最终显示的数据
        private async void UpdateStateInfo()
        {
            var skillState = _skill.GetSkillState();
            // 设置解锁按钮可见性
            unlockBtn.gameObject.SetActive(skillState == SkillState.ConditionsNotMet || skillState == SkillState.ConditionsMetLock);
            // 设置花费可解性
            costObj.gameObject.SetActive(unlockBtn.gameObject.activeInHierarchy);
            // 设置解锁条件可见性
            unlockConditions.gameObject.SetActive(skillState == SkillState.ConditionsNotMet || skillState == SkillState.ConditionsMetLock);
            // 设置前往学习按钮可见性
            trainBtn.gameObject.SetActive(skillState == SkillState.UnlockNoTraining);
            // 设置小人头像可见性
            trainingInfo.gameObject.SetActive(skillState == SkillState.UnlockTraining);
            // 设置解锁花费
            costText.text = _skill.Config.UnlockMoney.ToString();
            costText.color = _skill.IsPlayerMoneyEnough() ? normalColor : redColor;

            var card = _skill.GetTrainingCard();
            if (card == null) return;
            trainingCardIcon.sprite = await SpriteProxy.GetPlayerPortrait(card.Config.Portrait);
            trainingCardNameText.text = PlayerCard.GetFullName(card.Config);
        }

        private string MakeConditionString(int trainId, int targetLevel)
        {
            var playerTrain = Player.TrainManager.GetTrainItem(trainId);
            if (playerTrain == null) return "";
            var trainName = playerTrain.GetConfig().Name;
            var levelStr = ColorString.GetColorString(playerTrain.Level < targetLevel ? "#d54b4c" : "#62b5e0", targetLevel.ToString());
            return $"{trainName} LV.{levelStr}";
        }

        private void OnUnlockBtn()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            Player.CardManager.SkillController.DoUnlockSkill(_skill.Id, OnUnlockSucceed, OnUnlockFaild);
        }

        // 解锁成功
        [EditorButton("播放解锁动画")]
        private void OnUnlockSucceed()
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_UNLOCKSKILL);
            OnUnlock?.Invoke();
            // 播放解锁动画
            unlockBtn.GetComponent<ButtonAnim>().Play(() => Anim.PlayUnlockAnim(UpdateStateInfo));
        }

        // 解锁失败
        private void OnUnlockFaild()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_NULL);
            // 播放按钮动画
            unlockBtn.GetComponent<ButtonAnim>().PlayNull();
        }

        private void OnTrain()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            var room = Player.CardManager.SkillController.GetIdleRoom();
            if (room == null)
            {
                Tips.PopError(ErrorID.SkillTrainRoomNoIdle);
                return;
            }
            Babu.EventManager.Instance.Dispatch(EventID.OnStudySkill, room, _skill.Config);
            //skillUI.ShowRoom(() => UIController.Instance.OpenWindow<SkillTrainRoomSelectUI>(new SkillTrainRoomSelectProperties(room, null, _skill.Config)));
        }
        private void OnTraining()
        {
            Babu.EventManager.Instance.Dispatch(EventID.OnClickTrainingBtn);
        }

        private void HidSelf()
        {
            gameObject.SetAlpha(0);
            GetComponent<CanvasGroup>().interactable = false;
        }

        private void ShowSelf()
        {
            gameObject.SetAlpha(1);
            GetComponent<CanvasGroup>().interactable = true;
        }
    }
}