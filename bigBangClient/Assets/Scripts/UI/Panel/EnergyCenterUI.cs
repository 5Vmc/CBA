using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Coffee.UIEffects;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{

    public class EnergyCenterUI : MonoBehaviour, IActivityClient
    {
        [SerializeField]
        private List<BabuButton> getRewardBtnList;
        [SerializeField]
        private List<TMP_Text> infoTxt;
        [SerializeField]
        private List<Image> maskList;
        [SerializeField]
        private List<Image> bgImgList;
        [SerializeField]
        private List<Image> redDotList;
        //[SerializeField]
        //private Button btnClose;

        private int enableIndex = -1;
        #region 初始化

        private void OnEnable()
        {
            for (var index = 0; index < getRewardBtnList.Count; index++)
            {
                getRewardBtnList[index].OnClick += OnGetReward;
            }
            //btnClose.onClick.AddListener(OnClose);
        }

        private void OnDisable()
        {
            for (var index = 0; index < getRewardBtnList.Count; index++)
            {
                getRewardBtnList[index].OnClick -= OnGetReward;
            }
            //btnClose.onClick.RemoveListener(OnClose);
        }

        public void LoadActivityClient(ActivityConfig activityConfig)
        {
            //在refreshUI里，根据Properties选过selectedIndex
            RefreshUI();
        }

        private void OnGetReward(BabuButton sender) {
            if (enableIndex < 0) {
                Tips.PopTips("无法领取体力");
                return;
            }

            ActivityController.Instance.GetEnergyReward(enableIndex + 1, (resp) => {
                if (resp.ReceiveSucceed)
                {
                    ActivityController.Instance.EnergyRecord[enableIndex + 1] = 1;

                    var energyItem1 = GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Energy, 50);
                    var energyItem2 = GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Energy, 50);
                    var energyItem3 = GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Energy, 50);
                    var rewardsList = new List<GameItem>() { energyItem1, energyItem2, energyItem3 };

                    var properties = new InventoryObtainedUIProperties(rewardsList);
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);// 打开通用收益界面

                }
                else {
                    Tips.PopTips("体力领取失败");
                }
                RefreshUI();
                ActivityController.Instance.RefreshClientRedDot(ActivityClientType.EnergyCenter);
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            });
        }

        private void RefreshUI(object[] args = null)
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);
            DateTime time = DateTimeOffset.FromUnixTimeSeconds((long)Utils.DataConvUtil.ServerTime).DateTime.ToLocalTime();

            //哪个可以领取
            enableIndex = ActivityController.Instance.GetCurrentEnergyStatus();
            //处理遮罩
            for (var index = 0; index < maskList.Count; index++) {
                maskList[index].gameObject.SetActive(index != enableIndex);
            }

            //处理流光
            for (var index = 0; index < bgImgList.Count; index++)
            {
                if (index == enableIndex)
                {
                    bgImgList[index].GetComponent<UIShiny>().Play(true);
                    redDotList[index].gameObject.SetActive(ActivityController.Instance.EnergyRecord[index + 1] == 0);
                }
                else
                {
                    bgImgList[index].GetComponent<UIShiny>().Stop(true);
                    redDotList[index].gameObject.SetActive(false);
                }
            }

            for (var index = 0; index < getRewardBtnList.Count; index++) {
                if (ActivityController.Instance.EnergyRecord[index + 1] == 1)
                {
                    getRewardBtnList[index].gameObject.SetActive(false);
                    infoTxt[0].text = "已领取";
                }
                else if (enableIndex == index)
                {
                    getRewardBtnList[index].gameObject.SetActive(true);
                }
                else {
                    if (index < enableIndex)
                    {
                        getRewardBtnList[index].gameObject.SetActive(false);
                        infoTxt[0].text = "已过期";
                        //上面的没有领取都是过期
                    }
                    else {
                        getRewardBtnList[index].gameObject.SetActive(true);
                    }
                }
            }
        }
        #endregion

        //#region 关闭界面
        //private void OnClose()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
        //    UIController.Instance.HidePanel<EnergyCenterUI>();
        //}
        //#endregion

    }
}
