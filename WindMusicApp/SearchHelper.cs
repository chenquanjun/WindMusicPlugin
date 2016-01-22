using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace WindMusicApp
{
    enum SearchType : byte
    {
        Invalid,
        SongList,
        SongInfo,
        SongDownload,
    }

    class SearchHelper
    {
        WebClient m_httpClient = null;

        Dictionary<UInt32, SearchType> m_searchDic;
        private UInt32 requestIdOrder = 0;

        private UInt32 genRequestId()
        {
            var requestId = requestIdOrder;
            requestIdOrder += 1;
            return requestId;
        }

        public SearchHelper()
        {
            //download
            m_httpClient = new WebClient();
            m_httpClient.Referer = "http://music.163.com/search/";
            m_httpClient.DownloadEvent += new WebClientDownloadEvent(onDownloadEvent);
            m_httpClient.SaveFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\WindMusic\cache\";

            m_searchDic = new Dictionary<UInt32, SearchType>();
        }

        private void setSearchType(UInt32 requestId, SearchType searchType)
        {
            Debug.Assert(!m_searchDic.ContainsKey(requestId), "Warning: search id already exist:" + requestId + " type:" + searchType);
            m_searchDic.Add(requestId, searchType);
        }

        private SearchType getSearchType(UInt32 requestId)
        {
            if (m_searchDic.ContainsKey(requestId)) // True 
            {
                var searchType = m_searchDic[requestId];
                m_searchDic.Remove(requestId);
                return searchType;
            }
            return SearchType.Invalid;
        }

        private void onDownloadEvent(object sender, DownloadEventArgs e)
        {
            var result = e.result;
            var requestId = e.requestId;

            var searchType = getSearchType(requestId);
            Debug.WriteLine("request id:" + requestId + " result:" + result + " searchType" + searchType);

            switch (searchType)
            {
                case SearchType.Invalid:
                    break;
                case SearchType.SongList:
                    figureSongList(result);
                    break;
                case SearchType.SongInfo:
                    figureSongInfo(result);
                    break;
                case SearchType.SongDownload:

                    break;
                default:
                    break;
            }
        }

        private void figureSongInfo(string result)
        {
            var song = JsonHelper.getSong(result);
            if (song == null) {
                return; //音乐不存在
            }
            var quality = song.Quality;
            if (quality == null) {
                return; //不能下载
            }

            //下载歌曲
            var dfsIdStr = quality.DfsId;
            var encryptStr = Util.GetEncryptStr(dfsIdStr);

            var url = String.Format("http://m2.music.126.net/{0}/{1}.mp3", encryptStr, dfsIdStr);
            var requestId = genRequestId();
            setSearchType(requestId, SearchType.SongDownload);
            m_httpClient.Download(url, requestId);
        }

        private void figureSongList(string result)
        {
            var songList = JsonHelper.getSongList(result);

            if (songList.Count > 0)
            {
                var song = songList[0]; //只下载第一首歌
                downloadSongInfo(song);
            }
            else
            {
                Debug.WriteLine("Search: no song download " + result);
            }
        }


        public bool downloadSongList(string keyword)
        {
            if (keyword == null) { return false; }
            if (keyword.Length == 0) { return false; }

            //关键字过滤
            //结果过滤
            //用户过滤
            //数量限制
            var encodeKeyword = System.Web.HttpUtility.UrlEncode(keyword);
            var url = "http://music.163.com/api/search/get/web";
            var postData = "limit=1&s=" + encodeKeyword + "&total=true&type=1&offset=0";
            var requestId = genRequestId();
            setSearchType(requestId, SearchType.SongList);
            m_httpClient.Post(url, postData, requestId);

            return true;
        }

        private void downloadSongInfo(Song song)
        {
            var requestId = genRequestId();
            setSearchType(requestId, SearchType.SongInfo);
            var id = song.Id;

            var url = String.Format("http://music.163.com/api/song/detail/?id={0}&ids=%5B{1}%5D", id, id);
            m_httpClient.Get(url, requestId);
            
               
        }
    }
}
