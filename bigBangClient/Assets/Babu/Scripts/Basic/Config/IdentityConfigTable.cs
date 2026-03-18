using System.Collections.Generic;

namespace Babu.Config
{
    public abstract class IdentityConfigTable<T> : ConfigTable<T> where T : ConfigBase, new()
    {
        public T GetConfig(int id)
        {
            if (_dic.ContainsKey(id))
            {
                return _dic[id];
            }
            else
            {
                return default(T);
            }
        }
        
        public Dictionary<int, T> GetDataDictionary()
        {
            return _dic;
        }
    }
}