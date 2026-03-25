using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace Babu
{
    public class HttpService : BabuSingleton<HttpService>
    {
        public delegate void HttpResponseCallback(bool result, string response);

        public delegate void HttpBytesResponseCallback(bool result, byte[] response);

        public bool Get(string url, out string response, int timeout = 2)
        {
            return Get(url, null, out response, timeout);
        }

        public bool Get(string url, ArrayList headers, out string response, int timeout = 2)
        {
            try
            {
                HttpWebRequest request = CreateGetHttpRequest(url, headers, timeout);
                response = GetHttpResponse(request.GetResponse() as HttpWebResponse);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Http Service Get {url} Error: " + e.Message);
                response = "";
                return false;
            }
        }

        public bool Post(string url, string data, out string response, int timeout = 2)
        {
            return Post(url, Encoding.UTF8.GetBytes(data), out response, timeout);
        }

        public bool Post(string url, byte[] data, out string response, int timeout = 2)
        {
            return Post(url, null, null, data, out response, timeout);
        }

        public bool Post(string url, string contentType, string data, out string response, int timeout = 2)
        {
            return Post(url, contentType, Encoding.UTF8.GetBytes(data), out response, timeout);
        }

        public bool Post(string url, string contentType, byte[] data, out string response, int timeout = 2)
        {
            return Post(url, contentType, null, data, out response, timeout);
        }

        public bool Post(string url, ArrayList headers, string data, out string response, int timeout = 2)
        {
            return Post(url, headers, Encoding.UTF8.GetBytes(data), out response, timeout);
        }

        public bool Post(string url, ArrayList headers, byte[] data, out string response, int timeout = 2)
        {
            return Post(url, null, headers, data, out response, timeout);
        }

        public bool Post(string url, string contentType, ArrayList headers, string data, out string response, int timeout = 2)
        {
            return Post(url, contentType, headers, Encoding.UTF8.GetBytes(data), out response, timeout);
        }

        public bool Post(string url, string contentType, ArrayList headers, byte[] data, out string response, int timeout = 2)
        {
            try
            {
                HttpWebRequest request = CreateSyncPostHttpRequest(url, contentType, headers, data, timeout);
                response = GetHttpResponse(request.GetResponse() as HttpWebResponse);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Http Service Post {url} Error: " + e.Message);
                response = "";
                return false;
            }
        }

        public void AsyncGet(string url, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncGet(url, null, callback, timeout);
        }

        public void AsyncGet(string url, ArrayList headers, HttpResponseCallback callback, int timeout = 2)
        {
            try
            {
                HttpWebRequest request = CreateGetHttpRequest(url, headers, timeout);
                AsyncGetHttpResponse(request, callback);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Http Service Async Get {url} Error: " + e.Message);
                callback(false, string.Empty);
            }
        }

        public void AsyncGet(string url, HttpBytesResponseCallback callback, int timeout = 2)
        {
            AsyncGet(url, null, callback, timeout);
        }

        public void AsyncGet(string url, ArrayList headers, HttpBytesResponseCallback callback, int timeout = 2)
        {
            try
            {
                HttpWebRequest request = CreateGetHttpRequest(url, headers, timeout);
                AsyncGetHttpBytesResponse(request, callback);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Http Service Async Get {url} Error: " + e.Message);
                callback(false, null);
            }
        }

        public void AsyncPost(string url, string data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, Encoding.UTF8.GetBytes(data), callback, timeout);
        }

        public void AsyncPost(string url, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, null, new ArrayList(), data, callback, timeout);
        }

        public void AsyncPost(string url, string contentType, string data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, contentType, Encoding.UTF8.GetBytes(data), callback, timeout);
        }

        public void AsyncPost(string url, string contentType, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, contentType, new ArrayList(), data, callback, timeout);
        }

        public void AsyncPost(string url, ArrayList headers, string data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, headers, Encoding.UTF8.GetBytes(data), callback, timeout);
        }



        public void AsyncPost(string url, ArrayList headers, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, null, headers, data, callback, timeout);
        }

        public void AsyncPost(string url, string contentType, ArrayList headers, string data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, contentType, headers, Encoding.UTF8.GetBytes(data), callback, timeout);
        }

        public void AsyncPost(string url, string contentType, Dictionary<string, string> headers, string data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, contentType, headers, Encoding.UTF8.GetBytes(data), callback, timeout);
        }

        public void AsyncPost(string url, string contentType, Dictionary<string, string> headers, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            try
            {
                HttpWebRequest request = CreateHttpRequest(url, headers, timeout);
                request.Method = "Post";
                if (contentType != null)
                {
                    request.ContentType = contentType;
                }
                request.ContentLength = data.Length;
                request.BeginGetRequestStream((result) =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(result);
                        stream.Write(data, 0, data.Length);

                        AsyncGetHttpResponse(request, callback);
                    }
                    catch (Exception ex)
                    {
                        MainThreadTaskService.Instance.Run(() =>
                        {
                            Debug.LogError($"Http Service Async Post  {url} Error: " + ex.Message);
                            callback(false, string.Empty);
                        });
                    }
                }, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"Http Service Async Post {url} Error: " + e.Message);
                callback(false, string.Empty);
            }
        }

        public void AsyncPost(string url, string contentType, ArrayList headers, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            try
            {
                HttpWebRequest request = CreateHttpRequest(url, headers, timeout);
                request.Method = "Post";
                if (contentType != null)
                {
                    request.ContentType = contentType;
                }
                request.ContentLength = data.Length;
                request.BeginGetRequestStream((result) =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(result);
                        stream.Write(data, 0, data.Length);

                        AsyncGetHttpResponse(request, callback);
                    }
                    catch (Exception ex)
                    {
                        MainThreadTaskService.Instance.Run(() =>
                        {
                            Debug.LogError($"Http Service Async Post  {url} Error: " + ex.Message);
                            callback(false, string.Empty);
                        });
                    }
                }, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"Http Service Async Post {url} Error: " + e.Message);
                callback(false, string.Empty);
            }
        }

        private HttpWebRequest CreateHttpRequest(string url, ArrayList headers, int timeout)
        {
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Timeout = timeout * 1000;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add((string)header);
                }

            }

            return request;
        }


        private HttpWebRequest CreateHttpRequest(string url, Dictionary<string, string> headers, int timeout)
        {
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Timeout = timeout * 1000;

            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }

            }

            return request;
        }

        private HttpWebRequest CreateGetHttpRequest(string url, ArrayList headers, int timeout)
        {
            HttpWebRequest request = CreateHttpRequest(url, headers, timeout);
            request.Method = "Get";
            return request;
        }

        private HttpWebRequest CreateSyncPostHttpRequest(string url, string contentType, ArrayList headers, byte[] data, int timeout)
        {
            HttpWebRequest request = CreateHttpRequest(url, headers, timeout);
            request.Method = "Post";

            if (contentType != null)
            {
                request.ContentType = contentType;
            }

            request.ContentLength = data.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);

            return request;
        }

        private string GetHttpResponse(HttpWebResponse response)
        {
            string result = "";
            string encoding = "UTF-8";
            if (response.ContentEncoding != null && response.ContentEncoding.Length > 0)
            {
                encoding = response.ContentEncoding;
            }

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding)))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        private byte[] GetHttpBytesResponse(HttpWebResponse response)
        {
            byte[] result = null;

            using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
            {
                result = reader.ReadBytes((int)response.ContentLength);
            }
            return result;
        }

        private void AsyncGetHttpResponse(HttpWebRequest request, HttpResponseCallback callback)
        {
            request.BeginGetResponse((result) =>
            {
                try
                {
                    string response = GetHttpResponse(request.EndGetResponse(result) as HttpWebResponse);
                    MainThreadTaskService.Instance.Run(() =>
                    {
                        callback(true, response);
                    });
                }
                catch (Exception e)
                {
                    MainThreadTaskService.Instance.Run(() =>
                    {
                        Debug.LogWarning($"Http Service Async Get {request.RequestUri} Response Error: " + e.Message);
                        callback(false, string.Empty);
                    });
                }
            }, null);
        }

        private void AsyncGetHttpBytesResponse(HttpWebRequest request, HttpBytesResponseCallback callback)
        {
            request.BeginGetResponse((result) =>
            {
                try
                {
                    byte[] response = GetHttpBytesResponse(request.EndGetResponse(result) as HttpWebResponse);
                    MainThreadTaskService.Instance.Run(() =>
                    {
                        callback(true, response);
                    });
                }
                catch (Exception e)
                {
                    MainThreadTaskService.Instance.Run(() =>
                    {
                        Debug.LogWarning($"Http Service Async Get {request.RequestUri} Response Error: " + e.Message);
                        callback(false, null);
                    });
                }
            }, null);
        }
    }
}
