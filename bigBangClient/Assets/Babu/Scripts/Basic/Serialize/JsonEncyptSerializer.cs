using System.Text;
using Newtonsoft.Json;

namespace Babu
{
    public class JsonEncyptSerializer : ISerializer
    {
        public byte[] Serialize(object obj)
        {
#if UNITY_EDITOR
            string data = JsonConvert.SerializeObject(obj, Formatting.Indented);
#else
            string data = JsonConvert.SerializeObject(obj, Formatting.None);
#endif
            return DesEncryptor.Encrypt(data);
        }

        public T Deserialize<T>(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(DesEncryptor.DecryptToString(data));
        }
    }
}
