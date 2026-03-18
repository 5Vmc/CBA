using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using GameConfig.Config;

namespace BigBang.UI
{
    public class TaskItemData
    {
        public int TaskID;
        public Sprite Icon;
        public float Sum;
        public float Count;
        public int State;
        public string Desc;
        public bool IsUnlock;
        public int Way;
        public int Point;
        public int ViewHolderIndex;
        public int moduleId;

        public float Progress { get => Count / Sum; }
    }

    public class TaskItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image progressValue;
        [SerializeField] private Image completeImg;
        [SerializeField] private TMP_Text progressTxt;
        [SerializeField] private TMP_Text infoTxt;
        [SerializeField] private TMP_Text descTxt;
        [SerializeField] private TMP_Text pointTxt1;
        [SerializeField] private TMP_Text pointTxt2;
        [SerializeField] private BabuButton getBtn;
        [SerializeField] private BabuButton goBtn;
        [SerializeField] private GameObject progressRect;
        [SerializeField] private Sprite dailySprite;
        [SerializeField] private Sprite weeklySprite;
        [SerializeField] private List<GameObject> blueParticles;
        [SerializeField] private List<GameObject> yellowParticles;

        [SerializeField] public TaskItemAnim Anim;

        private TaskItemData data;
        private Timer getBtnLoopAnim;

        private void OnEnable()
        {
            getBtn.OnClick += OnGet;
            goBtn.OnClick += OnGo;
            getBtnLoopAnim = Timer.Register(this.gameObject, 3, null, GetButtonLoopAnim, isLooped: true);
        }

        private void OnDisable()
        {
            getBtn.OnClick -= OnGet;
            goBtn.OnClick -= OnGo;
            getBtnLoopAnim.Cancel();
        }

        private void GetButtonLoopAnim(float time)
        {
            if (time >= 0 && time < 1)
            {
                getBtn.transform.localScale = Vector3.one + Vector3.one * PeriodicFunction.Trigonometric(time) * 0.08f;
            }
        }

        public void SetData(TaskItemData data)
        {
            this.data = data;
            // 前往按钮
            goBtn.gameObject.SetActive(data.State == TaskState.IN_PROGRESS || data.State == TaskState.LOCK);
            // 获得按钮
            getBtn.gameObject.SetActive(data.State == TaskState.COMPLETE);
            getBtn.gameObject.SetAlpha(1);
            // 完成图片
            completeImg.gameObject.SetActive(data.State == TaskState.COLLECTED);
            // 解锁
            infoTxt.gameObject.SetActive(!data.IsUnlock);
            // 进度条
            progressRect.SetActive(data.IsUnlock);
            // 设置进度
            progressValue.fillAmount = data.Progress;
            progressTxt.text = $"{data.Count}/{data.Sum}";
            // 设置任务目标
            descTxt.text = data.Desc;
            // 设置解锁条件
            TaskDemandConfig taskDemandConfig = Configs.TaskDemand.GetConfig(data.TaskID);
            if (taskDemandConfig == null)
            {
                Debug.LogError("TaskItem , SetData , TaskDemandConfig dont have id : " + data.TaskID);
                infoTxt.text = "--";
            }
            else
            {
                infoTxt.text = taskDemandConfig.Content;
            }

            // 设置活跃点
            pointTxt1.text = pointTxt2.text = data.Point.ToString();
            var taskType = (TaskType)Configs.Task.GetConfig(data.TaskID).Type;
            switch (taskType)
            {
                case TaskType.Daily:
                    blueParticles.ForEach(item => item.SetActive(true));
                    yellowParticles.ForEach(item => item.SetActive(false));
                    icon.sprite = dailySprite;
                    break;
                case TaskType.Weekly:
                    blueParticles.ForEach(item => item.SetActive(false));
                    yellowParticles.ForEach(item => item.SetActive(true));
                    icon.sprite = weeklySprite;
                    break;
            }
        }

        // 获取单个任务奖励
        private void OnGet(BabuButton sender)
        {
            var moduleOpen = TriggerManager.Instance.CheckModuleOpen(data.moduleId, true);
            if (moduleOpen)
            {
                // 领取奖励
                NetworkManager.Instance.CollectTaskReward(data.TaskID, response =>
                {
                    Anim.PlayObtain(() =>
                    {
                        var taskItemData = new TaskItemData();
                        taskItemData.TaskID = data.TaskID;
                        taskItemData.State = TaskState.COLLECTED;
                        taskItemData.IsUnlock = true;
                        var cfg = Configs.Task.GetConfig(data.TaskID);
                        taskItemData.Sum = taskItemData.Count = cfg.Condition;
                        taskItemData.Desc = cfg.Desc;
                        Babu.EventManager.Instance.Dispatch(EventID.OnRefreshTaskUI, data.ViewHolderIndex, taskItemData);
                        CbaLogManager.Instance.AddLog(1001, cfg.Id, cfg.Type);
                    });
                });
            }
        }

        // 前往
        private void OnGo(BabuButton sender)
        {
            var moduleOpen = TriggerManager.Instance.CheckModuleOpen(data.moduleId, true);
            if (moduleOpen)
            {
                TriggerManager.Instance.JumpPanel(data.moduleId);
            }
        }
    }
}
