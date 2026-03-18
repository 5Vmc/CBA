using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 开启透明度测试
/// 方便不规则形状的按钮
/// 图片上需要开启ReadWriteEnable
/// 图片不能打成图集！
/// </summary>
public class AlphaTestImage : MonoBehaviour
{
    private void Awake()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }
}
