using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Utils
{
    public static class Extensions
    {
        /// <summary>
        /// 设置显示优先级，优先级越高越后渲染
        /// </summary>
        /// <param name="level">级别</param>
        public static void SetPriority(this RectTransform rect, int level)
        {
            var canvas = rect.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = rect.gameObject.AddComponent<Canvas>();
            }
            if (rect.GetComponent<GraphicRaycaster>() == null)
            {
                rect.gameObject.AddComponent<GraphicRaycaster>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = level;
        }

        /// <summary>
        /// 恢复之前的优先级
        /// </summary>
        /// <param name="rect"></param>
        public static void RecoverPriority(this RectTransform rect)
        {
            Component.Destroy(rect.GetComponent<GraphicRaycaster>());
            Component.Destroy(rect.GetComponent<Canvas>());
        }

        /// <summary>
        /// 获得所有的子物体
        /// </summary>
        /// <returns>无子物体返回null</returns>
        public static IEnumerable<Transform> GetChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                yield return transform.GetChild(i);
            }
        }

        public static IEnumerable<T> GetChildren<T>(this Transform transform) where T : Component
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var component = transform.GetChild(i).GetComponent<T>();
                if (component != null)
                {
                    yield return component;
                }
            }
        }

        /// <summary>
        /// 获得UI物体透明度
        /// </summary>
        public static float GetAlpha(this GameObject obj)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = obj.AddComponent<CanvasGroup>();
            }
            return canvasGroup.alpha;
        }

        /// <summary>
        /// 根据路径获得组件
        /// </summary>
        /// <param name="path">路径</param>
        /// <typeparam name="T">组件类型</typeparam>
        public static T GetComponentAtPath<T>(this Transform transform, string path) where T : Component
        {
            Transform t = null;
            if (path == null)
            {
                Debug.LogError("路径为空");
                return null;
            }
            t = transform.Find(path);
            if (t == null)
            {
                Debug.LogError($"找不到路径为：{path}的组件");
                return null;
            }
            return t.GetComponent<T>();
        }

        /// <summary>
        /// 设置图片透明度
        /// </summary>
        /// <param name="alpha">取整范围0-1</param>
        public static void SetAlpha(this Image img, float alpha)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// 设置文本透明度
        /// </summary>
        /// <param name="alpha">取整范围0-1</param>
        public static void SetAlpha(this TMP_Text txt, float alpha)
        {
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// 设置UI物体透明度
        /// </summary>
        /// <param name="alpha">取整范围0-1</param>
        public static void SetAlpha(this GameObject obj, float alpha)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = obj.AddComponent<CanvasGroup>();
            }
            canvasGroup.SetAlpha(alpha);
        }

        /// <summary>
        /// 设置UI物体透明度
        /// </summary>
        /// <param name="alpha">取整范围0-1</param>
        public static void SetAlpha(this CanvasGroup cg, float alpha)
        {
            cg.alpha = alpha;
        }
    }
}