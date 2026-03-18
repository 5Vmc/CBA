# Add project specific ProGuard rules here.
# You can control the set of applied configuration files using the
# proguardFiles setting in build.gradle.
#
# For more details, see
#   http://developer.android.com/guide/developing/tools/proguard.html

# If your project uses WebView with JS, uncomment the following
# and specify the fully qualified class name to the JavaScript interface
# class:
#-keepclassmembers class fqcn.of.javascript.interface.for.webview {
#   public *;
#}

# Uncomment this to preserve the line number information for
# debugging stack traces.
#-keepattributes SourceFile,LineNumberTable

# If you keep the line number information, uncomment this to
# hide the original source file name.
#-renamesourcefileattribute SourceFile

# messagesdk
-keep class com.cloudplay.messagesdk.**{ *;}
#-keep class sun.misc.Unsafe { *; }
-keepclassmembers enum * { #保持枚举 enum 类不被混淆
 public static **[] values();
 public static ** valueOf(java.lang.String);
}
-keep class * implements android.os.Parcelable {#保持 Parcelable 不被混淆
 public static final android.os.Parcelable$Creator *;
}
# Explicitly preserve all serialization members. The Serializable interface
# is only a marker interface, so it wouldn't save them.
-keep public class * implements java.io.Serializable {*;}
-keepclassmembers class * implements java.io.Serializable {
 static final long serialVersionUID;
 private static final java.io.ObjectStreamField[] serialPersistentFields;
 private void writeObject(java.io.ObjectOutputStream);
 private void readObject(java.io.ObjectInputStream);
 java.lang.Object writeReplace();
 java.lang.Object readResolve();
}
# pay
-keep class cn.emagsoftware.gamehall.**{*;}
# cpsdk
-keep class com.migugame.cpsdk.**{*;}
#okdownload
-keep class com.liulishuo.okdownload.**{*;}
-keep class cn.hutool.**{*;}
-keep class org.bouncycastle.**{*;}
#for amber
-keep class com.migu.sdk.union.**{*;}
-keep class com.migu.unionsdk.**{*;}
-keep class com.android.internal.http.**{*;}
-keep class org.apache.**{*;}
-keep class com.migu.uem.**{*;}
-keep class com.migu.hotfix.**{*;}
-keepattributes *Annotation*
#dory
-keep class org.webrtc.**{*;}
-keep class cn.migu.tsg.**{*;}