using UnityEngine;

namespace Babu.Globalization
{
    class GlobalizationUISelector : MonoBehaviour
    {
        private GameObject _selectedGameObject;

        void Awake()
        {
            // 遍历所有子物体，设置相关UI
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var childTransform = transform.GetChild(i);
                childTransform.gameObject.SetActive(childTransform.gameObject.name.EndsWith(Globalizer.Instance.GetCurLanguageSuffix()));
                if (childTransform.gameObject.activeSelf)
                {
                    _selectedGameObject = childTransform.gameObject;
                }
            }
        }

        public T GetUICompoment<T>()
        {
            if (_selectedGameObject == null)
            {
                return default(T);
            }
            return _selectedGameObject.GetComponent<T>();
        }
    }
}
