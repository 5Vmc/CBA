using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using System.Linq;
using BigBang.Animation;
using Utils;

namespace BigBang.UI
{
    public class DailyTaskPad : MonoBehaviour
    {
        [SerializeField] private TMP_Text dayTitleTxt;
        [SerializeField] private TMP_Text weekTitleTxt;
        [SerializeField] private TaskUIAdapter adapter;
        [SerializeField] private TaskProgressItem progressItem;

        [SerializeField] public DailyTaskPadAnim Anim;

        private CyclicTasks currentTask;

        // 选中日常任务
        public void OnDaySelect(bool savePad = false)
        {
            dayTitleTxt.gameObject.SetActive(true);
            weekTitleTxt.gameObject.SetActive(false);
            progressItem.SetData(Player.TaskManager.DailyTasks, TaskType.Daily, false);
            adapter.SetData(Player.TaskManager.DailyTasks.Tasks.Select(item => item.Value), Player.TaskManager.WeeklyTasks.Tasks.Select(item => item.Value));
            adapter.ShowDailyData();
            currentTask = Player.TaskManager.DailyTasks;
            Anim.PlayEnter(currentTask.Point);
        }

        // 选中周常任务
        public void OnWeekSelect(bool savePad = false)
        {
            dayTitleTxt.gameObject.SetActive(false);
            weekTitleTxt.gameObject.SetActive(true);
            progressItem.SetData(Player.TaskManager.WeeklyTasks, TaskType.Weekly, false);
            adapter.SetData(Player.TaskManager.WeeklyTasks.Tasks.Select(item => item.Value), Player.TaskManager.WeeklyTasks.Tasks.Select(item => item.Value));
            adapter.ShowWeeklyData();
            currentTask = Player.TaskManager.WeeklyTasks;
            Anim.PlayEnter(currentTask.Point);
        }


    }
}