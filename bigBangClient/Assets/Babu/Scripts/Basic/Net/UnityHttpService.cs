using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Babu
{
    public class UnityHttpService : BabuSingleton<UnityHttpService>
    {
        public delegate void HttpResponseCallback(bool result, string response);

        public void AsyncGet(string url, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncGet(url, null, callback, timeout);
        }

        public void AsyncGet(string url, ArrayList headers, HttpResponseCallback callback, int timeout = 2)
        {
            MainThreadTaskService.Instance.StartCoroutine(AsyncGetInner(url, headers, callback, timeout));
        }

        IEnumerator AsyncGetInner(string url, ArrayList headers, HttpResponseCallback callback, int timeout = 2)
        {
            Debug.Log("Request Url: " + url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = timeout;
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    var arr = ((string)header).Split(':');
                    request.SetRequestHeader(arr[0].Trim(), arr[1].Trim());
                }
            }

            yield return request.SendWebRequest();
            Debug.Log("Request Url Result: " + request.result);
            if (request.result != UnityWebRequest.Result.Success)
            {
                callback(false, string.Empty);
            }
            else
            {
                callback(true, request.downloadHandler.text);
            }
        }

        public void AsyncPost(string url, string data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, Encoding.UTF8.GetBytes(data), callback, timeout);
        }

        public void AsyncPost(string url, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, null, null, data, callback, timeout);
        }

        public void AsyncPost(string url, string contentType, string data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, contentType, Encoding.UTF8.GetBytes(data), callback, timeout);
        }

        public void AsyncPost(string url, string contentType, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            AsyncPost(url, contentType, null, data, callback, timeout);
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

        public void AsyncPost(string url, string contentType, ArrayList headers, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            MainThreadTaskService.Instance.StartCoroutine(AsyncPostInner(url, contentType, headers, data, callback, timeout));
        }

        IEnumerator AsyncPostInner(string url, string contentType, ArrayList headers, byte[] data, HttpResponseCallback callback, int timeout = 2)
        {
            Debug.Log("Request Url: " + url);
            //Debug.Log("Request Data: " + Encoding.UTF8.GetString(data));
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (contentType != null)
            {
                request.SetRequestHeader("Content-Type", contentType);
            }
            request.timeout = timeout;
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    var arr = ((string)header).Split(':');
                    request.SetRequestHeader(arr[0].Trim(), arr[1].Trim());
                }
            }

            yield return request.SendWebRequest();
            Debug.Log($"Request Url {url} Result: " + request.result);
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request Url Failed: {request.error}");
                callback(false, string.Empty);
            }
            else
            {
                callback(true, request.downloadHandler.text);
            }
        }
    }
}