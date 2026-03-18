using UnityEngine;

namespace Babu.Globalization
{
    class GlobalizationImageSelector : MonoBehaviour
    {
        void Awake()
        {
            // 遍历所有字物体，设置相关图片
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var childTransform = transform.GetChild(i);
                childTransform.gameObject.SetActive(childTransform.gameObject.name.EndsWith(Globalizer.Instance.GetCurLanguageSuffix()));
            }
        }
    }
}
