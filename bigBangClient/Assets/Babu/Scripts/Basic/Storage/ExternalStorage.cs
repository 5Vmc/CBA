using UnityEngine;

namespace Babu
{
    class ExternalStorage : FileStorage
    {
        private static string _storagePath;

        protected override string GetStoragePath()
        {
            if (_storagePath == null)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    _storagePath = Application.dataPath + "/External/Storage";
                }
                else
                {
                    if (Platform.HasExternalStoragePermission())
                    {
                        _storagePath = Platform.GetExternalStorageDirectory() + "/Babu/" + Application.identifier + "/Storage";
                    }
                    else
                    {
                        _storagePath = Application.persistentDataPath + "/External/Storage";
                    }
                }

                FileUtils.CreateDirectory(_storagePath);
                Debug.Log("Init External Storage Path: " + _storagePath);
            }
            return _storagePath;
        }
    }
}
