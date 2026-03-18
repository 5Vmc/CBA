package com.boyou.cba.cbamigulibrary;

import android.app.Activity;
import android.content.Context;
import android.util.Log;

import com.migugame.cpsdk.MiguGame;
import com.migugame.cpsdk.MiguSdk;
import com.migugame.cpsdk.bean.LoginReplyBean;
import com.migugame.cpsdk.callback.AvoidGameListener;
import com.migugame.cpsdk.callback.LoginListener;
import com.migugame.cpsdk.callback.PayListener;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.UUID;

public class MiGuPlayManager {

    private final static MiGuPlayManager INSTANCE = new MiGuPlayManager();

    private MiGuPlayManager() {
    }

    public static MiGuPlayManager getInstance() {
        return INSTANCE;
    }

    public UnityCallBackManager unityCallBackManager = null;

    public void _setCallBack(UnityCallBackManager unityCallBackManager) {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , _setCallBack");
        this.unityCallBackManager = unityCallBackManager;
//        this.unityCallBackManager.StringCallBack("测试通信");
    }

    public static void setCallBack(UnityCallBackManager unityCallBackManager) {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , setCallBack , start");
        MiGuPlayManager.getInstance()._setCallBack(unityCallBackManager);
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , setCallBack , end");
    }

    public static Activity getUnityActivity() {
        return UnityPlayer.currentActivity;
    }

    public static String ContentCode = "";

    /**
     * 设置ContentCode
     * @param contentCode 咪咕云游戏平台的内容代码
     */
    public static void SetContentCode(String contentCode) {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , SetContentCode , contentCode = " + contentCode);
        ContentCode = contentCode;
    }

    /**
     * 初始化 MiguSdk
     *
     * @param packageName 当前应用包名（即游戏包名）
     */
    public static void Init(String packageName) {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , Init , start");
        MiguSdk.init(getUnityActivity(), packageName, ContentCode);
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , Init , end");
    }

    /**
     * SDK 绑定游戏主 Activity
     */
    public static void OnCreate() {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , OnCreate , start");
        MiguGame miguGame = MiguGame.getInstance();
        miguGame.onCreate(getUnityActivity());
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , OnCreate , end");
    }

    /**
     * SDK 解绑游戏主 Activity
     */
    public static void OnDestroy() {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , OnDestroy , start");
        MiguGame miguGame = MiguGame.getInstance();
        miguGame.onDestroy(getUnityActivity());
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , OnDestroy , end");
    }

    /**
     * 设置登录回调
     */
    public static void SetLoginCallBack() {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , SetLoginCallBack , start");
        MiguSdk.setmLogincallback(new LoginListener() {
            @Override
            public void success(LoginReplyBean loginReplyBean) {
                //登录成功, loginReplyBean 参数为登录结果
                Log.d("CbaMiGu", "[Java] MiGuPlayManager , Login , success , info = " + loginReplyBean.toBeanString());
                MiGuPlayManager.getInstance().unityCallBackManager.OnLoginEnd(loginReplyBean.userId);
            }

            @Override
            public void fail(String code, String s) {
                //登录失败, errormsg 参数为登录失败提示信息
                Log.d("CbaMiGu", "[Java] MiGuPlayManager , Login , fail , code = " + code + " , message = " + s);
                MiGuPlayManager.getInstance().unityCallBackManager.OnLoginEnd("");
            }
        });
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , SetLoginCallBack , end");
    }

    /**
     * 上报游戏登录结果
     *
     * @param isSuccess 是否登陆成功
     */
    public static void ReportCPLoginResult(boolean isSuccess) {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , ReportCPLoginResult , start , isSuccess = " + isSuccess);
        String msg = "";
        try {
            JSONObject contentJson = new JSONObject();
            int code = 0;
            if (isSuccess == false) code = 1;
            contentJson.put("code", code);
            String message = "游戏登录成功";
            if (isSuccess == false) message = "游戏登录失败";
            contentJson.put("message", message);

            JSONObject otherJson = new JSONObject();
            otherJson.put("eventType", 50001);
            otherJson.put("Content", contentJson);
            msg = otherJson.toString();
        } catch (JSONException e) {
            Log.d("CbaMiGu", "[Java] MiGuPlayManager , ReportCPLoginResult , send error , e = " + e);
        }
        if (msg.isEmpty() == false) {
            MiguGame.getInstance().sendOtherMessage(msg);
            Log.d("CbaMiGu", "[Java] MiGuPlayManager , ReportCPLoginResult , send success , msg = " + msg);
        }
    }

    /**
     * 咪咕登录
     */
    public static void Login() {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , Login , start");
        MiguGame miguGame = MiguGame.getInstance();
        miguGame.login(getUnityActivity(), ContentCode);
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , Login , end");
    }

    /**
     * 登出
     */
    public static void LogOut() {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , LogOut , start");
        MiguGame miguGame = MiguGame.getInstance();
        miguGame.loginOut(getUnityActivity());
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , LogOut , end");
    }

    /**
     * 设置支付结果回调
     * 这里是客户端的结果，不准
     * 最终支付结果以服务端回调结果为准
     */
    public static void SetPayListen() {
        MiguSdk.setPayListen(new PayListener() {
            @Override
            public void success(String orderid) {
                Log.d("CbaMiGu", "[Java] MiGuPlayManager , Pay reply success , orderid = " + orderid);
                MiGuPlayManager.getInstance().unityCallBackManager.OnPayEnd(true);
            }

            @Override
            public void fail(String orderid, String code, String reason) {
                Log.d("CbaMiGu", "[Java] MiGuPlayManager , Pay reply fail , orderid = " + orderid + " , code = " + code + " , reason = " + reason);
                MiGuPlayManager.getInstance().unityCallBackManager.OnPayEnd(false);
            }

            @Override
            public void cancel(String orderid) {
                Log.d("CbaMiGu", "[Java] MiGuPlayManager , Pay reply cancel , orderid = " + orderid);
                MiGuPlayManager.getInstance().unityCallBackManager.OnPayEnd(false);
            }
        });
    }

    /**
     * 发起支付
     *
     * @param gameName    游戏名称，可以是中文、英文字符串
     * @param gameAccount CP 的游戏账户 ID, 数字/英文字符串（不能是中文）
     * @param orderId     订单号，由游戏生成唯一订单号
     * @param orderAmount 订单金额（单位：分）
     * @param propName    道具名称，可以是中文、英文字符串
     */
    public static void Pay(String gameAccount, String orderId, int orderAmount, String propName, String gameName
    ) {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , Pay , start");
        MiguGame miguGame = MiguGame.getInstance();
        miguGame.pay(getUnityActivity(), gameName, ContentCode, gameAccount, orderId, orderAmount, propName);
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , Pay , end");
    }

    /**
     * 设置被防沉迷的回调
     */
    public static void SetAvoidGame() {
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , SetAvoidGame , start");
        MiguSdk.setmAvoidGameListener(new AvoidGameListener() {
            @Override
            public void avoidGame() {
                Log.d("CbaMiGu", "[Java] MiGuPlayManager , avoidGame , trig");
                //暂停游戏，禁声
                //UI 线程内，如有耗时操作请另起线程
                MiGuPlayManager.getInstance().unityCallBackManager.OnAvoidGameTrig();
            }
        });
        Log.d("CbaMiGu", "[Java] MiGuPlayManager , SetAvoidGame , end");
    }

}
