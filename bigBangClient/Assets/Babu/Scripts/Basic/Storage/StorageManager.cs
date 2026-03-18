using UnityEngine;

namespace Babu
{
    public class StorageManager : BabuSingleton<StorageManager>
    {
        public enum StorageType
        {
            Inner,
            External
        }

        [SerializeField] private StorageType storageType = StorageType.Inner;

        Storage _storage;

        public override void Awake()
        {
            base.Awake();

            _storage = CreateStorage();
        }

        Storage CreateStorage()
        {
            Debug.Log("Create Storage: " + storageType);
            switch (storageType)
            {
                case StorageType.Inner: return new InternalStorage();
                case StorageType.External: Platform.RequestExternalStoragePermission(); return new ExternalStorage();
            }
            return null;
        }

        public bool Exists(string file)
        {
            return _storage.Exists(file);
        }

        public void CreateDirectory(string dir)
        {
            _storage.CreateDirectory(dir);
        }

        public void Store(string file, string data)
        {
            _storage.Store(file, data);
        }

        public void StoreBytes(string file, byte[] data)
        {
            _storage.StoreBytes(file, data);
        }

        public void StoreObject(string file, object obj, SerializeType serializeType)
        {
            StoreBytes(file, SerializeManager.Instance.Serialize(obj, serializeType));
        }

        public string Load(string file)
        {
            return _storage.Load(file);
        }

        public byte[] LoadBytes(string file)
        {
            return _storage.LoadBytes(file);
        }

        public T LoadObject<T>(string file, SerializeType serializeType)
        {
            byte[] data = LoadBytes(file);
            if (data == null)
            {
                return default(T);
            }
            return SerializeManager.Instance.Deserialize<T>(data, serializeType);
        }
    }
}
