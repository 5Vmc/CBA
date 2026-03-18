using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SkillTrainRoomSelectProperties : WindowProperties
    {
        public SkillTrainRoom Room { get; set; }
        public PlayerCard SelectCard { get; set; }
        public SkillConfig SelectSkill { get; set; }

        public SkillTrainRoomSelectProperties(SkillTrainRoom room, PlayerCard selectCard = null, SkillConfig selectSkill = null)
        {
            Room = room;
            SelectCard = selectCard;
            SelectSkill = selectSkill;
        }
    }

    public class SkillTrainRoomSelectUI : AWindowController<SkillTrainRoomSelectProperties>
    {
        [SerializeField] private SkillTrainRoomCardItem selectCardItem;
        [SerializeField] private SkillTrainRoomSkillItem selectSkillItem;

        [SerializeField] private GameObject cardPad;

        [SerializeField] private SkillTrainSelectCardGridAdapter cardGridAdapter;

        [SerializeField] private GameObject skillPad;
        [SerializeField] private SkillTrainSelectSkillGridAdapter skillGridAdapter;
        [SerializeField] private Toggle skillAllBtn;
        [SerializeField] private Toggle skillAtkBtn;
        [SerializeField] private Toggle skillDefBtn;
        [SerializeField] private Toggle skillAssistBtn;

        [SerializeField] private Image arrowImage;
        [SerializeField] private GameObject estimatedTime;
        [SerializeField] private TMP_Text estimatedTimeText;
        [SerializeField] private Button clearBtn;
        [SerializeField] private Button beginTrainBtn;

        [SerializeField] private Button closeBtn;

        private PlayerCard _selectCard;
        private SkillConfig _selectSkillConfig;

        public SkillTrainRoomSelectUIAnim Anim;

        public static RectTransform PlayerCloneStatic;
        public static RectTransform SkillCloneStatic;

        public static RectTransform PlayerTargetPosStatic;
        public static RectTransform SkillTargetPosStatic;

        [SerializeField] private RectTransform playerClone;
        [SerializeField] private RectTransform skillClone;
        [SerializeField] private RectTransform playerTargetPos;
        [SerializeField] private RectTransform skillTargetPos;

        protected override void Awake()
        {
            base.Awake();
            cardGridAdapter.SelectActionRegister(SelectCard);
            skillGridAdapter.SelectActionRegister(SelectSkill);
            PlayerCloneStatic = playerClone;
            PlayerCloneStatic.gameObject.SetActive(false);
            SkillCloneStatic = skillClone;
            PlayerTargetPosStatic = playerTargetPos;
            SkillTargetPosStatic = skillTargetPos;

            transform.gameObject.AddComponent<CanvasGroup>();
        }


        protected override void AddListeners()
        {
            base.AddListeners();

            closeBtn.onClick.AddListener(OnClose);
            beginTrainBtn.onClick.AddListener(OnBegin);
            clearBtn.onClick.AddListener(OnClear);

            skillAllBtn.onValueChanged.AddListener(OnAll);
            skillAtkBtn.onValueChanged.AddListener(OnAtk);
            skillDefBtn.onValueChanged.AddListener(OnDef);
            skillAssistBtn.onValueChanged.AddListener(OnAssist);

            transform.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            beginTrainBtn.onClick.RemoveListener(OnBegin);
            clearBtn.onClick.RemoveListener(OnClear);

            skillAllBtn.onValueChanged.RemoveListener(OnAll);
            skillAtkBtn.onValueChanged.RemoveListener(OnAtk);
            skillDefBtn.onValueChanged.RemoveListener(OnDef);
            skillAssistBtn.onValueChanged.RemoveListener(OnAssist);


        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            Anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<SkillTrainRoomSelectUI>();
                transform.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
            });
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            ClearSelectInfo();
            _selectSkillConfig = Properties.SelectSkill;
            _selectCard = Properties.SelectCard;
            UpdateInfo();

            if (_selectCard == null)
            {
                ShowCardPad(false);
            }
            else
            {
                ShowSkillPad(false);
            }
            transform.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
            Anim.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ANI_STUDYLIST);
        }

        private int GetSelectSkillId()
        {
            if (_selectSkillConfig == null) return 0;
            return _selectSkillConfig.Id;
        }

        private int GetSelectCardId()
        {
            if (_selectCard == null) return 0;
            return _selectCard.Config.Id;
        }

        private void ShowCardPad(bool playAnim = true)
        {
            var cardList = Player.CardManager.GetCardList(PositionType.All).OrderByDescending(p => p.Quality).ToList();
            cardGridAdapter.SetData(cardList, GetSelectCardId(), GetSelectSkillId());
            Anim.PlayShowCardPadAnim(playAnim);
        }

        private void ShowSkillPad(bool playAnim = true)
        {

            // _showSkillList = Player.CardManager.SkillController.GetCanTrainSkillList(_selectCardConfig.Id);
            //目前是所有的特技
            _showSkillList = Player.CardManager.SkillController.GetUnlockSkillList();

            skillAllBtn.isOn = true;
            skillAtkBtn.isOn = skillDefBtn.isOn = skillAssistBtn.isOn = false;
            OnAll(true);
            OnAtk(false);
            OnDef(false);
            OnAssist(false);
            Anim.PlayShowSkillPadAnim(playAnim);
        }

        private List<Skill> _showSkillList = new List<Skill>();


        //所有
        private void OnAll(bool flag)
        {
            skillAllBtn.GetComponent<StatusControl>().SetStatus(flag);
            if (flag)
            {
                AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
                skillGridAdapter.SetData(_showSkillList, GetSelectSkillId(), GetSelectCardId());
            }
        }

        //进攻技能
        private void OnAtk(bool flag)
        {
            skillAtkBtn.GetComponent<StatusControl>().SetStatus(flag);
            if (flag)
            {
                AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
                skillGridAdapter.SetData(_showSkillList.Where(item => item.Config.Type == SkillType.Atk).ToList(), GetSelectSkillId(), GetSelectCardId());
            }
        }

        //防守技能
        private void OnDef(bool flag)
        {
            skillDefBtn.GetComponent<StatusControl>().SetStatus(flag);
            if (flag)
            {
                AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
                skillGridAdapter.SetData(_showSkillList.Where(item => item.Config.Type == SkillType.Def).ToList(), GetSelectSkillId(), GetSelectCardId());
            }
        }

        //辅助技能
        private void OnAssist(bool flag)
        {
            skillAssistBtn.GetComponent<StatusControl>().SetStatus(flag);
            if (flag)
            {
                AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
                skillGridAdapter.SetData(_showSkillList.Where(item => item.Config.Type == SkillType.Assist).ToList(), GetSelectSkillId(), GetSelectCardId());
            }
        }

        private void OnBegin()
        {
            if (_selectCard == null)
            {
                Tips.PopError(ErrorID.SkillTrainRoomSelectCardNull);
                return;
            }

            if (_selectSkillConfig == null)
            {
                Tips.PopError(ErrorID.SkillTrainRoomSelectSkillNull);
                return;
            }
            AudioManager.Instance.PlaySound(AudioNames.BTN_STARTSTUDY);
            Anim.PlayExit();
            Player.CardManager.SkillController.BeginTrainSkill(Properties.Room.RoomId, _selectCard.Config.Id, _selectSkillConfig.Id, OnBeginTrainSkillSuccess);
        }

        private void OnBeginTrainSkillSuccess()
        {
            OnClose();
        }

        // 清除重新选择
        private void OnClear()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_RESET);
            ClearSelectInfo();
            UpdateInfo();

            ShowCardPad(!cardPad.activeInHierarchy);
        }

        private void ClearSelectInfo()
        {
            _showSkillList.Clear();

            _selectCard = null;
            _selectSkillConfig = null;
            selectCardItem.SetData(null);
            selectSkillItem.SetData(null);
            SkillFlag = CardFlag = false;
        }

        private bool skillFlag = false;
        private bool cardFlag = false;

        private bool SkillFlag
        {
            get
            {
                return skillFlag;
            }
            set
            {
                skillFlag = value;
                if (skillFlag)
                {
                    Anim.PlayPlayerLoom();
                }
                if (skillFlag || cardFlag)
                {
                    ShowClearBtn();
                }
                else
                {
                    HidClearBtn();
                    Anim.StopSkillLoom();
                    Anim.PlayPlayerLoom();
                }
                if (skillFlag && cardFlag)
                {
                    ShowStartBtn();
                    Anim.StopPlayerLoom();
                    Anim.StopSkillLoom();
                }
                else
                {
                    HidStartBtn();
                }
            }
        }

        private bool CardFlag
        {
            get
            {
                return cardFlag;
            }
            set
            {
                cardFlag = value;
                if (cardFlag)
                {
                    Anim.PlaySkillLoom();
                }
                if (skillFlag || cardFlag)
                {
                    ShowClearBtn();
                }
                else
                {
                    HidClearBtn();
                    Anim.StopSkillLoom();
                    Anim.PlayPlayerLoom();
                }

                if (skillFlag && cardFlag)
                {
                    ShowStartBtn();
                    Anim.StopPlayerLoom();
                    Anim.StopSkillLoom();
                }
                else
                {
                    HidStartBtn();
                }
            }
        }

        private void ShowClearBtn()
        {
            if (!clearBtn.gameObject.activeInHierarchy)
            {
                clearBtn.gameObject.SetActive(true);
                clearBtn.gameObject.SetAlpha(0);
                clearBtn.gameObject.DOFade(1, 0.3f);
            }
        }

        private void HidClearBtn()
        {
            clearBtn.gameObject.SetActive(false);
        }

        private void ShowStartBtn()
        {
            if (!beginTrainBtn.gameObject.activeInHierarchy)
            {
                beginTrainBtn.gameObject.SetActive(true);
                beginTrainBtn.gameObject.SetAlpha(0);
                beginTrainBtn.gameObject.DOFade(1, 0.3f);
            }
        }

        private void HidStartBtn()
        {
            beginTrainBtn.gameObject.SetActive(false);
        }

        private void SelectSkill(Skill skill)
        {
            _selectSkillConfig = skill.Config;

            if (_selectCard == null)
            {
                ShowCardPad();
            }
            else
            {
                CardFlag = true;
            }

            UpdateInfo();
            SkillFlag = true;
            Anim.PlaySelectSkillAnim();

        }

        private void SelectCard(PlayerCard card)
        {
            transform.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
            _selectCard = card;

            if (_selectSkillConfig == null)
            {
                ShowSkillPad();
            }
            else
            {
                SkillFlag = true;
            }

            UpdateInfo();
            CardFlag = true;

            // 播放动画

            Anim.PlaySelectCardAnim();
        }

        private bool CheckSkillSelect(SkillConfig skill)
        {
            if (_selectSkillConfig == null)
            {
                SkillFlag = false;
                return false;
            }
            SkillFlag = true;
            return _selectSkillConfig.Id == skill.Id;
        }

        private bool CheckCardSelect(PlayerCard card)
        {
            if (_selectCard == null)
            {
                CardFlag = false;
                return false;
            }
            CardFlag = true;
            return _selectCard.Config.Id == card.Config.Id;
        }

        private void UpdateInfo()
        {
            if (_selectCard == null)
            {
                selectCardItem.SetData(null);
            }
            else
            {
                selectCardItem.SetData(_selectCard.Config);
                selectCardItem.SetQuality(_selectCard.Quality);
            }
            selectSkillItem.SetData(_selectSkillConfig, true);

            arrowImage.gameObject.SetActive(true);
            estimatedTime.SetActive(false);

            if (_selectCard == null)
            {
                arrowImage.rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                return;
            }

            if (_selectSkillConfig == null)
            {
                arrowImage.rectTransform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                return;
            }

            estimatedTime.SetActive(true);
            var span = TimeSpan.FromSeconds(_selectSkillConfig.TrainTime);
            estimatedTimeText.text = $"{(int)span.TotalHours}:{((int)span.Minutes).ToString("D2")}:{((int)span.Seconds).ToString("D2")}";
            arrowImage.gameObject.SetActive(false);
        }
    }
}