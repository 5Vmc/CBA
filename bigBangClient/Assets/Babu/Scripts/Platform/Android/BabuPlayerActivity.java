package com.unity3d.player;

import android.os.Bundle;

import com.babu.PlatformTool;

public class BabuPlayerActivity extends UnityPlayerActivity
{
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        PlatformTool.unityActivity = this;
    }
}