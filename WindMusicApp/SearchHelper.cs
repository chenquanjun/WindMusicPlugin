using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindMusicApp
{
    class SearchHelper
    {
        WebClient m_httpClient = null;

        public SearchHelper()
        {
            //download
            m_httpClient = new WebClient();
            m_httpClient.Referer = "http://music.163.com/search/";
            m_httpClient.DownloadEvent += new WebClientDownloadEvent(onDownloadEvent);
        }

        private void onDownloadEvent(object sender, DownloadEventArgs e)
        {
            var result = e.result;
            var requestId = e.requestId;
            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
            Debug.WriteLine("request id:" + requestId + " result:" + result);
        }

        public bool search(string keyword)
        {
            if (keyword == null) { return false; }
            if (keyword.Length == 0) { return false; }

            //关键字过滤
            //结果过滤
            //用户过滤
            //数量限制

            var url = "http://music.163.com/api/search/get/web";
            var postData = "limit=1&s=" + keyword + "&total=true&type=1&offset=0";
            m_httpClient.Post(url, postData);

            return true;

            //JObject jo = (JObject)JsonConvert.DeserializeObject(respone);
        }
    }
}
