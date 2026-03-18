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

namespace BigBang.UI
{
    public class FBTowerBuffUI : AWindowController<WindowProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button closeBtn1;
        [SerializeField] private List<TMP_Text> buffTextList;
        [SerializeField] private FBTowerBuffUIAnim anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            closeBtn1.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            closeBtn1.onClick.RemoveListener(OnClose);
        }


        private void RefreshData()
        {
            StringBuilder sb = new StringBuilder();
            int Counter = 0;
            foreach (var key in FBTowerController.Instance.FBData.buffs.Keys)
            {
                sb.AppendFormat("<color=#E7E7E7><size=28>{0}</size></color>\n", Configs.SeparatedPosition.GetConfig(key).Name);
                foreach (var key1 in FBTowerController.Instance.FBData.buffs[key].Keys)
                {
                    sb.AppendFormat("<color=#233859><size=24>{0}</size></color><color=#FED701><size=26>+{1}%</size></color>\n", Configs.CardAbility.GetConfig(key1).Name, FBTowerController.Instance.FBData.buffs[key][key1]);
                }
                buffTextList[Counter].text = sb.ToString();
                sb.Clear();
                Counter++;
            }
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            RefreshData();
            anim.PlayEnter();
        }

        private void OnClose()
        {
            anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<FBTowerBuffUI>();
            });
        }

    }
}