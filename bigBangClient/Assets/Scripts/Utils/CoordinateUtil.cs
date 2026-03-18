using UnityEngine;
namespace Utils
{
    public static class CoordinateUtil
    {
        public static Vector2 World2Screen(Vector3 worldPos, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            return camera.WorldToScreenPoint(worldPos);
        }

        public static Vector2 Screen2UI(Vector2 v, RectTransform rt, Camera camera = null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, v, camera, out Vector2 uiPos);
            return uiPos;
        }

        public static Vector2 World2UI(Vector3 worldPos, RectTransform rt, Camera uiCamera, Camera worldCamera = null)
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
            Vector2 screenPos = World2Screen(worldPos, worldCamera);
            return Screen2UI(screenPos, rt, uiCamera);
        }
    }
}
