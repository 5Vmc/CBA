using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolicyConst
{
//#if UNITY_IOS
//        //道义
//        public static readonly string URL_POLICY = "https://xmzt.ximiplay.com/web/daoyi/privacy%20policy.html"; //隐私政策
//        public static readonly string URL_PRIVACY = "https://xmzt.ximiplay.com/web/daoyi/service%20permission.html";//中职篮：全力以赴软件许可协议
//#else
//    //西米
//    public static readonly string URL_POLICY = "https://xmzt.ximiplay.com/web/ximi/privacy%20policy.html"; //隐私政策
//    public static readonly string URL_PRIVACY = "https://xmzt.ximiplay.com/web/ximi/service%20permission.html";//中职篮：全力以赴软件许可协议

//    //咪咕
//    //public static readonly string MIGU_URL_POLICY = "https://passport.migu.cn/portal/privacy/appprotocol?sourceid=206026"; //隐私政策
    public static readonly string MIGU_URL_POLICY = "https://cdncba.ximiplay.com/Protocol/migu-privacy-appprotocol.html"; //隐私政策
    public static readonly string MIGU_URL_PRIVACY = "https://passport.migu.cn/portal/appprotocol?sourceid=206026";//中职篮：全力以赴软件许可协议
//#endif

    public static string GetPolicyUrl()//隐私政策
    {
        var url = PolicyConst.MIGU_URL_POLICY;
        return url;
    }
    public static string GetPrivacyUrl()//用户协议
    {
        var url = PolicyConst.MIGU_URL_PRIVACY;
        return url;
    }
}
