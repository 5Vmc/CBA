using Babu;
using UnityEngine;

public class MiGuPlayManager : Singleton<MiGuPlayManager>
{
#if MiGuNft
    private readonly string contentCode = "006220750000";//咪咕云游戏平台的内容代码
#else
    private readonly string contentCode = "006220029000";//咪咕云游戏平台的内容代码
#endif
    private AndroidJavaClass javaClassMiGuPlay = null;
    public void Init()
    {
#if UNITY_EDITOR
        return;
#endif
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 1");
        SetAndrdoiCallBackMiGuPlay _setAndrodCallback = new SetAndrdoiCallBackMiGuPlay();
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 2");
        // 使用完整的Java类名（包括包名）
        javaClassMiGuPlay = new AndroidJavaClass("com.boyou.cba.cbamigulibrary.MiGuPlayManager");
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 3");
        // 调用静态方法
        javaClassMiGuPlay.CallStatic("setCallBack", _setAndrodCallback);
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 4");
        javaClassMiGuPlay.CallStatic("SetContentCode", contentCode);
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 5");
        javaClassMiGuPlay.CallStatic("Init", Application.identifier);
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 6");
        javaClassMiGuPlay.CallStatic("OnCreate");
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 7");
        javaClassMiGuPlay.CallStatic("SetLoginCallBack");
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 8");
        javaClassMiGuPlay.CallStatic("SetPayListen");
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 9");
        javaClassMiGuPlay.CallStatic("SetAvoidGame");
        Debug.Log("CbaMiGu , MiGuPlayManager , Init , 10");
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        return;
#endif
        javaClassMiGuPlay.CallStatic("OnDestroy");
    }
    public void ReportCPLoginResult(bool isLoginSuccess)
    {
#if UNITY_EDITOR
        return;
#endif
        javaClassMiGuPlay.CallStatic("ReportCPLoginResult", isLoginSuccess);
    }
    public void Login()
    {
#if UNITY_EDITOR
        return;
#endif
        javaClassMiGuPlay.CallStatic("Login");
    }
    public void LogOut()
    {
#if UNITY_EDITOR
        return;
#endif
        javaClassMiGuPlay.CallStatic("LogOut");
    }
    public void Pay(string gameAccount, string orderId, int orderAmount, string propName, string gameName)
    {
#if UNITY_EDITOR
        return;
#endif
        javaClassMiGuPlay.CallStatic("Pay", gameAccount, orderId, orderAmount, propName, gameName);
    }
}

public class SetAndrdoiCallBackMiGuPlay : AndroidJavaProxy
{
    public SetAndrdoiCallBackMiGuPlay() : base("com.boyou.cba.cbamigulibrary.UnityCallBackManager") { }
    public void OnLoginEnd(string userId)
    {
        if(string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("MiGu登录失败");
            EventManager.Instance?.Dispatch(EventManager.CanNotHotFixId.QUICK_LOGIN_FAIL);
        }
        else
        {
            Debug.Log("MiGu登录成功");
            EventManager.Instance?.Dispatch(EventManager.CanNotHotFixId.QUICK_LOGIN_SUCCESS, userId);
            ByteDanceManager.Instance.SetUserUniqueID(userId);
        }
    }
    public void OnAvoidGameTrig()
    {
        Time.timeScale = 0;
        EventManager.Instance?.Dispatch(EventManager.CanNotHotFixId.AVOID_GAME);
    }
    public void OnPayEnd(bool success)
    {
        if(!success)
        {
            UnityEngine.Debug.Log("MiGu支付失败");
            EventManager.Instance.Dispatch(EventManager.CanNotHotFixId.CHARGE_FAIL);
        }
    }
}
