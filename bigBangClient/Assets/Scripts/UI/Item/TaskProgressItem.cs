using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;
using BigBang.Animation;
using Babu;

namespace BigBang.UI
{
    public class TaskProgressItem : MonoBehaviour
    {
        [SerializeField] private Image progressValue;
        [SerializeField] private RectTransform obtain;
        [SerializeField] private List<BabuButton> rewardList;
        [SerializeField] private List<TMP_Text> pointList;
        [SerializeField] private List<RectTransform> positions;
        [SerializeField] private List<InventoryItem> items;
        [SerializeField] private TMP_Text pointTxt;
        [SerializeField] private RectTransform pointImgRect;
        [SerializeField] private Image icon;
        [SerializeField] private Image icon2;
        [SerializeField] private Sprite dailySprite;
        [SerializeField] private Sprite weeklySprite;

        [SerializeField] public TaskProgressItemAnim Anim;


        // 活跃点位置,数值飞去的目标位置
        public static RectTransform PointImgPos;

        private List<TaskRewardBoxConfig> rewardConfigs;
        private CyclicTasks currentTask;
        private TaskType currentType = TaskType.Daily;
        private float lastPoint = 0;

        // 最大奖励数量
        private const int MAX_REWARD_ITEM = 3;

        private void Awake()
        {
            // 关闭动画
            rewardList.ForEach(item => item.Anim = null);
            // 启用点击弹出道具Tips
            items.ForEach(item => item.canShowTip = true);
            PointImgPos = pointImgRect;
        }

        private void OnEnable()
        {
            rewardList.ForEach(item => item.OnClick += OnReward);
            Babu.EventManager.Instance.Register(EventID.OnRefreshTaskProgressItem, OnRefreshTaskProgressItem);
        }

        private void OnDisable()
        {
            rewardList.ForEach(item => item.OnClick -= OnReward);
            Babu.EventManager.Instance.Unregister(EventID.OnRefreshTaskProgressItem, OnRefreshTaskProgressItem);
        }

        // 数值刷新事件
        private void OnRefreshTaskProgressItem(object[] args)
        {
            if (currentType == TaskType.Daily)
            {
                SetData(Player.TaskManager.DailyTasks, currentType);
            }
            else
            {
                SetData(Player.TaskManager.WeeklyTasks, currentType);
            }
        }

        // 点击领取奖励
        private void OnReward(BabuButton sender)
        {
            // 点击的按钮下标
            int index = rewardList.IndexOf(sender);

            // 如果可以领取,则领取宝箱
            if (currentTask.Point >= rewardConfigs[index].NeedPoint)
            {
                // 如果已经领取过了
                if (currentTask.CollectedBoxes.Exists(boxid => boxid == rewardConfigs[index].Id))
                {
                    AudioManager.Instance.PlaySound(AudioNames.BTN_2);
                    SetBoxContent(index);
                    return;
                }
                sender.GetComponent<RewardBoxAnim>().Play();
                AudioManager.Instance.PlaySound(AudioNames.BTN_STREN);
                NetworkManager.Instance.CollectTaskBoxReward(rewardConfigs[index].Id, response =>
                {
                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(rewardConfigs[index].Reward).ToList());
                    // 打开通用收益界面
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                    // 刷新界面
                    OnRefreshTaskProgressItem(null);
                });
            }
            else
            {
                // 显示宝箱内容
                SetBoxContent(index);
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            }
        }

        // 设置活跃点图标
        private void SetPointIcon()
        {
            switch (currentType)
            {
                // 蓝色图标
                case TaskType.Daily:
                    icon.sprite = icon2.sprite = dailySprite;
                    break;
                // 黄色图标
                case TaskType.Weekly:
                    icon.sprite = icon2.sprite = weeklySprite;
                    break;
            }
        }

        // 任务类型发生改变
        private void OnTaskTypeChanged(TaskType oldType, TaskType newType)
        {
            // 设置活跃点
            pointTxt.text = currentTask.Point.ToString();
            // 设置进度条
            progressValue.fillAmount = currentTask.Point / 100f;
        }

        public void SetData(CyclicTasks task, TaskType taskType, bool playAnim = true)
        {
            // 任务
            currentTask = task;
            obtain.gameObject.SetActive(false);
            // 如果任务类型发生了改变(周常,日常)
            if (currentType != taskType)
            {
                currentType = taskType;
                lastPoint = currentTask.Point;
                // 触发任务类型改变事件
                OnTaskTypeChanged(currentType, taskType);
            }
            else
            {
                if (lastPoint != task.Point)
                {
                    if (playAnim)
                    {
                        // 播放进度条动画
                        Anim.PlayProgressValueAnim(lastPoint, task.Point);
                    }
                    else
                    {
                        pointTxt.text = task.Point.ToString();
                        progressValue.fillAmount = task.Point / 100f;
                    }
                    lastPoint = task.Point;
                }
            }
            // 设置活跃点图标
            SetPointIcon();
            // 设置宝箱
            SetBox();
        }

        // 设置日常和周任务进度宝箱
        private void SetBox()
        {
            // 宝箱列表
            rewardConfigs = Configs.TaskRewardBox.GetConfigList().Where(item => item.Type == (int)currentType).ToList();
            // 设置宝箱状态
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "/" + ((int)currentType).ToString() + "/box");
            var isred = false;
            for (int i = 0; i < rewardList.Count; i++)
            {
                var boxItem = rewardList[i];
                if (currentTask.Point >= rewardConfigs[i].NeedPoint)
                {
                    // 已领取
                    if (currentTask.CollectedBoxes.Exists(boxid => boxid == rewardConfigs[i].Id))
                    {
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Open, s => boxItem.image.sprite = s);
                    }
                    // 未领取
                    else
                    {
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Obtain, s => boxItem.image.sprite = s);
                        isred = true;
                    }
                    boxItem.transform.localScale = Vector3.one * 1.2f;
                }
                else
                {
                    // 未解锁
                    SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Close, s => boxItem.image.sprite = s);
                    boxItem.transform.localScale = Vector3.one;
                }
                // 设置活跃点
                pointList[i].text = rewardConfigs[i].NeedPoint.ToString();
            }
            node.AddValue(isred ? 1 : -1);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        // 设置宝箱内容
        private void SetBoxContent(int index)
        {
            obtain.gameObject.SetActive(true);
            obtain.transform.SetParent(positions[index]);
            obtain.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            // 宝箱内容
            var gameItems = GameItemUtils.CreateGameItems(rewardConfigs[index].Reward).ToArray();
            for (int i = 0; i < MAX_REWARD_ITEM; i++)
            {
                if (i < gameItems.Length)
                {
                    items[i].gameObject.SetActive(true);
                    items[i].SetData(gameItems[i]);
                }
                else
                {
                    items[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
