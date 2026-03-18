using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class InventoryUIProperties : PanelProperties
    {
        public InventoryUI.SubUIID SubUI;
        public bool isProp = true;
        public InventoryUIProperties(InventoryUI.SubUIID ui, bool isProp = true)
        {
            SubUI = ui;
            this.isProp = isProp;
        }
    }

    public class InventoryUI : APanelController<InventoryUIProperties>
    {
        public enum SubUIID
        {
            Inventory,
            Recycle
        }

        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup recycleAndInventoryGroup;
        [SerializeField] private BabuToggle recycleBtn;
        [SerializeField] private BabuToggle inventoryBtn;
        [SerializeField] private BabuToggleGroup switchGroup;
        [SerializeField] private BabuToggle propBtn;
        [SerializeField] private BabuToggle debrisBtn;
        [SerializeField] private InventoryGridAdapter adapter;
        [SerializeField] private InventorySelectedPad selectedPad;
        [SerializeField] private InventoryRecyclePad recyclePad;
        [SerializeField] private RectTransform osaRect;
        [SerializeField] private Image reddotimg;

        public static List<GoodsData> ShowDataList;

        private GoodsData selectData;

        private InventoryUIAnim Anim;

        private enum CurrentState
        {
            Inventory,
            Recycle
        }

        private CurrentState currentState = CurrentState.Inventory;

        protected override void Awake()
        {
            base.Awake();
            Anim = GetComponent<InventoryUIAnim>();
        }
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            recycleBtn.OnSelect += OnRecycleSelect;
            inventoryBtn.OnSelect += OnInventorySelect;
            propBtn.OnSelect += OnPropSelect;
            debrisBtn.OnSelect += OnDebrisSelect;
            adapter.OnSelect += OnSelect;
            recyclePad.OnQuality += OnQuality;
            selectedPad.RequestRefresh += RefreshInventoryData;
            Babu.EventManager.Instance.Register(EventID.OnRefreshInventoryAdapter, Refresh);
            Babu.EventManager.Instance.Register(EventID.RefreshInventoryProp, RefreshInventoryProp);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            recycleBtn.OnSelect -= OnRecycleSelect;
            inventoryBtn.OnSelect -= OnInventorySelect;
            propBtn.OnSelect -= OnPropSelect;
            debrisBtn.OnSelect -= OnDebrisSelect;
            adapter.OnSelect -= OnSelect;
            recyclePad.OnQuality -= OnQuality;
            selectedPad.RequestRefresh -= RefreshInventoryData;
            Babu.EventManager.Instance.Unregister(EventID.OnRefreshInventoryAdapter, Refresh);
            Babu.EventManager.Instance.Unregister(EventID.RefreshInventoryProp, RefreshInventoryProp);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            if (Properties.SubUI == SubUIID.Inventory)
            {
                // 默认打开仓库界面
                recycleAndInventoryGroup.Switch(inventoryBtn);
                // 默认打开道具栏
                switchGroup.Switch(Properties.isProp ? propBtn : debrisBtn);
                OnPropSelect();
            }
            else if (Properties.SubUI == SubUIID.Recycle)
            {
                recycleAndInventoryGroup.Switch(recycleBtn);
                switchGroup.Switch(Properties.isProp ? propBtn : debrisBtn);
                OnRecycleSelect();
            }

            Anim.PlayEnter();
            adapter.PlayAnim();
            refreshRedDot();
        }

        private void Refresh(object[] args)
        {
            RefreshInventoryData();
            // 刷新物品栏
            if (propBtn.isOn)
            {
                OnPropSelect();
            }
            else
            {
                OnDebrisSelect();
            }
        }

        private void RefreshInventoryData()
        {
            adapter.SetData(ShowDataList, -1);
            selectData = ShowDataList.First();
            selectedPad.SetData(ShowDataList.First());
        }

        private void OnSelect(GameObject selectItem)
        {
            // 设置选中的元素
            var item = selectItem.GetComponent<InventoryItem>();
            selectData = item.Data;
            selectedPad.SetData(selectData);
            if (currentState == CurrentState.Recycle)
            {
                item.ShowSubButton();
            }
        }

        private void OnQuality(int quality)
        {
            if (ShowDataList == null) return;
            if (quality == QualityType.All)
            {
                adapter.SetData(ShowDataList.OrderBy(item => item.Id).ToList(), -1);
            }
            else
            {
                adapter.SetData(ShowDataList.Where(item => item.Config.Quality == quality).ToList(), -1);
            }
        }

        // 选中仓库栏
        private void OnInventorySelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            // 禁用多选
            adapter.DisableMultipleChoose();

            currentState = CurrentState.Inventory;
            osaRect.SetBottom(430);
            osaRect.anchorMin = new Vector2(0.5f, 0);
            osaRect.anchorMax = new Vector2(0.5f, 1);
            selectedPad.gameObject.SetActive(true && Player.PackageManager.GetGoodsList().Any());
            recyclePad.gameObject.SetActive(false);
            if (propBtn.isOn)
            {
                OnPropSelect();
            }
            else
            {
                OnDebrisSelect();
            }
        }

        // 选中回收栏
        private void OnRecycleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            // 启用多选
            adapter.EnableMultipleChoose();

            currentState = CurrentState.Recycle;
            osaRect.SetBottom(600);
            osaRect.anchorMin = new Vector2(0.5f, 0);
            osaRect.anchorMax = new Vector2(0.5f, 1);
            selectedPad.gameObject.SetActive(false);
            recyclePad.gameObject.SetActive(true && Player.PackageManager.GetGoodsList().Any());
            recyclePad.ClearPad();
            OnQuality(QualityType.All);
            if (propBtn.isOn)
            {
                OnPropSelect();
            }
            else
            {
                OnDebrisSelect();
            }
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            foreach (var item in Player.PackageManager.GetGoodsList().Where(item => item.IsNew))
            {
                Player.PackageManager.NewToOld.Add(item.Id);
            }
            if (Player.PackageManager.NewToOld.Count > 0)
            {
                // 去除所有New标签
                NetworkManager.Instance.SetGoodsAsOldRequest(Player.PackageManager.NewToOld.ToList(), response =>
                {
                    Debug.Log("去除New标签");
                });
                Player.PackageManager.NewToOld.Clear();
            }
            closeBtn.GetComponent<ButtonAnim>().PlayBack(() => UIController.Instance.ShowPanel<HomeUI>());
        }

        // 选中道具栏
        private void OnPropSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            ShowDataList = Player.PackageManager.GetGoodsList().Where(item => item.Config.Type != (int)GoodsType.Pieces).OrderByDescending(item=>item.Config.Type == 2).ThenBy(item => item.Id).ToList();
            if (!CheckListAndSetPad()) return;
            adapter.SetData(ShowDataList, -1);
            selectData = ShowDataList.First();
            selectedPad.SetData(ShowDataList.First());
        }

        public void RefreshInventoryProp(object[] args)
        {
            int itemConfId = (int)args[0];

            //Debug.Log("itemConfId = " + itemConfId);

            ShowDataList = Player.PackageManager.GetGoodsList().Where(item => item.Config.Type != (int)GoodsType.Pieces).OrderBy(item => item.Id).ToList();


            GoodsData findData = ShowDataList.Find(item => item.Config.Id == itemConfId);
            if (findData == null)
            {
                selectData = ShowDataList.First();
                selectedPad.SetData(ShowDataList.First());
                adapter.SetData(ShowDataList, -1);
                refreshRedDot();


            }
            else
            {
                selectData = findData;
                selectedPad.SetData(findData);
                adapter.SetData(ShowDataList, itemConfId);
                refreshRedDot();
            }

        }

        private void refreshRedDot() { 
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Bag, "/Storage");
            node.IsRed(reddotimg.transform);
        }

        // 选中碎片栏
        private void OnDebrisSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            ShowDataList = Player.PackageManager.GetGoodsList().Where(item => item.Config.Type == (int)GoodsType.Pieces).OrderBy(item => item.Id).ToList();
            if (!CheckListAndSetPad()) return;
            adapter.SetData(ShowDataList, -1);
            selectData = ShowDataList.First();
            selectedPad.SetData(ShowDataList.First());
        }

        // 检查是否有元素并设置选中面板会回收面板的启用状态
        private bool CheckListAndSetPad()
        {
            if (ShowDataList == null || ShowDataList.Count() <= 0)
            {
                if (inventoryBtn.isOn)
                {
                    selectedPad.gameObject.SetActive(false);
                }
                else
                {
                    recyclePad.gameObject.SetActive(false);
                }
                adapter.SetEmptyData();
                return false;
            }
            else
            {
                if (inventoryBtn.isOn)
                {
                    selectedPad.gameObject.SetActive(true);
                }
                else
                {
                    recyclePad.gameObject.SetActive(true);
                }
                return true;
            }
        }
    }
}