using Babu;
using BigBang.Animation;
using CBA;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class FormationPad : MonoBehaviour, AnimOut
    {
        #region 初始化

        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [SerializeField] private RectTransform soccerField;
        [SerializeField] private RectTransform backupRoot;

        [SerializeField] private RectTransform formationNameATK;
        [SerializeField] private RectTransform formationNameDEF;
        [SerializeField] private TMP_Text NameTextATK;
        [SerializeField] private TMP_Text NameTextDEF;

        [SerializeField] private Button stateABtn;
        [SerializeField] private Button stateBBtn;
        [SerializeField] FormationDragableItemManager dragableItemManager;
        [SerializeField] private Button BackupLeftPageBtn;
        [SerializeField] private Button BackupRightPageBtn;
        [SerializeField] private FormationBackupAnim formationBackupAnim;

        [SerializeField] private Button BackupBtn;
        [SerializeField] private Button BackupConfirmBtn;
        [SerializeField] private Button NoBackupBtn;
        [SerializeField] private GameObject TitleBench;

        [SerializeField] private Button AutoChangeBtnClassic;
        [SerializeField] private Button AutoChangeBtnHero;
        [SerializeField] private Button StartBattleBtnHero;

        [SerializeField] private Button AutoChangeBtnBounty;
        [SerializeField] private Button StartBattleBtnBounty;

        [SerializeField] private GameObject BenchRoot;
        [SerializeField] private TMP_Text BackupBotttomText;

        [SerializeField] private HorizontalLayoutGroup stateLayout = null;

        #region 爆发的控件
        [SerializeField] private GameObject stateLimit;
        [SerializeField] private GameObject stateFire;
        [SerializeField] private Button btnLimitTip;
        [SerializeField] private BabuButton btnSwitchLimit;
        [SerializeField] private TMP_Text txtTeamUp;
        [SerializeField] private TMP_Text txtMemberUp;
        [SerializeField] private TMP_Text txtBtnSwitch;
        [SerializeField] private List<Image> powerStarList;
        [SerializeField] private List<TMP_Text> txtpowerStarList;
        #endregion

        private FormationBase _formation;

        private float _moveInDuration = 0.3f;

        private void Awake()
        {
#if UNITY_WEBGL
            stateLayout.childAlignment = TextAnchor.MiddleLeft;
#endif
            dragableItemManager.Init();
            RememberAniPos();
        }

        private void OnEnable()
        {
            stateABtn.onClick.AddListener(OnStateA);
            stateBBtn.onClick.AddListener(OnStateB);
            BackupBtn.onClick.AddListener(OnOpenBackup);
            BackupConfirmBtn.onClick.AddListener(OnCloseBackup);
            BackupLeftPageBtn.onClick.AddListener(OnBackupLeftPage);
            BackupRightPageBtn.onClick.AddListener(OnBackupRightPage);
            NoBackupBtn.onClick.AddListener(OnNoBackup);

            AutoChangeBtnClassic.onClick.AddListener(OnAutoChangeClassicBtn);
            AutoChangeBtnHero.onClick.AddListener(OnAutoChangeBtnHero);
            StartBattleBtnHero.onClick.AddListener(OnStartBattleBtnHero);
            AutoChangeBtnBounty.onClick.AddListener(OnAutoChangeBtnBounty);
            StartBattleBtnBounty.onClick.AddListener(OnStartBattleBtnBounty);
            btnLimitTip.onClick.AddListener(showFireTip);
            btnSwitchLimit.OnClick += OnSwitchLimit;

            formationBackupAnim.InitAnim(false);

            dragableItemManager.MainBoardChangeAction += CheckTeamTotalCombat;
            dragableItemManager.MainBoardOrBenchBoardChangeAction += CheckTeamTotalCombat;
            dragableItemManager.MainBoardOrBenchBoardChangeAction += CheckLimit;
        }

        private void OnDisable()
        {
            stateABtn.onClick.RemoveListener(OnStateA);
            stateBBtn.onClick.RemoveListener(OnStateB);
            BackupBtn.onClick.RemoveListener(OnOpenBackup);
            BackupConfirmBtn.onClick.RemoveListener(OnCloseBackup);
            BackupLeftPageBtn.onClick.RemoveListener(OnBackupLeftPage);
            BackupRightPageBtn.onClick.RemoveListener(OnBackupRightPage);
            NoBackupBtn.onClick.RemoveListener(OnNoBackup);

            AutoChangeBtnClassic.onClick.RemoveListener(OnAutoChangeClassicBtn);
            AutoChangeBtnHero.onClick.RemoveListener(OnAutoChangeBtnHero);
            StartBattleBtnHero.onClick.RemoveListener(OnStartBattleBtnHero);
            AutoChangeBtnBounty.onClick.RemoveListener(OnAutoChangeBtnBounty);
            StartBattleBtnBounty.onClick.RemoveListener(OnStartBattleBtnBounty);
            btnLimitTip.onClick.RemoveListener(showFireTip);
            btnSwitchLimit.OnClick -= OnSwitchLimit;

            dragableItemManager.MainBoardChangeAction -= CheckTeamTotalCombat;
            dragableItemManager.MainBoardOrBenchBoardChangeAction -= CheckTeamTotalCombat;
            dragableItemManager.MainBoardOrBenchBoardChangeAction -= CheckLimit;

            dragableItemManager.PackData();
            dragableItemManager.Clear();
        }
        #endregion

        #region 爆发
        /// <summary>
        /// 默认显示限制条件
        /// </summary>
        private bool isFireShow = false;
        private void OnSwitchLimit(BabuButton sender)
        {
            stateFire.gameObject.SetActive(isFireShow);
            stateLimit.gameObject.SetActive(!isFireShow);
            txtBtnSwitch.text = isFireShow ? "查看限制" : "查看爆发";
            isFireShow = !isFireShow;
        }

        private void SetSwitchLimit(bool fireShow)
        {
            stateFire.gameObject.SetActive(fireShow);
            stateLimit.gameObject.SetActive(!fireShow);
            txtBtnSwitch.text = fireShow ? "查看限制" : "查看爆发";
            isFireShow = !fireShow;
        }

        private void showFireTip()
        {
            UIController.Instance.OpenWindow<FormationFireUI>();
        }

        private void ShowFire()
        {
            _formation.Analysis();
            //设置星星点亮
            for (var index = 0; index < 5; index++)
            {
                powerStarList[index].gameObject.SetActive(_formation.fireSection.Value >= (index + 1));
                txtpowerStarList[index].text = _formation.fireSection.Key.ToString();
            }
            if (_formation.fireSection.Value > 0)
            {
                txtMemberUp.text = string.Format("第{0}节有{1}名球员爆发", _formation.fireSection.Key, _formation.fireSection.Value);
            }
            else
            {
                txtMemberUp.text = "无爆发，进阶球员可获得爆发能力";
            }
            txtTeamUp.text = string.Format("球队爆发：(爆发节全员能力+{0}%)", _formation.FireAddList[_formation.fireSection.Value]);
            txtMemberUp.color = CBAColorUtil.Instance.GetColor(_formation.fireSection.Value);
            txtTeamUp.color = CBAColorUtil.Instance.GetColor(_formation.fireSection.Value);
            dragableItemManager.SyncFireStarState(_formation.boardGiftSkillList, _formation.sectionGiftSkillList, _formation.fireSection.Key);
        }

        private KeyValuePair<int, int> getMaxKey(Dictionary<int, HashSet<int>> list)
        {
            int result = -1;
            int count = 0;
            foreach (var key in list.Keys)
            {
                int newcount = list[key].Count;
                if (newcount > count)
                {
                    result = key;
                    count = newcount;
                }
            }
            return new KeyValuePair<int, int>(result, count);
        }

        private void setBoardIdStar()
        {
            List<int> _boardLst = new() { 101, 102, 103, 201, 202 };

            foreach (var _boardId in dragableItemManager.MainPlayerCards.Keys)
            {
                //var starObj = dragableItemManager.mainContainer.find
            }
        }

        #endregion

        #region show

        private void OnStateA()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            dragableItemManager.ChangeState(0);
            stateABtn.gameObject.SetActive(false);
            stateBBtn.gameObject.SetActive(true);
        }

        private void OnStateB()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            dragableItemManager.ChangeState(1);
            stateABtn.gameObject.SetActive(true);
            stateBBtn.gameObject.SetActive(false);
        }

        private void HideAllAutoButton()
        {
            AutoChangeBtnClassic.gameObject.SetActive(false);
            AutoChangeBtnHero.gameObject.SetActive(false);
            StartBattleBtnHero.gameObject.SetActive(false);
            AutoChangeBtnBounty.gameObject.SetActive(false);
            StartBattleBtnBounty.gameObject.SetActive(false);
        }
        public void OnShowArena()
        {
            BackupBotttomText.text = "更换替补席球员";
            backupRoot.gameObject.SetActive(false);
            BenchRoot.gameObject.SetActive(true);
            dragableItemManager.SetBenchPanelIsShow(true);
            backupRoot.SetLocalPositionY(225.3726f);
            HideAllAutoButton();
            AutoChangeBtnClassic.gameObject.SetActive(true);
            SetLimitText(null);
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.ARENA, formation =>
            {
                OnShow(formation);
                ResetTeamTotalCombat();
            });
            SetSwitchLimit(true);
        }
        public void OnShowHero(FormationBase formation)
        {
            BackupBotttomText.text = "更换替补席球员";
            backupRoot.gameObject.SetActive(false);
            BenchRoot.gameObject.SetActive(true);
            dragableItemManager.SetBenchPanelIsShow(true);
            backupRoot.SetLocalPositionY(225.3726f);
            HideAllAutoButton();
            AutoChangeBtnHero.gameObject.SetActive(true);
            StartBattleBtnHero.gameObject.SetActive(true);
            SetLimitText(Player.BattleManager.heroClubData.challengeHeroConfig.OnBattle);
            if (!CheckHeroFormationNotNull(formation))
            {
                OnShow(formation);
                ResetTeamTotalCombat();
            }
            SetSwitchLimit(false);
        }
        public void OnShowTower(FormationBase formation)
        {
            BackupBotttomText.text = "更换替补席球员";
            backupRoot.gameObject.SetActive(false);
            BenchRoot.gameObject.SetActive(true);
            dragableItemManager.SetBenchPanelIsShow(true);
            backupRoot.SetLocalPositionY(225.3726f);
            HideAllAutoButton();
            AutoChangeBtnClassic.gameObject.SetActive(true);
            SetLimitText(null);
            OnShow(formation);
            ResetTeamTotalCombat();
            SetSwitchLimit(true);
        }
        public void OnShowPVEandPVP(FormationBase formation)
        {
            BackupBotttomText.text = "更换替补席球员";
            backupRoot.gameObject.SetActive(false);
            BenchRoot.gameObject.SetActive(true);
            dragableItemManager.SetBenchPanelIsShow(true);
            backupRoot.SetLocalPositionY(225.3726f);
            HideAllAutoButton();
            AutoChangeBtnClassic.gameObject.SetActive(true);
            SetLimitText(null);
            OnShow(formation);
            ResetTeamTotalCombat();
            SetSwitchLimit(true);
        }

        private Action<FormationBase, bool> afterFormationCallBack;
        private int[] limitIntArrBounty;
        public void OnShowBounty(FormationBase formation, int[] limitIntArr, Action<FormationBase, bool> afterFormationCallBack)
        {
            BackupBotttomText.text = "更换要派遣的球员";
            BenchRoot.gameObject.SetActive(false);
            dragableItemManager.SetBenchPanelIsShow(false);
            backupRoot.SetLocalPositionY(-359.86f);//225.3726
            backupRoot.gameObject.SetActive(true);
            dragableItemManager.RefreshNextBtnShow(BackupRightPageBtn, BackupLeftPageBtn);
            BackupBtn.gameObject.SetActive(false);
            BackupConfirmBtn.gameObject.SetActive(false);
            NoBackupBtn.gameObject.SetActive(false);
            formationBackupAnim.PlayEnter(null, true);

            BountyTaskManager.Instance.RefreshCardUseSet();
            _formation = formation;
            HideAllAutoButton();
            AutoChangeBtnBounty.gameObject.SetActive(true);
            StartBattleBtnBounty.gameObject.SetActive(true);
            this.afterFormationCallBack = afterFormationCallBack;
            this.limitIntArrBounty = limitIntArr;
            SetLimitText(this.limitIntArrBounty);
            OnAutoChangeEnterBounty();
            OnShow(formation, true);
            dragableItemManager.OnBackupWindowOpened(); //由于SpawnPool需要初始化，所以要先SetActive再Open
            ResetTeamTotalCombat();
            SetSwitchLimit(false);
        }
        public void OnShow(FormationBase formation, bool playAni = true)
        {
#if UNITY_EDITOR
            dragableItemManager.HideDebugPoses();
#endif

            TitleBench.SetActive(true);
            stateABtn.gameObject.SetActive(false);
            stateBBtn.gameObject.SetActive(true);
            dragableItemManager.ChangeState(0);
            _formation = formation;


            string textATK = "";
            string textDEF = "";
            DataConvUtil.TacticsIdList2AtkDef(_formation.TacticsIdList, ref textATK, ref textDEF);
            NameTextATK.text = textATK;
            NameTextDEF.text = textDEF;
            switch (_formation.FormationId)
            {
                case FormationID.HERO: dragableItemManager.SetData(formation, Player.BattleManager.heroClubData.challengeHeroConfig.OnBattle); break;
                case FormationID.Bounty: dragableItemManager.SetData(formation, limitIntArrBounty, true); break;
                default: dragableItemManager.SetData(formation); break;
            }

            if (_formation.FormationId != FormationID.Bounty)
            {
                if (dragableItemManager.IsBackupWindowOpening())
                {
                    OnCloseBackup();
                }
                formationBackupAnim.SetHideState();

                var backupCount = dragableItemManager.GetBackupCardCount();
                if (backupCount > 0)
                {
                    BackupBtn.gameObject.SetActive(true);
                    BackupConfirmBtn.gameObject.SetActive(false);
                    NoBackupBtn.gameObject.SetActive(false);
                }
                else
                {
                    BackupBtn.gameObject.SetActive(false);
                    BackupConfirmBtn.gameObject.SetActive(false);
                    NoBackupBtn.gameObject.SetActive(true);
                }
                backupRoot.gameObject.SetActive(false);
            }

            ShowMainCambat(false);

            ShowFire();

            CheckLimit();

            if (playAni)
            {
                AnimPlayEnter(formation, () =>
                {
                    //#if UNITY_EDITOR
                    //                    dragableItemManager.ShowDebugPoses();
                    //#endif
                });
            }
        }
        #endregion

        #region 替补按钮与翻页按钮

        private void OnOpenBackup()
        {
            backupRoot.gameObject.SetActive(true);
            dragableItemManager.OnBackupWindowOpened(); //由于SpawnPool需要初始化，所以要先SetActive再Open
            dragableItemManager.RefreshNextBtnShow(BackupRightPageBtn, BackupLeftPageBtn);
            BackupBtn.gameObject.SetActive(false);
            formationBackupAnim.PlayEnter(() =>
            {
                BackupConfirmBtn.gameObject.SetActive(true);
                BackupConfirmBtn.gameObject.DOFade(1, 0.3f).AddTo(this.gameObject);
            }, false);

        }

        private void OnCloseBackup()
        {
            dragableItemManager.OnBackupWindowClosed(true);

            BackupConfirmBtn.gameObject.SetActive(false);
            formationBackupAnim.PlayExit(() =>
            {
                backupRoot.gameObject.SetActive(false);
                BackupBtn.gameObject.SetActive(true);
                BackupBtn.gameObject.DOFade(1, 0.3f).AddTo(this.gameObject);
            });
        }

        public void BackupClose()
        {
            OnCloseBackup();
        }

        private void OnBackupLeftPage()
        {
            dragableItemManager.GetPreviousPage();
            dragableItemManager.RefreshNextBtnShow(BackupRightPageBtn, BackupLeftPageBtn);
        }

        private void OnBackupRightPage()
        {
            dragableItemManager.GetNextPage();
            dragableItemManager.RefreshNextBtnShow(BackupRightPageBtn, BackupLeftPageBtn);
        }

        private void OnNoBackup()
        {
            Tips.PopError(ErrorID.NoReserveCard);
        }

        public void CheckBackupClose()
        {
            if (dragableItemManager.IsBackupWindowOpening())
            {
                dragableItemManager.OnBackupWindowClosed(false);
                BackupBtn.gameObject.SetActive(true);
                BackupConfirmBtn.gameObject.SetActive(false);
                formationBackupAnim.PlayExit(() =>
                {
                    backupRoot.gameObject.SetActive(false);
                    BackupBtn.gameObject.DOFade(1, 0.3f).AddTo(this.gameObject);
                });
            }
        }
        #endregion

        #region 进入动画

        private float bottomLocalPosY = 0;
        private float soccerFieldLocalPosY = 0;
        private float formationNameATKLocalPosX = 0;
        private float formationNameDEFLocalPosX = 0;
        private float stateBBtnLocalPosX = 0;
        private bool isRememberPos = false;
        private void RememberAniPos()
        {
            if (isRememberPos) return;
            isRememberPos = true;
            bottomLocalPosY = bottom.localPosition.y;
            soccerFieldLocalPosY = soccerField.localPosition.y;
            formationNameATKLocalPosX = formationNameATK.GetComponent<RectTransform>().localPosition.x;
            formationNameDEFLocalPosX = formationNameDEF.GetComponent<RectTransform>().localPosition.x;
            stateBBtnLocalPosX = stateBBtn.GetComponent<RectTransform>().localPosition.x;
        }

        private List<Tween> aniTweenList = new();
        private void ClearAniTween()
        {
            for (int i = 0; i < aniTweenList.Count; i++)
            {
                aniTweenList[i]?.Kill();
            }
            aniTweenList.Clear();
        }

        private void PrepareAni()
        {
            bottom.SetLocalPositionY(bottomLocalPosY - 160);
            soccerField.SetLocalPositionY(soccerFieldLocalPosY - 120);
            formationNameATK.GetComponent<RectTransform>().SetLocalPositionX(formationNameATKLocalPosX - 400);
            formationNameDEF.GetComponent<RectTransform>().SetLocalPositionX(formationNameDEFLocalPosX - 400);
            stateBBtn.GetComponent<RectTransform>().SetLocalPositionX(stateBBtnLocalPosX + 185);
        }

        //进入动画
        private void AnimPlayEnter(FormationBase formationToSet, Action onCompleteCallBack)
        {
            RememberAniPos();
            ClearAniTween();
            PrepareAni();
            bottom.DOLocalMoveY(bottomLocalPosY, _moveInDuration).AddTo(this.gameObject);
            bottom.gameObject.DOFade(0, 0).OnComplete(() =>
            {
                bottom.gameObject.DOFade(1, 0.3f).AddTo(this.gameObject);
            }).AddTo(this.gameObject);
            soccerField.gameObject.DOFade(0, 0).OnComplete(() =>
            {
                soccerField.gameObject.DOFade(1.0f, 0.2f).SetDelay(0.1f).AddTo(this.gameObject);
            }).AddTo(this.gameObject);//注意，这里的OnComplete是必须的，直接再下面写相同的代码会没有duration
            soccerField.DOLocalMoveY(soccerFieldLocalPosY, 0.3f).SetDelay(0.1f).OnComplete(() =>
            {
                onCompleteCallBack?.Invoke();
            }).AddTo(this.gameObject);
            formationNameATK.GetComponent<RectTransform>().DOLocalMoveX(formationNameATKLocalPosX, _moveInDuration).SetDelay(2 * _moveInDuration).AddTo(this.gameObject);
            formationNameDEF.GetComponent<RectTransform>().DOLocalMoveX(formationNameDEFLocalPosX, _moveInDuration).SetDelay(2 * _moveInDuration).AddTo(this.gameObject);
            stateBBtn.GetComponent<RectTransform>().DOLocalMoveX(stateBBtnLocalPosX, _moveInDuration).SetDelay(2 * _moveInDuration).AddTo(this.gameObject);
        }

        public void Play(Action callback)
        {
            callback?.Invoke();
        }

        #endregion

        #region 经典赛自动换人

        private void OnAutoChangeClassicBtn()
        {
            OnAutoChangeClassic();
        }
        /*
            一键换人规则：优先顺序
            1.  位置对应优先，完全一致优先，没有位置完全一致按大类一致优先（使用配置表里的擅长位置）
            [大前锋，小前锋] 
            [控球后卫， 得分后卫]
            [中锋]
            //2. 颜色品质高优先
            //3. 星级优先
            4. 能力优先
            5. 上述都没有，按能力排序选一个
            6. 选7人进入大名单：先选5个按照 [ 1 - 5 ]步骤，最后2个按照能力排序选2个
         */
        private bool OnAutoChangeClassic()//按规则自动上阵
        {
            Tuple<Dictionary<int, int>, Dictionary<int, int>> StartAndSubstituteDic = Player.FightManager.FormationController.GetStartAndSubstituteDic(_formation);
            bool isSave = CheckAndSaveFormation(StartAndSubstituteDic);
            CheckLimit();
            return isSave;
        }
        private bool CheckAndSaveFormation(Tuple<Dictionary<int, int>, Dictionary<int, int>> StartAndSubstituteDic)
        {
            if (StartAndSubstituteDic == null)
            {
                Tips.PopTips("无法达成上阵条件");
                return false;
            }

            if (Player.FightManager.FormationController.IsFormationSame(_formation, StartAndSubstituteDic))
            {
                Tips.PopTips("已为最佳阵容");
                return false;
            }

            AudioManager.Instance.PlaySound(AudioNames.BTN_CFM);

            _formation.StarterBoardCardDic = StartAndSubstituteDic.Item1;
            _formation.SubstituteBoardCardDic = StartAndSubstituteDic.Item2;

            dragableItemManager.Clear();
            OnShow(_formation);
            CheckTeamTotalCombat();

            _formation.SetChangeFlag(true);
            _formation.SaveToServer();

            return true;
        }

        public void SaveToServer()
        {
            if (_formation != null)
                _formation.SaveToServer();
        }

        #endregion

        #region 首发战斗力Tip

        private int savedMainTotalCombat = 0;
        public void ResetTeamTotalCombat()
        {
            savedMainTotalCombat = _formation.GetMainTotalCombat();
            ShowMainCambat(false);
        }
        public void CheckTeamTotalCombat()
        {
            if (_formation.FormationId == FormationID.Bounty)
            {
                ResetTeamTotalCombat();
                return;
            }
            int newMainTotalCombat = _formation.GetMainTotalCombat();
            string tipsText = "";
            if (newMainTotalCombat > savedMainTotalCombat)
            {
                tipsText = "球队战力 <color=#13b237>+{0}</color>".SafeFormat(newMainTotalCombat - savedMainTotalCombat);
                savedMainTotalCombat = newMainTotalCombat;
                ShowMainCambat();
            }
            else if (newMainTotalCombat < savedMainTotalCombat)
            {
                tipsText = "球队战力 <color=#e12c2c>-{0}</color>".SafeFormat(savedMainTotalCombat - newMainTotalCombat);
                savedMainTotalCombat = newMainTotalCombat;
                ShowMainCambat();
            }
            //重新算爆发，并且在各个节点上放爆发星星
            ShowFire();
            if (_formation.oldfireSection.Key != _formation.fireSection.Key || _formation.oldfireSection.Value != _formation.fireSection.Value)
            {
                if (_formation.fireSection.Value == 0)
                {
                    tipsText += "|" + string.Format("无球员爆发");
                }
                else
                {
                    tipsText += "|" + string.Format("第{0}节有{1}名球员爆发", _formation.fireSection.Key, _formation.fireSection.Value);
                    tipsText += "|" + string.Format("球队爆发预测：(爆发球员全能力+{0}%)", 5);
                }
                _formation.oldfireSection = _formation.fireSection;
            }
            if (string.IsNullOrWhiteSpace(tipsText) == false)
            {
                Tips.PopTips(tipsText);
            }
        }

        #endregion

        #region 战斗力滚动动画

        private Tween mainCambatTween;
        private void clearCombatTween()
        {
            mainCambatTween?.Kill();
        }

        [SerializeField] private TMP_Text combatNumText;
        private void ShowMainCambat(bool needAni = true)
        {
            if (needAni == false)
            {
                clearCombatTween();
                combatNumText.text = _formation.GetMainTotalCombat().ToString("###,###");
            }
            else
            {
                clearCombatTween();
                int num = 0;
                int.TryParse(combatNumText.text.Replace(",", ""), out num);
                combatNumText.DOChangeNumberEx(_formation.GetMainTotalCombat(), 1.0f, 1.2f, num, "###,###").AddTo(this.gameObject);
            }
        }

        #endregion

        #region 上场条件限制

        [SerializeField] private GameObject noLimitItem;
        [SerializeField] private GameObject limitLayout;
        [SerializeField] private List<GameObject> limitItemList;
        [SerializeField] private List<TMP_Text> limitDescTextList;
        [SerializeField] private List<Image> limitCheckMarkList;

        private void SetLimitText(int[] limitIntArr)
        {
            if (limitIntArr == null || limitIntArr.Length <= 0)
            {
                noLimitItem.SetActive(true);
                limitLayout.SetActive(false);
            }
            else
            {
                noLimitItem.SetActive(false);
                limitLayout.SetActive(true);

                List<ChallengeRuleConfig> challengeRuleConfigList = new();
                foreach (int limitInt in limitIntArr)
                {
                    ChallengeRuleConfig challengeRuleConfig = Configs.ChallengeRule.GetConfig(limitInt);
                    if (challengeRuleConfig == null)
                    {
                        Debug.LogWarningFormat("FormationPad , SetLimitText , challengeRuleConfig == null , limitInt = {0}", limitInt);
                        continue;
                    }
                    challengeRuleConfigList.Add(challengeRuleConfig);
                }
                for (int i = 0; i < 2; i++)
                {
                    GameObject limitItem = limitItemList[i];
                    if (i < challengeRuleConfigList.Count)
                    {
                        limitItem.SetActive(true);
                        TMP_Text limitDescText = limitDescTextList[i];
                        ChallengeRuleConfig challengeRuleConfig = challengeRuleConfigList[i];
                        limitDescText.text = challengeRuleConfig.Desc;
                    }
                    else
                    {
                        limitItem.SetActive(false);
                    }
                }
            }
        }
        bool isLimitAllPass = true;
        private void CheckLimit()
        {
            bool isPassAll = true;
            switch (_formation.FormationId)
            {
                case FormationID.HERO: isPassAll = CheckLimit(Player.BattleManager.heroClubData.challengeHeroConfig.OnBattle); break;
                case FormationID.Bounty: isPassAll = CheckLimit(this.limitIntArrBounty); break;
                default: isPassAll = CheckLimit(null); break;
            }
            isLimitAllPass = isPassAll;
        }
        private ChallengeRuleConfig unPassRule = null;
        private bool CheckLimit(int[] limitIntArr)
        {
            if (limitIntArr == null || limitIntArr.Length <= 0) return true;

            List<ChallengeRuleConfig> challengeRuleConfigList = new();
            foreach (int limitInt in limitIntArr)
            {
                ChallengeRuleConfig challengeRuleConfig = Configs.ChallengeRule.GetConfig(limitInt);
                if (challengeRuleConfig == null)
                {
                    Debug.LogWarningFormat("FormationPad , CheckLimit , challengeRuleConfig == null , limitInt = {0}", limitInt);
                    continue;
                }
                challengeRuleConfigList.Add(challengeRuleConfig);
            }
            unPassRule = null;
            bool isPassAll = true;
            for (int i = 0; i < 2 && i < challengeRuleConfigList.Count; i++)
            {
                Image limitCheckMark = limitCheckMarkList[i];
                ChallengeRuleConfig challengeRuleConfig = challengeRuleConfigList[i];
                bool isPass = CheckLimitOne(challengeRuleConfig);
                if (!isPass) isPassAll = false;
                if (unPassRule == null && !isPass) unPassRule = challengeRuleConfig;
                bool oldActive = limitCheckMark.gameObject.activeSelf;
                limitCheckMark.gameObject.SetActive(isPass);
                if (isPass && !oldActive)
                {
                    Sequence seq = DOTween.Sequence();
                    seq.Append(limitCheckMark.transform.DOScale(1.5f, 0.2f));
                    seq.Append(limitCheckMark.transform.DOScale(1.0f, 0.6f));
                    seq.AddTo(this.gameObject);
                }
            }
            return isPassAll;
        }
        private bool CheckLimitOne(ChallengeRuleConfig challengeRuleConfig)
        {
            if (challengeRuleConfig.Action != "include") return false;

            List<PlayerCard> playerCardList = new();
            foreach (var item in _formation.StarterBoardCardDic)
            {
                PlayerCard playerCard = Player.CardManager.GetCard(item.Value);
                if (playerCard != null) playerCardList.Add(playerCard);
            }
            foreach (var item in _formation.SubstituteBoardCardDic)
            {
                PlayerCard playerCard = Player.CardManager.GetCard(item.Value);
                if (playerCard != null) playerCardList.Add(playerCard);
            }

            switch (challengeRuleConfig.Key)
            {
                case "count"://3人出场
                    {
                        //bool isEnough = Utils.ConfigUtil.CompareByStr(playerCardList.Count, challengeRuleConfig.Judge, challengeRuleConfig.Value);
                        //return isEnough;

                        //限制出场人数会导致很大的改动，此条件暂时不会用到
                        return true;
                    }
                case "pos"://控球后卫至少3人
                    {
                        PositionSeparatedType positionSeparatedType = (PositionSeparatedType)challengeRuleConfig.KeyValue;
                        playerCardList = playerCardList.Where((pc) =>
                        {
                            return pc.GetAdaptPosition() == positionSeparatedType;
                        }).ToList();
                        bool isEnough = Utils.ConfigUtil.CompareByStr(playerCardList.Count, challengeRuleConfig.Judge, challengeRuleConfig.Value);
                        return isEnough;
                    }
                case "quality"://紫色球员至少3人
                    {
                        playerCardList = playerCardList.Where((pc) =>
                        {
                            return pc.Quality >= challengeRuleConfig.KeyValue;
                        }).ToList();
                        bool isEnough = Utils.ConfigUtil.CompareByStr(playerCardList.Count, challengeRuleConfig.Judge, challengeRuleConfig.Value);
                        return isEnough;
                    }
                case "player"://易建联必须上场
                    {
                        playerCardList = playerCardList.Where((pc) =>
                        {
                            return pc.Config.Id == challengeRuleConfig.KeyValue;
                        }).ToList();
                        bool isEnough = Utils.ConfigUtil.CompareByStr(playerCardList.Count, challengeRuleConfig.Judge, challengeRuleConfig.Value);
                        return isEnough;
                    }
                default: return false;
            }
        }

        #endregion

        #region 剧情推图自动换人

        private bool CheckHeroFormationNotNull(FormationBase formation)
        {
            _formation = formation;
            if (_formation == null)
            {
                Debug.LogWarning("FormationPad , CheckHeroFormationNotNull , _formation == null");
                return false;
            }
            if (_formation.StarterBoardCardDic.Count < 5/* || _formation.SubstituteBoardCardDic.Count < 7*/)
            {
                OnAutoChangeBtnHero();
                return true;
            }
            int playerCount = 0;
            foreach (var item in _formation.StarterBoardCardDic)
            {
                PlayerCard playerCard = Player.CardManager.GetCard(item.Value);
                if (playerCard == null)
                {
                    OnAutoChangeBtnHero();
                    return true;
                }
                playerCount++;
            }
            foreach (var item in _formation.SubstituteBoardCardDic)
            {
                if (item.Value == 0) continue;
                PlayerCard playerCard = Player.CardManager.GetCard(item.Value);
                if (playerCard == null)
                {
                    OnAutoChangeBtnHero();
                    return true;
                }
                playerCount++;
            }
            if (playerCount < 12 && playerCount < Player.CardManager.CardList.Count)
            {
                OnAutoChangeBtnHero();
                return true;
            }
            return false;
        }

        private void OnAutoChangeBtnHero()
        {
            switch (_formation.FormationId)
            {
                case FormationID.HERO: OnAutoChangeBtnHero(Player.BattleManager.heroClubData.challengeHeroConfig.OnBattle); break;
                default: OnAutoChangeBtnHero(null); break;
            }

        }
        private void OnAutoChangeBtnHero(int[] limitIntArr)
        {
            if (limitIntArr == null || limitIntArr.Length <= 0)
            {
                OnAutoChangeClassic();
                return;
            }

            Tuple<Dictionary<int, int>, Dictionary<int, int>> StartAndSubstituteDic = GetStartAndSubstituteDic(limitIntArr);

            bool isSave = CheckAndSaveFormation(StartAndSubstituteDic);
            if (StartAndSubstituteDic == null)
            {
                isSave = OnAutoChangeClassic();
            }
            if (!isSave)
            {
                dragableItemManager.Clear();
                OnShow(_formation);
                ResetTeamTotalCombat();
            }

            CheckLimit();
        }

        private Tuple<Dictionary<int, int>, Dictionary<int, int>> GetStartAndSubstituteDic(int[] limitIntArr)//获取首发与替补阵容
        {
            //提前筛选出一定要使用的和一定不使用的

            List<PlayerCard> playerCardListAll = new();//所有卡牌
            playerCardListAll.AddRange(Player.CardManager.CardList);
            playerCardListAll = playerCardListAll
                .OrderByDescending(card => card.FightPoint)
                .ThenBy(card => card.CardId)
                .ToList();

            HashSet<int> mustUseSet = new();
            HashSet<int> mustFirstUseSet = new();
            HashSet<int> mustNoUseSet = new();
            List<ChallengeRuleConfig> challengeRuleConfigList = new();
            foreach (int limitInt in limitIntArr)
            {
                ChallengeRuleConfig challengeRuleConfig = Configs.ChallengeRule.GetConfig(limitInt);
                if (challengeRuleConfig == null)
                {
                    Debug.LogWarningFormat("FormationPad , CheckLimit , challengeRuleConfig == null , limitInt = {0}", limitInt);
                    continue;
                }
                challengeRuleConfigList.Add(challengeRuleConfig);
            }
            bool isCanAuto = true;
            foreach (ChallengeRuleConfig challengeRuleConfig in challengeRuleConfigList)
            {
                if (isCanAuto == false) break;
                switch (challengeRuleConfig.Key)
                {
                    case "count"://3人出场
                        {
                            //限制出场人数会导致很大的改动，此条件暂时不会用到
                        }
                        break;
                    case "pos"://控球后卫至少3人
                        {
                            PositionSeparatedType positionSeparatedType = (PositionSeparatedType)challengeRuleConfig.KeyValue;
                            List<PlayerCard> playerCardList = playerCardListAll.Where((pc) =>
                            {
                                return pc.GetAdaptPosition() == positionSeparatedType;
                            })
                            .OrderByDescending(card => card.FightPoint)
                            .ThenBy(card => card.CardId)
                            .ToList();
                            if (challengeRuleConfig.Judge == "=" || challengeRuleConfig.Judge == ">=" || challengeRuleConfig.Judge == ">")
                            {
                                int value = 0;
                                switch (challengeRuleConfig.Judge)
                                {
                                    case "=": value = challengeRuleConfig.Value; break;
                                    case ">=": value = challengeRuleConfig.Value; break;
                                    case ">": value = challengeRuleConfig.Value + 1; break;
                                }
                                if (playerCardList.Count < value) { isCanAuto = false; break; }
                                for (int i = 0; i < playerCardList.Count; i++)
                                {
                                    if (i < value)
                                    {
                                        if (mustUseSet.Contains(playerCardList[i].CardId) == false) mustUseSet.Add(playerCardList[i].CardId);
                                    }
                                    else
                                    {
                                        if (challengeRuleConfig.Judge == "=")
                                            if (mustNoUseSet.Contains(playerCardList[i].CardId) == false) mustNoUseSet.Add(playerCardList[i].CardId);
                                    }
                                }
                            }
                            if (challengeRuleConfig.Judge == "<" || challengeRuleConfig.Judge == "<=")
                            {
                                int value = 0;
                                switch (challengeRuleConfig.Judge)
                                {
                                    case "<": value = challengeRuleConfig.Value - 1; break;
                                    case "<=": value = challengeRuleConfig.Value; break;
                                }
                                for (int i = 0; i < playerCardList.Count; i++)
                                {
                                    if (i >= value)
                                    {
                                        if (mustNoUseSet.Contains(playerCardList[i].CardId) == false) mustNoUseSet.Add(playerCardList[i].CardId);
                                    }
                                }
                            }
                        }
                        break;
                    case "quality"://紫色球员至少3人
                        {
                            PositionSeparatedType positionSeparatedType = (PositionSeparatedType)challengeRuleConfig.KeyValue;
                            List<PlayerCard> playerCardList = playerCardListAll.Where((pc) =>
                            {
                                return pc.Quality >= challengeRuleConfig.KeyValue;
                            })
                            .OrderByDescending(card => card.FightPoint)
                            .ThenBy(card => card.CardId)
                            .ToList();
                            if (challengeRuleConfig.Judge == "=" || challengeRuleConfig.Judge == ">=" || challengeRuleConfig.Judge == ">")
                            {
                                int value = 0;
                                switch (challengeRuleConfig.Judge)
                                {
                                    case "=": value = challengeRuleConfig.Value; break;
                                    case ">=": value = challengeRuleConfig.Value; break;
                                    case ">": value = challengeRuleConfig.Value + 1; break;
                                }
                                if (playerCardList.Count < value) { isCanAuto = false; break; }
                                for (int i = 0; i < playerCardList.Count; i++)
                                {
                                    if (i < value)
                                    {
                                        if (mustUseSet.Contains(playerCardList[i].CardId) == false) mustUseSet.Add(playerCardList[i].CardId);
                                    }
                                    else
                                    {
                                        if (challengeRuleConfig.Judge == "=")
                                            if (mustNoUseSet.Contains(playerCardList[i].CardId) == false) mustNoUseSet.Add(playerCardList[i].CardId);
                                    }
                                }
                            }
                        }
                        break;
                    case "player"://易建联必须上场
                        {
                            if (challengeRuleConfig.Judge == "=" && challengeRuleConfig.Value == 1 ||
                                challengeRuleConfig.Judge == ">=" && challengeRuleConfig.Value == 1 ||
                                challengeRuleConfig.Judge == ">" && challengeRuleConfig.Value == 0)
                            {
                                PlayerCard playerCard = Player.CardManager.GetCard(challengeRuleConfig.KeyValue);
                                if (playerCard == null) { isCanAuto = false; break; }
                                if (mustFirstUseSet.Contains(playerCard.CardId) == false) mustFirstUseSet.Add(playerCard.CardId);
                            }
                            else
                            {
                                mustNoUseSet.Add(challengeRuleConfig.KeyValue);
                                if (mustNoUseSet.Contains(challengeRuleConfig.KeyValue) == false) mustNoUseSet.Add(challengeRuleConfig.KeyValue);
                            }

                        }
                        break;
                    default: break;
                }
            }
            if (isCanAuto == false)
            {
                Debug.Log("isCanAuto == false , 1");
                return null;
            }

            //筛选首发5个

            HashSet<PlayerCard> mustUsePlayerSet = new();//必须上场
            foreach (var item in mustUseSet)
            {
                mustUsePlayerSet.Add(Player.CardManager.GetCard(item));
            }
            HashSet<PlayerCard> mustFirstUsePlayerSet = new();
            foreach (var item in mustFirstUseSet)//必须在首发
            {
                mustFirstUsePlayerSet.Add(Player.CardManager.GetCard(item));
            }
            HashSet<PlayerCard> usedPlayerSet = new();//不可使用
            foreach (int cardId in mustNoUseSet)
            {
                PlayerCard playerCard = Player.CardManager.GetCard(cardId);
                if (playerCard != null) usedPlayerSet.Add(playerCard);
            }
            Dictionary<int, int> starterBoardCardDic = Player.FightManager.FormationController.Get5PosPlayerCardDic(_formation, playerCardListAll, usedPlayerSet, mustUsePlayerSet, mustFirstUsePlayerSet);
            foreach (var item in starterBoardCardDic)
            {
                if (item.Value == 0)
                {
                    Debug.Log("isCanAuto == false , 2");
                    return null;
                }
            }

            //筛选替补5个
            Dictionary<int, int> substituteBoardCardDic = new();
            Dictionary<int, int> substituteBoardCardDicTemp = Player.FightManager.FormationController.Get5PosPlayerCardDic(_formation, playerCardListAll, usedPlayerSet, mustUsePlayerSet, mustFirstUsePlayerSet);

            //筛选替补2个(替补一共需要7个)
            int addCount = -2;
            foreach (PlayerCard playerCard in playerCardListAll)
            {
                if (usedPlayerSet.Contains(playerCard) == true) continue;
                substituteBoardCardDicTemp.Add(addCount, playerCard.Config.Id);
                usedPlayerSet.Add(playerCard);
                addCount++;
                if (addCount == 0) break;
            }
            int substituteBoardCardPosIndex = 1;
            foreach (var substituteBoardCardPair in substituteBoardCardDicTemp)
            {
                if (substituteBoardCardPair.Value == 0) continue;
                substituteBoardCardDic.Add(substituteBoardCardPosIndex, substituteBoardCardPair.Value);
                substituteBoardCardPosIndex++;
            }
            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (substituteBoardCardDic.ContainsKey(i) == false)
                {
                    substituteBoardCardDic.Add(i, 0);
                }
            }

            if (mustUsePlayerSet.Count > 0 || mustFirstUsePlayerSet.Count > 0)
            {
                Debug.Log("isCanAuto == false , 3");
                return null;
            }

            Tuple<Dictionary<int, int>, Dictionary<int, int>> tuple = new(starterBoardCardDic, substituteBoardCardDic);
            return tuple;
        }

        #endregion

        #region 剧情推图进入战斗

        private void OnStartBattleBtnHero()
        {
            if (isLimitAllPass == false)
            {
                Tips.PopTips(unPassRule.Desc);
                return;
            }

            NetworkManager.Instance.ChallengeStartHero(Player.BattleManager.heroClubData.challengeHeroConfig.Id, _formation.Pack(), (resp) =>
            {
                Player.BattleManager.challengeStartHeroResponse = resp;

                if (resp.Succeed)
                {
                    HeroManager.Instance.UpdatePassData(Player.BattleManager.heroClubData.challengeHeroConfig.Chapter, resp);
                    HeroManager.Instance.CheckRedDot();
                }

                CbaLogManager.Instance.AddLog(1004, Player.BattleManager.heroClubData.challengeHeroConfig.Id, resp.Stars.Count > 0 && resp.Stars[0] > 0 ? 1 : 0, resp.Stars.Sum());
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.HeroUI;
                Player.BattleManager.SetFightInfo(FightType.Hero, resp.Fight);
                UIController.Instance.HidePanel<FormationUI>(true);
                Player.BattleManager.StartPlayFight();
            });
        }

        #endregion

        #region 悬赏任务

        private void OnAutoChangeBtnBounty()
        {
            if (limitIntArrBounty == null)
                limitIntArrBounty = new int[0];

            BountyTaskManager.Instance.RefreshCardUseSet();
            Dictionary<int, int> mainDic = GetStartDicBounty(limitIntArrBounty);

            if (mainDic == null)
            {
                Debug.LogError("OnAutoChangeBtnBounty , mainDic == null");
                Tips.PopTips("没有发现更佳阵容");
            }
            if (Player.FightManager.FormationController.IsDicSame(mainDic, _formation.StarterBoardCardDic) == true)
            {
                Tips.PopTips("已为最佳阵容");
                return;
            }

            _formation.StarterBoardCardDic = mainDic;
            dragableItemManager.Clear();
            dragableItemManager.OnBackupWindowOpened(); //由于SpawnPool需要初始化，所以要先SetActive再Open
            OnShow(_formation);
            CheckTeamTotalCombat();
            CheckLimit();
            //formationBackupAnim.PlayEnter(null, true);
        }

        private void OnAutoChangeEnterBounty()
        {
            if (limitIntArrBounty == null)
                limitIntArrBounty = new int[0];

            BountyTaskManager.Instance.RefreshCardUseSet();
            Dictionary<int, int> mainDic = GetStartDicBounty(limitIntArrBounty);

            if (mainDic == null)
            {
                Debug.LogError("OnAutoChangeEnterBounty , mainDic == null");
                Tips.PopTips("没有发现更佳阵容");
            }

            _formation.StarterBoardCardDic = mainDic;
        }

        private Dictionary<int, int> GetStartDicBounty(int[] limitIntArr)//获取首发阵容
        {
            //提前筛选出一定要使用的和一定不使用的

            List<PlayerCard> playerCardListAll = new();//所有卡牌
            playerCardListAll.AddRange(Player.CardManager.CardList);
            playerCardListAll = playerCardListAll
                .OrderBy(card => card.IsUsingInBounty)
                .ThenBy(card => card.Quality)
                .ThenBy(card => card.Star)
                .ThenBy(card => card.FightPoint)
                .ThenBy(card => card.CardId)
                .ToList();

            HashSet<int> mustUseSet = new();
            HashSet<int> mustFirstUseSet = new();
            HashSet<int> mustNoUseSet = new();
            List<ChallengeRuleConfig> challengeRuleConfigList = new();
            foreach (int limitInt in limitIntArr)
            {
                ChallengeRuleConfig challengeRuleConfig = Configs.ChallengeRule.GetConfig(limitInt);
                if (challengeRuleConfig == null)
                {
                    Debug.LogWarningFormat("FormationPad , CheckLimit , challengeRuleConfig == null , limitInt = {0}", limitInt);
                    continue;
                }
                challengeRuleConfigList.Add(challengeRuleConfig);
            }
            foreach (ChallengeRuleConfig challengeRuleConfig in challengeRuleConfigList)
            {
                switch (challengeRuleConfig.Key)
                {
                    case "count"://3人出场
                        {
                            //限制出场人数会导致很大的改动，此条件暂时不会用到
                        }
                        break;
                    case "pos"://控球后卫至少3人
                        {
                            PositionSeparatedType positionSeparatedType = (PositionSeparatedType)challengeRuleConfig.KeyValue;
                            List<PlayerCard> playerCardList = playerCardListAll.Where((pc) =>
                            {
                                return pc.GetAdaptPosition() == positionSeparatedType;
                            })
                            .OrderBy(card => card.IsUsingInBounty)
                            .ThenBy(card => card.Quality)
                            .ThenBy(card => card.Star)
                            .ThenBy(card => card.FightPoint)
                            .ThenBy(card => card.CardId)
                            .ToList();
                            if (challengeRuleConfig.Judge == "=" || challengeRuleConfig.Judge == ">=" || challengeRuleConfig.Judge == ">")
                            {
                                int value = 0;
                                switch (challengeRuleConfig.Judge)
                                {
                                    case "=": value = challengeRuleConfig.Value; break;
                                    case ">=": value = challengeRuleConfig.Value; break;
                                    case ">": value = challengeRuleConfig.Value + 1; break;
                                }
                                for (int i = 0; i < playerCardList.Count; i++)
                                {
                                    if (i < value)
                                    {
                                        if (mustUseSet.Contains(playerCardList[i].CardId) == false) mustUseSet.Add(playerCardList[i].CardId);
                                    }
                                    else
                                    {
                                        if (challengeRuleConfig.Judge == "=")
                                            if (mustNoUseSet.Contains(playerCardList[i].CardId) == false) mustNoUseSet.Add(playerCardList[i].CardId);
                                    }
                                }
                            }
                            if (challengeRuleConfig.Judge == "<" || challengeRuleConfig.Judge == "<=")
                            {
                                int value = 0;
                                switch (challengeRuleConfig.Judge)
                                {
                                    case "<": value = challengeRuleConfig.Value - 1; break;
                                    case "<=": value = challengeRuleConfig.Value; break;
                                }
                                for (int i = 0; i < playerCardList.Count; i++)
                                {
                                    if (i >= value)
                                    {
                                        if (mustNoUseSet.Contains(playerCardList[i].CardId) == false) mustNoUseSet.Add(playerCardList[i].CardId);
                                    }
                                }
                            }
                        }
                        break;
                    case "quality"://紫色球员至少3人
                        {
                            PositionSeparatedType positionSeparatedType = (PositionSeparatedType)challengeRuleConfig.KeyValue;
                            List<PlayerCard> playerCardList = playerCardListAll.Where((pc) =>
                            {
                                return pc.Quality >= challengeRuleConfig.KeyValue;
                            })
                            .OrderBy(card => card.IsUsingInBounty)
                            .ThenBy(card => card.Quality)
                            .ThenBy(card => card.Star)
                            .ThenBy(card => card.FightPoint)
                            .ThenBy(card => card.CardId)
                            .ToList();
                            if (challengeRuleConfig.Judge == "=" || challengeRuleConfig.Judge == ">=" || challengeRuleConfig.Judge == ">")
                            {
                                int value = 0;
                                switch (challengeRuleConfig.Judge)
                                {
                                    case "=": value = challengeRuleConfig.Value; break;
                                    case ">=": value = challengeRuleConfig.Value; break;
                                    case ">": value = challengeRuleConfig.Value + 1; break;
                                }
                                for (int i = 0; i < playerCardList.Count; i++)
                                {
                                    if (i < value)
                                    {
                                        if (mustUseSet.Contains(playerCardList[i].CardId) == false) mustUseSet.Add(playerCardList[i].CardId);
                                    }
                                    else
                                    {
                                        if (challengeRuleConfig.Judge == "=")
                                            if (mustNoUseSet.Contains(playerCardList[i].CardId) == false) mustNoUseSet.Add(playerCardList[i].CardId);
                                    }
                                }
                            }
                        }
                        break;
                    case "player"://易建联必须上场
                        {
                            if (challengeRuleConfig.Judge == "=" && challengeRuleConfig.Value == 1 ||
                                challengeRuleConfig.Judge == ">=" && challengeRuleConfig.Value == 1 ||
                                challengeRuleConfig.Judge == ">" && challengeRuleConfig.Value == 0)
                            {
                                PlayerCard playerCard = Player.CardManager.GetCard(challengeRuleConfig.KeyValue);
                                if (playerCard == null) continue;
                                if (mustFirstUseSet.Contains(playerCard.CardId) == false) mustFirstUseSet.Add(playerCard.CardId);
                            }
                            else
                            {
                                mustNoUseSet.Add(challengeRuleConfig.KeyValue);
                                if (mustNoUseSet.Contains(challengeRuleConfig.KeyValue) == false) mustNoUseSet.Add(challengeRuleConfig.KeyValue);
                            }

                        }
                        break;
                    default: break;
                }
            }

            //筛选首发5个

            HashSet<PlayerCard> mustUsePlayerSet = new();//必须上场
            foreach (var item in mustUseSet)
            {
                mustUsePlayerSet.Add(Player.CardManager.GetCard(item));
            }
            HashSet<PlayerCard> mustFirstUsePlayerSet = new();
            foreach (var item in mustFirstUseSet)//必须在首发
            {
                mustFirstUsePlayerSet.Add(Player.CardManager.GetCard(item));
            }
            HashSet<PlayerCard> usedPlayerSet = new();//不可使用
            foreach (int cardId in mustNoUseSet)
            {
                PlayerCard playerCard = Player.CardManager.GetCard(cardId);
                if (playerCard != null) usedPlayerSet.Add(playerCard);
            }
            Dictionary<int, int> starterBoardCardDic = Player.FightManager.FormationController.Get5PosPlayerCardDic(_formation, playerCardListAll, usedPlayerSet, mustUsePlayerSet, mustFirstUsePlayerSet);
            foreach (var item in starterBoardCardDic)
            {
                if (item.Value == 0)
                {
                    Debug.Log("starterBoardCardDic not Full , isCanAuto == false");
                    return null;
                }
            }

            return starterBoardCardDic;
        }

        private void OnStartBattleBtnBounty()
        {
            bool hasUsingPlayer = false;
            foreach (int cardId in _formation.StarterBoardCardDic.Values)
            {
                if (BountyTaskManager.Instance.IsPlayerCardUsing(cardId))
                {
                    hasUsingPlayer = true;
                    break;
                }
            }
            if (hasUsingPlayer)
            {
                Tips.PopTips("不可重复派遣相同英雄");
                return;
            }

            afterFormationCallBack?.Invoke(_formation, isLimitAllPass);
            afterFormationCallBack = null;
            UIController.Instance.HidePanel<FormationUI>();
        }

        #endregion


    }
}
