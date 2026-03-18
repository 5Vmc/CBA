using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using System.Linq;
using BigBang.Animation;
using Utils;
using UnityEngine.UI;
using GameConfig.Config;
using GameConfig;
using System.Collections.Generic;
using Utils.GameItem;
using Babu;
using static BigBang.BountyTaskManager;

namespace BigBang.UI
{
    public class BountyTaskPad : MonoBehaviour
    {

        [SerializeField] public BountyTaskPadAnim Anim;

        [SerializeField] private RectTransform boxPanel = null;
        [SerializeField] private TMP_Text progressText = null;
        [SerializeField] private List<InventoryItem> boxInventoryList = null;
        [SerializeField] private TMP_Text rewardTipText = null;
        [SerializeField] private TMP_Text getRewardTipText = null;
        [SerializeField] private BabuButton getBoxRewardButton = null;

        [SerializeField] private BountyTaskAdapter bountyTaskAdapter = null;

        [SerializeField] private RectTransform waitPanel = null;

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.OnBountyTaskDataChange, OnBountyTaskDataChange);
            EventManager.Instance.Register(EventID.OnBountyTaskDataRefreshList, OnBountyTaskDataRefreshList);
            EventManager.Instance.Register(EventID.OnBountyTaskDataRefreshTopBox, OnBountyTaskDataRefreshTopBox);
            getBoxRewardButton.OnClick += OnClickGetBoxRewardButton;
        }
        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.OnBountyTaskDataChange, OnBountyTaskDataChange);
            EventManager.Instance.Unregister(EventID.OnBountyTaskDataRefreshList, OnBountyTaskDataRefreshList);
            EventManager.Instance.Unregister(EventID.OnBountyTaskDataRefreshTopBox, OnBountyTaskDataRefreshTopBox);
            getBoxRewardButton.OnClick -= OnClickGetBoxRewardButton;
        }
        private void OnBountyTaskDataChange(object[] objects)
        {
            OnBountySelect();

            BountyTaskManager.Instance.CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
        private void OnBountyTaskDataRefreshList(object[] objects)
        {
            OnBountySelect(false);

            BountyTaskManager.Instance.CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
        private void OnBountyTaskDataRefreshTopBox(object[] objects)
        {
            SetTopInfo();

            BountyTaskManager.Instance.CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public void OnBountySelect(bool needAni = true)
        {
            SetTopInfo();

            List<BountyTaskData> bountyTaskDataList = BountyTaskManager.Instance.GetBountyTaskDataList();
            waitPanel.gameObject.SetActive(bountyTaskDataList == null || bountyTaskDataList.Count <= 0);
            bountyTaskAdapter.SetData(bountyTaskDataList);

            if (needAni)
            {
                bountyTaskAdapter.ScrollToTop();
                Anim.PlayEnter();
            }
        }

        public void SetTopInfo()
        {
            BountyTaskBoxConfig bountyTaskBoxConfig = Configs.BountyTaskBox.GetConfig(BountyTaskManager.Instance.boxId);
            boxPanel.gameObject.SetActive(bountyTaskBoxConfig != null);
            if (bountyTaskBoxConfig == null) return;

            int taskCountNow = BountyTaskManager.Instance.completedCount;
            int taskCountNeed = bountyTaskBoxConfig.Count;
            progressText.text = "（<color=#5DD554>{0}</color>/{1}）".SafeFormat(taskCountNow, taskCountNeed);
            GameItemUtils.SetRewards(boxInventoryList, bountyTaskBoxConfig.Rewards);
            bool isEnough = taskCountNow >= taskCountNeed;
            rewardTipText.gameObject.SetActive(!isEnough);
            getRewardTipText.gameObject.SetActive(isEnough);
            getBoxRewardButton.gameObject.SetActive(isEnough);
            if (isEnough == false)
            {
                rewardTipText.text = bountyTaskBoxConfig.Desc.Replace("{s1}", (taskCountNeed - taskCountNow).ToString());
            }
        }
        private void OnClickGetBoxRewardButton(BabuButton sender)
        {
            BountyTaskBoxConfig bountyTaskBoxConfig = Configs.BountyTaskBox.GetConfig(BountyTaskManager.Instance.boxId);
            if (bountyTaskBoxConfig == null)
            {
                EventManager.Instance.Dispatch(EventID.OnBountyTaskDataRefreshTopBox);
                return;
            }
            string rewards = bountyTaskBoxConfig.Rewards;
            NetworkManager.Instance.CollectBountyTaskBoxReward(bountyTaskBoxConfig.Id, (resp) =>
            {
                var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(rewards).ToList(), null, "获得悬赏任务宝箱奖励");
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                BountyTaskManager.Instance.boxId++;
                EventManager.Instance.Dispatch(EventID.OnBountyTaskDataRefreshTopBox);
            });
        }

    }
}