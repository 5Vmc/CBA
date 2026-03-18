using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Babu.SDK
{
    class SDKLocation : Task
    {
        public static SDKLocation Instance;

        bool _init = false;
        string countryCode;

        public string CountryCode
        { 
            get
            {
                string simCountryCode = Platform.GetSimCountryCode();
                if (simCountryCode != null && simCountryCode.Length > 0)
                {
                    Debug.Log("Get Country From Sim Card");
                    return simCountryCode;
                }
                return _init ? countryCode : Platform.GetCountryCode();
            }
        }

        public override string GetTaskName()
        {
            return nameof(SDKLocation);
        }

        public override void Run(TaskExecutor executor)
        {
            Instance = this;

            //StartCoroutine(GetIpApiData());
            executor.OnChildTaskCompleted();
        }

        [Serializable]
        public class IpApiData
        {
            public string country_code;

            public static IpApiData CreateFromJSON(string jsonString)
            {
                return JsonUtility.FromJson<IpApiData>(jsonString);
            }
        }

        public IEnumerator GetIpApiData()
        {
            string uri = "https://ipapi.co/json/";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                IpApiData ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);

                _init = true;
                countryCode = ipApiData.country_code;
                Debug.Log("Country Code: " + countryCode);
            }
        }
    }
}
