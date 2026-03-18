using UnityEngine;
using System.Linq;

namespace Utils
{
    public static class RectTransformExtensions
    {
        /// <summary>
        /// 判断2个矩形是否有重叠
        /// </summary>
        /// <returns>有重叠返回true，没有重叠返回false</returns>
        static Vector3[] worldPos1 = new Vector3[4];
        static Vector3[] worldPos2 = new Vector3[4];
        public static bool IsOverlap(this RectTransform rect1, RectTransform rect2)
        {
            rect1.GetWorldCorners(worldPos1);
            rect2.GetWorldCorners(worldPos2);
            bool xNotOverlap = worldPos1[2].x <= worldPos2[0].x || worldPos2[2].x <= worldPos1[0].x;
            bool yNotOverlap = worldPos1[1].y <= worldPos2[3].y || worldPos2[1].y <= worldPos1[3].y;
            bool notOverlap = xNotOverlap || yNotOverlap;
            return !notOverlap;
        }

        /// <summary>
        /// 获得世界坐标下左下角的坐标值
        /// </summary>
        public static Vector3 GetLeftBottomFromWorld(this RectTransform rect)
        {
            //左下角、左上角、右上角、右下角
            var worldPos = new Vector3[4];
            rect.GetWorldCorners(worldPos);
            return worldPos[0];
        }

        /// <summary>
        /// 获得屏幕坐标下左下角的坐标值
        /// </summary>
        public static Vector3 GetLeftBottomFromScreen(this RectTransform rect, Camera camera)
        {
            return camera.WorldToScreenPoint(rect.GetLeftBottomFromWorld());
        }

        /// <summary>
        /// 获得视口坐标下坐下的坐标值
        /// </summary>
        public static Vector3 GetLeftBottomFromViewport(this RectTransform rect, Camera camera)
        {
            return camera.WorldToViewportPoint(rect.GetLeftBottomFromWorld());
        }


        /// <summary>
        /// 获得世界坐标下左上角的坐标值
        /// </summary>
        public static Vector3 GetLeftTopFromWorld(this RectTransform rect)
        {
            //左下角、左上角、右上角、右下角
            var worldPos = new Vector3[4];
            rect.GetWorldCorners(worldPos);
            return worldPos[1];
        }

        /// <summary>
        /// 获得屏幕坐标下左上角的坐标值
        /// </summary>
        public static Vector3 GetLeftTopFromScreen(this RectTransform rect, Camera camera)
        {
            return camera.WorldToScreenPoint(rect.GetLeftTopFromWorld());
        }

        /// <summary>
        /// 获得视口坐标下左上角的坐标值
        /// </summary>
        public static Vector3 GetLeftTopFromViewport(this RectTransform rect, Camera camera)
        {
            return camera.WorldToViewportPoint(rect.GetLeftTopFromWorld());
        }

        /// <summary>
        /// 获得世界坐标下右上角的坐标值
        /// </summary>
        public static Vector3 GetRightTopFromWorld(this RectTransform rect)
        {
            //左下角、左上角、右上角、右下角
            var worldPos = new Vector3[4];
            rect.GetWorldCorners(worldPos);
            return worldPos[3];
        }

        /// <summary>
        /// 获得屏幕坐标下右上角的坐标值
        /// </summary>
        public static Vector3 GetRightTopFromScreen(this RectTransform rect, Camera camera)
        {
            return camera.WorldToScreenPoint(rect.GetRightTopFromWorld());
        }

        /// <summary>
        /// 获得视口坐标下右上角的坐标值
        /// </summary>
        public static Vector3 GetRightTopFromViewport(this RectTransform rect, Camera camera)
        {
            return camera.WorldToViewportPoint(rect.GetRightTopFromWorld());
        }

        /// <summary>
        /// 获得世界坐标下右下角的坐标值
        /// </summary>
        public static Vector3 GetRightBottomFromWorld(this RectTransform rect)
        {
            //左下角、左上角、右上角、右下角
            var worldPos = new Vector3[4];
            rect.GetWorldCorners(worldPos);
            return worldPos[2];
        }

        /// <summary>
        /// 获得屏幕坐标下右下角的坐标值
        /// </summary>
        public static Vector3 GetRightBottomFromScreen(this RectTransform rect, Camera camera)
        {
            return camera.WorldToScreenPoint(rect.GetRightBottomFromWorld());
        }

        /// <summary>
        /// 获得视口坐标下右下角的坐标值
        /// </summary>
        public static Vector3 GetRightBottomFromViewport(this RectTransform rect, Camera camera)
        {
            return camera.WorldToViewportPoint(rect.GetRightBottomFromWorld());
        }

