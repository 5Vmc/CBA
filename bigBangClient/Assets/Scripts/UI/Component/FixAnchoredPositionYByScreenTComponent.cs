using BigBang;
using deVoid.UIFramework;
using UnityEditor;
using UnityEngine;
using Utils;

[ExecuteAlways]
public class FixAnchoredPositionYByScreenTComponent : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform = null;
    [SerializeField] private float y219 = 190f;
    [SerializeField] private float y169 = 180f;
    private void OnEnable()
    {
        FixScreen();
    }
    public float fixY = 0;
    private float FixScreen()
    {
        if (rectTransform == null) rectTransform = transform as RectTransform;
        float screenT = UICommon.HomeScreenLerpT;
        fixY = Mathf.Lerp(y169, y219, screenT);
        rectTransform.SetAnchoredPositionY(fixY);
        return fixY;
    }

}
