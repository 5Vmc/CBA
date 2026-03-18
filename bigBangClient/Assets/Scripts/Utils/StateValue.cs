using System;

public class StateValue
{
    public int Value { get; private set; }

    public int Length { get => 8; }

    public StateValue() { Value = 0; }

    public StateValue(int value) { Value = value; }

    public int this[int index]
    {
        get
        {
            if (index < 0 || index >= 8) throw new IndexOutOfRangeException("index取值范围[0-7]");
            return GetValue(index, Value);
        }
        set
        {
            if (index < 0 || index >= 8) throw new IndexOutOfRangeException("index取值范围[0-7]");
            if (value < 0 || value > 15) throw new IndexOutOfRangeException("value取值范围[0-15]");
            Value = SetValue(index, Value, value);
        }
    }

    /// <summary>
    /// 获得位值
    /// </summary>
    /// <param name="index">取值范围[0-7]</param>
    /// <param name="value">取值范围[0-15]</param>
    /// <returns>位值</returns>
    public static int GetValue(int index, int value)
    {
        ++index;
        var tmp = 0xf;
        while (--index > 0)
        {
            value >>= 4;
        }
        var res = value & tmp;
        return res;
    }

    /// <summary>
    /// int32存储，存储数8个
    /// </summary>
    /// <param name="index">取值范围[0-7]</param>
    /// <param name="target">被修改的值</param>
    /// <param name="value">取值范围[0-15]</param>
    /// <returns>修改后的值</returns>
    public static int SetValue(int index, int target, int value)
    {
        ++index;
        var tmp = 0xf;
        while (--index > 0)
        {
            tmp <<= 4;
            value <<= 4;
        }
        tmp = ~tmp;
        target &= tmp;
        target |= value;
        return target;
    }

    public override string ToString()
    {
        var str = Value.ToString();
        for (int i = 0; i < 7; i++)
        {
            str += " " + GetValue(Value, i);
        }

        return str;
    }
}
