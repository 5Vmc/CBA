using UnityEngine;
using UnityEngine.Assertions;

namespace Babu
{
    public class BabuSingleton<T> : MonoBehaviour
        where T : Component
    {
        private static T _instance;
        public static T Instance { get => _instance; }

        public virtual void Awake()
        {
            Assert.IsTrue(_instance == null);
            _instance = this as T;
        }
    }
    
    public abstract class Singleton<T> where T : new()
    {
        private static T _instance;
        static object _lock = new object();
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new T();
                    }
                }
                return _instance;
            }
        }
    }
}
