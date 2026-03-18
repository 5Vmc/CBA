using System.Collections;
using System.Collections.Generic;
using BigBang;

using UnityEngine;
using UnityEngine.UI;
using Utils;

/// <summary>
/// 使用图片拼接形成一句文字
/// </summary>
[DisallowMultipleComponent]
public class ImageFont : MonoBehaviour
{
    [Header("此节点的pivot同时控制文字的伸展方向")]
    [CustomLabel("默认文字")]
    public string savedString = "";
    [CustomLabel("图集名称")]
    public string atlasName = "";
    [CustomLabel("图片名称前缀")]
    public string perfix = "";
    [CustomLabel("空格宽度")]
    public float spaceWidth = 10;
    [CustomLabel("文字间距")]
    public float charDistanceX = 2;
    [CustomLabel("可被点击")]
    public bool raycastTarget = false;
    [CustomLabel("可被mask")]
    public bool maskable = false;

    private RectTransform _NowRectTrans = null;
    public RectTransform NowRectTrans
    {
        get
        {
            if (_NowRectTrans == null)
            {
                _NowRectTrans = GetComponent<RectTransform>();
            }
            return _NowRectTrans;
        }
    }



    public void OnEnable()
    {
        if (Application.isPlaying == false) return;
        RefreshStringImage();
    }

    /// <summary>
    /// 设置或获取文字，当文字不同时会刷新
    /// </summary>
    public string text
    {
        set
        {
            if (Application.isPlaying == false) return;
            savedString = value;
            if (string.IsNullOrWhiteSpace(savedString) == true)
            {
                savedString = "";
            }
            if (this.enabled == true && gameObject.activeInHierarchy == true)
            {
                RefreshStringImage();
            }
        }
        get
        {
            return savedString;
        }
    }


    private string oldStr = "";
    private List<Image> charImageList = new();
    private List<RectTransform> charRectTransList = new();
    /// <summary>
    /// 刷新文字
    /// </summary>
    /// <param name="ignoreSame">当文字不改变时不刷新</param>
    public async void RefreshStringImage(bool ignoreSame = true)
    {
        if (Application.isPlaying == false) return;
        if (string.IsNullOrWhiteSpace(savedString) == true)
        {
            savedString = "";
        }
        if (ignoreSame == true)
        {
            if (this.oldStr == this.savedString)
            {
                return;
            }
        }
        oldStr = savedString;
        for (int index = 0; index < savedString.Length; index++)
        {
            char character = savedString[index];
            string imageName = character.ToString();

            Image charImage = null;
            GameObject charGo = null;
            RectTransform charRectTrans = null;
            if (index < charImageList.Count)
            {
                charImage = charImageList[index];
                charGo = charImage.gameObject;
                charGo.SetActive(true);
                charRectTrans = charRectTransList[index];
            }
            else
            {
                charGo = new();
                charRectTrans = charGo.AddComponent<RectTransform>();
                charRectTrans.SetParent(transform);
                charRectTrans.pivot = new Vector2(0, 0.5f);
                charRectTrans.localScale = Vector3.one;
                charRectTrans.localPosition = Vector3.zero;
                charRectTrans.localRotation = Quaternion.identity;
                charImage = charGo.AddComponent<Image>();
                charImage.raycastTarget = raycastTarget;
                charImage.maskable = maskable;
                charGo.SetActive(true);
                charImageList.Add(charImage);
                charRectTransList.Add(charRectTrans);
            }
            Sprite sprite = null;
            charImage.SetAlpha(0);
            if (character != ' ')
            {
                if (character >= 'A' && character <= 'Z') imageName += "L";
                imageName = perfix + imageName;
                sprite = await SpriteManager.GetSprite(atlasName, imageName);
            }
            if (sprite == null || sprite.name == "default" || sprite.name == "error")
            {
                charRectTrans.sizeDelta = new Vector2(this.spaceWidth, charRectTrans.sizeDelta.y);
                charImage.SetAlpha(0);
                if (character != ' ')
                {
                    Debug.Log("ImageFont RefreshToString : no sprite loaded . imageName : " + imageName);
                }
            }
            else
            {
                charImage.sprite = sprite;
                charImage.SetNativeSize();
                charImage.SetAlpha(1);
            }
        }
        for (int index = savedString.Length; index < charImageList.Count; index++)
        {
            charImageList[index].gameObject.SetActive(false);
        }
        float xNow = 0;
        float width = 0;
        for (int index = 0; index < charImageList.Count; index++)
        {
            Image charImage = charImageList[index];
            GameObject charGo = charImage.gameObject;
            if (charGo.activeSelf == false) break;
            RectTransform charRectTrans = charRectTransList[index];
            xNow += charRectTrans.sizeDelta.x;
            if (index != charImageList.Count - 1)
            {
                xNow += charDistanceX;
            }
        }
        width = xNow;
        if (charImageList.Count > 0 && charImageList[0].gameObject.activeSelf == true)
        {
            NowRectTrans.sizeDelta = new Vector2(width, charRectTransList[0].sizeDelta.y);
        }
        else
        {
            NowRectTrans.sizeDelta = Vector2.zero;
        }
        xNow = 0;
        for (int index = 0; index < charImageList.Count; index++)
        {
            Image charImage = charImageList[index];
            GameObject charGo = charImage.gameObject;
            if (charGo.activeSelf == false) break;
            RectTransform charRectTrans = charRectTransList[index];
            charRectTrans.localPosition = new Vector3(xNow - NowRectTrans.pivot.x * width, -NowRectTrans.sizeDelta.y * (NowRectTrans.pivot.y - 0.5f), 0);
            xNow += charRectTrans.sizeDelta.x;
            if (index != charRectTransList.Count - 1)
            {
                xNow += charDistanceX;
            }
        }
    }

}
