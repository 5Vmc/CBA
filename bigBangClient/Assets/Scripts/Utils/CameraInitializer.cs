using UnityEngine;
namespace BigBang
{
    public class CameraInitializer : MonoBehaviour
    {
        public CameraID ID;
        public Camera RenderCamera;

        private void Awake()
        {
            RegistCameraOnce();
        }

        private bool isRegisted = false;
        public void RegistCameraOnce()
        {
            if (isRegisted)
            {
                return;
            }
            CameraManager.Instance.Register(ID, RenderCamera);
            isRegisted = true;
        }

        private void OnDestroy()
        {
            LogoutCameraOnce();
        }

        public void LogoutCameraOnce()
        {
            if (!isRegisted)
            {
                return;
            }
            CameraManager.Instance.Logout(ID);
            isRegisted = false;
        }

    }
}