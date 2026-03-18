using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Babu
{
    public class FileUtils
    {
        public static void CreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateFile(string path)
        {
            if (File.Exists(path))
            {
                return;
            }
            else
            {
                File.Create(path).Close();
            }
        }

        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        public static string ReadFile(string path)
        {
            try
            {
                StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
                string data = streamReader.ReadToEnd();
                streamReader.Close();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Read File: {path} Catch Exception: {e.Message}");
                return null;
            }
        }

        public static byte[] ReadFileBytes(string path)
        {
            try
            {
                FileStream fileStream = File.OpenRead(path);
                byte[] data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);
                fileStream.Close();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Read File Bytes: {path} Catch Exception: {e.Message}");
                return null;
            }
        }

        public static void WriteFile(string path, string data)
        {
            File.WriteAllText(path, data);
        }

        public static void WriteFileBytes(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }

        public static void ReplaceFileContent(string path, string source, string target)
        {
            string data = ReadFile(path);
            if (data == null)
            {
                return;
            }

            data = data.Replace(source, target);
            WriteFile(path, data);
        }
    }
}
