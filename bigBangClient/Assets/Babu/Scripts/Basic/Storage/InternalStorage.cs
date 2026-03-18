using UnityEngine;

namespace Babu
{
    class InternalStorage : FileStorage
    {
        private static string _storagePath;

        protected override string GetStoragePath()
        {
            if (_storagePath == null)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    _storagePath = Application.dataPath + "/Storage";
                }
                else
                {
                    _storagePath = Application.persistentDataPath + "/Storage";
                }

                FileUtils.CreateDirectory(_storagePath);
                Debug.Log("Init Internal Storage Path: " + _storagePath);
            }
            return _storagePath;
        }
    }
}
