using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using System.Linq;
using BigBang.Animation;
using Utils;
using System.Collections.Generic;
using Babu;
using System;
using GameConfig;
using DG.Tweening;
using GameConfig.Config;

namespace BigBang.UI
{
    public class TimeGiftCollectionUI : MonoBehaviour, IActivityClient
    {

        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private GameObject giftContainer;
        [SerializeField] private TimeGiftItemPad prefab;

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.OnRefreshGiftShop, refreshData);
            EventManager.Instance.Register(EventID.OnTimeGiftTimeEnd, refreshData);
        }
        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.OnRefreshGiftShop, refreshData);
            EventManager.Instance.Unregister(EventID.OnTimeGiftTimeEnd, refreshData);
        }
        public void LoadActivityClient(ActivityConfig activityConfig)
        {
            refreshData();
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
        }

        private void refreshData(object[] args = null)
        {
            var data = TimeGiftController.Instance.Data;
            if (data == null || data.Count == 0)
            {
                UIController.Instance.HidePanel<ActivityMainUI>();
                //Debug.LogWarning("TimeGiftCollectionUI , refreshData , data == null || data.Count == 0");
                return;
            }

            //被刷新，通知过来的ID
            var _giftid = 0;
            if (args != null)
            {
                _giftid = int.Parse(args[0].ToString());
            }

            var children = giftContainer.GetComponentsInChildren<TimeGiftItemPad>();
            var itemPadCount = children.Length;
            var giftCount = data.Count;
            var maxCount = Math.Max(itemPadCount, giftCount);

            for (var index = 0; index < maxCount; index++)
            {
                TimeGiftItemPad pad;
                if (index >= itemPadCount)
                {
                    pad = Instantiate<TimeGiftItemPad>(prefab, giftContainer.transform);
                }
                else
                {
                    pad = children[index];
                }

                if (index >= giftCount)
                {
                    pad.gameObject.SetActive(false);
                }
            }

            children = giftContainer.GetComponentsInChildren<TimeGiftItemPad>();
            var index1 = 0;
            foreach (var actData in data.Values)
            {
                if (_giftid == 0)
                {
                    children[index1].SetData(actData, true);
                }
                else
                {
                    children[index1].SetData(actData, false);
                }

                children[index1].gameObject.SetActive(true);
                index1++;
            }

            PlayListAni();
        }

        private List<Tween> listAniList = new();
        private void PlayListAni()
        {
            ClearListAni();
            var children = giftContainer.GetComponentsInChildren<TimeGiftItemPad>();
            var index = 0;
            foreach (var child in children)
            {
                if (child.gameObject.activeSelf == false) continue;
                child.transform.localScale = Vector3.zero;
                listAniList.Add(child.transform.DOScale(Vector3.one * 0.94f, 0.3f).SetDelay(index * 0.1f).SetEase(Ease.OutBack));
                index++;
            }
        }
        private void ClearListAni()
        {
            foreach (var item in listAniList)
            {
                item?.Kill();
            }
            listAniList.Clear();
        }


    }
}