using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using GameConfig.Config;
using Babu;
using static BigBang.ClassicManager;
using Protocol;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;
using System.Linq;
using System;
using DG.Tweening;

namespace BigBang.UI
{
    public enum ClassicTeamItemState
    {
        Unknow,
        Open,
        Select,
        Lock
    }

    public class ClassicTeamItem : MonoBehaviour
    {
        [SerializeField] private List<ClassicMapLevelTabItem> starList = new();
        [SerializeField] private TMP_Text clubNameText;
        [SerializeField] private TMP_Text challengeTipText;
        [SerializeField] private BabuButton challengeOnceButton;
        [SerializeField] private BabuButton canNotChallengeButton;
        [SerializeField] private BabuButton quickChallengeButton;
        [SerializeField] private BabuButton canNotQuickChallengeButton;
        [SerializeField] private Image lockImage;
        [SerializeField] private Image lightImage;

        public ClassicTeamData data;
        public ClassicTeamItemState classicTeamItemState = ClassicTeamItemState.Unknow;

        public Transform GetChallengeOnceButtonTrans()
        {
            return challengeOnceButton.transform;
        }

        private void OnEnable()
        {
            challengeOnceButton.OnClick += OnClickChallengeOnceButton;
            canNotChallengeButton.OnClick += OnClickCanNotChallengeButton;
            quickChallengeButton.OnClick += OnClickQuickChallengeButton;
            canNotQuickChallengeButton.OnClick += OnClickCanNotQuickChallengeButton;
        }

        private void OnDisable()
        {
            challengeOnceButton.OnClick -= OnClickChallengeOnceButton;
            canNotChallengeButton.OnClick -= OnClickCanNotChallengeButton;
            quickChallengeButton.OnClick -= OnClickQuickChallengeButton;
            canNotQuickChallengeButton.OnClick -= OnClickCanNotQuickChallengeButton;
        }
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private HorizontalLayoutGroup rewardLayout;
        public void SetData(ClassicTeamData data, int _lookitemid = 0)
        {
            this.data = data;

            bool isCanQuick = true;
            int starCount = 0;
            for (int i = 0; i < 3; i++)
            {
                starList[i].SetLight(false);
                bool isLight = data.passData.Stars[i] > 0;
                if (isLight)
                {
                    starList[starCount].SetLight(isLight);
                    starCount++;
                }
            }
            isCanQuick = starCount == 3;

            clubNameText.text = data.challengeClubConfig.Name;

            int leftChallengeTimes = data.challengeClubConfig.Count - data.passData.ChallengeTimes;
            bool hasTimes = leftChallengeTimes > 0;
            if (data.isOpen)
            {
                if (hasTimes)
                {
                    challengeTipText.text = "今日还可挑战<color=#13b237>{0}</color>次".SafeFormat(leftChallengeTimes);
                }
                else
                {
                    challengeTipText.text = "今日还可挑战<color=#dc5858>0</color>次";
                }
            }
            else
            {
                switch (data.countryLockReason)
                {
                    case ClassicCountryLockReason.UserLevel: challengeTipText.text = "等级{0}解锁".SafeFormat(data.countryUnlockLevel); break;
                    case ClassicCountryLockReason.PreCountry: challengeTipText.text = "需完成前置章节"; break;
                    default: challengeTipText.text = "需完成前置挑战"; break;
                }

            }

            bool isPassedOnce = data.passData.Stars[0] > 0;
            SetRewards(isPassedOnce ? data.challengeClubConfig.Reward : data.challengeClubConfig.FirstReward, rewardLayout, "", _lookitemid, !isPassedOnce);

            lockImage.gameObject.SetActive(!data.isOpen);
            challengeOnceButton.gameObject.SetActive(data.isOpen && !isCanQuick && hasTimes);
            canNotChallengeButton.gameObject.SetActive(data.isOpen && !isCanQuick && !hasTimes);
            quickChallengeButton.gameObject.SetActive(data.isOpen && isCanQuick && hasTimes);
            canNotQuickChallengeButton.gameObject.SetActive(data.isOpen && isCanQuick && !hasTimes);
            lightImage.gameObject.SetActive(data.isOpen && !isCanQuick && hasTimes && !isPassedOnce);
        }

        [SerializeField] private ScrollRect rewardScrollView = null;
        private void SetRewards(string rewardStr, HorizontalLayoutGroup layout, string rewardStr2 = null, int _lookitemid = 0, bool first = false)
        {
            Transform layoutTrans = layout.transform;
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            if (string.IsNullOrWhiteSpace(rewardStr2) == false)
            {
                List<GameItem> gameItemList2 = GameItemUtils.CreateGameItems(rewardStr2).ToList();
                gameItemList.AddRange(gameItemList2);
            }
            while (layoutTrans.childCount < gameItemList.Count) Instantiate(itemPrefab, layoutTrans);
            for (int i = 0; i < layoutTrans.childCount; i++)
            {
                if (i < gameItemList.Count)
                {
                    var reward = gameItemList[i];
                    var child = layoutTrans.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.transform.localScale = Vector3.one * 0.93f;

                    InventoryItem item = child.GetComponent<InventoryItem>();
                    item.canShowTip = true;
                    item.EnableShouTong(first);
                    item.SetGameItemViews(reward);
                    item.SetGameItemData(reward);

                    if (_lookitemid != 0 && _lookitemid == reward.Id)
                    {
                        item.ShowSelectBorder();
                    }
                    else
                    {
                        item.HidSelectBorder();
                    }
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
            rewardScrollView.enabled = gameItemList.Count > 4;
            rewardScrollView.horizontalNormalizedPosition = 0;
        }

        private void OnClickChallengeOnceButton(BabuButton sender)
        {
            if (!Player.PackageManager.AskBuyEnergy()) return;
            EventManager.Instance.Dispatch(EventID.ClassicCountryUIOnClickClallengeButton, this.data);
            UIController.Instance.OpenWindow<ClassicEnterFightUI>(new ClassicEnterFightUIProperties(data));
        }
        private void OnClickCanNotChallengeButton(BabuButton sender)
        {
            Tips.PopTips("无挑战次数");
        }

        private void OnClickQuickChallengeButton(BabuButton sender)
        {
            if (!Player.PackageManager.AskBuyEnergy()) return;

            TouchManager.Instance.DisableTouch();
            ClassicManager.Instance.FastChallenge(data.challengeClubConfig.Id, 1, true, (ChallengeTimes) =>
            {
                data.passData.ChallengeTimes = ChallengeTimes;
                SetData(data);
                TouchManager.Instance.EnableTouch();
            });
        }
        private void OnClickCanNotQuickChallengeButton(BabuButton sender)
        {
            Tips.PopTips("无挑战次数");
        }
    }
}
