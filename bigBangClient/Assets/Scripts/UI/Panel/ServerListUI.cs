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
using LightJson;
using System.Linq;

namespace BigBang.UI
{

    public class ServerListUI : AWindowController<WindowProperties>
    {
        delegate void ServerCallback(List<ServerData> dataList);

        [SerializeField] private Button closeBtn;
        [SerializeField] private ConfirmationBoxUIAnim Anim;

        [SerializeField] private ServerListAdapter allServerOsa;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle recommendToggle;
        [SerializeField] private BabuToggle allToggle;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);

            recommendToggle.OnSelect += OnRecommendToggleSelect;
            allToggle.OnSelect += OnAllToggleSelect;
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);

            recommendToggle.OnSelect -= OnRecommendToggleSelect;
            allToggle.OnSelect -= OnAllToggleSelect;
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            string url = $"{ServerConst.SERVER_LIST_URL}?game_name={Application.identifier}&get_new=1";
            this.GetServerList(url, this.SetServers, true);

            toggleGroup.Switch(recommendToggle);

            Anim.PlayEnter();
        }

        private void GetServerList(String url, ServerCallback callback, bool isNew)
        {

            //获取新的服务器
            UnityHttpServiceFix.Instance.AsyncGet(url, (bool result, string response) =>
            {
                List<ServerData> serverList = new List<ServerData>();
                try
                {

                    if (result == false) throw new Exception("Request Failed");

                    JsonValue json = JsonValue.Parse(response);
                    foreach (JsonValue jsonItem in json.AsJsonArray)
                    {
                        serverList.Add(new ServerData(jsonItem));
                    }

                    List<ServerData> orderdServerList = null;
                    if(isNew)
                    {
                        orderdServerList = serverList.Where(server => server.IsNew).OrderByDescending(server => server.Id).ToList();
                    }
                    else
                    {
                        orderdServerList = serverList.OrderByDescending(server => server.Id).ToList();
                    }
                    callback(orderdServerList);
                }
                catch (Exception ex)
                {
                    Debug.LogError("获取服务器列表失败:" + ex.Message);
                    Tips.PopTips("获取服务器列表失败:" + ex.Message);
                }
            });
        }

        private void SetServers(List<ServerData> dataList)
        {
            allServerOsa.SetItems(dataList);
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.CloseWindow<ServerListUI>();
        }


        private void OnRecommendToggleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            //adapter.SetData(true, Properties.CompitionID, Properties.leagueLevel);
            string url = $"{ServerConst.SERVER_LIST_URL}?game_name={Application.identifier}&get_new=1";
            this.GetServerList(url, this.SetServers, true);
        }

        private void OnAllToggleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            string url = $"{ServerConst.SERVER_LIST_URL}?game_name={Application.identifier}";
            this.GetServerList(url, this.SetServers, false);
        }
    }
}