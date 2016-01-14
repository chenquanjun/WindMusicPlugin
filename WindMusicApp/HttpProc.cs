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
/***************************************************************************************************************************************************  
 * *文件名：HttpProc.cs  
 * *创建人：kenter  
 * *日 期：2010.02.23 修改  
* *描 述：实现HTTP协议中的GET、POST请求  
* *使 用：HttpProc.WebClient client = new HttpProc.WebClient();  
 client.Encoding = System.Text.Encoding.Default;//默认编码方式，根据需要设置其他类型  
client.OpenRead("http://www.baidu.com");//普通get请求  
MessageBox.Show(client.RespHtml);//获取返回的网页源代码  
client.DownloadFile("http://www.codepub.com/upload/163album.rar",@"C:\163album.rar");//下载文件  
client.OpenRead("http://passport.baidu.com/?login","username=zhangsan&password=123456");//提交表单，此处是登录百度的示例  
client.UploadFile("http://hiup.baidu.com/zhangsan/upload", @"file1=D:\1.mp3");//上传文件  
client.UploadFile("http://hiup.baidu.com/zhangsan/upload", "folder=myfolder&size=4003550",@"file1=D:\1.mp3");//提交含文本域和文件域的表单  
*****************************************************************************************************************************************************/

namespace HttpProc
{
    ///<summary>  
    ///上传事件委托  
    ///</summary>  
    ///<param name="sender"></param>  
    ///<param name="e"></param>  
    public delegate void WebClientUploadEvent(object sender, HttpProc.UploadEventArgs e);

    ///<summary>  
    ///下载事件委托  
    ///</summary>  
    ///<param name="sender"></param>  
    ///<param name="e"></param>  
    public delegate void WebClientDownloadEvent(object sender, HttpProc.DownloadEventArgs e);


    ///<summary>  
    ///上传事件参数  
    ///</summary>  
    public struct UploadEventArgs
    {
        ///<summary>  
        ///上传数据总大小  
        ///</summary>  
        public long totalBytes;
        ///<summary>  
        ///已发数据大小  
        ///</summary>  
        public long bytesSent;
        ///<summary>  
        ///发送进度(0-1)  
        ///</summary>  
        public double sendProgress;
        ///<summary>  
        ///发送速度Bytes/s  
        ///</summary>  
        public double sendSpeed;
    }

    ///<summary>  
    ///下载事件参数  
    ///</summary>  
    public struct DownloadEventArgs
    {
        ///<summary>  
        ///下载数据总大小  
        ///</summary>  
        public long totalBytes;
        ///<summary>  
        ///已接收数据大小  
        ///</summary>  
        public long bytesReceived;
        ///<summary>  
        ///接收数据进度(0-1)  
        ///</summary>  
        public double ReceiveProgress;
        ///<summary>  
        ///当前缓冲区数据  
        ///</summary>  
        public byte[] receivedBuffer;
        ///<summary>  
        ///接收速度Bytes/s  
        ///</summary>  
        public double receiveSpeed;
    }

    [DataContract]
    public class Items
    {
        [DataMember]
        public List<result> result { get; set; }
        public Items()
        {
            result = new List<result>();
        }

