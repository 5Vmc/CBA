using Babu.SDK;
using System;
using System.IO;
using UnityEngine;

namespace Babu
{
    class Logger : BabuSingleton<Logger>
    {
        const string LOG_FILE_NAME = "babu.log";
        const string UPLOAD_URL = "http://upload.babuyo.com/api/upload.php";

        int enableLogTypes = 0;
        FileStream _fileStream;
        StreamWriter _streamWriter;

        public override void Awake()
        {
            base.Awake();
            Application.logMessageReceived += OnUnityLog;
            _fileStream = File.Open(GetLogPath(), FileMode.OpenOrCreate);
            _fileStream.Position = _fileStream.Length;
            _streamWriter = new StreamWriter(_fileStream);

            if (_fileStream != null)
            {
                Debug.Log("------- Open Logger Succ");
            }
            else
            {
                Debug.Log("--------Open Log Failed");
            }

            EnableLogType(LogType.Error);
            EnableLogType(LogType.Assert);
            EnableLogType(LogType.Warning);
            EnableLogType(LogType.Exception);
            EnableLogType(LogType.Log);
        }

        public static string GetLogPath()
        {
            return Application.persistentDataPath + "/" + LOG_FILE_NAME;
        }

        public long GetLogSize()
        {
            if (_fileStream == null)
            {
                return 0;
            }
            else
            {
                return _fileStream.Length;
            }
        }

        public void EnableLogType(LogType logType)
        {
            enableLogTypes |= 1 << (int)logType;
        }

        public void DisableLogType(LogType logType)
        {
            enableLogTypes &= ~(1 << (int)logType);
        }

        public bool IsLogTypeEnabled(LogType logType)
        {
            return (enableLogTypes & (1 << (int)logType)) != 0;
        }

        void OnUnityLog(string condition, string stackTrace, LogType logType)
        {
            if (!IsLogTypeEnabled(logType))
            {
                return;
            }

            if (_streamWriter != null)
            {
                _streamWriter.Write(DateTime.Now.ToString());
                _streamWriter.Write(" ");
                _streamWriter.Write(logType.ToString());
                _streamWriter.Write(" ");
                _streamWriter.WriteLine(condition);
                if (logType != LogType.Log)
                {
                    // 普通log不显示堆栈
                    _streamWriter.WriteLine(stackTrace);
                }
                _streamWriter.Flush();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream = null;
            }

            if (_streamWriter != null)
            {
                _streamWriter = null;
            }
        }

        public void UploadLog()
        {
            byte[] data = LoadLogContent();
            if (data != null)
            {
                Debug.Log("Upload Data Size: " + data.Length);
                string response;
                Debug.Log("Upload Log result: " + HttpService.Instance.Post(GetUploadUrl(), data, out response));
            }
        }

        private string GetUploadUrl()
        {
            string gameName = Application.productName;
            string channelId = Environment.GetValue<string>("channel_id", "unknown");
            string platform = Environment.GetValue<string>("platform", "unknown");
            string version = Application.version;
            string country = SDKLocation.Instance.CountryCode;
            string language = Platform.GetSystemLanguage();
            return UPLOAD_URL + "?game=" + gameName + "&channel_id=" + channelId + "&platform=" + platform + "&version=" + version + "&country=" + country + "&language=" + language;
        }

        private byte[] LoadLogContent()
        {
            byte[] content = null;
            long originPos = _fileStream.Position;
            try
            {
                // 最多上传最近的2mb数据
                const int MAX_LOAD_SIZE = 2 * 1024 * 1024;
                if (_fileStream.Length > MAX_LOAD_SIZE)
                {
                    content = new byte[MAX_LOAD_SIZE];
                    _fileStream.Position = _fileStream.Length - MAX_LOAD_SIZE;
                    _fileStream.Read(content, 0, MAX_LOAD_SIZE);
                }
                else
                {
                    _fileStream.Position = 0;
                    content = new byte[_fileStream.Length];
                    _fileStream.Read(content, 0, (int)_fileStream.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Load Log Content Catch Exception: " + e.Message);
            }

            _fileStream.Position = originPos;
            return content;
        }
    }
}
