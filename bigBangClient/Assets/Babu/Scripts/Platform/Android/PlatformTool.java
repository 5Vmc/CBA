package com.babu;

import android.Manifest;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Environment;
// import android.support.v4.app.ActivityCompat;
import android.telephony.TelephonyManager;

import androidx.core.app.ActivityCompat;

import java.util.Locale;

public class PlatformTool {
    private static final int REQUEST_EXTERNAL_STORAGE = 1;
    private static String[] PERMISSIONS_STORAGE = {
            Manifest.permission.READ_EXTERNAL_STORAGE,
            Manifest.permission.WRITE_EXTERNAL_STORAGE
    };
    public static Activity unityActivity = null;

    public static void requestExternalStoragePermission() {
        if (hasExternalStoragePermission()) {
            return;
        }

        ActivityCompat.requestPermissions(
                unityActivity,
                PERMISSIONS_STORAGE,
                REQUEST_EXTERNAL_STORAGE);
    }

    public static boolean hasExternalStoragePermission() {
        int permission = ActivityCompat.checkSelfPermission(unityActivity, Manifest.permission.WRITE_EXTERNAL_STORAGE);
        return permission == PackageManager.PERMISSION_GRANTED;
    }

    public static String getExternalStorageDirectory() {
        return Environment.getExternalStorageDirectory().getPath();
    }

    public static String getSystemLanguage() {
        Locale locale = Locale.getDefault();
        return locale.toLanguageTag();
    }

    public static String getPackageName() {
        return unityActivity.getPackageName();
    }

    public static String getCountryCode() {
        return Locale.getDefault().getCountry();
    }

    public static String getSimCountryCode() {
        try {
            final TelephonyManager tm = (TelephonyManager) unityActivity
                    .getSystemService(unityActivity.TELEPHONY_SERVICE);
            final String simCountry = tm.getSimCountryIso();
            if (simCountry != null && simCountry.length() == 2) { // SIM country code is available
                return simCountry.toUpperCase(Locale.US);
            } else if (tm.getPhoneType() != TelephonyManager.PHONE_TYPE_CDMA) { // device is not 3G (would be
                                                                                // unreliable)
                String networkCountry = tm.getNetworkCountryIso();
                if (networkCountry != null && networkCountry.length() == 2) { // network country code is available
                    return networkCountry.toUpperCase(Locale.US);
                }
            }
        } catch (Exception e) {
        }
        return "";
    }

    public static String getCountryName() {
        Locale locale = Locale.getDefault();
        return locale.getDisplayCountry();
    }
}