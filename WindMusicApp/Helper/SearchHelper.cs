using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace WindMusicApp
{

    public class SearchResult
    {
        public UInt32 QueueId { get; set; }

        public Song SongInfo { get; set; }

        public string SongPath { get; set; }
    }

    public delegate void SearchEventSearchResultHandler(SearchResult data);

    class SearchHelper
    {
        private enum SearchType : byte
        {
            Invalid,
            SongList,
            SongInfo,
            SongDownload,
        }

        private class SearchInfoType
        {
            public SearchType searchType;
            public UInt32 queueId;
        }

        public event SearchEventSearchResultHandler SearchResultEvent;


        private WebClient m_httpClient = null;
        private Dictionary<UInt32, SearchInfoType> m_searchTypeInfoDic;
        private UInt32 m_requestIdOrder;

        private System.Windows.Forms.Control m_invokeObj;

        private UInt32 genRequestId()
        {
            var requestId = m_requestIdOrder;
            m_requestIdOrder += 1;
            return requestId;
        }

        public SearchHelper(System.Windows.Forms.Control invokeObj)
        {

            m_invokeObj = invokeObj;
            //download
            m_httpClient = new WebClient();
            m_httpClient.Referer = "http://music.163.com/search/";
            m_httpClient.DownloadEvent += new WebClientDownloadEvent(onDownloadEvent);
            m_httpClient.SaveFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\WindMusic\cache\";

            m_searchTypeInfoDic = new Dictionary<UInt32, SearchInfoType>();

            m_requestIdOrder = 0;
        }

        private void setSearchType(UInt32 requestId, SearchType searchType, UInt32 queueId)
        {
            Debug.Assert(!m_searchTypeInfoDic.ContainsKey(requestId), "Warning: search id already exist:" + requestId + " type:" + searchType);
            var info = new SearchInfoType();
            info.searchType = searchType;
            info.queueId = queueId;
            m_searchTypeInfoDic.Add(requestId, info);
        }

        private SearchInfoType getSearchType(UInt32 requestId)
        {
            if (m_searchTypeInfoDic.ContainsKey(requestId)) // True 
            {
                var searchType = m_searchTypeInfoDic[requestId];
                m_searchTypeInfoDic.Remove(requestId);
                return searchType;
            }
            return null;
        }

        private delegate void OnDownloadCallback(DownloadEventArgs e);
        private void onDownload(DownloadEventArgs e)
        {
            
            // InvokeRequired需要比较调用线程ID和创建线程ID
            // 如果它们不相同则返回true
            if (m_invokeObj.InvokeRequired)
            {
                Debug.WriteLine("onDownload 1:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                var d = new OnDownloadCallback(onDownload);
                m_invokeObj.Invoke(d, new object[] { e });
            }
            else
            {
                Debug.WriteLine("onDownload 2:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                var result = e.result;
                var requestId = e.requestId;

                var searchInfo = getSearchType(requestId);

                if (searchInfo == null)
                {
                    Debug.WriteLine("Warning: requst not exist id:" + requestId + " result:" + result);
                    return;
                }
                var searchType = searchInfo.searchType;
                var queueId = searchInfo.queueId;
                Debug.WriteLine("request id:" + requestId + " que:" + queueId + " result:" + result + " searchType" + searchType);

                Song song = null;
                string songPath = "";
                switch (searchType)
                {
                    case SearchType.Invalid:
                        break;
                    case SearchType.SongList:
                        var songList = JsonHelper.getSongList(result);
                        if (songList.Count > 0)
                        {
                            song = songList[0]; //只下载第一首歌
                        }
                        break;
                    case SearchType.SongInfo:
                        song = JsonHelper.getSong(result);
                        if (song != null)
                        {
                            var quality = song.Quality;
                            if (quality == null)
                            {
                                song = null;
                            }
                        }
                        break;
                    case SearchType.SongDownload:
                        songPath = result;
                        break;
                    default:
                        break;
                }

                var resultInfo = new SearchResult();
                resultInfo.SongInfo = song;
                resultInfo.QueueId = queueId;
                resultInfo.SongPath = songPath;

                if (SearchResultEvent != null)
                {
                    SearchResultEvent(resultInfo);
                }
            }
        }

        private void onDownloadEvent(object sender, DownloadEventArgs e)
        {
            onDownload(e);//切换线程
        }

        public void DownloadSong(Song song, UInt32 queueId)
        {
            var quality = song.Quality;
            var dfsIdStr = quality.DfsId;
            var encryptStr = Util.GetEncryptStr(dfsIdStr);

            var url = String.Format("http://m2.music.126.net/{0}/{1}.mp3", encryptStr, dfsIdStr);
            var requestId = genRequestId();
            setSearchType(requestId, SearchType.SongDownload, queueId);
            m_httpClient.Download(url, requestId);
        }

        public void SearchSong(string keyword, UInt32 queueId)
        {
            var encodeKeyword = System.Web.HttpUtility.UrlEncode(keyword);
            var url = "http://music.163.com/api/search/get/web";
            var postData = "limit=1&s=" + encodeKeyword + "&total=true&type=1&offset=0";
            var requestId = genRequestId();
            setSearchType(requestId, SearchType.SongList, queueId);
            m_httpClient.Post(url, postData, requestId);
        }

        public void GetSongDetail(Song song, UInt32 queueId)
        {
            var requestId = genRequestId();
            setSearchType(requestId, SearchType.SongInfo, queueId);
            var id = song.Id;

            var url = String.Format("http://music.163.com/api/song/detail/?id={0}&ids=%5B{1}%5D", id, id);
            m_httpClient.Get(url, requestId);
            
               
        }
    }
}
