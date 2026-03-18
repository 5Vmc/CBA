using System.Collections.ObjectModel;
using Google.Protobuf;
using System.Collections.Generic;
using Google.Protobuf.Collections;

namespace Babu
{
    public class ProtoUtils
    {
        public static List<T> UnPackRepeatedField<T>(RepeatedField<T> protoList) where T: IMessage
        {
            List<T> retList = new List<T>();
            foreach(T a in protoList){
                retList.Add(a);
            }
            //return retList.AsReadOnly();
            return retList;
        }

        public static List<NumericType> UnPackRepeatedField2<NumericType>(RepeatedField<NumericType> protoList)
        {
            List<NumericType> retList = new List<NumericType>();
            foreach(NumericType a in protoList){
                retList.Add(a);
            }
            //return retList.AsReadOnly();
            return retList;
        }
    }
}