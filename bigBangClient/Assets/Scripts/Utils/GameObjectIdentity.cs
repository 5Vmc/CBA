using UnityEngine;

namespace BigBang
{
    public class GameObjectIdentity : MonoBehaviour
    {
        public GameObjectID ID;

        private void Awake()
        {
            RegistGameObjectOnce();
        }

        private void OnDestroy()
        {
            LogoutGameObjectOnce();
        }

        private bool isRegisted = false;

        public void RegistGameObjectOnce()
        {
            if (isRegisted)
            {
                return;
            }
            isRegisted = true;
            // Debug.Log("RegistGameObjectOnce + " + gameObject.name);
            GameObjectManager.Instance.Register(ID, gameObject);
        }
        public void LogoutGameObjectOnce()
        {
            if (!isRegisted)
            {
                return;
            }
            isRegisted = false;
            // Debug.Log("LogoutGameObjectOnce + " + gameObject.name);
            GameObjectManager.Instance.Logout(ID);

        }

    }
}