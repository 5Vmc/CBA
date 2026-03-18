using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Babu.Config
{
    public class ConfigManager
    {
        private static ConfigManager _instance;

        public static ConfigManager Instance
        {
            get { return _instance ??= new ConfigManager(); }
        }

        List<IConfigTable> _tables = new List<IConfigTable>();
        private Dictionary<Type, IConfigTable> _tableDic = new Dictionary<Type, IConfigTable>();

        public async Task<T> LoadTableAsync<T>(string tableName) where T : IConfigTable, new()
        {
            //var startTime = DateTime.Now;
            T table = new T();
            await table.LoadAsync(tableName);
            AddTable(table);
        //    Debug.Log("配置表<" + tableName + ">加载耗时:" + (DateTime.Now - startTime).TotalSeconds + "s");
            return table;
        }

        public async void LoadTableAsync<T>(string tableName, Action<T> callback) where T : IConfigTable, new()
        {
            //var startTime = DateTime.Now;
            T table = new T();
            await table.LoadAsync(tableName);
            AddTable(table);
            //    Debug.Log("配置表<" + tableName + ">加载耗时:" + (DateTime.Now - startTime).TotalSeconds + "s");
            callback(table);
        }

        public T LoadTable<T>(string tableName) where T : IConfigTable, new()
        {
            var startTime = DateTime.Now;
            T table = new T();
            table.Load(tableName);
            AddTable(table);
        //    Debug.Log("配置表<" + tableName + ">加载耗时:" + (DateTime.Now - startTime).TotalSeconds + "s");
            return table;
        }

        private void AddTable<T>(T table) where T : IConfigTable, new()
        {
            if (!_tableDic.ContainsKey(typeof(T)))
            {
                _tableDic.Add(typeof(T), table);
                _tables.Add(table);
            }
        }
        public T GetTable<T>() where T : IConfigTable
        {
            IConfigTable table;
            _tableDic.TryGetValue(typeof(T), out table);
            return (T)table;
        }
    }
}