        //把字符串转换为对象
        public static Items FormJson(string json)
        {
            try
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer a = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Items));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    return (Items)a.ReadObject(stream);
                }
            }
            catch (Exception)
            {

                return null;
            }
        }
    }

    [DataContract]
    public class result
    {
        [DataMember]
        public int songCount { get; set; }
        [DataMember]
        public List<songs> songs { get; set; }
    }

    /*
       简单的对象
    */
    [DataContract]
    public class songs
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int duration { get; set; }
    }

    ///<summary>  
    ///实现向WEB服务器发送和接收数据  
    ///</summary>  
    public class WebClient
    {
        private WebHeaderCollection requestHeaders, responseHeaders;
        private TcpClient clientSocket;
        private MemoryStream postStream;
        private Encoding encoding = Encoding.UTF8;
        private const string BOUNDARY = "--HEDAODE--";
        private const int SEND_BUFFER_SIZE = 10245;
        private const int RECEIVE_BUFFER_SIZE = 10245;
        private string cookie = "";
        private string respHtml = "";
        private string strRequestHeaders = "";
        private string strResponseHeaders = "";
        private int statusCode = 0;
        private bool isCanceled = false;
        public event WebClientUploadEvent UploadProgressChanged;
        public event WebClientDownloadEvent DownloadProgressChanged;

        ///<summary>  
        ///初始化WebClient类  
        ///</summary>  
        public WebClient()
        {
            responseHeaders = new WebHeaderCollection();
            requestHeaders = new WebHeaderCollection();
        }


        public void addHeader(string name, string value)
        {
            requestHeaders.Add(name, value);
        }

        ///<summary>  
        ///读取指定URL的文本  
        ///</summary>  
        ///<param name="URL">请求的地址</param>  
        ///<returns>服务器响应文本</returns>  
        public string OpenRead(string URL)
        {
            requestHeaders.Add("Connection", "close");
            SendRequestData(URL, "GET");
            return GetHtml();
        }

        ///<summary>  
        ///读取指定URL的文本  
        ///</summary>  
        ///<param name="URL">请求的地址</param>  
        ///<param name="postData">向服务器发送的文本数据</param>  
        ///<returns>服务器响应文本</returns>  
        public string OpenRead(string URL, string postData)
        {
            byte[] sendBytes = encoding.GetBytes(postData);
            postStream = new MemoryStream();
            postStream.Write(sendBytes, 0, sendBytes.Length);

            requestHeaders.Add("Accept", "*/*");
            requestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
            requestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.4");
            requestHeaders.Add("Cookie", "appver=1.5.0.75771;");
            requestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.152 Safari/537.36");
            requestHeaders.Add("Content-Length", postStream.Length.ToString());
            requestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
            requestHeaders.Add("Connection", "close");

            SendRequestData(URL, "POST");
            return GetHtml();
        }


        ///<summary>  
        ///从指定URL下载文件  
        ///</summary>  
        ///<param name="URL">文件URL地址</param>  
        ///<param name="fileName">文件保存路径,含文件名(如:C:\test.jpg)</param>  
        public void DownloadFile(string URL, string fileName)
        {
            requestHeaders.Add("Connection", "close");
            SendRequestData(URL, "GET");
            FileStream fs = new FileStream(fileName, FileMode.Create);
            SaveNetworkStream(fs, true);
            fs.Close();
            fs = null;
        }

        ///<summary>  
        ///向服务器发送请求  
        ///</summary>  
        ///<param name="URL">请求地址</param>  
        ///<param name="method">POST或GET</param>  
        ///<param name="showProgress">是否显示上传进度</param>  
        private void SendRequestData(string URL, string method, bool showProgress)
        {
            clientSocket = new TcpClient();
            Uri URI = new Uri(URL);
            clientSocket.Connect(URI.Host, URI.Port);

            requestHeaders.Add("Host", URI.Host);

            Debug.WriteLine(requestHeaders);

            byte[] request = GetRequestHeaders(method + " " + URI.PathAndQuery + " HTTP/1.1");
            clientSocket.Client.Send(request);

            //若有实体内容就发送它  
            if (postStream != null)
            {
                byte[] buffer = new byte[SEND_BUFFER_SIZE];
                int count = 0;
                Stream sm = clientSocket.GetStream();
                postStream.Position = 0;

                UploadEventArgs e = new UploadEventArgs();
                e.totalBytes = postStream.Length;
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();//计时器  
                timer.Start();
                do
                {
                    //如果取消就推出  
                    if (isCanceled) { break; }

                    //读取要发送的数据  
                    count = postStream.Read(buffer, 0, buffer.Length);
                    //发送到服务器  
                    sm.Write(buffer, 0, count);

                    //是否显示进度  
                    if (showProgress)
                    {
                        //触发事件  
                        e.bytesSent += count;
                        e.sendProgress = (double)e.bytesSent / (double)e.totalBytes;
                        double t = timer.ElapsedMilliseconds / 1000;
                        t = t <= 0 ? 1 : t;
                        e.sendSpeed = (double)e.bytesSent / t;
                        if (UploadProgressChanged != null) { UploadProgressChanged(this, e); }
                    }

                } while (count > 0);
                timer.Stop();
                postStream.Close();
                //postStream.Dispose();  
                postStream = null;

            }//end if  

        }

        ///<summary>  
        ///向服务器发送请求  
        ///</summary>  
        ///<param name="URL">请求URL地址</param>  
        ///<param name="method">POST或GET</param>  
        private void SendRequestData(string URL, string method)
        {
            SendRequestData(URL, method, false);
        }


        ///<summary>  
        ///获取请求头字节数组  
        ///</summary>  
        ///<param name="request">POST或GET请求</param>  
        ///<returns>请求头字节数组</returns>  
        private byte[] GetRequestHeaders(string request)
        {
            string headers = request + "\r\n";

            foreach (string key in requestHeaders)
            {
                headers += key + ":" + requestHeaders[key] + "\r\n";
            }

            //有Cookie就带上Cookie  
            if (cookie != "") { headers += "Cookie:" + cookie + "\r\n"; }

            //空行，请求头结束  
            headers += "\r\n";

            strRequestHeaders = headers;
            requestHeaders.Clear();
            return encoding.GetBytes(headers);
        }

        ///<summary>  
        ///获取服务器响应文本  
        ///</summary>  
        ///<returns>服务器响应文本</returns>  
        private string GetHtml()
        {
            MemoryStream ms = new MemoryStream();
            SaveNetworkStream(ms);//将网络流保存到内存流  
            StreamReader sr = new StreamReader(ms, encoding);
            respHtml = sr.ReadToEnd();
            sr.Close(); ms.Close();
            return respHtml;
        }

        ///<summary>  
        ///将网络流保存到指定流  
        ///</summary>  
        ///<param name="toStream">保存位置</param>  
        ///<param name="needProgress">是否显示进度</param>  
        private void SaveNetworkStream(Stream toStream, bool showProgress)
        {
            //获取要保存的网络流  
            NetworkStream NetStream = clientSocket.GetStream();

            byte[] buffer = new byte[RECEIVE_BUFFER_SIZE];
            int count = 0, startIndex = 0;

            MemoryStream ms = new MemoryStream();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    count = NetStream.Read(buffer, 0, 500);
                    ms.Write(buffer, 0, count);
                }
                catch (Exception pExc)
                {
                    ms.Flush();
                    Debug.WriteLine("远程服务器断开");
                    break;
                }
            }

            if (ms.Length == 0) { 
                NetStream.Close(); 
                //throw new Exception("远程服务器没有响应");
                Debug.WriteLine("远程服务器没有响应");

                if (showProgress)
                {
                    DownloadEventArgs error = new DownloadEventArgs();

                    error.totalBytes = -1;
                    error.bytesReceived = -1;
                    error.receiveSpeed = -1;
                    if (DownloadProgressChanged != null) { DownloadProgressChanged(this, error); }
                }

                return;
            }

            buffer = ms.GetBuffer();
            count = (int)ms.Length;

            GetResponseHeader(buffer, out startIndex);//分析响应，获取响应头和响应实体  
            count -= startIndex;
            toStream.Write(buffer, startIndex, count);

            DownloadEventArgs e = new DownloadEventArgs();

            if (responseHeaders["Content-Length"] != null)
            { e.totalBytes = long.Parse(responseHeaders["Content-Length"]); }
            else
            { e.totalBytes = -1; }

            //启动计时器  
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            do
            {
                //如果取消就推出  
                if (isCanceled) { break; }

                //显示下载进度  
                if (showProgress)
                {
                    e.bytesReceived += count;
                    e.ReceiveProgress = (double)e.bytesReceived / (double)e.totalBytes;

                    byte[] tempBuffer = new byte[count];
                    Array.Copy(buffer, startIndex, tempBuffer, 0, count);
                    e.receivedBuffer = tempBuffer;

                    double t = (timer.ElapsedMilliseconds + 0.1) / 1000;
                    e.receiveSpeed = (double)e.bytesReceived / t;

                    startIndex = 0;
                    if (DownloadProgressChanged != null) { DownloadProgressChanged(this, e); }
                }

                //读取网路数据到缓冲区  
                count = NetStream.Read(buffer, 0, buffer.Length);

                //将缓存区数据保存到指定流  
                toStream.Write(buffer, 0, count);
            } while (count > 0);

            timer.Stop();//关闭计时器  

            if (responseHeaders["Content-Length"] != null)
            {
                toStream.SetLength(long.Parse(responseHeaders["Content-Length"]));
            }
            else  
            {  
                toStream.SetLength(toStream.Length);  
                responseHeaders.Add("Content-Length", toStream.Length.ToString());//添加响应标头  
            }  

            toStream.Position = 0;

            //关闭网络流和网络连接  
            NetStream.Close();
            clientSocket.Close();
        }


        ///<summary>  
        ///将网络流保存到指定流  
        ///</summary>  
        ///<param name="toStream">保存位置</param>  
        private void SaveNetworkStream(Stream toStream)
        {
            SaveNetworkStream(toStream, false);
        }

        ///<summary>  
        ///分析响应流，去掉响应头  
        ///</summary>  
        ///<param name="buffer"></param>  
        private void GetResponseHeader(byte[] buffer, out int startIndex)
        {
            responseHeaders.Clear();
            string html = encoding.GetString(buffer);
            StringReader sr = new StringReader(html);

            int start = html.IndexOf("\r\n\r\n") + 4;//找到空行位置  
            strResponseHeaders = html.Substring(0, start);//获取响应头文本  

            //获取响应状态码  
            //  
            if (sr.Peek() > -1)
            {
                //读第一行字符串  
                string line = sr.ReadLine();

                //分析此行字符串,获取服务器响应状态码  
                Match M = RE.Match(line, @"\d\d\d");
                if (M.Success)
                {
                    statusCode = int.Parse(M.Value);
                }
            }

            //获取响应头  
            //  
            while (sr.Peek() > -1)
            {
                //读一行字符串  
                string line = sr.ReadLine();

                //若非空行  
                if (line != "")
                {
                    //分析此行字符串，获取响应标头  
                    Match M = RE.Match(line, "([^:]+):(.+)");
                    if (M.Success)
                    {
                        try
                        { //添加响应标头到集合  
                            responseHeaders.Add(M.Groups[1].Value.Trim(), M.Groups[2].Value.Trim());
                        }
                        catch
                        { }


                        //获取Cookie  
                        if (M.Groups[1].Value == "Set-Cookie")
                        {
                            M = RE.Match(M.Groups[2].Value, "[^=]+=[^;]+");
                            cookie += M.Value.Trim() + ";";
                        }
                    }

                }
                //若是空行，代表响应头结束响应实体开始。（响应头和响应实体间用一空行隔开）  
                else
                {
                    //如果响应头中没有实体大小标头，尝试读响应实体第一行获取实体大小  
                    if (responseHeaders["Content-Length"] == null && sr.Peek() > -1)
                    {
                        //读响应实体第一行  
                        line = sr.ReadLine();

                        //分析此行看是否包含实体大小  
                        Match M = RE.Match(line, "~[0-9a-fA-F]{1,15}");

                        if (M.Success)
                        {
                            //将16进制的实体大小字符串转换为10进制  
                            int length = int.Parse(M.Value, System.Globalization.NumberStyles.AllowHexSpecifier);
                            responseHeaders.Add("Content-Length", length.ToString());//添加响应标头  
                            strResponseHeaders += M.Value + "\r\n";
                        }
                    }
                    break;//跳出循环   
                }//End If  
            }//End While  

            sr.Close();

            //实体开始索引  
            startIndex = encoding.GetBytes(strResponseHeaders).Length;
        }

        ///<summary>  
        ///取消上传或下载,要继续开始请调用Start方法  
        ///</summary>  
        public void Cancel()
        {
            isCanceled = true;
        }

        ///<summary>  
        ///启动上传或下载，要取消请调用Cancel方法  
        ///</summary>  
        public void Start()
        {
            isCanceled = false;
        }

        //*************************************************************  
        //以下为属性  
        //*************************************************************  

        ///<summary>  
        ///获取或设置请求头  
        ///</summary>  
        public WebHeaderCollection RequestHeaders
        {
            set { requestHeaders = value; }
            get { return requestHeaders; }
        }

        ///<summary>  
        ///获取响应头集合  
        ///</summary>  
        public WebHeaderCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }

        ///<summary>  
        ///获取请求头文本  
        ///</summary>  
        public string StrRequestHeaders
        {
            get { return strRequestHeaders; }
        }

        ///<summary>  
        ///获取响应头文本  
        ///</summary>  
        public string StrResponseHeaders
        {
            get { return strResponseHeaders; }
        }

        ///<summary>  
        ///获取或设置Cookie  
        ///</summary>  
        public string Cookie
        {
            set { cookie = value; }
            get { return cookie; }
        }

        ///<summary>  
        ///获取或设置编码方式(默认为系统默认编码方式)  
        ///</summary>  
        public Encoding Encoding
        {
            set { encoding = value; }
            get { return encoding; }
        }

        ///<summary>  
        ///获取服务器响应文本  
        ///</summary>  
        public string RespHtml
        {
            get { return respHtml; }
        }


        ///<summary>  
        ///获取服务器响应状态码  
        ///</summary>  
        public int StatusCode
        {
            get { return statusCode; }
        }
    }
}