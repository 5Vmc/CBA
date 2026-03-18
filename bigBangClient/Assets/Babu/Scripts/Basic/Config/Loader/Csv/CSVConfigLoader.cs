using System.Text;
using UnityEngine;
using YooAsset;

namespace Babu.Config
{
    public class CSVConfigLoader : IConfigLoader
    {
        public static readonly string ConfigsPath = "Config/";
        public DataTable LoadTable(string tableName)
        {
            try
            {
                var path = ConfigsPath + tableName + ".csv";
                var handle = YooAssets.LoadAssetSync<TextAsset>(path);
                var ret = LoadTableFromAsset(tableName, handle.AssetObject as TextAsset);
                handle.Release();
                return ret;
            }
            catch (System.Exception e)
            {
                Debug.LogError("load data table <" + tableName + "> with exception:" + e.Message);
                return null;
            }
        }

        public async System.Threading.Tasks.Task<DataTable> LoadTableAsync(string tableName)
        {
            try
            {
                var path = ConfigsPath + tableName + ".csv";
                var handle = YooAssets.LoadAssetAsync<TextAsset>(path);
                await handle.Task;
                TextAsset binAsset = handle.AssetObject as TextAsset;
                handle.Release();
                return LoadTableFromAsset(tableName, binAsset);
            }
            catch (System.Exception e)
            {
                Debug.LogError("load data table <" + tableName + "> with exception:" + e.Message);
                return null;
            }
        }

        private DataTable LoadTableFromAsset(string tableName, TextAsset binAsset)
        {
            //#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            //            string text = Encoding.UTF8.GetString(DesEncryptor.Decrypt(binAsset.bytes));
            //#else
            string text = Encoding.UTF8.GetString(binAsset.bytes);
            //#endif
            var table = CSVService.LoadTables(text);

            if (table.getRowCount() <= 0)
            {
                Debug.LogErrorFormat("load table count <= 0, path: {0}", tableName);
            }

            return table;
        }
    }
}