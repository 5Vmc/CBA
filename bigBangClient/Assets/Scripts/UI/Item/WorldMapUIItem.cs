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

namespace BigBang.UI
{
    public enum WorldMapUIItemState
    {
        Unknow,
        Open,
        Select,
        Lock
    }

    public class WorldMapUIItem : MonoBehaviour
    {
        [SerializeField] private GameObject openPanel;
        [SerializeField] private Image bgImageOpen;
        [SerializeField] private TMP_Text starTextOpen;
        [SerializeField] private TMP_Text nameTextOpen;

        [SerializeField] private GameObject selectPanel;
        [SerializeField] private Image bgImageSelect;
        [SerializeField] private TMP_Text starTextSelect;
        [SerializeField] private TMP_Text nameTextSelect;

        [SerializeField] private GameObject lockPanel;
        [SerializeField] private Image bgImageLock;
        [SerializeField] private TMP_Text nameTextLock;

        [SerializeField] private BabuButton goToBtn;

        [SerializeField] private GameObject selectImage;

        public ClassicMapLevelData data;
        public WorldMapUIItemState worldMapUIItemState = WorldMapUIItemState.Unknow;

        private void OnEnable()
        {
            goToBtn.OnClick += OnClick;
            EventManager.Instance.Register(EventID.OnClickWorldUIItem, OnClickWorldUIItem);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        }

        private void OnDisable()
        {
            goToBtn.OnClick -= OnClick;
            EventManager.Instance.Unregister(EventID.OnClickWorldUIItem, OnClickWorldUIItem);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        }

        private void RefreshRedDot(object[] args = null) {
            if (this.data !=null && this.data.isOpen) {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/" + data.challengeMapConfig.Level.ToString() + "/" + data.challengeMapConfig.Id.ToString());
                node.IsRed(transform.Find("DotNodeImg"));
            }
        }

        public async void SetData(ClassicMapLevelData data)
        {
            this.data = data;

            if(data.isOpen == false)
            {
                worldMapUIItemState = WorldMapUIItemState.Lock;
            }
            else
            {
                if (data.isSelect == false)
                {
                    worldMapUIItemState = WorldMapUIItemState.Open;
                }
                else
                {
                    worldMapUIItemState = WorldMapUIItemState.Select;
                }
                RefreshRedDot();
            }

            selectImage.SetActive(data.isSelect);
            openPanel.SetActive(false);
            selectPanel.SetActive(false);
            lockPanel.SetActive(false);

            
            switch (worldMapUIItemState)
            {
                case WorldMapUIItemState.Open:
                    {
                        openPanel.SetActive(true);
                        bgImageOpen.sprite = await SpriteProxy.GetMapFlag(data.challengeMapConfig.IconUI);
                        starTextOpen.text = "{0}/{1}".SafeFormat(data.passCountry, data.totalCountry);
                        nameTextOpen.text = data.challengeMapConfig.Name;
                    }
                    break;
                case WorldMapUIItemState.Select:
                    {
                        selectPanel.SetActive(true);
                        bgImageSelect.sprite = await SpriteProxy.GetMapFlag(data.challengeMapConfig.IconUI);
                        starTextSelect.text = "{0}/{1}".SafeFormat(data.passCountry, data.totalCountry);
                        nameTextSelect.text = data.challengeMapConfig.Name;
                    }
                    break;
                case WorldMapUIItemState.Lock:
                    {
                        lockPanel.SetActive(true);
                        bgImageLock.sprite = await SpriteProxy.GetMapFlag(data.challengeMapConfig.IconUI + "_gray");
                        nameTextLock.text = data.challengeMapConfig.Name;
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnClick(BabuButton sender)
        {
            if(worldMapUIItemState == WorldMapUIItemState.Lock)
            {
                Tips.PopTips("未解锁该区域");
                return;
            }
            EventManager.Instance.Dispatch(EventID.OnClickWorldUIItem, this);
        }

        public void OnClickWorldUIItem(object[] args)
        {
            //WorldMapUIItem worldMapUIItem = (WorldMapUIItem)args[0];
            //data.isSelect
            //if (worldMapUIItem == this)
        }
    }
}
