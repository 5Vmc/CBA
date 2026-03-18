package com.unity3d.player;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.os.Bundle;
import android.text.Html;
import android.text.Spanned;
import android.text.method.LinkMovementMethod;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.os.Process;
import android.widget.TextView;

public class UnityPlayerActivity extends Activity implements IUnityPlayerLifecycleEvents
{
    protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code

    // Override this in your custom UnityPlayerActivity to tweak the command line arguments passed to the Unity Android Player
    // The command line arguments are passed as a string, separated by spaces
    // UnityPlayerActivity calls this from 'onCreate'
    // Supported: -force-gles20, -force-gles30, -force-gles31, -force-gles31aep, -force-gles32, -force-gles, -force-vulkan
    // See https://docs.unity3d.com/Manual/CommandLineArguments.html
    // @param cmdLine the current command line arguments, may be null
    // @return the modified command line string or null
    protected String updateUnityCommandLineArguments(String cmdLine)
    {
        return cmdLine;
    }

    Boolean anInt = false;

    // Setup activity layout
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        super.onCreate(savedInstanceState);

        String cmdLine = updateUnityCommandLineArguments(getIntent().getStringExtra("unity"));
        getIntent().putExtra("unity", cmdLine);

        SharedPreferences base = getSharedPreferences("base",MODE_PRIVATE);
        anInt = base.getBoolean("isFirstStart",true);

        if (anInt==true){

            AlertDialog.Builder builder=new AlertDialog.Builder(UnityPlayerActivity.this);
            builder.setTitle("个人信息保护指引");  //设置标题

            String message = "欢迎下载本游戏，我们非常重视个人信息和隐私保护。请在使用我们的服务前，详细阅读并同意 <a href=\"https://cdncba.ximiplay.com/Protocol/migu-privacy-appprotocol.html\">\n《隐私政策》</a> 和 <a href=\"https://passport.migu.cn/portal/appprotocol?sourceid=206026\">《中职篮: 全力以赴软件许可协议》</a> 。\n为了提供完整的游戏体验，我们会向您申请必要的权限和信息。您可选择同意或拒绝权限申请，如果拒绝可能会导致部分游戏体验异常。权限和信息包括:\n存储：用于存储游戏的资源文件，下载游戏包等功能\n电话权限：获取设备标识，用于生成游客账号，实现手机号码登陆功能\n获取账号：获取账号用于登录游戏\n读系统日志：获取游戏错误日志，改进和优化游戏体验\n任务列表：获取游戏是否退到后台或者回到前台\nMAC地址：维护网络运营安全\n应用列表：支付时判断是否已安装微信支付宝\n设备信息: 读取Android-ID、硬件序列号、MEID 、IMEI、IMSI、手机号、OAID、Mac地址信息目的为保护帐号安全\n";
            Spanned myMessage = Html.fromHtml(message);

            builder.setMessage(myMessage); //设置内容
            builder.setCancelable(true);  //是否可以取消
            builder .setNegativeButton("拒绝", new DialogInterface.OnClickListener() {
                @Override
                public void onClick(DialogInterface dialogInterface, int i) {
                    dialogInterface.dismiss();
                    android.os.Process.killProcess(android.os.Process.myPid());
                }
            });

            builder.setPositiveButton("同意", new DialogInterface.OnClickListener() {
                @Override
                public void onClick(DialogInterface dialog, int which) {
                    SharedPreferences.Editor editor = base.edit();
                    editor.putBoolean("isFirstStart",false);
                    editor.commit();
                }
            });

            AlertDialog alertDialog = builder.create();
            alertDialog.show();

            TextView msgTxt = (TextView) alertDialog.findViewById(android.R.id.message);
            msgTxt.setMovementMethod(LinkMovementMethod.getInstance());
        }
        mUnityPlayer = new UnityPlayer(this, this);
        setContentView(mUnityPlayer);
        mUnityPlayer.requestFocus();
    }

    // When Unity player unloaded move task to background
    @Override public void onUnityPlayerUnloaded() {
        moveTaskToBack(true);
    }

    // Callback before Unity player process is killed
    @Override public void onUnityPlayerQuitted() {
    }

    @Override protected void onNewIntent(Intent intent)
    {
        // To support deep linking, we need to make sure that the client can get access to
        // the last sent intent. The clients access this through a JNI api that allows them
        // to get the intent set on launch. To update that after launch we have to manually
        // replace the intent with the one caught here.
        setIntent(intent);
        mUnityPlayer.newIntent(intent);
    }

    // Quit Unity
    @Override protected void onDestroy ()
    {
        mUnityPlayer.destroy();
        super.onDestroy();
    }

    // If the activity is in multi window mode or resizing the activity is allowed we will use
    // onStart/onStop (the visibility callbacks) to determine when to pause/resume.
    // Otherwise it will be done in onPause/onResume as Unity has done historically to preserve
    // existing behavior.
    @Override protected void onStop()
    {
        super.onStop();

        if (!MultiWindowSupport.getAllowResizableWindow(this))
            return;

        mUnityPlayer.pause();
    }

    @Override protected void onStart()
    {
        super.onStart();

        if (!MultiWindowSupport.getAllowResizableWindow(this))
            return;

        mUnityPlayer.resume();
    }

    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();

        MultiWindowSupport.saveMultiWindowMode(this);

        if (MultiWindowSupport.getAllowResizableWindow(this))
            return;

        mUnityPlayer.pause();
    }

    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();

        if (MultiWindowSupport.getAllowResizableWindow(this) && !MultiWindowSupport.isMultiWindowModeChangedToTrue(this))
            return;

        mUnityPlayer.resume();
    }

    // Low Memory Unity
    @Override public void onLowMemory()
    {
        super.onLowMemory();
        mUnityPlayer.lowMemory();
    }

    // Trim Memory Unity
    @Override public void onTrimMemory(int level)
    {
        super.onTrimMemory(level);
        if (level == TRIM_MEMORY_RUNNING_CRITICAL)
        {
            mUnityPlayer.lowMemory();
        }
    }

    // This ensures the layout will be correct.
    @Override public void onConfigurationChanged(Configuration newConfig)
    {
        super.onConfigurationChanged(newConfig);
        mUnityPlayer.configurationChanged(newConfig);
    }

    // Notify Unity of the focus change.
    @Override public void onWindowFocusChanged(boolean hasFocus)
    {
        super.onWindowFocusChanged(hasFocus);
        mUnityPlayer.windowFocusChanged(hasFocus);
    }

    // For some reason the multiple keyevent type is not supported by the ndk.
    // Force event injection by overriding dispatchKeyEvent().
    @Override public boolean dispatchKeyEvent(KeyEvent event)
    {
        if (event.getAction() == KeyEvent.ACTION_MULTIPLE)
            return mUnityPlayer.injectEvent(event);
        return super.dispatchKeyEvent(event);
    }

    // Pass any events not handled by (unfocused) views straight to UnityPlayer
    @Override public boolean onKeyUp(int keyCode, KeyEvent event)     { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onKeyDown(int keyCode, KeyEvent event)   { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onTouchEvent(MotionEvent event)          { return mUnityPlayer.injectEvent(event); }
    /*API12*/ public boolean onGenericMotionEvent(MotionEvent event)  { return mUnityPlayer.injectEvent(event); }
}
