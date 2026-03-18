using BigBang;
using deVoid.UIFramework;
using UnityEngine;
using Utils;

[ExecuteAlways]
public class FixHeightByScreenTComponent : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform = null;
    [SerializeField] private float height219 = 190f;
    [SerializeField] private float height169 = 180f;
    private void OnEnable()
    {
        FixScreen();
    }
    private void FixScreen()
    {
        if (rectTransform == null) rectTransform = transform as RectTransform;
        float screenT = UICommon.HomeScreenLerpT;
        float fixHeight = Mathf.Lerp(height169, height219, screenT);
        rectTransform.SetSizeDeltaHeight(fixHeight);
    }

}
