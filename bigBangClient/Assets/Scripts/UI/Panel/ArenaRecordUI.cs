using System;
using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;
using Protocol;
using Babu;

namespace BigBang.UI
{
    public class ArenaRecordUIProperties : WindowProperties
    {
        public IList<ArenaLogInfo> DataList;
        public ArenaRecordUIProperties(IList<ArenaLogInfo> logs)
        {
            this.DataList = logs;
        }
    }
    public class ArenaRecordUI :  AWindowController<ArenaRecordUIProperties>
    {
        [SerializeField] private Button closeBtn;
        //[SerializeField] private LeagueRewardsUIAnim Anim;

        [SerializeField] private ArenaRecordListAdapter osa;
        [SerializeField] private TMP_Text noRecordText;
        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            NetworkManager.Instance.getBattleLog(
                resp=>{

               /* List<ArenaLogInfo> list = new List<ArenaLogInfo>();
                for(int i=0; i<5; i++){
                    ArenaLogInfo log = new ArenaLogInfo();
                    log.Type = 101;
                    list.Add(log);
                }
                osa.SetItems(list);*/
                noRecordText.gameObject.SetActive(resp.Logs.Count == 0);
                osa.SetItems( ProtoUtils.UnPackRepeatedField<ArenaLogInfo>(resp.Logs) );
            });
            //osa.SetItems(Properties.DataList);
        }

        private void OnClose()
        {
            Debug.Log("OnClose");
            //TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<ArenaRecordUI>();
            
        }
    }
}