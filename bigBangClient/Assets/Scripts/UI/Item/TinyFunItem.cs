using Babu;
using Babu.Client.Fsm;
using Babu.SDK;
using BigBang.Battle;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class TinyFunItemData
    {
        public int FunId;
        public ModuleDefineConfig cfg;
        /// <summary>
        /// 剩余参与次数
        /// </summary>
        public int Times;

        /// <summary>
        /// 礼包商城的物品
        /// </summary>
        /// <param name="id"></param>
        public TinyFunItemData(int _moduleId)
        {
            cfg = Configs.ModuleDefine.GetConfig(_moduleId);
            FunId = _moduleId;
        }
    }

    public class TinyFunItem : MonoBehaviour
    {
        [SerializeField] private Image bgImg;
        /// <summary>
        /// 活动时间
        /// </summary>
        [SerializeField] private TMP_Text txtTime;
        /// <summary>
        /// 今日剩余次数
        /// </summary>
        [SerializeField] private TMP_Text txtTimes;
        [SerializeField] private TMP_Text txtTitle;

        [SerializeField] private GameObject slotContainer;
        [SerializeField] private BabuButton btnEnter;
        [SerializeField] private InventoryItem prefab;
        [SerializeField] private Transform DotNodeImg;

        public ModuleDefineConfig data;
        protected void OnEnable()
        {
            btnEnter.OnClick += OnEnter;
        }

        protected void OnDisable()
        {
            btnEnter.OnClick -= OnEnter;
        }

        public void SetData(ModuleDefineConfig _data)
        {
            this.data = _data;
            RefreshUI();
        }

        public async void RefreshUI()
        {
            txtTitle.text = data.Name;

            slotContainer.gameObject.SetActive(true);
            switch (data.Id)
            {
                case 1801:
                    {
                        slotContainer.gameObject.SetActive(false);
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Games, "/" + data.Id);
                        if (Player.ActivityManager.ShootGameTimesLeft > 0)
                        {
                            txtTimes.text = "今日免费次数: <color=#40F569>{0}</color>/{1}".SafeFormat(Player.ActivityManager.ShootGameTimesLeft, GameConst.ChallengeTimes);
                        }
                        else
                        {
                            txtTimes.text = "";
                        }
                        node.AddValue(Player.ActivityManager.CheckFunRed(data.Id) ? 1 : -1);
                        node.IsRed(DotNodeImg);
                    }
                    break;
                case 1802:
                    {
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Games, "/" + data.Id);
                        txtTimes.text = "领取体力";
                        node.AddValue(Player.ActivityManager.CheckFunRed(data.Id) ? 1 : -1);
                        node.IsRed(DotNodeImg);
                    }
                    break;
                case 1200:
                    {
                        txtTimes.text = "";
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBClassicHero, "");
                        node.IsRed(DotNodeImg);
                    }
                    break;
                case 2900:
                    {
                        if (FBTowerController.Instance.FBData.resetCount <= 0 && FBTowerController.Instance.IsCanGetStarRewards)
                        {
                            txtTimes.text = "有奖励待领取";
                        }
                        else
                        {
                            txtTimes.text = string.Format("今日剩余重置次数: {0}", FBTowerController.Instance.LeftResetCount);
                        }
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBTower, "");
                        node.IsRed(DotNodeImg);
                    }
                    break;
                default:
                    {
                        txtTimes.text = "";
                        Debug.LogWarningFormat("TinyFunItem , RefreshUI , unknow data.Id , data.Id = {0}", data.Id);
                    }
                    break;
            }
            txtTimes.transform.parent.gameObject.SetActive(!string.IsNullOrWhiteSpace(txtTimes.text));

            bgImg.sprite = await SpriteProxy.GetHomeIcon("banner_" + data.Id);
            List<GameItem> rewards = new();
            if (string.IsNullOrWhiteSpace(data.RewardsPreview) == false)
            {
                rewards = GameItemUtils.CreateGameItems(data.RewardsPreview).ToList();
            }
            var children = slotContainer.GetComponentsInChildren<InventoryItem>();
            int slotCount = children.Length;
            int rewardCount = rewards.Count;
            int counter = Math.Max(slotCount, rewardCount);

            for (int index = 0; index < counter; index++)
            {
                InventoryItem item;
                if (index > slotCount - 1)
                {
                    item = Instantiate<InventoryItem>(prefab, slotContainer.transform);
                    item.transform.localScale = new Vector3(0.8f, 0.8f);
                }
                else
                {
                    item = children[index];
                }

                if (index > rewardCount - 1)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
                    item.gameObject.SetActive(true);
                    item.SetGameItemData(rewards[index]);
                    item.SetCount("");
                }
            }
        }

        private void OnEnter(BabuButton sender)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            if (data.Id == 1801)
            {
                if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Games)) return;
                FsmManager.Instance.ChangeToState<StateTinyFun>(new StateCommonUserData()
                {
                    OpenUIAction = async () =>
                    {
                        await UIController.Instance.ShowPanel<ShootUI>(new ShootUIProperties(ShootUIEnterPos.tinyFun));
                    }
                });
            }
            else if (data.Id == 1802)
            {
                UIController.Instance.ShowPanel<EnergyCenterUI>();
            }
            else if (data.Id == 1200)
            {
                if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicHero)) return;
                UIController.Instance.ShowPanel<HeroChapterUI>();
            }
            else if (data.Id == 2900)
            {
                if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Fuben_DianTang)) return;
                UIController.Instance.ShowPanel<FBTowerHomeUI>();
            }
        }
    }
}