using System.Collections.Generic;
using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang
{
    public class CameraManager
    {
        private static CameraManager instance;
        private Dictionary<CameraID, Camera> cameras = new Dictionary<CameraID, Camera>();

        public static CameraManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CameraManager();
                }
                return instance;
            }
        }

        public void Register(CameraID ID, Camera camera)
        {
            if (!cameras.ContainsKey(ID))
            {
                cameras.Add(ID, camera);
            }
            else
            {
                cameras[ID] = camera;
            }
        }

        public void Logout(CameraID ID)
        {
            cameras.Remove(ID);
        }

        public Camera GetCamera(CameraID ID)
        {
            if (cameras.TryGetValue(ID, out var camera))
            {
                return camera;
            }
            return null;
        }

        public void SetTexture(CameraID cameraID, RawImage rawImage, int width = 0, int height = 0)
        {
            if (width <= 0) width = (int)UIFrame.width;
            if (height <= 0) height = (int)UIFrame.height;

            Camera cam = GetCamera(cameraID);
            cam.gameObject.SetActive(true);
            var temporary = RenderTexture.GetTemporary(width, height, 24);
            // ⚠ 设置抗锯齿（根据机型来动态调整）
            //temporary.antiAliasing = 8;//Setting anti-aliasing of already created render texture is not supported!
            temporary.autoGenerateMips = false;
            //temporary.useMipMap = false;//Setting mipmap mode of already created render texture is not supported!
            rawImage.texture = temporary;
            cam.targetTexture = temporary;
        }

        public void ReleaseTexture(CameraID cameraID, RawImage rawImage)
        {
            var cam = GetCamera(cameraID);
            cam.targetTexture = null;
            cam.gameObject.SetActive(false);
            RenderTexture.ReleaseTemporary(rawImage.texture as RenderTexture);
            rawImage.texture = null;
        }
    }
}