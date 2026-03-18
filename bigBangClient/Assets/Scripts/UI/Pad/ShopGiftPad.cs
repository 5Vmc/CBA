using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Babu;
using GameConfig;
using Utils.GameItem;
using System.Linq;
using GameConfig.Config;
using Utils;
using DG.Tweening;

namespace BigBang.UI
{

    public class ShopGiftPad : MonoBehaviour, IDataPad
    {
        [SerializeField] private ShopGiftAdapter adapter;
        private ActivityData activityData;
        public int giftType;

        protected void OnEnable()
        {
            Babu.EventManager.Instance.Register(EventID.OnRefreshGiftShop, SetData);
        }

        protected void OnDisable()
        {
            Babu.EventManager.Instance.Unregister(EventID.OnRefreshGiftShop, SetData);
        }

        public void SetData(object[] args = null)
        {
            ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
            bool isTimeRecruitNeedShow = activityData != null;
            var dataConfigs = Configs.GiftShop.GetConfigList().FindAll(p => p.Type == giftType);
            List<GiftItemData> ShopGiftItemDataList = new();
            if (giftType == 2 && isTimeRecruitNeedShow)
            {
                ShopGiftItemDataList.Add(new GiftItemData(activityData.cfg.Id, ETimeGiftType.None, 0, activityData));
            }
            dataConfigs.ForEach((p) =>
            {
                ShopGiftItemDataList.Add(new GiftItemData(p.Id, (ETimeGiftType)p.Type, 0));
            });
            adapter.SetData(ShopGiftItemDataList);
            RefreshTimeRecruit(giftType == 2);
        }

        public List<GameObject> timeRecruitNeedShowList = new();
        public List<GameObject> timeRecruitNeedHideList = new();

        [SerializeField] private Image timeRecruitHeadIconImage = null;
        [SerializeField] private TMP_Text timeRecruitNameText = null;
        [SerializeField] private TMP_Text timeRecruitDescText = null;
        public async void RefreshTimeRecruit(bool isTimeRecruitNeedShow)
        {
            timeRecruitNeedShowList.ForEach((p) => { p.SetActive(isTimeRecruitNeedShow); });
            timeRecruitNeedHideList.ForEach((p) => { p.SetActive(!isTimeRecruitNeedShow); });
            if (isTimeRecruitNeedShow)
            {
                ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
                int cardId = 0;
                int.TryParse(activityData.cfg.Param2, out cardId);
                CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
                if (cardModelConfig == null)
                {
                    Debug.LogWarningFormat("ShopGiftPad , RefreshTimeRecruit , cardModelConfig is null , activityData.cfg.Id = {0} , cardId = {1}", activityData.cfg.Id, cardId);
                    return;
                }

                timeRecruitHeadIconImage.sprite = await SpriteProxy.GetActivityRecruitSprite(cardModelConfig.Id.ToString());
                timeRecruitNameText.text = PlayerCard.GetFullName(cardModelConfig);
                timeRecruitDescText.text = cardModelConfig.RecruitWords;
            }
        }
    }
}