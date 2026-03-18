using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using System.Text;
using YooAsset;

namespace Babu.Config
{
    public class ByteConfigLoader : IConfigLoader
    {
        public static readonly string ConfigsPath = "Config/";
        public DataTable LoadTable(string tableName)
        {
            throw new NotSupportedException();
            //try
            //{

            //var path = ConfigsPath + tableName + ".byte";
            //BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open));
            //return LoadTableFromAsset(tableName, br);
            //}
            ///catch (System.Exception e)
            //{
            //    Debug.LogError("load data table <" + tableName + "> with exception:" + e.Message);
            //    return null;
            //}
        }

        public async Task<DataTable> LoadTableAsync(string tableName)
        {
            try
            {
                var path = ConfigsPath + tableName + ".bytes";
                var handle = YooAssets.LoadAssetAsync<TextAsset>(path);
                await handle.Task;
                TextAsset binAsset = handle.AssetObject as TextAsset;
                handle.Release();
                BinaryReader br = new BinaryReader(new MemoryStream(binAsset.bytes));
                return LoadTableFromAsset(tableName, br);
            }
            catch (System.Exception e)
            {
                Debug.LogError("load data table <" + tableName + "> with exception:" + e.Message);
                return null;
            }
        }

        private DataTable LoadTableFromAsset(string tableName, BinaryReader br)
        {
            var table = ByteService.LoadTables(br);

            if (table._binaryReader == null)
            {
                Debug.LogError(tableName + " - BinaryReader is null");
            }

            return table;
        }
    }
}
