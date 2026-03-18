using BigBang;
using deVoid.UIFramework;
using UnityEditor;
using UnityEngine;
using Utils;

[ExecuteAlways]
public class FixScaleByScreenTComponent : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform = null;
    [SerializeField] private float scale219 = 1.0f;
    [SerializeField] private float scale169 = 1.0f;
    private void OnEnable()
    {
        FixScreen();
    }
    public float scaleY = 0;
    private float FixScreen()
    {
        if (rectTransform == null) rectTransform = transform as RectTransform;
        float screenT = UICommon.HomeScreenLerpT;
        scaleY = Mathf.Lerp(scale169, scale219, screenT);
        rectTransform.SetLocalScale(scaleY);
        return scaleY;
    }

}
