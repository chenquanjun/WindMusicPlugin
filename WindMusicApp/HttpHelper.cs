using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using RE = System.Text.RegularExpressions.Regex;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net.Security;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.IO.Compression;

namespace WindMusicApp
{
    public delegate void WebClientDownloadEvent(object sender, DownloadEventArgs e);

    ///<summary>  
    ///下载事件参数  
    ///</summary>  
    public struct DownloadEventArgs
    {
        public string result;
        public int requestId;

    }

    public class WebAsyncObject
    {
        public HttpWebRequest request { get; set; }
        public int requestId { get; set; }

        public WebClient webClient { get; set; }
    } 

    ///<summary>  
    ///实现向WEB服务器发送和接收数据  
    ///</summary>  
    public class WebClient
    {
        public event WebClientDownloadEvent DownloadEvent;

        private string referer = ""; //http://music.163.com/search/
        private int timeout = 10000; //ms
        private string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.152 Safari/537.36"; 
        private int requestIdOrder = 0;
        ///<summary>  
        ///初始化WebClient类  
        ///</summary>  
        public WebClient()
        {

        }

        public string UserAgent
        {
            set { userAgent = value; }
            get { return userAgent; }
        }

        public int Timeout
        {
            set { timeout = value; }
            get { return timeout; }
        }

        public string Referer
        {
            set { referer = value; }
            get { return referer; }
        }
        private HttpWebRequest CreateRequest(string url, string method)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = method;
            req.UserAgent = userAgent;
            req.Timeout = timeout;
            req.KeepAlive = false;
            return req;
        }

        private int genRequestId(){
            var requestId = requestIdOrder;
            requestIdOrder += 1;
            return requestId;
        }

        public int Get(string url)
        {
            var requestId = genRequestId();

            HttpWebRequest req = CreateRequest(url, "GET");

            WebAsyncObject webObject = new WebAsyncObject();
            webObject.request = req;
            webObject.requestId = requestId;
            webObject.webClient = this;

            req.BeginGetResponse(new AsyncCallback(OnResponse), webObject);

            return requestId;
        }

        public int Post(string url, string postData)
        {
            var requestId = genRequestId();

            HttpWebRequest req = CreateRequest(url, "POST");
            byte[] bs = Encoding.ASCII.GetBytes(postData);
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;
            
            if (referer.Length > 0)
            {
                req.Referer = referer;
            }
            req.Accept = "*/*";

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Close();
            }

            WebAsyncObject webObject = new WebAsyncObject();
            webObject.request = req;
            webObject.requestId = requestId;
            webObject.webClient = this;

            req.BeginGetResponse(new AsyncCallback(OnResponse), webObject);

            return requestId;
        }

        static void OnResponse(IAsyncResult ar)
        {
            var webObject = (WebAsyncObject)ar.AsyncState;
            var request = webObject.request;
            var requestId = webObject.requestId;
            var webClient = webObject.webClient;

            HttpWebResponse response = null;
            Stream stream = null;
            StreamReader sr = null;
            String result = "";
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(ar);
                stream = response.GetResponseStream();
                sr = new StreamReader(stream, Encoding.UTF8);
                result = sr.ReadToEnd();
                //Debug.WriteLine(result);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

            }
            finally
            {

                if (sr != null)
                {
                    sr.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                }

                if (response != null)
                {
                    response.Close();
                }
                request.Abort();
            }

            if (webClient.DownloadEvent != null)
            {
                DownloadEventArgs args =new DownloadEventArgs();
                args.requestId = requestId;
                args.result = result;
                webClient.DownloadEvent(webClient, args);
            }

        }



    }
}