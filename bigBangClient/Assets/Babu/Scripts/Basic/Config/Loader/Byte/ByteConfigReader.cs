using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ByteConfigReader
{
    public static Babu.BigNumber.BigNumber GetBigNumber(BinaryReader binaryReader)
    {
        Babu.BigNumber.BigNumber result = new();
        int length = binaryReader.ReadInt32();

        if (length == 1)
        {
            result.Value = binaryReader.ReadDouble();
        }
        else if (length == 2)
        {
            result.Value = binaryReader.ReadDouble();
            result.UnitId = binaryReader.ReadInt32();
        }

        return result;
    }

    public static T GetEnum<T>(BinaryReader binaryReader)
    {
        return (T)Enum.Parse(typeof(T), binaryReader.ReadString());
    }

    public static float GetFloat32(BinaryReader binaryReader)
    {
        return binaryReader.ReadSingle();
    }

    public static float[] GetFloat32Array(BinaryReader binaryReader)
    {
        int length = binaryReader.ReadInt32();
        float[] result = new float[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = binaryReader.ReadSingle();
        }

        return result;
    }

    public static int GetInt32(BinaryReader binaryReader)
    {
        return binaryReader.ReadInt32();
    }

    public static System.Collections.Generic.Dictionary<int, float> GetIntFloatDic(BinaryReader binaryReader)
    {
        System.Collections.Generic.Dictionary<int, float> result = new();
        int length = binaryReader.ReadInt32();

        for (int i = 0; i < length; i++)
        {
            result.Add(binaryReader.ReadInt32(), binaryReader.ReadSingle());
        }

        return result;
    }

    public static System.Collections.Generic.Dictionary<int, int> GetIntIntDic(BinaryReader binaryReader)
    {
        System.Collections.Generic.Dictionary<int, int> result = new();
        int length = binaryReader.ReadInt32();

        for (int i = 0; i < length; i++)
        {
            result.Add(binaryReader.ReadInt32(), binaryReader.ReadInt32());
        }

        return result;
    }

    public static int[] GetInt32Array(BinaryReader binaryReader)
    {
        int length = binaryReader.ReadInt32();
        int[] result = new int[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = binaryReader.ReadInt32();
        }

        return result;
    }

    public static System.Collections.Generic.Dictionary<int, string> GetIntStringDic(BinaryReader binaryReader)
    {
        System.Collections.Generic.Dictionary<int, string> result = new();
        int length = binaryReader.ReadInt32();

        for (int i = 0; i < length; i++)
        {
            result.Add(binaryReader.ReadInt32(), binaryReader.ReadString());
        }

        return result;
    }

    public static long GetInt64(BinaryReader binaryReader)
    {
        return binaryReader.ReadInt64();
    }

    public static string GetString(BinaryReader binaryReader)
    {
        return binaryReader.ReadString();
    }

    public static System.Collections.Generic.Dictionary<string, int> GetStringIntDic(BinaryReader binaryReader)
    {
        System.Collections.Generic.Dictionary<string, int> result = new();
        int length = binaryReader.ReadInt32();

        for (int i = 0; i < length; i++)
        {
            result.Add(binaryReader.ReadString(), binaryReader.ReadInt32());
        }

        return result;
    }

    public static string[] GetStringArray(BinaryReader binaryReader)
    {
        int length = binaryReader.ReadInt32();
        string[] result = new string[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = binaryReader.ReadString();
        }

        return result;
    }
}