        /// <summary>
        /// 获得世界坐标下的宽度
        /// </summary>
        public static float GetWidthFromWorld(this RectTransform rect)
        {
            return Mathf.Abs(rect.GetRightBottomFromWorld().x - rect.GetLeftBottomFromWorld().x);
        }

        /// <summary>
        /// 获得屏幕坐标下的宽度
        /// </summary>
        public static float GetWidthFromScreen(this RectTransform rect, Camera camera)
        {
            return Mathf.Abs(rect.GetRightBottomFromScreen(camera).x - rect.GetLeftBottomFromScreen(camera).x);
        }

        /// <summary>
        /// 获得视口坐标下的宽度
        /// </summary>
        public static float GetWidthFromViewport(this RectTransform rect, Camera camera)
        {
            return Mathf.Abs(rect.GetRightBottomFromViewport(camera).x - rect.GetLeftBottomFromViewport(camera).x);
        }

        /// <summary>
        /// 获得世界坐标下的高度
        /// </summary>
        public static float GetHeightFromWorld(this RectTransform rect)
        {
            return Mathf.Abs(rect.GetLeftTopFromWorld().y - rect.GetLeftBottomFromWorld().y);
        }

        /// <summary>
        /// 获得屏幕坐标下的高度
        /// </summary>
        public static float GetHeightFromScreen(this RectTransform rect, Camera camera)
        {
            return Mathf.Abs(rect.GetLeftTopFromScreen(camera).y - rect.GetLeftBottomFromScreen(camera).y);

        }

        /// <summary>
        /// 获得视口坐标下的高度
        /// </summary>
        public static float GetHeightFromViewport(this RectTransform rect, Camera camera)
        {
            return Mathf.Abs(rect.GetLeftTopFromViewport(camera).y - rect.GetLeftBottomFromViewport(camera).y);
        }

        /// <summary>
        /// 获得世界坐标下的矩形面积
        /// </summary>
        public static float GetAreaFromWorld(this RectTransform rect)
        {
            return rect.GetWidthFromWorld() * rect.GetHeightFromWorld();
        }

        /// <summary>
        /// 获得屏幕坐标下的矩形面积
        /// </summary>
        public static float GetAreaFromScreen(this RectTransform rect, Camera camera)
        {
            return rect.GetWidthFromScreen(camera) * rect.GetHeightFromScreen(camera);
        }

        /// <summary>
        /// 获得视口坐标下的矩形面积
        /// </summary>
        public static float GetAreaFromViewport(this RectTransform rect, Camera camera)
        {
            return rect.GetWidthFromViewport(camera) * rect.GetHeightFromViewport(camera);
        }

        /// <summary>
        /// 设置锚点x坐标
        /// </summary>
        public static void SetAnchoredPosition(this RectTransform rect, Vector2 vec2)
        {
            rect.anchoredPosition = vec2;
        }

        /// <summary>
        /// 设置锚点x坐标
        /// </summary>
        public static void SetAnchoredPositionX(this RectTransform rect, float x)
        {
            rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
        }

        /// <summary>
        /// 设置锚点y坐标
        /// </summary>
        public static void SetAnchoredPositionY(this RectTransform rect, float y)
        {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        }

        /// <summary>
        /// 设置右边界
        /// </summary>
        public static void SetRight(this RectTransform rect, float x)
        {
            rect.offsetMax = new Vector2(-x, rect.offsetMax.y);
        }

        /// <summary>
        /// 设置上边界
        /// </summary>
        public static void SetTop(this RectTransform rect, float y)
        {
            rect.offsetMax = new Vector2(rect.offsetMax.x, -y);
        }

        /// <summary>
        /// 设置左边界
        /// </summary>
        public static void SetLeft(this RectTransform rect, float x)
        {
            rect.offsetMin = new Vector2(x, rect.offsetMin.y);
        }

        /// <summary>
        /// 设置下边界
        /// </summary>
        public static void SetBottom(this RectTransform rect, float y)
        {
            rect.offsetMin = new Vector2(rect.offsetMin.x, y);
        }

        /// <summary>
        /// 获得世界坐标下的重叠面积
        /// </summary>
        public static float GetOverlapAreaFromWorld(this RectTransform rect1, RectTransform rect2)
        {
            if (!rect1.IsOverlap(rect2)) return 0;
            var worldPos1 = new Vector3[4];
            var worldPos2 = new Vector3[4];
            rect1.GetWorldCorners(worldPos1);
            rect2.GetWorldCorners(worldPos2);
            float[] posX = new float[] { worldPos1[0].x, worldPos2[0].x, worldPos1[3].x, worldPos2[3].x };
            float[] posY = new float[] { worldPos1[2].y, worldPos2[2].y, worldPos1[3].y, worldPos2[3].y };
            float width = Mathf.Abs(posX.Max() - posX.Min()) - Mathf.Abs(worldPos1[3].x - worldPos2[3].x) - Mathf.Abs(worldPos1[0].x - worldPos2[0].x);
            float height = Mathf.Abs(posY.Max() - posY.Min()) - Mathf.Abs(worldPos1[0].y - worldPos2[0].y) - Mathf.Abs(worldPos1[1].y - worldPos2[1].y);
            return width * height;
        }

