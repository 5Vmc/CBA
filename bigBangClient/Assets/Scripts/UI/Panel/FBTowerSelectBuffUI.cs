using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;
using System.Text;
using System;
using Babu;

namespace BigBang.UI
{
    [Serializable]
    public class FBTowerSelectBuffUIProperties : WindowProperties
    {
        public TowerLevelData towerLevelData = null;
        public FBTowerSelectBuffUIProperties(TowerLevelData towerLevelData)
        {
            this.towerLevelData = towerLevelData;
        }
    }
    public class FBTowerSelectBuffUI : AWindowController<FBTowerSelectBuffUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text starCountText = null;
        [SerializeField] private List<FBTowerSelectBuffItem> buffItemList = new();
        [SerializeField] private FBTowerSelectBuffUIAnim anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.OnClickFBTowerBuff, OnClickFBTowerBuff);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.OnClickFBTowerBuff, OnClickFBTowerBuff);
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            bool isRight = SetData();
            if (isRight == false)
            {
                UIController.Instance.CloseWindow<FBTowerSelectBuffUI>();
            }
            SetButtonCanUse(true);
            anim.PlayEnter();
        }
        private void OnClose()
        {
            anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<FBTowerSelectBuffUI>();
            });
        }

        private bool SetData()
        {
            starCountText.text = FBTowerController.Instance.FBData.currentStar.ToString();
            if (string.IsNullOrWhiteSpace(Properties.towerLevelData.towerConfig.Buff))
            {
                Debug.LogWarningFormat("FBTowerSelectBuffUI , Refresh , string.IsNullOrWhiteSpace(Properties.towerLevelData.towerConfig.Buff) == true , Properties.towerLevelData.towerConfig.Id = {0}", Properties.towerLevelData.towerConfig.Id);
                return false;
            }
            string[] buffStrList = Properties.towerLevelData.towerConfig.Buff.Split("|");
            if (buffStrList.Length != 3)
            {
                Debug.LogWarningFormat("FBTowerSelectBuffUI , Refresh , buffList.Length != 3 , Properties.towerLevelData.towerConfig.Id = {0}", Properties.towerLevelData.towerConfig.Id);
                return false;
            }
            for (int i = 0; i < buffStrList.Length; i++)
            {
                string buffStr = buffStrList[i];
                bool isItemRight = buffItemList[i].SetBuffStr(Properties.towerLevelData.towerConfig.Id, buffStr, i);
                buffItemList[i].RefreshShow();
                if (!isItemRight) return false;
            }
            return true;
        }

        private void OnClickFBTowerBuff(object[] objs)
        {
            SetButtonCanUse(false);
            FBTowerSelectBuffItem buffItem = objs[0] as FBTowerSelectBuffItem;
            FBTowerController.Instance.ChooseBuff(buffItem.index + 1 , ()=>
            {
                EventManager.Instance.Dispatch(EventID.AfterGetFBTowerBuff, buffItem);
                OnClose();
            });
        }
        private void SetButtonCanUse(bool isCanUse)
        {
            for (int i = 0; i < buffItemList.Count; i++)
            {
                buffItemList[i].SetButtonCanUse(isCanUse);
            }
            closeBtn.interactable = isCanUse;
        }

    }
}