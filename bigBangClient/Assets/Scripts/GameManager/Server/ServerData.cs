using System;
using LightJson;
using Utils;

namespace BigBang
{
    public class ServerData
    {
        public int Id;
        public int Status;
        public string OfficialName;
        public string AliasName;

        public string Ip;
        public int Port;

        public bool IsNew;

        public ServerData(JsonValue json)
        {
            try{
#if !UNITY_WEBGL
                string[] ipAndPort = json["host"].AsString.Split(":");
#else
                string[] ipAndPort = json["wsHost"].AsString.Split(":");
#endif
                Ip = ipAndPort[0];
                Port = int.Parse(ipAndPort[1]);

                Id = ((int)json["id"].AsNumber);

                Status = ((int)json["status"].AsNumber);

                OfficialName = json["officialName"].AsString;

                AliasName = json["aliasName"].AsString;

                IsNew = json["new"].AsBoolean;
            }
            catch(Exception e){
                Tips.PopTips("解析json错误: " + e.ToString());
            }


        }

    }
}