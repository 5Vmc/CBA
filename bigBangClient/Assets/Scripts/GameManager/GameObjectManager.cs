using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace BigBang
{
    public class GameObjectManager
    {
        private static GameObjectManager instance;
        private Dictionary<GameObjectID, GameObject> objects = new Dictionary<GameObjectID, GameObject>();

        public static GameObjectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObjectManager();
                }
                return instance;
            }
        }

        public void Register(GameObjectID ID, GameObject obj)
        {
            if (!objects.ContainsKey(ID))
            {
                objects.Add(ID, obj);
            }
        }

        public void Logout(GameObjectID ID)
        {
            objects.Remove(ID);
        }


        public GameObject GetGameObject(GameObjectID ID)
        {
            if (objects.TryGetValue(ID, out var obj))
            {
                return obj;
            }
            return null;
        }

        public T GetComponent<T>(GameObjectID ID) where T : Component
        {
            var obj = GetGameObject(ID);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            return null;
        }

        public T GetComponentAtPath<T>(GameObjectID ID, string path) where T : Component
        {
            var obj = GetGameObject(ID);
            if (obj != null)
            {
                return obj.transform.GetComponentAtPath<T>(path);
            }
            return null;
        }
    }
}