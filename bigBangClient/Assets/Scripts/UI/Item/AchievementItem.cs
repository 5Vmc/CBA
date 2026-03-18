using Babu;
using DG.Tweening;
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
    public class AchievementItem : MonoBehaviour
    {
        [SerializeField] private RectTransform moveRect = null;
        [SerializeField] private Image icon;
        [SerializeField] private List<InventoryItem> rewardsList;
        [SerializeField] private TMP_Text nameTxt;
        [SerializeField] private BabuButton btnReward;
        [SerializeField] private TMP_Text txtBtn;
        [SerializeField] private GameObject rewardsObj;
        [SerializeField] private TMP_Text allClearTipText = null;
        [SerializeField] private Image allClearImage = null;

        private AchievementGroupData groupData;
        private int currentId;
        protected void OnEnable()
        {
            btnReward.OnClick += GetRewards;
            StopGetAnim();
        }

        protected void OnDisable()
        {
            btnReward.OnClick -= GetRewards;
            StopGetAnim();
        }

        private void StopGetAnim()
        {
            isCanClick = true;
            getSeq?.Kill();
            getSeq = null;
            moveRect.SetLocalPositionX(0);
            moveRect.SetLocalScale(1);
        }

        private bool isCanClick = false;
        Sequence getSeq = null;
        private void GetRewards(BabuButton obj)
        {
            if (!isCanClick)
            {
                return;
            }
            // Debug.LogWarning("GetRewards + id : " + groupData.CurrentData.ID);
            var data = groupData.CurrentData;
            if (data.IsComplete)
            {
                isCanClick = false;
                Player.AchievementManager.GetAchievementRewards(data.ID, (resp) =>
                {
                    if (resp.ReceiveSucceed)
                    {
                        if (data != null)
                        {
                            data.Received = 1;
                            groupData.Next(resp.Next);
                            var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(data.Config.Reward).ToList(), () =>
                            {
                                getSeq = DOTween.Sequence();
                                getSeq.Append(moveRect.DOLocalMoveX(720, 0.5f).SetEase(Ease.InBack));
                                getSeq.AppendCallback(() =>
                                {
                                    moveRect.SetLocalPositionX(0);
                                    moveRect.SetLocalScale(0);
                                    Refreshdata();
                                });
                                getSeq.Append(moveRect.DOScale(1, 0.3f));
                                getSeq.AppendCallback(() =>
                                {
                                    isCanClick = true;
                                });
                                getSeq.AddTo(transform.parent.parent.parent.gameObject);
                            }, "");
                            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                            EventManager.Instance.Dispatch(EventID.OnAfterGetAchievementReward);
                            Player.AchievementManager.CheckAchievementRedDot();
                            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                        }
                        else
                        {
                            Debug.LogWarning("data == null");
                        }
                    }
                    else
                    {
                        if (data == null)
                        {
                            Debug.LogWarning("resp.ReceiveSucceed = " + resp.ReceiveSucceed);
                        }
                        else
                        {
                            Debug.LogWarning("resp.ReceiveSucceed = " + resp.ReceiveSucceed + " , data.ID = " + data.ID);
                        }
                    }
                });
            }
            else
            {
                if (data.Config.Moduleid != -1)
                {
                    if (TriggerManager.Instance.CheckModuleOpen(data.Config.Moduleid, true))
                    {
                        TriggerManager.Instance.JumpPanel((TriggerModuleType)data.Config.Moduleid);
                    }
                }
                else if (data.Config.Info != "")
                {
                    Tips.PopTips(data.Config.Info);
                }
            }

        }

        public void SetData(AchievementGroupData _groupData)
        {
            groupData = _groupData;
            StopGetAnim();
            Refreshdata();
        }

        private async void Refreshdata()
        {
            var data = groupData.CurrentData;

            if (groupData.AllFinish)
            {
                icon.sprite = await SpriteProxy.GetAchievementIcon(data.Config.Icon);
                //本组任务全部完成
                rewardsObj.SetActive(false);
                nameTxt.text = data.Config.Name;
                btnReward.gameObject.SetActive(false);
                allClearTipText.gameObject.SetActive(true);
                allClearImage.gameObject.SetActive(true);
            }
            else
            {
                currentId = data.Config.Id;
                rewardsObj.SetActive(true);
                btnReward.gameObject.SetActive(true);
                allClearTipText.gameObject.SetActive(false);
                allClearImage.gameObject.SetActive(false);
                icon.sprite = await SpriteProxy.GetAchievementIcon(data.Config.Icon);

                var current = 0;
                var MaxProgress = 0;
                if (data.Config.Fungroup == 1033)
                {
                    //对于通关的成就，max = 1
                    current = data.Current >= data.Config.Id ? 0 : 1;
                    MaxProgress = 1;
                }
                else
                {
                    current = data.Current;
                    MaxProgress = data.Config.Target[0];
                }

                var achDesc = data.Config.Name.SafeFormat(data.Config.Target);


                var descStr = "<color=#C1AB6B><size=30>{0}</size></color>".SafeFormat(data.Config.Name);
                if (current >= MaxProgress)
                {
                    descStr += " <size=24>" + string.Format(data.Config.Desc, "<color=#C1AB6B>" + current.ToString() + "</color>", MaxProgress) + "</size>";
                }
                else
                {
                    descStr += " <size=24>" + string.Format(data.Config.Desc, current.ToString(), MaxProgress) + "</size>";
                }

                nameTxt.text = descStr;

                int slotCount = rewardsList.Count;
                var gameItems = data.Rewards.ToList();
                int rewardsCount = gameItems.Count;
                int total = Math.Max(slotCount, rewardsCount);

                for (var index = 0; index < total; index++)
                {
                    if (index >= slotCount) continue;
                    if (index >= rewardsCount)
                    {
                        rewardsList[index].gameObject.SetActive(false);
                    }
                    else
                    {
                        rewardsList[index].gameObject.SetActive(true);
                        rewardsList[index].SetGameItemData(gameItems[index]);
                    }

                }

                //设置领取按钮
                if (data.IsComplete)
                {
                    btnReward.GetComponent<Image>().sprite = await SpriteProxy.YellowBtnEnable;
                    ColorUtility.TryParseHtmlString("#5B4646", out Color brown);
                    txtBtn.color = brown;
                    txtBtn.text = "领 取";
                    btnReward.gameObject.SetActive(true);
                }
                else
                {
                    if (data.Config.Moduleid == -1)
                    {
                        if (data.Config.Info == "")
                        {
                            btnReward.gameObject.SetActive(false);
                        }
                        else
                        {
                            txtBtn.text = "提 示";
                            btnReward.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        btnReward.gameObject.SetActive(true);
                        txtBtn.text = "前 往";
                    }

                    btnReward.GetComponent<Image>().sprite = await SpriteProxy.YellowSmallBtnDisable;
                    ColorUtility.TryParseHtmlString("#243325", out Color brown);
                    txtBtn.color = brown;
                }
            }
        }
    }
}