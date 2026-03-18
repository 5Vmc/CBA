using System.Collections.Generic;

namespace Babu
{
    public enum SerializeType
    {
        Json,
        EncyptJson
    }

    public class SerializeManager : BabuSingleton<SerializeManager>
    {
        private Dictionary<SerializeType, ISerializer> _serializerDict = new Dictionary<SerializeType, ISerializer>();

        public override void Awake()
        {
            _serializerDict.Add(SerializeType.Json, new JsonSerializer());
            _serializerDict.Add(SerializeType.EncyptJson, new JsonEncyptSerializer());
        }

        public byte[] Serialize(object obj, SerializeType serializeType)
        {
            return _serializerDict[serializeType].Serialize(obj);
        }

        public T Deserialize<T>(byte[] data, SerializeType serializeType)
        {
            return _serializerDict[serializeType].Deserialize<T>(data);
        }
    }
}
