using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utils
{
    public class CopyUtils
    {
        public static List<T> DeepCopy<T>(object List)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, List);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as List<T>;
            }
        }
    }
}