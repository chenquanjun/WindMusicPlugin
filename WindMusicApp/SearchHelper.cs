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

            m_searchDic = new Dictionary<UInt32, SearchType>();

            //var result = "{\"result\":{\"songCount\":25,\"songs\":[{\"id\":40915964,\"name\":\"???\",\"artists\":[{\"id\":0,\"name\":\"Xe(Xenoglossy)\",\"picUrl\":null,\"alias\":[],\"albumSize\":0,\"picId\":0,\"trans\":null,\"img1v1Url\":\"http://p4.music.126.net/6y-UleORITEDbvrOLV0Q8A==/5639395138885805.jpg\",\"img1v1\":0}],\"album\":{\"id\":3439827,\"name\":\"ノコギリシャルロット -Black Label-\",\"artist\":{\"id\":0,\"name\":\"\",\"picUrl\":null,\"alias\":[],\"albumSize\":0,\"picId\":0,\"trans\":null,\"img1v1Url\":\"http://p4.music.126.net/6y-UleORITEDbvrOLV0Q8A==/5639395138885805.jpg\",\"img1v1\":0},\"publishTime\":1451491200000,\"size\":0,\"copyrightId\":0,\"status\":0,\"picId\":3294136844431904},\"duration\":198582,\"copyrightId\":0,\"status\":0,\"alias\":[],\"fee\":0,\"mvid\":0,\"rtype\":0,\"rUrl\":null,\"ftype\":0}]},\"code\":200}";
            //var songList = JsonHelper.getSongList(result);
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
                default:
                    break;
            }
        }

        private void figureSongInfo(string result)
        {

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

            var url = "http://music.163.com/api/search/get/web";
            var postData = "limit=1&s=" + keyword + "&total=true&type=1&offset=0";
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
