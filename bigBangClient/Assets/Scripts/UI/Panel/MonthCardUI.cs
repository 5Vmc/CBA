using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using GameConfig;
using TMPro;
using System.Linq;
using Utils.GameItem;
using Utils;
using Babu.SDK;
using Babu;

namespace BigBang.UI
{
    public class MonthCardUI : MonoBehaviour
    {

        [SerializeField] private MonthCardUIItem item1;
        [SerializeField] private MonthCardUIItem item2;

        protected void OnEnable()
        {
            item1.SetData(Configs.MonthCardShop.GetConfigList()[0]);
            item2.SetData(Configs.MonthCardShop.GetConfigList()[1]);
            PurchaseServiceManager.Instance.CheckPurchase += PurchaseUtil.OnCheckPurchase;

            EventManager.Instance.Register(PurchaseServiceManager.Event.PurchaseResult, OnPurchaseResult);
        }

        protected void OnDisable()
        {
           PurchaseServiceManager.Instance.CheckPurchase -= PurchaseUtil.OnCheckPurchase;
           EventManager.Instance.Unregister(PurchaseServiceManager.Event.PurchaseResult, OnPurchaseResult);
        }

        private void OnPurchaseResult(object[] args)
        {
            item1.RefreshUI();
            item2.RefreshUI();
        }
    }
}