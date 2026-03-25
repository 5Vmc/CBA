namespace Utils
{
    public class Resource
    {
        public int Count { get; set; }

        public Resource()
        {
            Count = 0;
        }

        public Resource(int count)
        {
            Count = count;
        }

        public void AddCount(int number)
        {
            if (number < 0)
            {
                //todo error tips
                return;
            }

            Count += number;
        }

        public void DelCount(int number)
        {
            if (number < 0)
            {
                //todo error tips
                return;
            }

            Count -= number;
            if (Count < 0) Count = 0;
        }

        public bool IsEnough(int number)
        {
            return Count >= number;
        }

        public override string ToString()
        {
            return Count.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                Resource other => Count == other.Count,
                int value => Count == value,
                _ => false
            };
        }

        public override int GetHashCode()
        {
            return Count;
        }

        public static implicit operator Resource(int value)
        {
            return new Resource(value);
        }
        
        #region Overriding cmp

        public static bool operator ==(Resource a, int b)
        {
            return a?.Count == b;
        }

        public static bool operator !=(Resource a, int b)
        {
            return a?.Count != b;
        }

        public static bool operator >(Resource a, int b)
        {
            return a.Count > b;
        }

        public static bool operator >=(Resource a, int b)
        {
            return a.Count >= b;
        }

        public static bool operator <(Resource a, int b)
        {
            return a.Count < b;
        }

        public static bool operator <=(Resource a, int b)
        {
            return a.Count <= b;
        }

        #endregion
    }
}