        /// <summary>
        /// 获得屏幕坐标下的重叠面积
        /// </summary>
        public static float GetOverlapAreaFromScreen(this RectTransform rect1, RectTransform rect2, Camera camera)
        {
            if (!rect1.IsOverlap(rect2)) return 0;
            var worldPos1 = new Vector3[4];
            var worldPos2 = new Vector3[4];
            rect1.GetWorldCorners(worldPos1);
            rect2.GetWorldCorners(worldPos2);
            var screenPos1 = worldPos1.Select(item => camera.WorldToScreenPoint(item)).ToArray();
            var screenPos2 = worldPos2.Select(item => camera.WorldToScreenPoint(item)).ToArray();
            float[] posX = new float[] { screenPos1[0].x, screenPos2[0].x, screenPos1[3].x, screenPos2[3].x };
            float[] posY = new float[] { screenPos1[2].y, screenPos2[2].y, screenPos1[3].y, screenPos2[3].y };
            float width = Mathf.Abs(posX.Max() - posX.Min()) - Mathf.Abs(screenPos1[3].x - screenPos2[3].x) - Mathf.Abs(screenPos1[0].x - screenPos2[0].x);
            float height = Mathf.Abs(posY.Max() - posY.Min()) - Mathf.Abs(screenPos1[0].y - screenPos2[0].y) - Mathf.Abs(screenPos1[1].y - screenPos2[1].y);
            return width * height;
        }

        /// <summary>
        /// 获得视口坐标下的重叠面积
        /// </summary>
        public static float GetOverlapAreaFromViewport(this RectTransform rect1, RectTransform rect2, Camera camera)
        {
            if (!rect1.IsOverlap(rect2)) return 0;
            var worldPos1 = new Vector3[4];
            var worldPos2 = new Vector3[4];
            rect1.GetWorldCorners(worldPos1);
            rect2.GetWorldCorners(worldPos2);
            var viewportPos1 = worldPos1.Select(item => camera.WorldToViewportPoint(item)).ToArray();
            var viewportPos2 = worldPos2.Select(item => camera.WorldToViewportPoint(item)).ToArray();
            float[] posX = new float[] { viewportPos1[0].x, viewportPos2[0].x, viewportPos1[3].x, viewportPos2[3].x };
            float[] posY = new float[] { viewportPos1[2].y, viewportPos2[2].y, viewportPos1[3].y, viewportPos2[3].y };
            float width = Mathf.Abs(posX.Max() - posX.Min()) - Mathf.Abs(viewportPos1[3].x - viewportPos2[3].x) - Mathf.Abs(viewportPos1[0].x - viewportPos2[0].x);
            float height = Mathf.Abs(posY.Max() - posY.Min()) - Mathf.Abs(viewportPos1[0].y - viewportPos2[0].y) - Mathf.Abs(viewportPos1[1].y - viewportPos2[1].y);
            return width * height;
        }

        /// <summary>
        /// 获得世界坐标下左边界的值
        /// </summary>
        public static float GetLeftPosFromWorld(this RectTransform rect)
        {
            return rect.GetLeftBottomFromWorld().x;
        }

        /// <summary>
        /// 获得屏幕坐标下左边界的值
        /// </summary>
        public static float GetLeftPosFromScreen(this RectTransform rect, Camera camera)
        {
            return rect.GetLeftBottomFromScreen(camera).x;
        }


        /// <summary>
        /// 获得视口坐标下左边界的值
        /// </summary>
        public static float GetLeftPosFromViewport(this RectTransform rect, Camera camera)
        {
            return rect.GetLeftBottomFromViewport(camera).x;
        }

        /// <summary>
        /// 获得世界坐标下右边界的值
        /// </summary>
        public static float GetRightPosFromWorld(this RectTransform rect)
        {
            return rect.GetRightBottomFromWorld().x;
        }

        /// <summary>
        /// 获得屏幕坐标下右边界的值
        /// </summary>
        public static float GetRightPosFromScreen(this RectTransform rect, Camera camera)
        {
            return rect.GetRightBottomFromScreen(camera).x;
        }

