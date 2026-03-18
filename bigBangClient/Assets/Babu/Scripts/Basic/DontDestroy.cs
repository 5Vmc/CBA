using UnityEngine;

namespace Babu
{
    class DontDestroy : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
