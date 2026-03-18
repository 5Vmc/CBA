
using Babu;
using BigBang.Animation;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class SkillTrainRoomItem : MonoBehaviour
    {
        [SerializeField] private int roomId;
        [SerializeField] private GameObject trainingCanvas;
        [SerializeField] private SkillTrainRoomCardItem cardItem;
        [SerializeField] private SkillTrainRoomSkillItem skillItem;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text cdTimeText;
        [SerializeField] private TMP_Text clearCdDiamondText;
        [SerializeField] private TMP_Text unlockCostText;
        [SerializeField] private GameObject noTrainingCanvas;
        [SerializeField] private GameObject lockCanvas;
        [SerializeField] private GameObject lockImages;
        [SerializeField] private Button clearCdBtn;
        [SerializeField] private Button unlockBtn;
        [SerializeField] private Button selectToTrainBtn;
        [SerializeField] private Image progressImage;

        [SerializeField] public SkillTrainRoomItemAnim Anim;
        private SkillTrainRoom _room;
        private SkillTrainRoomState lastState;
        private long lastCD;
        private bool updateState = true;
        private bool updateProgress = true;
        private Timer updateTimer;

        public static float MinShowProgress = 0.09f;
        public static float MaxShowProgress = 0.91f;

        public event Action OnLockChanged;

        /// <summary>
        /// 获得显示进度，用来赋值给进度条图片的fillAmount字段
        /// </summary>
        /// <param name="progress">取值范围0-1</param>
        /// <returns>显示进度</returns>
        public static float GetShowProgress(float progress)
        {
            return (MaxShowProgress - MinShowProgress) * progress + MinShowProgress;
        }

        private void Awake()
        {
            _room = Player.CardManager.SkillController.GetTrainRoom(roomId);
            updateTimer = Timer.Register(this.gameObject, 0.1f, OnUpdateItem, null, isLooped: true);
            SkillTrainRoomPad.talkAllowClick += ResetClearBtn;
        }



        private void OnDestroy()
        {
            updateTimer.Cancel();
            SkillTrainRoomPad.talkAllowClick -= ResetClearBtn;
        }

        private void OnEnable()
        {
            clearCdBtn.onClick.AddListener(OnClearCd);
            oKBtn.onClick.AddListener(OnClearOK);
            selectToTrainBtn.onClick.AddListener(OnSelectToTrain);
            unlockBtn.onClick.AddListener(OnUnlock);
            updateTimer.Resume();
        }

        private void OnDisable()
        {
            clearCdBtn.onClick.RemoveListener(OnClearCd);
            oKBtn.onClick.RemoveListener(OnClearOK);
            selectToTrainBtn.onClick.RemoveListener(OnSelectToTrain);
            unlockBtn.onClick.RemoveListener(OnUnlock);
            updateTimer.Pause();
        }

        public void ResetClearBtn()
        {
            //重置Click变量
            click = false;
        }

        // 每0.1秒执行一次
        private void OnUpdateItem()
        {
            // 检查状态
            if (lastState != _room.State)
            {
                OnStateChanged(lastState, _room.State);
                lastState = _room.State;
            }

            // 检查CD
            if (_room.State == SkillTrainRoomState.Training)
            {
                var cdTime = _room.GetCdSecond();
                if (lastCD != cdTime)
                {
                    OnCDChanged(lastCD, cdTime);
                    lastCD = cdTime;
                }
            }
        }

        public void SetData()
        {
            if (_room == null) return;
            // 设置状态
            UpdateState();
            // 设置进度
            UpdateProgress();
            // 解锁花费
            unlockCostText.text = $" {_room.GetUnlockCostGoodsCount()}";
        }

        bool click = false;
        // 加速按钮
        private void OnClearCd()
        {
            if (click)
            {
                return;
            }
            click = true;//锁门

            //Debug.Log("1111    "+ Player.PackageManager.Diamond);
            clearCdBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                DoClearCd();
            }, 0, playAudio: false, audioCallback: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            });
        }
        // 加速按钮
        private void OnClearOK()
        {
            if (click)
            {
                return;
            }
            click = true;//锁门

            //Debug.Log("1111    "+ Player.PackageManager.Diamond);
            oKBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                DoClearCd();
            }, 0, playAudio: false, audioCallback: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            });
        }
        private void DoClearCd()
        {
            var room = Player.CardManager.SkillController.GetTrainRoom(roomId);
            if (room == null)
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }
            int costDiamond = room.GetClearCdDiamond();
            if (!Player.PackageManager.IsResourceEnough(ResourceId.Diamond, costDiamond))
            {
                Tips.PopError(ErrorID.DiamondNotEnough);
                SkillTrainRoomPad.talkAllowClick?.Invoke();
                return;
            }
            if (!isEnd)
            {
                UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("确定花费钻石，直接完成特技学习吗？".SafeFormat(costDiamond), () =>
                {
                    Player.CardManager.SkillController.ClearTrainRoomCD(roomId);
                }, () =>
                {
                    SkillTrainRoomPad.talkAllowClick?.Invoke();
                }));
            }
            else
            {
                Player.CardManager.SkillController.ClearTrainRoomCD(roomId);
            }
        }

        // 解锁按钮
        private void OnUnlock()
        {
            Player.CardManager.SkillController.UnlockSkillTrainRoom(roomId);
        }

        // 学习按钮
        private void OnSelectToTrain()
        {
            var list = Player.CardManager.SkillController.GetUnlockSkillList();
            if (list.Count == 0)
            {
                Tips.PopTips("请先通过训练激活特技");
                return;
            }


            selectToTrainBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                UIController.Instance.OpenWindow<SkillTrainRoomSelectUI>(new SkillTrainRoomSelectProperties(_room));
            }, playAudio: false, audioCallback: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
                //开门
                click = false;
            });
        }

        public void SetLockVisible(bool flag)
        {
            lockCanvas.SetAlpha(flag ? 1 : 0);
            lockCanvas.GetComponent<CanvasGroup>().interactable = flag;
        }

        // 项目是否已解锁,解锁返回true，未解锁返回false
        public bool IsUnlock()
        {
            if (_room == null) return false;
            return _room.State != SkillTrainRoomState.Lock;
        }

        [SerializeField] private RectTransform diamond = null;
        [SerializeField] private Button oKBtn = null;
        bool isEnd = false;
        // 更新面板状态
        private void UpdateState()
        {
            isEnd = false;
            trainingCanvas.SetActive(_room.State == SkillTrainRoomState.Training);
            noTrainingCanvas.SetActive(_room.State == SkillTrainRoomState.Idle);
            lockCanvas.SetActive(_room.State == SkillTrainRoomState.Lock);
            lockImages.SetActive(_room.State == SkillTrainRoomState.Lock);
            if (_room.State == SkillTrainRoomState.Lock)
            {
                var costCount = _room.GetUnlockCostGoodsCount();
                bool isGoodsEnough = (Player.PackageManager.IsGoodsEnough(GoodsId.TrainRoomUnlockGoods, costCount));
                if (isGoodsEnough)
                {
                    Anim.PlayLockAnim();
                }
                else
                {
                    Anim.StopLockAnim();
                }
            }
            if (_room.State == SkillTrainRoomState.Training)
            {
                cardItem.SetData(_room.Card.Config);
                cardItem.SetQuality(_room.Card.Quality);
                skillItem.SetData(_room.Skill.Config, false);
            }
            if (_room.State == SkillTrainRoomState.Training)
            {
                isEnd = (_room.EndTime <= Utils.DataConvUtil.ServerTimeEx);
                diamond.gameObject.SetActive(!isEnd);
                clearCdBtn.gameObject.SetActive(!isEnd);
                oKBtn.gameObject.SetActive(isEnd);
            }

        }

        // 更新进度
        private void UpdateProgress()
        {
            var cd = _room.GetCdSecond();
            var totalCD = _room.GetTotalSecond();
            float progress = 1f - (float)cd / totalCD;
            // 更新进度条
            progressImage.fillAmount = GetShowProgress(progress);
            // 更新百分比文本
            progressText.text = progress.ToString("0%");
            // 更新时间文本
            cdTimeText.text = TimeUtils.GetTimeSpanString(TimeSpan.FromSeconds(cd));
            // 更新钻石文本
            clearCdDiamondText.text = _room.GetClearCdDiamond().ToString();
        }

        // CD改变事件
        private void OnCDChanged(long lastCD, long newCD)
        {
            if (!updateProgress) return;
            // 更新进度
            UpdateProgress();
        }

        // 状态改变事件
        private void OnStateChanged(SkillTrainRoomState oldState, SkillTrainRoomState newState)
        {
            // 解锁
            if (oldState == SkillTrainRoomState.Lock && newState != SkillTrainRoomState.Lock)
            {
                OnItemUnlock();
            }
            // 开始训练
            if (oldState == SkillTrainRoomState.Idle && newState == SkillTrainRoomState.Training)
            {
                OnTrainStart();
            }
            // 训练结束
            if (oldState == SkillTrainRoomState.Training && newState == SkillTrainRoomState.Idle)
            {
                OnTrainOver();
                //Debug.Log("222   " + Player.PackageManager.Diamond);
                Timer.Register(this.gameObject, 2, () => { Tips.PopTips("技能学习成功"); });

            }
            if (!updateState) return;
            // 更新状态
            UpdateState();

        }

        public void PlayFadeInAnim()
        {
            if (_room == null) _room = Player.CardManager.SkillController.GetTrainRoom(roomId);
            if (_room == null) return;
            if (_room.State == SkillTrainRoomState.Idle)
            {
                Anim.FadeInNoTraining();
            }
            if (_room.State == SkillTrainRoomState.Training)
            {
                Anim.FadeInTraining();
            }
        }

        // 项目解锁
        private void OnItemUnlock()
        {
            updateState = false;
            // 播放解锁动画
            Anim.PlayUnlockAnim(() =>
            {
                updateState = true;
                UpdateState();
                OnLockChanged?.Invoke();
            });
        }

        // 训练开始
        private void OnTrainStart()
        {
            UpdateProgress();
            Anim.FadeInTraining();
        }

        // 训练结束
        private void OnTrainOver()
        {
            updateState = false;
            updateProgress = false;
            Anim.PlaySpeedUp(() =>
            {
                updateState = true;
                updateProgress = true;
                UpdateState();
                // 淡入
                Anim.FadeInNoTraining();
            });
        }
    }
}