        /// <summary>
        /// 获得视口坐标下右边界的值
        /// </summary>
        public static float GetRightPosFromViewport(this RectTransform rect, Camera camera)
        {
            return rect.GetRightBottomFromViewport(camera).x;
        }

        /// <summary>
        /// 获得世界坐标下上边界的值
        /// </summary>
        public static float GetTopPosFromWorld(this RectTransform rect)
        {
            return rect.GetLeftTopFromWorld().y;
        }

        /// <summary>
        /// 获得屏幕坐标下上边界的值
        /// </summary>
        public static float GetTopPosFromScreen(this RectTransform rect, Camera camera)
        {
            return rect.GetLeftTopFromScreen(camera).y;
        }

        /// <summary>
        /// 获得视口坐标下上边界的值
        /// </summary>
        public static float GetTopPosFromViewport(this RectTransform rect, Camera camera)
        {
            return rect.GetLeftTopFromViewport(camera).y;
        }

        /// <summary>
        /// 获得世界坐标下下边界的值
        /// </summary>
        public static float GetBottomPosFromWorld(this RectTransform rect)
        {
            return rect.GetLeftBottomFromWorld().y;
        }

        /// <summary>
        /// 获得屏幕坐标下下边界的值
        /// </summary>
        public static float GetBottomPosFromScreen(this RectTransform rect, Camera camera)
        {
            return rect.GetLeftBottomFromScreen(camera).y;
        }

        /// <summary>
        /// 获得视口坐标下下边界的值
        /// </summary>
        public static float GetBottomPosFromViewport(this RectTransform rect, Camera camera)
        {
            return rect.GetLeftBottomFromViewport(camera).y;
        }

        /// <summary>
        /// 设置本地坐标中的
        /// </summary>
        public static void SetLocalPosition(this Transform rect, Vector3 newLocalPos)
        {
            rect.localPosition = newLocalPos;
        }
        /// <summary>
        /// 设置本地坐标中的x
        /// </summary>
        public static void SetLocalPositionX(this Transform rect, float x)
        {
            Vector3 newLocalPos = rect.localPosition;
            newLocalPos.x = x;
            rect.localPosition = newLocalPos;
        }
        /// <summary>
        /// 设置本地坐标中的y
        /// </summary>
        public static void SetLocalPositionY(this Transform rect, float y)
        {
            Vector3 newLocalPos = rect.localPosition;
            newLocalPos.y = y;
            rect.localPosition = newLocalPos;
        }
        /// <summary>
        /// 设置本地坐标中的z
        /// </summary>
        public static void SetLocalPositionZ(this Transform rect, float z)
        {
            Vector3 newLocalPos = rect.localPosition;
            newLocalPos.z = z;
            rect.localPosition = newLocalPos;
        }

        /// <summary>
        /// 设置本地缩放
        /// </summary>
        public static void SetLocalScale(this Transform rect, float scale)
        {
            rect.localScale = Vector3.one * scale;
        }
        /// <summary>
        /// 设置本地缩放中的x
        /// </summary>
        public static void SetLocalScaleX(this Transform rect, float x)
        {
            Vector3 newLocalSca = rect.localScale;
            newLocalSca.x = x;
            rect.localScale = newLocalSca;
        }
        /// <summary>
        /// 设置本地缩放中的y
        /// </summary>
        public static void SetLocalScaleY(this Transform rect, float y)
        {
            Vector3 newLocalSca = rect.localScale;
            newLocalSca.y = y;
            rect.localScale = newLocalSca;
        }
        /// <summary>
        /// 设置本地缩放中的z
        /// </summary>
        public static void SetLocalScaleZ(this Transform rect, float z)
        {
            Vector3 newLocalSca = rect.localScale;
            newLocalSca.z = z;
            rect.localScale = newLocalSca;
        }
        /// <summary>
        /// 设置SizeDelta高度
        /// </summary>
        public static void SetSizeDeltaHeight(this RectTransform rect, float height)
        {
            Vector2 newSizeDelta = rect.sizeDelta;
            newSizeDelta.y = height;
            rect.sizeDelta = newSizeDelta;
        }
        /// <summary>
        /// 设置SizeDelta宽度
        /// </summary>
        public static void SetSizeDeltaWidth(this RectTransform rect, float width)
        {
            Vector2 newSizeDelta = rect.sizeDelta;
            newSizeDelta.x = width;
            rect.sizeDelta = newSizeDelta;
        }

        /// <summary>
        /// 设置本地旋转z
        /// </summary>
        public static void SetLocalRotationZ(this Transform rect, float z)
        {
            rect.localRotation = Quaternion.Euler(0, 0, z);
        }
    }
}