package com.boyou.cba.cbabytedancelibrary;

import android.app.Activity;
import android.util.Log;

import com.bytedance.ads.convert.BDConvert;
import com.bytedance.applog.AppLog;
import com.bytedance.applog.InitConfig;
import com.bytedance.applog.game.GameReportHelper;
import com.bytedance.applog.util.UriConstants;
import com.unity3d.player.UnityPlayer;

public class ByteDanceManager {

    private final static ByteDanceManager INSTANCE = new ByteDanceManager();

    private ByteDanceManager() {
    }

    public static ByteDanceManager getInstance() {
        return INSTANCE;
    }

    public UnityCallBackManager unityCallBackManager = null;

    public void _setCallBack(UnityCallBackManager unityCallBackManager) {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , _setCallBack");
        this.unityCallBackManager = unityCallBackManager;
    }

    public static void setCallBack(UnityCallBackManager unityCallBackManager) {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , setCallBack , start");
        ByteDanceManager.getInstance()._setCallBack(unityCallBackManager);
        Log.d("CbaMiGu", "[Java] ByteDanceManager , setCallBack , end");
    }

    public static Activity getUnityActivity() {
        return UnityPlayer.currentActivity;
    }

    public static String AppId = "";

    /**
     * 设置AppId
     *
     * @param appId 咪咕云游戏平台的内容代码
     */
    public static void SetAppId(String appId) {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , SetAppId , appId = " + appId);
        AppId = appId;
    }

    public static void OnCreate() {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , OnCreate , start");
        // 对线程不敏感，可放入子线程执行初始化
        /* 初始化SDK开始 */
        // 第一个参数APPID: 参考2.1节获取
        // 第二个参数CHANNEL: 填写渠道信息，请注意不能为空
        final InitConfig config = new InitConfig(AppId, "MiGuPlay");
        Log.d("CbaMiGu", "[Java] ByteDanceManager , OnCreate , 1");

        // 设置数据上送地址
        config.setUriConfig(UriConstants.DEFAULT);
        config.setImeiEnable(true);//建议关停获取IMEI（出于合规考虑）
        config.setAutoTrackEnabled(true); // 全埋点开关，true开启，false关闭
        config.setLogEnable(true); // true:开启日志，参考4.3节设置logger，false:关闭日志
        AppLog.setEncryptAndCompress(true); // 加密开关，true开启，false关闭
        config.setEnablePlay(true); // 配置心跳事件（时长统计）
        Log.d("CbaMiGu", "[Java] ByteDanceManager , OnCreate , 2");

        //SDK会采集OAID、ANDROID_ID和其他的设备特征字段，请遵循相关合规要求在隐私弹窗后采集
        BDConvert.getInstance().init(getUnityActivity(), AppLog.getInstance());
        Log.d("CbaMiGu", "[Java] ByteDanceManager , OnCreate , 3");
        // 如果在 onCreate 阶段初始化拿不到 XXXActivity 则不需要传递第三个参数
        AppLog.init(getUnityActivity(), config, getUnityActivity());
        /* 初始化SDK结束 */
        Log.d("CbaMiGu", "[Java] ByteDanceManager , OnCreate , end");
    }

    /**
     * 上报注册
     * 内置事件: “注册” ，属性：注册方式，是否成功，属性值为：wechat ，true
     */
    public static void ReportRegister() {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportRegister , start");
        GameReportHelper.onEventRegister("MiGuPlay", true);
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportRegister , end");
    }

    /**
     * 上报支付
     * 内置事件 “支付”，属性：商品类型，商品名称，商品ID，商品数量，支付渠道，币种，是否成功（必传），金额（必传）
     * 付费金额单位为元
     */
    public static void ReportPay(String type, String name, String id, int yuan, boolean isSuccess) {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportPay , start");
        GameReportHelper.onEventPurchase(type, name, id, 1, "unknow", "¥", isSuccess, yuan);
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportPay , end");
    }

    /**
     * 设置用户Id
     *
     * @param userId 用户id
     */
    public static void SetUserUniqueID(String userId) {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , SetUserUniqueID , start");
        AppLog.setUserUniqueID(userId);
        Log.d("CbaMiGu", "[Java] ByteDanceManager , SetUserUniqueID , end");
    }

    /**
     * 上报登录
     */
    public static void ReportLogin() {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportLogin , start");
        GameReportHelper.onEventLogin("MiGuPlay", true);
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportLogin , end");
    }

    /**
     * 上报升级
     *
     * @param level 等级
     */
    public static void ReportLevelUp(int level) {
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportLevelUp , start , level = " + level);
        GameReportHelper.onEventUpdateLevel(level);
        Log.d("CbaMiGu", "[Java] ByteDanceManager , ReportLevelUp , end");
    }

}
