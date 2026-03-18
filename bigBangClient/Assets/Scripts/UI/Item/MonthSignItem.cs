using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils.GameItem;
using Utils;
using BigBang.Animation;
using DG.Tweening;
using System.Threading.Tasks;
using System;

namespace BigBang.UI
{
    public class MonthSignItemData
    {
        public int MonthSiginID;
        public int MonthSiginType;
        public int Count;
        public int Date;
        public int State;
        public Sprite Icon;
        public GameItem gameItem;
        public MonthSignUIAdapter Adapter;
    }

    public class MonthSignItem : MonoBehaviour
    {
        [SerializeField] private Image rewardIcon;
        [SerializeField] private Image missImage;
        [SerializeField] private Image blackImg;
        [SerializeField] private TMP_Text countTxt;
        [SerializeField] private TMP_Text countTxt2;
        [SerializeField] private BabuButton rewardBtn;
        [SerializeField] private Image lightRect;
        [SerializeField] private TMP_Text dayTxt;
        [SerializeField] private TMP_Text dayTxt2;
        [SerializeField] private GameObject trailParticle;

        [SerializeField] public MonthSignItemAnim Anim;

        private MonthSignItemData data;
        private List<GameItem> obtainList = new List<GameItem>();

        private RectTransform selfRect;
        private RectTransform SelfRect
        {
            get
            {
                selfRect ??= GetComponent<RectTransform>();
                return selfRect;
            }
        }

        public int MonthID { get => data.MonthSiginID; }

        private void Awake()
        {
            rewardBtn.Anim = null;
            rewardBtn.Sound = null;
        }

        private void OnEnable()
        {
            rewardBtn.OnClick += OnReward;
        }

        private void OnDisable()
        {
            rewardBtn.OnClick -= OnReward;
        }

        public void SetData(MonthSignItemData data)
        {
            Anim.InitMiss();
            this.data = data;
            Debug.Log("======>MonthSignItemData=" + RewardStates.COLLECT);
            //设置图片
            rewardIcon.sprite = data.Icon;
            //设置数量
            countTxt.text = countTxt2.text = data.Count.ToString();
            //设置天数
            dayTxt.text = dayTxt2.text = data.Date.ToString();
            //设置是否可领取
            lightRect.gameObject.SetActive(data.State == (int)RewardStates.COLLECT);
            //设置是否可补签
            missImage.gameObject.SetActive(data.State == (int)RewardStates.UNCOLLECT);
            //设置是否已领取
            blackImg.gameObject.SetActive(data.State == (int)RewardStates.RECEIVED);
        }

        private async void OnReward(BabuButton sender)
        {
            // 判断点击范围
            var overlap = data.Adapter.Viewport.GetOverlapAreaFromScreen(SelfRect, UIController.Instance.GetCamera());
            // 如果点击的物体显示不完整,则滚动视图,让其显示完整
            if (Mathf.Abs(SelfRect.GetAreaFromScreen(UIController.Instance.GetCamera()) - overlap) > 1)
            {
                if (data.Adapter.Viewport.GetBottomPosFromScreen(UIController.Instance.GetCamera()) < SelfRect.GetBottomPosFromScreen(UIController.Instance.GetCamera()))
                {
                    data.Adapter.SmoothScrollTo(0, 0.3f);
                }
                else
                {
                    data.Adapter.SmoothScrollTo(data.Adapter.CellsCount - 1, 0.3f);
                }
                TouchManager.Instance.DisableTouch();
                // 等等滚动动画完成
                await Task.Delay(TimeSpan.FromSeconds(0.4f));
                TouchManager.Instance.EnableTouch();
            }

            obtainList.Add(data.gameItem);
            //领取奖励
            if (data.State == (int)RewardStates.COLLECT)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
                //领取
                NetworkManager.Instance.ReceiveMonthSignReward(data.MonthSiginID, data.MonthSiginType, response =>
                {
                    if (response.ReceiveSucceed)
                    {
                        UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList, () =>
                        {
                            PlayFlyAnim();
                        }));
                        Player.ActivityManager.SignMonth[data.MonthSiginID - 1].SetState((int)RewardStates.RECEIVED);
                        Player.ActivityManager.SignDay = response.SignDay;
                    }
                    else
                    {
                        Debug.LogError("领取失败");
                    }
                });
            }
            else if (data.State == (int)RewardStates.UNCOLLECT)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
                //打开是否补签面板
                UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties(Lang.Get(LangID.MonthRewardCostTxt), OnDetermine));
            }
            else
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);
                //提示
                UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(data.gameItem));
            }
        }

        private void OnDetermine()
        {
            NetworkManager.Instance.ReceiveMonthSignReward(data.MonthSiginID, data.MonthSiginType, response =>
            {
                if (response.ReceiveSucceed)
                {
                    Player.ActivityManager.SignMonth[data.MonthSiginID - 1].SetState((int)RewardStates.RECEIVED);
                    Player.ActivityManager.SignDay = response.SignDay;
                    var obtainList = new List<GameItem>();
                    obtainList.Add(data.gameItem);
                    // 打开物品获得窗口
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList, () =>
                    {
                        Anim.PlayMissExit(() =>
                        {
                            PlayFlyAnim();
                        });
                    }));
                }
                else
                {
                    Tips.PopTips("您的钻石不足或无法完成签到");
                    Debug.Log("钱不够");
                    Debug.Log("签到失败");
                    // 跳转通用充值界面
                }
            });
        }

        private void PlayFlyAnim()
        {
            TouchManager.Instance.DisableTouch();
            Anim.PlaySign();
            // 克隆物品
            var clone = Instantiate(rewardIcon.gameObject, rewardIcon.transform.parent);
            clone.transform.position = rewardIcon.transform.position;
            clone.transform.SetParent(MonthSignUI.MoveLayer, false);
            trailParticle.SetActive(true);
            trailParticle.transform.SetParent(clone.transform, false);
            clone.transform.DOScale(0.8f, 0.8f);
            clone.transform.DOMove(MonthSignUI.SignPos.position, 0.8f).OnComplete(() =>
            {
                TouchManager.Instance.EnableTouch();
                trailParticle.SetActive(false);
                trailParticle.transform.SetParent(transform, false);
                clone.SetActive(false);
                Babu.EventManager.Instance.Dispatch(EventID.OnRefreshMonthSiginUI, data.MonthSiginID);
            });
            // 销毁物品
            Destroy(clone, 1.5f);
        }
    }
}

