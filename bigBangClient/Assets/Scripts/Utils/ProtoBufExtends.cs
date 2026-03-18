using System.Collections.Generic;
using UnityEngine;
using Babu.BigNumber;
using Google.Protobuf.Collections;
using Protocol;


namespace Utils
{
    public class ProtoBufExtends
    {
        
    }
    
    public static class ProtoBigNumberInfoExtends
    {
        public static BigNumber ToBigNumber(this BigNumberInfo data)
        {
            return data == null ? null : new BigNumber(data.Value, data.UnitId);
        }
        
        public static BigNumberInfo ToProto(this BigNumber number)
        {
            if (number == null) return null;
            number.Format();
            return new BigNumberInfo()
            {
                Value = number.Value,
                UnitId = number.UnitId
            };
        }
    }
    
    public static class ProtoVector2Extends
    {
       
        public static Quaternion ToViewRotate(this Protocol.Vector2 vec)
        {
            Vector3 dir = new Vector3(vec.X, vec.Y);
            return Quaternion.Euler(0, 0, Vector3.Angle(dir, Vector3.right));
        }

        public static Vector3 ToViewPos(this Protocol.Vector2 vec)
        {
            Vector3 pos = new Vector3(vec.X, vec.Y);
            pos.x = pos.x * 720 / 105f;
            pos.y = pos.y * 720 / 105f;
            return pos;
        }


        public static Quaternion To3dDir(this Protocol.Vector2 vec)
        {
            Vector3 dir = new Vector3(vec.Y, 0, vec.X);
            return Quaternion.Euler(0, Vector3.Angle(dir, Vector3.forward), 0);
        }
    }

    public static class ProtoListExtends
    {
        public static RepeatedField<int> ToRepeatedField(this List<int> list)
        {
            RepeatedField<int> resList = new RepeatedField<int>();
            foreach (var id in list)
            {
                resList.Add(id);
            }

            return resList;
        }
    }

}