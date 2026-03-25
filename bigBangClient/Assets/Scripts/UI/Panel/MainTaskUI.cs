using UnityEngine;
using deVoid.UIFramework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Utils.GameItem;
using DG.Tweening;
using GameConfig;
using Utils;
using Babu;
using BigBang.Animation;
using UnityTimer;
using UnityEngine.UI;
using GameConfig.Config;
using System;
using Random = UnityEngine.Random;
using Babu.Client.Fsm;

namespace BigBang.UI
{

    public class MainTaskUIProperties : PanelProperties
    {
        public MainTaskUI.SubUIID SubUI = MainTaskUI.SubUIID.Unknow;
        public MainTaskUIProperties(MainTaskUI.SubUIID ui)
        {
            SubUI = ui;
        }
    }

    public class MainTaskUI : APanelController<MainTaskUIProperties>
    {

        public enum SubUIID
        {
            Tab1,
            Tab2,
            Tab3,
            Tab4,
            Tab5,

            Unknow = 9999,
        }

        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private Image mainTaskImg;
        [SerializeField] private TMP_Text demandTxt;
        [SerializeField] private TMP_Text progressTxt;
        [SerializeField] private TMP_Text clubTxt;
        [SerializeField] private List<MainTaskItem> progressItems;
        [SerializeField] private List<InventoryItem> obtain;
        [SerializeField] private BabuButton getBtn;
        [SerializeField] private BabuButton undone;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private Image completedImg;
        [SerializeField] private TMP_Text completedTxt;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private TMP_Text titleTxt;

        [SerializeField] private List<MainTaskTabItem> mainTaskTabItems;
        [SerializeField] private List<RawImage> rawImages;
        [SerializeField] private List<BabuToggle> tabToggles;

        private TaskData currentTask;
        private MainTaskType currentType;

        [SerializeField] public MainTaskUIAnim Anim;

        [SerializeField] private MainTaskTabItem item2;
        [SerializeField] private RawImage raw;

        private int maxShowCount = 16;

        protected override void Awake()
        {
            base.Awake();
            closeBtn.Anim = null;
            closeBtn.Sound = null;
            // 打开Tips功能
            obtain.ForEach((item => item.canShowTip = true));

            getBtn.Sound = AudioNames.TECHBOARD_POP;

            mainTaskTabItems.ForEach(item =>
            {
                item.transform.SetParent(null);
                DontDestroyOnLoad(item);
            });
            tabToggles.ForEach(item => item.DisableStatusControl());
        }

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            getBtn.OnClick += OnGet;
            undone.OnClick += OnUndone;
            toggleGroup.OnValueChanged += OnToggleGroupChanged;
            EventManager.Instance.Register(EventID.Refresh_Normal_Task, _refresh_task);
            EventManager.Instance.Register(EventID.OnMainTaskItemSelected, OnMainTaskItemSelected);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            getBtn.OnClick -= OnGet;
            undone.OnClick -= OnUndone;
            toggleGroup.OnValueChanged -= OnToggleGroupChanged;
            EventManager.Instance.Unregister(EventID.Refresh_Normal_Task, _refresh_task);
            EventManager.Instance.Unregister(EventID.OnMainTaskItemSelected, OnMainTaskItemSelected);
        }

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            foreach (var (rawimage, item) in rawImages.Zip(mainTaskTabItems, (rawimage, item) => (rawimage, item)))
            {
                rawimage.texture = item.RenderTex;
            }
            // 俱乐部名称
            clubTxt.text = Player.Name;
            SetData();
            // 播放动画
            Anim.PlayEnter();
            //scroll.enabled = false;
            for (int i = 0; i < mainTaskTabItems.Count; i++)
            {
                mainTaskTabItems[i].SetIcon(await SpriteProxy.GetMainTaskTab(i + 1));
            }

