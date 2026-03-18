using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Babu;

namespace BigBang.UI
{
    
    public class ServerListItem : MonoBehaviour
    {
        [SerializeField] private GameObject[] statusList;
        [SerializeField] private GameObject newFlag;
        [SerializeField] private Button selectButton;
        [SerializeField] private TMP_Text textOfficialName;
        [SerializeField] private TMP_Text textAliasName;

        private ServerData data;
        public void SetData(ServerData data)
        {
            SetStatus(data.Status);
            textOfficialName.text = data.OfficialName;
            textAliasName.text = data.AliasName;

            newFlag.SetActive(data.IsNew);

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener( ()=>{
                AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
                EventManager.Instance.Dispatch(EventID.OnClickChangeServerBtn, data);
            } );
        }

        private void SetStatus(int status)
        {
            //枚举定义在Gameconst.ServerStatus中
            foreach (GameObject go in this.statusList){
                if(go.name == "S" + status){
                    go.SetActive(true);
                }
                else{
                    go.SetActive(false);
                }
            }
        }
    }
}