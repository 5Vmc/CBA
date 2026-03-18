//using Babu;
//using LightJson;
//using System;
//using System.Collections;
//using System.Security.Cryptography;
//using System.Text;
//using UnityEngine;

//// 头条SDK内容校验
//public class TTContentVerify
//{
//#if UNITY_ANDROID
//    const string APP_ID = "233634";
//#elif UNITY_IOS
//    const string APP_ID = "224021";
//#else
//    const string APP_ID = "233634";
//#endif

//    const int APPID_SRC = 0;
//    const int SCENE_TYPE = 1204;
//    const int CC_TYPE = 1;

//    const string ACCESS_KEY = "55edd8e2358679d004074cdbf1fdb8ad";
//    const string SECRET_KEY = "0d98add6aa0418448621ddc6a47052f7";

//    const string URL = "https://bcms.bytedance.com/gcs/api/v1/review_client";

//    public enum Result
//    {
//        Error,
//        Invalid,
//        Valid
//    }

//    public static Result VerifyUsername(string username)
//    {
//        string data = GetPostData(username);
//        ArrayList headers = new ArrayList();
//        headers.Add(GetAuthHeader(data, 100));

//        try
//        {
//            string response;
//            bool result = HttpService.Instance.Post(URL, "application/json;charset=utf-8", headers, data, out response);
//            if (result == false)
//            {
//                return Result.Error;
//            }

//            JsonValue obj = JsonValue.Parse(response);

//            if (obj["err_code"].AsInteger != 0)
//            {
//                return Result.Error;
//            }

//            JsonValue res = obj["data"]["result"];
//            int suggestionCode = res["suggestion"]["suggestion_code"].AsInteger;
//            return suggestionCode == 0 && res["review_details"][0]["word_match_list"].AsJsonArray.Count == 0 ? Result.Valid : Result.Invalid;
//        }
//        catch (Exception e)
//        {
//            Debug.LogError("VerifyUsername failed: " + e.Message);
//            return Result.Error;
//        }
//    }

//    private static string GetAuthHeader(string data, int expireTime = 5)
//    {
//        string signature = Sign(ACCESS_KEY, SECRET_KEY, data, expireTime);
//        return String.Format("Agw-Auth: {0}", signature);
//    }

//    private static string HMacSha256(string key, string data)
//    {
//        var encoding = new System.Text.UTF8Encoding();
//        var hmacsha256 = new HMACSHA256(encoding.GetBytes(key));
//        byte[] result = hmacsha256.ComputeHash(encoding.GetBytes(data));

//        StringBuilder sb = new StringBuilder();
//        foreach (byte b in result)
//        {
//            sb.Append(b.ToString("x2").ToLower());
//        }
//        return sb.ToString();
//    }

//    private static string Sign(string ak, string sk, string data, int expirTime = 5)
//    {
//        string signKeyInfo = String.Format("auth-v1/{0}/{1}/{2}", ak, GetTimeStamp(), expirTime);
//        string signKey = HMacSha256(sk, signKeyInfo);
//        return String.Format("{0}/{1}", signKeyInfo, HMacSha256(signKey, data));
//    }

//    private static string GetPostData(string content_str)
//    {
//        JsonObject data = new JsonObject();
//        data["app_id"] = APP_ID;
//        data["appid_src"] = APPID_SRC;
//        data["scene_type"] = SCENE_TYPE;
//        data["cc_type"] = CC_TYPE;
//        data["msg_id"] = "0";
//        data["send_ts_sec"] = GetTimeStamp();

//        JsonObject content = new JsonObject();
//        content["content_id"] = "0";
//        content["content_type"] = 1;
//        content["content"] = content_str;
//        JsonArray content_list = new JsonArray();
//        content_list.Add(content);
//        data["content_list"] = content_list;
//        return data.ToString(false);
//    }

//    private static long GetTimeStamp()
//    {
//        TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
//        return Convert.ToInt64(ts.TotalSeconds);
//    }
//}
