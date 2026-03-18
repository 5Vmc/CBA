using UnityEngine;

public class DefineBabu : MonoBehaviour
{
    public static void PrintDefineInError()
    {
        Debug.LogError("DefineBabu: " + GetDefineStr());
    }

    public static string GetDefineStr()
    {
        string defineStr = "";

#if USER_DEBUG
         defineStr += " , USER_DEBUG";
#endif

#if DEBUG
        defineStr += " , DEBUG";
#endif

#if RELEASE
        defineStr += " , RELEASE";
#endif

#if TD_GAME
        defineStr += " , TD_GAME";
#endif

#if TEST_BUNDLE
        defineStr += " , TEST_BUNDLE";
#endif

#if UNITY_EDITOR
        defineStr += " , UNITY_EDITOR";
#endif

#if UNITY_STANDALONE
        defineStr += " , UNITY_STANDALONE";
#endif

#if UNITY_ANDROID
        defineStr += " , UNITY_ANDROID";
#endif

#if UNITY_IOS
        defineStr += " , UNITY_IOS";
#endif

#if UNITY_WEBGL
        defineStr += " , UNITY_WEBGL";
#endif

        return defineStr;
    }
}
