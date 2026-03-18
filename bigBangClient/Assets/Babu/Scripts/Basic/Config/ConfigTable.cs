using System.Collections.Generic;
using UnityEngine;

namespace Babu.Config
{
    public interface IConfigTable
    {
        System.Threading.Tasks.Task LoadAsync(string tableName);
        void Load(string tableName);
    }

    public abstract class ConfigTable<T> : IConfigTable where T : ConfigBase, new()
    {
        private IConfigLoader _loader = new ByteConfigLoader();
        protected List<T> _list = new List<T>();
        protected Dictionary<int, T> _dic = new Dictionary<int, T>();

        private string _tableName;

        public List<T> GetConfigList()
        {
            return _list;
        }

        public T this[int index]
        {
            get
            {
                return _list[index];
            }
        }

        public void ClearData()
        {
            _list.Clear();
            _dic.Clear();
        }

        public virtual async System.Threading.Tasks.Task LoadAsync(string tableName)
        {
            DataTable table = await _loader.LoadTableAsync(tableName);

            if (table != null)
            {
                MakeConfig(table);
            }
        }

        public virtual void Load(string tableName)
        {
            _tableName = tableName;

            var table = _loader.LoadTable(tableName);
            if (table != null)
            {
                MakeConfig(table);
            }
        }

        private void AddConfig(T cfg)
        {
            _list.Add(cfg);

            if (!_dic.ContainsKey(cfg.Id))
            {
                _dic.Add(cfg.Id, cfg);
            }
            else
            {
                Debug.LogError("table " + _tableName + " An item with the same key has already been added. Key: " +
                               cfg.Id);
            }
        }

        private void MakeConfig(DataTable table)
        {
            if (table == null) return;
            int length = table._binaryReader.ReadInt32();

            for (var i = 0; i < length; i++)
            {
                var cfg = new T();
                cfg.LoadFromBinary(table._binaryReader);
                AddConfig(cfg);
            }

            table.CloseReader();
        }
    }
}