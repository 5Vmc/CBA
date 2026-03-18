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
    public enum ClassicCountryItemState
    {
        Unknow,
        Open,
        Select,
        Lock
    }

    public class ClassicCountryItem : MonoBehaviour
    {
        [SerializeField] private GameObject openPanel;
        [SerializeField] private TMP_Text nameTextOpen;

        [SerializeField] private GameObject selectPanel;
        [SerializeField] private TMP_Text nameTextSelect;

        [SerializeField] private GameObject lockPanel;
        [SerializeField] private TMP_Text nameTextLock;

        [SerializeField] private BabuButton goToBtn;

        public ClassicCountryLevelData data;
        public ClassicCountryItemState classicCountryItemState = ClassicCountryItemState.Unknow;

        private void OnEnable()
        {
            goToBtn.OnClick += OnClick;
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        }

        private void OnDisable()
        {
            goToBtn.OnClick -= OnClick;
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        }

        public void SetData(ClassicCountryLevelData data)
        {
            this.data = data;

            if (data.isSelect == true)
            {
                classicCountryItemState = ClassicCountryItemState.Select;
            }
            else
            {
                if (data.isOpen == false)
                {
                    classicCountryItemState = ClassicCountryItemState.Lock;
                }
                else
                {
                    classicCountryItemState = ClassicCountryItemState.Open;
                }
            }

            openPanel.SetActive(false);
            selectPanel.SetActive(false);
            lockPanel.SetActive(false);

            //Debug.Log("国家{0}  状态{1}".SafeFormat(data.challengeCountryConfig.Id, classicCountryItemState));

            switch (classicCountryItemState)
            {
                case ClassicCountryItemState.Open:
                    {
                        openPanel.SetActive(true);
                        nameTextOpen.text = data.challengeCountryConfig.Name;
                    }
                    break;
                case ClassicCountryItemState.Select:
                    {
                        selectPanel.SetActive(true);
                        nameTextSelect.text = data.challengeCountryConfig.Name;
                    }
                    break;
                case ClassicCountryItemState.Lock:
                    {
                        lockPanel.SetActive(true);
                        nameTextLock.text = data.challengeCountryConfig.Name;
                    }
                    break;
                default:
                    break;
            }
            RefreshRedDot();
        }

        private void RefreshRedDot(object[] args = null) {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/" + data.challengeCountryConfig.Level.ToString() + "/" + data.challengeCountryConfig.Map.ToString() + "/" + data.challengeCountryConfig.Id.ToString());
            node.IsRed(transform.Find("DotNodeImg"));
        }

        private void OnClick(BabuButton sender)
        {
            EventManager.Instance.Dispatch(EventID.ClassicCountryUIOnClickCountryButton, this.data);
        }
    }
}
