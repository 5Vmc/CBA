using System;
using Babu.Config;
using BigBang.Animation;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class CardUpgradeStatusItem : MonoBehaviour
    {
        [SerializeField] private Image statusImage;
        [SerializeField] private Button button;
        
        private CardUpgradeStatus myStatus;
        public void InitMe(CardUpgradeStatus status)
        {
            myStatus = status;
            
        }


        public CardUpgradeStatus Status{
            get {return this.myStatus;}
        }
        public void ShowMe(bool f)
        {
            gameObject.SetActive(f);
        }

        private void OnClickMe()
        {
           
            CardUpgradeUI.Instance.OnClickStatusImage(this.myStatus);
        }

        private void OnEnable() {
            button.onClick.AddListener(OnClickMe);
        }

        private void OnDisable() {
            button.onClick.RemoveListener(OnClickMe);
        }
    }
}