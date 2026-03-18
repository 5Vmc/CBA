using Babu;
using BigBang.UI;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;
using static BigBang.ClassicManager;

namespace BigBang
{
    public class WorldMap3DItem : MonoBehaviour
    {

        public enum WorldMap3DItemState
        {
            Unknow,
            Open,
            Star,
            Pass,
            Lock
        }

        [SerializeField] private GameObject openPanel;
        [SerializeField] private SpriteRenderer countryFlagImageOpen;
        [SerializeField] private TMP_Text levelTextOpen;
        [SerializeField] private TMP_Text countryNameTextOpen;

        [SerializeField] private GameObject starPanel;
        [SerializeField] private TMP_Text levelTextStar;
        [SerializeField] private SpriteRenderer countryFlagImageStar;

        [SerializeField] private GameObject passPanel;
        [SerializeField] private SpriteRenderer countryFlagImagePass;

        [SerializeField] private GameObject lockPanel;
        [SerializeField] private SpriteRenderer countryFlagImageLock;

        [SerializeField] public WorldMap3DItemAnim ani;

        public Transform GetMidTrans()
        {
            switch (worldMap3DItemState)
            {
                case WorldMap3DItemState.Open: return countryFlagImageOpen.transform;
                case WorldMap3DItemState.Star: return countryFlagImageStar.transform;
                case WorldMap3DItemState.Pass: return countryFlagImagePass.transform;
                case WorldMap3DItemState.Lock: return countryFlagImageLock.transform;
            }
            return this.transform;
        }

        public ClassicCountryLevelData data;
        public WorldMap3DItemState worldMap3DItemState = WorldMap3DItemState.Unknow;
        public async void SetData(ClassicCountryLevelData data)
        {
            this.data = data;

            if (data.isOpen == false)
            {
                worldMap3DItemState = WorldMap3DItemState.Lock;
            }
            else
            {
                bool isPass = data.chapterMapInfo != null && data.chapterMapInfo.Pass >= data.challengeCountryConfig.Number;
                if (isPass)
                {
                    bool isStar = data.chapterMapInfo.Star < data.challengeCountryConfig.Star3;
                    if (isStar)
                    {
                        worldMap3DItemState = WorldMap3DItemState.Star;
                    }
                    else
                    {
                        worldMap3DItemState = WorldMap3DItemState.Pass;
                    }
                }
                else
                {
                    worldMap3DItemState = WorldMap3DItemState.Open;
                }
            }

            openPanel.SetActive(false);
            starPanel.SetActive(false);
            passPanel.SetActive(false);
            lockPanel.SetActive(false);

            switch (worldMap3DItemState)
            {
                case WorldMap3DItemState.Open:
                    {
                        openPanel.SetActive(true);
                        countryFlagImageOpen.sprite = await SpriteProxy.GetCountryFlag(data.challengeCountryConfig.Icon);
                        levelTextOpen.text = "<color=#fffc0b>{0}</color>/{1}".SafeFormat(data.chapterMapInfo == null ? 0 : data.chapterMapInfo.Star, data.challengeCountryConfig.Star3);
                        countryNameTextOpen.text = data.challengeCountryConfig.Name;
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/" + data.challengeCountryConfig.Level.ToString() + "/" + data.challengeCountryConfig.Map.ToString() + "/" + data.challengeCountryConfig.Id.ToString());
                        node.IsRed(openPanel.transform.Find("DotNodeImg"));
                    }
                    break;
                case WorldMap3DItemState.Star:
                    {
                        starPanel.SetActive(true);
                        countryFlagImageStar.sprite = await SpriteProxy.GetCountryFlag(data.challengeCountryConfig.Icon);
                        levelTextStar.text = "<color=#fffc0b>{0}</color>/{1}".SafeFormat(data.chapterMapInfo == null ? 0 : data.chapterMapInfo.Star, data.challengeCountryConfig.Star3);
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/" + data.challengeCountryConfig.Level.ToString() + "/" + data.challengeCountryConfig.Map.ToString() + "/" + data.challengeCountryConfig.Id.ToString());
                        node.IsRed(starPanel.transform.Find("DotNodeImg"));
                    }
                    break;
                case WorldMap3DItemState.Pass:
                    {
                        passPanel.SetActive(true);
                        countryFlagImagePass.sprite = await SpriteProxy.GetCountryFlag(data.challengeCountryConfig.Icon);
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/" + data.challengeCountryConfig.Level.ToString() + "/" + data.challengeCountryConfig.Map.ToString() + "/" + data.challengeCountryConfig.Id.ToString());
                        node.IsRed(passPanel.transform.Find("DotNodeImg"));
                    }
                    break;
                case WorldMap3DItemState.Lock:
                    {
                        lockPanel.SetActive(true);
                        countryFlagImageLock.sprite = await SpriteProxy.GetCountryFlag(data.challengeCountryConfig.Icon);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
