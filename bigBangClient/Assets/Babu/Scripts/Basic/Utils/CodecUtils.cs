using System;
using System.Security.Cryptography;
using System.Text;

namespace Babu
{
    public class CodecUtils
    {
        public static string Hex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        public static byte[] UnHex(string data)
        {
            byte[] ret = new byte[data.Length / 2];
            for (int x = 0; x < ret.Length / 2; x++)
            {
                int i = (Convert.ToInt32(data.Substring(x * 2, 2), 16));
                ret[x] = (byte)i;
            }
            return ret;
        }

        public static byte[] HmacSha256(byte[] key, byte[] data)
        {
            var hmacsha256 = new HMACSHA256(key);
            return hmacsha256.ComputeHash(data);
        }

        public static byte[] HmacSha256(string key, byte[] data)
        {
            var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return hmacsha256.ComputeHash(data);
        }

        public static byte[] Md5(byte[] data)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(data);
        }
    }
}
