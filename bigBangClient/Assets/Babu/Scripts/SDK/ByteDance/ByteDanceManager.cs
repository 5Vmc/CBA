using System;
using Babu;
using Babu.SDK;
using UnityEngine;
using BigBang;

public class ByteDanceManager : Singleton<ByteDanceManager>
{
    private readonly string appId = "649738";//咪咕云游戏平台的内容代码
    private AndroidJavaClass javaClassByteDance = null;
    public void Init()
    {
#if !UNITY_EDITOR
        Debug.Log("CbaMiGu , ByteDanceManager , Init , 1");
        SetAndrdoiCallBackByteDance _setAndrodCallback = new SetAndrdoiCallBackByteDance();
        Debug.Log("CbaMiGu , ByteDanceManager , Init , 2");
        // 使用完整的Java类名（包括包名）
        javaClassByteDance = new AndroidJavaClass("com.boyou.cba.cbabytedancelibrary.ByteDanceManager");
        Debug.Log("CbaMiGu , ByteDanceManager , Init , 3");
        // 调用静态方法
        javaClassByteDance.CallStatic("setCallBack", _setAndrodCallback);
        Debug.Log("CbaMiGu , ByteDanceManager , Init , 4");
        javaClassByteDance.CallStatic("SetAppId", appId);
        Debug.Log("CbaMiGu , ByteDanceManager , Init , 5");
        javaClassByteDance.CallStatic("OnCreate");
        Debug.Log("CbaMiGu , ByteDanceManager , Init , 6");
#endif
    }

    public void ReportRegister()
    {
#if !UNITY_EDITOR
        javaClassByteDance.CallStatic("ReportRegister");
#endif
    }
    public void ReportPay(string type, string name, string id, int yuan, bool isSuccess)
    {
#if !UNITY_EDITOR
        javaClassByteDance.CallStatic("ReportPay", type, name, id, yuan, isSuccess);
#endif
    }
    public void SetUserUniqueID(string userId)
    {
#if !UNITY_EDITOR
        javaClassByteDance.CallStatic("SetUserUniqueID", userId);
#endif
    }
    public void ReportLogin()
    {
#if !UNITY_EDITOR
        javaClassByteDance.CallStatic("ReportLogin");
#endif
    }
    public void ReportLevelUp(int level)
    {
#if !UNITY_EDITOR
        javaClassByteDance.CallStatic("ReportLevelUp", level);
#endif
    }
}

public class SetAndrdoiCallBackByteDance : AndroidJavaProxy
{
    public SetAndrdoiCallBackByteDance() : base("com.boyou.cba.cbabytedancelibrary.UnityCallBackManager") { }

}
