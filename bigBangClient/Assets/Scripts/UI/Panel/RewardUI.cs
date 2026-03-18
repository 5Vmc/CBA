using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Utils.GameItem;

namespace BigBang.UI
{
    public class RewardUIProperties : WindowProperties
    {
        public List<GameItem> Rewards { get; private set; }

        public RewardUIProperties(List<GameItem> reward)
        {
            Rewards = reward;
        }
    }

    public class RewardUI : AWindowController<RewardUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            while (content.childCount < Properties.Rewards.Count) Instantiate(itemPrefab, content);
            for (int i = 0; i < content.childCount; i++)
            {
                if (i < Properties.Rewards.Count)
                {
                    var reward = Properties.Rewards[i];
                    var child = content.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.GetComponent<InventoryItem>().SetData(reward);
                }
                else
                {
                    content.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<RewardUI>();
        }
    }
}