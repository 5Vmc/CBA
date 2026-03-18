using System.Collections.Generic;
using System.Linq;
using Babu;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    public class TotalPayPad : MonoBehaviour, IActivity
    {
        [SerializeField] private TotalPayAdapter adapter;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Image bgImage;
        [SerializeField] private Image TitleTextImage1;
        [SerializeField] private Image lineImage;

        private List<TotalPayItemData> datasource = new();
        private ActivityData data;

        protected void OnEnable()
        {
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
            EventManager.Instance.Register(EventID.RefreshWindow, OnServerPushRefresh);
        }

        protected void OnDisable()
        {
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            EventManager.Instance.Unregister(EventID.RefreshWindow, OnServerPushRefresh);
        }

        private void RefreshLeftTime()
        {
            long leftTime = data.EndTime - Utils.DataConvUtil.ServerTime;
            timeText.text = "活动结束：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }

        private void refreshData()
        {
            datasource.Clear();
            var RewardsConfigList = Configs.ActivityPayReward.GetConfigList().FindAll(p => p.ActivityId == data.cfg.Id);
            RewardsConfigList.ForEach(p =>
            {
                var item = new TotalPayItemData();
                item.cfg = p;
                item.money = data.payData.TotalPay;

                if (data.payData.HasReceive(p.Id))
                {
                    item.state = 0;
                }
                else if (item.money >= p.Option)
                {
                    item.state = 2;
                }
                else
                {
                    item.state = 1;
                }
                datasource.Add(item);

            });

            datasource = datasource.OrderBy(p => p.cfg.Option).ThenByDescending(p => p.state).ThenBy(p => p.cfg.Option).ToList();
            TotalPayItemData totalPayItemData = new()
            {
                isFree = true,
                state = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(data.cfg.Id) ? 0 : 2
            };
            datasource.Insert(0, totalPayItemData);
            adapter.SetData(datasource, data);

            int needIndex = -1;
            for (var index = 0; index < datasource.Count; index++)//先选可领取的
            {
                var isRed = datasource[index].state == 2;
                if (isRed)
                {
                    needIndex = index;
                    break;
                }
            }
            if (needIndex == -1)//再选可以充值的
            {
                for (var index = 0; index < datasource.Count; index++)
                {
                    var isCanCharge = datasource[index].state == 1;
                    if (isCanCharge)
                    {
                        needIndex = index;
                        break;
                    }
                }
            }
            if (needIndex == -1) needIndex = datasource.Count - 1;//没有就最后一个
            adapter.ScrollTo(needIndex);
        }

        private void OnServerPushRefresh(object[] objects)
        {
            if ((int)objects[0] != data.cfg.Id) return;
            refreshData();
        }

        public void LoadActivity(ActivityData _data)
        {
            setImage(_data);
            data = _data;
            refreshData();
            RefreshLeftTime();
        }

        private async void setImage(ActivityData _data)
        {
            bgImage.sprite = await SpriteProxy.GetFestivalImg(_data.cfg.Param1, "img_691");
            TitleTextImage1.sprite = await SpriteProxy.GetFestivalImg(_data.cfg.Param1, "payTitle");
            lineImage.sprite = await SpriteProxy.GetFestivalImg(_data.cfg.Param1, "img_524");
        }
    }
}