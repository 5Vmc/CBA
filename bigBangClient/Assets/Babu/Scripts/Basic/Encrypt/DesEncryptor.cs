using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Babu
{
    public class DesEncryptor
    {
        public const string DEFAULT_KEY = "z9EAu8kB";

        public static byte[] Encrypt(byte[] inputArray, string key = DEFAULT_KEY)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = Encoding.UTF8.GetBytes(key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputArray, 0, inputArray.Length);
            cs.FlushFinalBlock();

            return ms.ToArray();
        }

        public static byte[] Encrypt(string inputStr, string key = DEFAULT_KEY)
        {
            byte[] inputArray = Encoding.GetEncoding("UTF-8").GetBytes(inputStr);
            return Encrypt(inputArray, key);
        }

        public static byte[] Decrypt(byte[] inputArray, string key = DEFAULT_KEY)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = Encoding.UTF8.GetBytes(key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputArray, 0, inputArray.Length);
            cs.FlushFinalBlock();

            return ms.ToArray();
        }

        public static string DecryptToString(byte[] inputArray, string key = DEFAULT_KEY)
        {
            return Encoding.UTF8.GetString(Decrypt(inputArray, key));
        }
    }
}
