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
using Protocol;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;
using System.Linq;
using System;
using static BigBang.HeroManager;

namespace BigBang.UI
{
    public class HeroClubItem : MonoBehaviour
    {
        [SerializeField] private List<ClassicMapLevelTabItem> starList = new();
        [SerializeField] private TMP_Text clubIndexText;
        [SerializeField] private TMP_Text clubNameText;
        [SerializeField] private BabuButton challengeOnceButton;
        [SerializeField] private TMP_Text lockText;
        [SerializeField] private Image lockImage;
        [SerializeField] private Image lightImage;

        public HeroClubData data;

        private void OnEnable()
        {
            challengeOnceButton.OnClick += OnClickChallengeOnceButton;
        }

        private void OnDisable()
        {
            challengeOnceButton.OnClick -= OnClickChallengeOnceButton;
        }
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private HorizontalAdapter rewardLayout;
        [SerializeField] private Image lightBtnImage = null;
        [SerializeField] private Image darkBtnImage = null;
        [SerializeField] private TMP_Text challengeLightText = null;
        [SerializeField] private TMP_Text challengeDarkText = null;
        public void SetData(HeroClubData data)
        {
            this.data = data;

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

            clubIndexText.text = (data.index + 1).ToString();
            clubNameText.text = data.challengeHeroConfig.Name;

            SetRewards(data.challengeHeroConfig.Reward, rewardLayout);

            lockImage.gameObject.SetActive(!data.isOpen);
            lockText.gameObject.SetActive(!data.isOpen);
            challengeOnceButton.gameObject.SetActive(data.isOpen);
            if(data.isOpen)
            {
                bool isFullStar = starCount >= 3;
                lightBtnImage.gameObject.SetActive(!isFullStar);
                darkBtnImage.gameObject.SetActive(isFullStar);
                challengeLightText.gameObject.SetActive(!isFullStar);
                challengeDarkText.gameObject.SetActive(isFullStar);
            }

            lightImage.gameObject.SetActive(data.isSelect);
        }

        private void SetRewards(string rewardStr, HorizontalAdapter layout, string rewardStr2 = null)
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
                    InventoryItem inventoryItem = child.GetComponent<InventoryItem>();
                    inventoryItem.SetData(gameItemList[i]);
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
            layout.Calculate();
        }

        private void OnClickChallengeOnceButton(BabuButton sender)
        {
            if (!Player.PackageManager.AskBuyEnergy()) return;

            UIController.Instance.OpenWindow<ClassicEnterFightUI>(new ClassicEnterFightUIProperties(data));
        }
    }
}