            if (Properties != null && Properties.SubUI != SubUIID.Unknow)
            {
                int index = (int)Properties.SubUI;
                tabToggles[index].isOn = true;
                mainTaskTabItems[index].Selected();
            }
        }

        private void OnMainTaskItemSelected(object[] args)
        {
            int index = (int)args[0];
            if (!Player.TaskManager.NormalTasks.GroupCompletedTasks.ContainsKey((int)currentType)) return;
            int currentTaskIndex = Player.TaskManager.NormalTasks.GroupCompletedTasks[(int)currentType].Count;
            if (index == currentTaskIndex)
            {
                SetData();
            }
            else if (index < currentTaskIndex)
            {
                // 当前大类任务列表
                var currentTaskCfgs = Configs.Task.GetConfigList().Where(item => item.Type == (int)TaskType.Normal && TaskData.GetTaskDataType(item.Id) == (int)currentType);
                var cfg = currentTaskCfgs.ElementAtOrDefault(index);
                if (cfg == null)
                {
                    Debug.LogError("cfg is null");
                    return;
                }
                progressTxt.text = $"({cfg.Condition}/{cfg.Condition})";
                SetReward(cfg);
                SetSelectedTaskData(index);
            }
        }

        [SerializeField] private GameObject TaskDetailPanel;
        [SerializeField] private GameObject AllTaskCompletePanel;
        private async void SetData()
        {
            // 当前选中的大类
            currentType = mainTaskTabItems[tabToggles.IndexOf(toggleGroup.EnableToggle)].Type;
            bool isAllComplete = Player.TaskManager.NormalTasks.CompletedTaskGroups.Contains((int)currentType);
            // 当前大类任务总数
            var count = Configs.Task.GetConfigList().Count(item => item.Type == (int)TaskType.Normal && TaskData.GetTaskDataType(item.Id) == (int)currentType);

            // 当前大类的任务
            var task = Player.TaskManager.NormalTasks.Tasks.FirstOrDefault(item => item.Value.Type == (int)currentType).Value;
            if (isAllComplete == false && task == null)
            {
                Tips.PopError(ErrorID.UnlockRequirements);
                return;
            }

            // 是否启用滚动条
            bool isNeedScroll = count > maxShowCount;
            scroll.enabled = isNeedScroll;
            if (isNeedScroll == false)
            {
                ScrollToPage1();
            }

            titleTxt.text = Configs.MainTaskTitle.GetConfig((int)currentType).Title;
            mainTaskImg.sprite = await SpriteProxy.GetMainTaskBG((int)currentType);
            currentTask = task;
            // 设置大类进度
            SetProgress();
            // 设置子任务进度
            SetSubProgress();

            if (isAllComplete == false)
            {
                // 设置奖励
                SetReward(currentTask.Config);
                // 设置当前任务数据
                SetCurrentTaskData();
                // 判断是否可以领取奖励
                CheckClaimable();
            }

            TaskDetailPanel.SetActive(!isAllComplete);
            AllTaskCompletePanel.SetActive(isAllComplete);

            Player.TaskManager.CheckTaskRedDot_Normal(TaskType.Normal);
            //todo://刷新事件
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public void PlayTabAnim()
        {
            foreach (var item in mainTaskTabItems)
            {
                item.PlayAnim();
            }
        }

        // 未完成按钮
        private void OnUndone(BabuButton sender)
        {
            Tips.PopError(ErrorID.PleaseFinishTheTask);
        }

        // 设置奖励
        private void SetReward(TaskConfig cfg)
        {
            var gameItems = GameItemUtils.CreateGameItems(cfg.Reward).ToArray();
            // 最多4个奖励
            for (int i = 0; i < 4; i++)
            {
                if (i < gameItems.Length)
                {
                    obtain[i].gameObject.SetActive(true);
                    obtain[i].SetData(gameItems[i]);
                }
                else
                {
                    // 关闭不显示的奖励
                    obtain[i].gameObject.SetActive(false);
                }
            }
            // 任务需求
            demandTxt.text = cfg.Desc;
        }

        private void SetSelectedTaskData(int index)
        {
            completedImg.gameObject.SetActive(true);
            getBtn.gameObject.SetActive(false);
            undone.gameObject.SetActive(false);
            completedTxt.text = Lang.Get(LangID.DayTxt).Replace("{value}", progressItems[index].Day.ToString());
        }

        private void _refresh_task(object[] args)
        {
            var _taskid = (int)args[0];
            if (currentTask != null && currentTask.Id == _taskid)
                SetCurrentTaskData();
        }

        public void SetCurrentTaskData()
        {
            if (currentTask == null) return;
            if (Player.TaskManager.NormalTasks.CompletedTasks.TryGetValue(currentTask.Id, out var day))
            {
                completedImg.gameObject.SetActive(true);
                getBtn.gameObject.SetActive(false);
                undone.gameObject.SetActive(false);
                completedTxt.text = Lang.Get(LangID.DayTxt).Replace("{value}", day.ToString());
            }
            else
            {
                completedImg.gameObject.SetActive(false);

                bool isCanGet = Utils.ConfigUtil.CompareByStr(currentTask.Progress, currentTask.Config.CompareType, currentTask.Config.Condition);
                // 领取按钮
                getBtn.gameObject.SetActive(isCanGet);
                // 未完成按钮
                undone.gameObject.SetActive(!isCanGet);
            }

            // 进度文本
            if (currentTask.Type == (int)MainTaskType.Challenge)
            {
                progressTxt.gameObject.SetActive(false);
            }
            else
            {
                progressTxt.gameObject.SetActive(true);
                progressTxt.text = $"({currentTask.Progress}/{currentTask.Config.Condition})";
            }

        }

        // 设置大类进度
        public void SetProgress()
        {
            foreach (var item in mainTaskTabItems)
            {
                Player.TaskManager.NormalTasks.GroupCompletedTasks.TryGetValue((int)item.Type, out var completedList);
                if (completedList == null)
                {
                    item.SetProgress(0);
                    continue;
                }
                int count = Configs.Task.GetConfigList().Count(cfg => cfg.Type == (int)TaskType.Normal && TaskData.GetTaskDataType(cfg.Id) == (int)item.Type);
                // 完成个数 / 总个数
                item.SetProgress(completedList.Count / (float)count);
            }
        }

        // 设置子任务进度
        public void SetSubProgress()
        {
            bool isAllComplete = Player.TaskManager.NormalTasks.CompletedTaskGroups.Contains((int)currentType);
            Player.TaskManager.NormalTasks.GroupCompletedTasks.TryGetValue((int)currentType, out var completedTasks);

            int completeCount = 0;
            if (completedTasks != null) completeCount = completedTasks.Count;
            // 是否再第二页
            bool isInPage2 = completeCount > maxShowCount;
            if (isInPage2)
            {
                ScrollToPage2();
            }
            else
            {
                ScrollToPage1();
            }

            int index = 0;
            // 子任务进度
            for (int i = 0; i < progressItems.Count; i++)
            {
                var item = progressItems[i];
                item.Index = i;
                item.Type = currentType;
                int day = 0;
                // 如果没有完成的任务,全部设置为未解锁
                if (completedTasks == null)
                {
                    item.SetData(i == 0 ? MainTaskState.InProgress : MainTaskState.Lock, day);
                    continue;
                }
                // 如果当前下标等于完成任务的总数量,该item设置为进行中
                if (i == completedTasks.Count)
                {
                    if (isAllComplete)
                    {
                        item.SetData(MainTaskState.Lock, day);
                    }
                    else
                    {
                        item.SetData(MainTaskState.InProgress, day);
                    }
                }
                // 设置完成的任务天数
                else if (i + 1 <= completedTasks.Count)
                {
                    // 完成时间
                    var completedTime = TimeUtils.ToDateTime(Player.TaskManager.NormalTasks.CompletedTasks[completedTasks[index++]]).Date;
                    // 账号创建时间
                    var createTime = TimeUtils.ToDateTime(Player.CreateTime).Date;
                    // 完成天数
                    day = (int)(completedTime - createTime).TotalDays + 1;
                    item.SetData(MainTaskState.Completed, day);
                }
                else
                {
                    item.SetData(MainTaskState.Lock, day);
                }
            }
        }

        private void OnToggleGroupChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int taskType = (int)mainTaskTabItems[tabToggles.IndexOf(newToggle)].Type;
            var task = Player.TaskManager.NormalTasks.Tasks.FirstOrDefault(item => item.Value.Type == taskType).Value;
            if (Player.TaskManager.NormalTasks.CompletedTaskGroups.Contains(taskType) == false && task == null)
            {
                oldToggle.isOn = true;
                newToggle.isOn = false;
                Tips.PopError(ErrorID.UnlockRequirements);
                return;
            }
            AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP);
            titleTxt.text = Configs.MainTaskTitle.GetConfig(taskType).Title;
            oldToggle.transform.DOScale(0.96f, 0.1f);
            newToggle.transform.DOScale(1.1f, 0.1f);
            newToggle.transform.SetAsLastSibling();
            mainTaskTabItems[tabToggles.IndexOf(oldToggle)].Deselectd();
            mainTaskTabItems[tabToggles.IndexOf(newToggle)].Selected();
            SetData();
        }

        private void OnGet(BabuButton sender)
        {
            NetworkManager.Instance.CollectTaskReward(currentTask.Id, response =>
            {
                // 打开通用收益界面
                var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(currentTask.Config.Reward).ToList(), () =>
                {
                    //刷新数据，PlayCompletedAnim必须在他下面。
                    SetData();
                    PlayCompletedAnim();
                    Player.TaskManager.CheckTaskRedDot_Normal(TaskType.Normal);
                });
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                CbaLogManager.Instance.AddLog(1001, currentTask.Config.Id, currentTask.Config.Type);
            });
        }

        private void PlayCompletedAnim()
        {
            Timer.Register(this.gameObject, 0.3f, () => AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT));
            progressItems.Last(item => item.State == MainTaskState.Completed).PlayAnim();
            Timer.Register(this.gameObject, 0.5f, PlayTabAnim);
        }

        private void ScrollToPage1()
        {
            scroll.DOHorizontalNormalizedPos(0, 0.3f);
            //DOTween.To(value => scroll.content.SetLeft(value), 0, -626, 0.3f);
        }

        private void ScrollToPage2()
        {
            scroll.DOHorizontalNormalizedPos(1, 0.3f);
            //DOTween.To(value => scroll.content.SetLeft(value), -626, 0, 0.3f);
        }

        private void SetAsPage1()
        {
            scroll.content.SetLeft(-626);
        }

        private void SetAsPage2()
        {
            scroll.content.SetLeft(0);
        }

        private void OnClose(BabuButton sender)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
            Anim.PlayExit(() =>
            {
                
                FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
                {
                    OpenUIAction = () =>
                    {
                        UIController.Instance.HidePanel<MainTaskUI>();
                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                });
            });
        }

        /**
        *判断是否可以领取奖励
        */
        private void CheckClaimable()
        {
            //currentTask.Progress >= currentTask.Config.Condition
            for (int i = 0; i < mainTaskTabItems.Count; i++)
            {
                MainTaskType type = mainTaskTabItems[i].Type;

                mainTaskTabItems[i].ClaimTip(false);

                //if (currentType == type)
                //    continue;

                var task = Player.TaskManager.NormalTasks.Tasks.FirstOrDefault(item => item.Value.Type == (int)type).Value;
                if (task != null)
                {
                    if (task.Progress >= task.Config.Condition)
                    {
                        mainTaskTabItems[i].ClaimTip(true);
                    }
                }
            }
        }


        private float ClaimAnimTip(GameObject go)
        {
            Sequence seq = DOTween.Sequence();
            //go.transform.localScale = Vector3.one * 1.05f;

            seq.Append(go.transform.DOScale(0.95f, 0.6f));
            seq.AppendInterval(0.2f);
            seq.Append(go.transform.DOScale(1.05f, 0.6f));
            seq.AppendInterval(0.2f);
            seq.SetLoops(-1);
            return 0.8f;
        }
    }
}
