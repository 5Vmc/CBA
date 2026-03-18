using Newtonsoft.Json;

namespace Babu
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize(object obj)
        {
#if UNITY_EDITOR
            string data = JsonConvert.SerializeObject(obj, Formatting.Indented);
#else
            string data = JsonConvert.SerializeObject(obj, Formatting.None);
#endif
            return StringUtils.StringToBytes(data);
        }

        public T Deserialize<T>(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(StringUtils.BytesToString(data));
        }
    }
}
