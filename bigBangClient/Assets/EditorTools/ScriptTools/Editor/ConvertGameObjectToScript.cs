using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BigBang.UI;
using Spine.Unity;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ConvertGameObjectToScript : MonoBehaviour
{
    [MenuItem("GameObject/将所选转换为代码", true)]
    private static bool ValidateCreateChild()
    {
        if (Selection.objects == null || Selection.objects.Length <= 0) return false;
        GameObject[] gameObjects = Selection.gameObjects;
        return gameObjects.Length > 0;
    }

    [MenuItem("GameObject/将所选对象转换为代码", false, 0)]
    private static void DoConvertGameObjectToScript()
    {
        if (Selection.objects == null || Selection.objects.Length <= 0) return;

        Debug.Log("开始将所选组件转换为代码");
        UnityEngine.Object[] objects = Selection.objects;
        string memberstring = "";
        for (int i = 0; i < objects.Length; i++)
        {
            string typeName = GetUIType((objects[i] as GameObject).transform).ToString();
            string memberName = FirstCharToLower(objects[i].name);
            if (memberstring != "") memberstring += "\n";
            memberstring += "[SerializeField] private " + typeName + " " + memberName + " = null;";
        }
        Selection.objects = null;

        Debug.Log(memberstring);
        CopyStrToClipboard(memberstring);
        Debug.Log("已将所选组件的代码写入剪切板");
    }

    public static void CopyStrToClipboard(string str)
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = str;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    public static string FirstCharToLower(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "unknownName";
        return input.First().ToString().ToLower() + input.Substring(1);
    }

    public enum UIType
    {
        Transform,
        Image,
        RawImage,
        Button,
        Toggle,
        Slider,
        Scrollbar,
        Dropdown,
        InputField,
        ScrollRect,
        Text,
        ToggleGroup,
        Canvas,
        RectTransform,
        Animator,
        CanvasGroup,
        GridLayoutGroup,
        HorizontalLayoutGroup,
        VerticalLayoutGroup,
        TMP_Text,
        SkeletonGraphic,

        ImageFont,
        BabuButton,
        InventoryItem,
        BabuLongPressButton,
        PeakImage,
    }

    public static UIType GetUIType(Transform trans)
    {
        if (null != trans.GetComponent<PeakImage>()) return UIType.PeakImage;
        if (null != trans.GetComponent<BabuLongPressButton>()) return UIType.BabuLongPressButton;
        if (null != trans.GetComponent<InventoryItem>()) return UIType.InventoryItem;
        if (null != trans.GetComponent<SkeletonGraphic>()) return UIType.SkeletonGraphic;
        if (null != trans.GetComponent<ScrollRect>()) return UIType.ScrollRect;
        if (null != trans.GetComponent<ImageFont>()) return UIType.ImageFont;
        if (null != trans.GetComponent<InputField>()) return UIType.InputField;
        if (null != trans.GetComponent<TMP_Text>()) return UIType.TMP_Text;
        if (null != trans.GetComponent<Text>()) return UIType.Text;
        if (null != trans.GetComponent<BabuButton>()) return UIType.BabuButton;
        if (null != trans.GetComponent<Button>()) return UIType.Button;
        if (null != trans.GetComponent<RawImage>()) return UIType.RawImage;
        if (null != trans.GetComponent<Toggle>()) return UIType.Toggle;
        if (null != trans.GetComponent<Slider>()) return UIType.Slider;
        if (null != trans.GetComponent<Scrollbar>()) return UIType.Scrollbar;
        if (null != trans.GetComponent<Image>()) return UIType.Image;
        if (null != trans.GetComponent<ToggleGroup>()) return UIType.ToggleGroup;
        if (null != trans.GetComponent<Animator>()) return UIType.Animator;
        if (null != trans.GetComponent<CanvasGroup>()) return UIType.CanvasGroup;
        if (null != trans.GetComponent<Canvas>()) return UIType.Canvas;
        if (null != trans.GetComponent<GridLayoutGroup>()) return UIType.GridLayoutGroup;
        if (null != trans.GetComponent<HorizontalLayoutGroup>()) return UIType.HorizontalLayoutGroup;
        if (null != trans.GetComponent<VerticalLayoutGroup>()) return UIType.VerticalLayoutGroup;
        if (null != trans.GetComponent<RectTransform>()) return UIType.RectTransform;
        return UIType.Transform;
    }

}
