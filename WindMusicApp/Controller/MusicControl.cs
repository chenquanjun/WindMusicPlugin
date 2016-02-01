using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Diagnostics;


namespace WindMusicApp
{
    /*
     * 点歌模块
     * 1、管理歌曲列表
     * 2、切换播放模式
     * 3、点歌控制
     * (1)点歌权限（一首/多首)
     * (2)点歌上限
     * (3)歌曲黑名单
     * (4)已存在
     */



    class MusicControl
    {
        private enum MusicMode
        {
            Invalid = 0, //无
            LocalMusic = 1, //本地音乐
            DemandMusic = 2, //点歌
        }

        private MusicMode m_curMusicMode = MusicMode.Invalid; //当前播放模式

        private UInt32 m_queueIdOrder; //queue id 生成

        private ArrayList m_localMusicFolderNames; //本地目录列表
        private ArrayList m_localMusicList; //本地音乐列表
        private int m_localMusicIdx;

        private Dictionary<string, bool> m_musicExtDic; //音乐扩展名字典

        private DemandQueue m_demandMusicQue; //点歌音乐队列
        private ArrayList m_tmpDemandQue; //临时点歌队列
        private UInt32 m_busyDemandQueueId; //当前处理的队列id

        private SearchHelper m_searchHelper = null; //搜索模块
        private MusicHelper m_musicPlayer = null; //播放模块

        public event MusicEventAddFolderHandler AddFolderEvent;
        public event MusicEventRemoveFolderHandler RemoveFolderEvent;

        public event MusicEventDemandInfoHandler DemandInfoEvent;
        public event MusicEventDemandInfoDurationHandler DemandInfoDurationEvent;

        
        private readonly string m_demandFormatStr = "点歌 ";


        public MusicControl(MusicHelper player, System.Windows.Forms.Control invokeObj)
        {
            var maxNum = 9;
            //init
            m_localMusicFolderNames = new ArrayList();
            m_localMusicList = new ArrayList();
            m_localMusicIdx = -1;
            m_demandMusicQue = new DemandQueue(maxNum);
            m_tmpDemandQue = new ArrayList();

            m_musicExtDic = new Dictionary<string, bool>();
            m_musicExtDic.Add(".mp3", true);
            m_musicExtDic.Add(".wav", true);
            m_musicExtDic.Add(".wma", true);

            m_searchHelper = new SearchHelper(invokeObj);
            m_searchHelper.SearchResultEvent += new SearchEventSearchResultHandler(onSearchResult);

            m_musicPlayer = player;
            m_musicPlayer.MusicPlayStateEvent += new MusicEventMusicPlayStateHandler(onPlayState);
            m_musicPlayer.MusicPlayDurationEvent += new MusicEventMusicPlayDurationHandler(onPlayDuration);

            m_queueIdOrder = 1; //从1开始,0用来判断无效

            m_busyDemandQueueId = 0;
        }

        public void TimerMusic_Tick(object sender, EventArgs e)
        {
            m_musicPlayer.TimerTicker(sender, e);
        }

        private void onPlayState(UInt32 queueId, MusicPlayState playState)
        {
            var demandInfo = m_demandMusicQue.GetInfo(queueId);
            Debug.Assert(demandInfo != null, "should exist demand info:" + queueId + " state:" + playState);

            switch (playState)
            {
                case MusicPlayState.Stop:
                    demandInfo.Status = DemandSongStatus.PlayEnd;
                    removeDemandInfo(demandInfo);
                    tryPlayMusic();
                    break;
                case MusicPlayState.Pause:
                    break;
                case MusicPlayState.Playing:
                    demandInfo.Status = DemandSongStatus.Playing;
                    postDemandInfo(demandInfo);
                    break;
                case MusicPlayState.Resume:
                    break;
                default:
                    break;
            }
        }

        private bool isKeywordExist(string keyword)
        {
            //首先判断是否在歌曲列表
            var isExistQue = m_demandMusicQue.IsKeywordExist(keyword);

            if (isExistQue){return true; }

            //再判断是否在待处理列表
            for (int i = 0; i < m_tmpDemandQue.Count; i++)
            {
                var tmpInfo = (DemandInfo)m_tmpDemandQue[i];
                if (tmpInfo.Keyword == keyword)
                {
                    isExistQue = true;
                    break;
                }
            }

            return isExistQue;
        }

        private void onPlayDuration(UInt32 queueId, double curDur, double totalDur)
        {
            if (DemandInfoDurationEvent != null)
            {
                DemandInfoDurationEvent(queueId, curDur, totalDur);
            }
        }

        public void tryPlayMusic()
        {
            //尝试播放音乐
            var firstInfo = m_demandMusicQue.First();
            if (firstInfo != null)
            {
                m_curMusicMode = MusicMode.DemandMusic; //点歌模式
                if (firstInfo.Status == DemandSongStatus.WaitPlay) //队列第一个刚好是等待播放状态
                {
                    m_musicPlayer.Play(firstInfo.QueueId, firstInfo.SongInfo.FileName);
                }
                
            }
            else //自定义音乐播放模式
            {
                var musicNum = m_localMusicList.Count;

                if (musicNum == 0)
                {
                    m_curMusicMode = MusicMode.Invalid; //无效状态
                    return;
                }

                m_curMusicMode = MusicMode.LocalMusic; //本地音乐

                var musicIdx = -1;

                if (m_localMusicIdx == -1) //初始化
                {
                    musicIdx = 0;
                }
                else
                {
                    musicIdx = m_localMusicIdx + 1; //下一曲
                    if (musicIdx >= musicNum) //循环
                    {
                        musicIdx = 0;
                    }
                }

                m_localMusicIdx = musicIdx;

                string fileName =(string)m_localMusicList[musicIdx];
                string musicName = Path.GetFileNameWithoutExtension(fileName);
                var demandInfo = new DemandInfo();
                demandInfo.Keyword = musicName;
                demandInfo.QueueId = genQueueId();
                demandInfo.UserName = "本地歌曲";
                demandInfo.Status = DemandSongStatus.WaitPlay;
                demandInfo.IsLocalMusic = true;
                demandInfo.SongInfo = new Song(null) { Id = 0, FileName = fileName, Name = musicName};
                m_demandMusicQue.Add(demandInfo);
                postDemandInfo(demandInfo);

                m_musicPlayer.Play(demandInfo.QueueId, fileName);
            }
        }

        private void onSearchResult(SearchResult resultInfo)
        {
            var queueId = resultInfo.QueueId;
            var songInfo = resultInfo.SongInfo;
            var songPath = resultInfo.SongPath;

            var demandInfo = m_demandMusicQue.GetInfo(queueId);

            var status = DemandSongStatus.Invalid;
            if (demandInfo != null)
            {
                status = demandInfo.Status;
            }

            bool isRemove = false;

            switch (status)
            {
                case DemandSongStatus.Search:
                    if (songInfo == null) //没有搜索结果
                    {
                        isRemove = true;
                        demandInfo.Error = DemandSongError.Search;
                    }
                    else //获取详细信息
                    {
                        var songId = songInfo.Id;
                        var isSongIdExist = m_demandMusicQue.IsSongIdExist(songId, queueId);

                        if (isSongIdExist)
                        {
                            isRemove = true;
                            demandInfo.Error = DemandSongError.Repeat;
                        }
                        else
                        {
                            demandInfo.Status = DemandSongStatus.GetDetail;
                            postDemandInfo(demandInfo);
                            m_searchHelper.GetSongDetail(songInfo, queueId);
                        }

                    }

                    break;
                case DemandSongStatus.GetDetail:
                    if (songInfo == null) //没有搜索结果
                    {
                        isRemove = true;
                        demandInfo.Error = DemandSongError.GetDetail;
                    }
                    else //下载
                    {
                        demandInfo.Status = DemandSongStatus.Download;
                        demandInfo.SongInfo = songInfo;
                        postDemandInfo(demandInfo);
                        m_searchHelper.DownloadSong(songInfo, queueId);

                        tryFinishQueue(queueId);//完成队列
                    }
                    break;
                case DemandSongStatus.Download:
                    if (songPath == null || songPath.Length == 0) //没有搜索结果
                    {
                        isRemove = true;
                        demandInfo.Error = DemandSongError.Download;
                    }
                    else //等待播放
                    {
                        demandInfo.Status = DemandSongStatus.WaitPlay;
                        demandInfo.SongInfo.FileName = songPath;
                        postDemandInfo(demandInfo);

                        //添加到播放列表
                        tryPlayMusic();
                    }
                    break;
                default:
                    Debug.WriteLine("Warning: search result error:" + status + " que:" + queueId);
                    isRemove = true;
                    break;
            }

            if (isRemove) //发生了错误，删除此队列
            {
                removeDemandInfo(demandInfo);
                tryFinishQueue(queueId);//完成队列
            }
        }

        private void tryFinishQueue(UInt32 queueId)
        {
            //尝试完成队列
            if (queueId == m_busyDemandQueueId)
            {
                m_busyDemandQueueId = 0;
                tryFigureTmpDemandQue();
            }
  
        }

        private void removeDemandInfo(DemandInfo info)
        {
            //删除点歌信息
            if (DemandInfoEvent != null)
            {
                DemandInfoEvent(info);
            }
            m_demandMusicQue.Remove(info);
        }

        private string checkDemandMusicName(string keyword)
        {
            //检验点歌格式是否正确
            if (keyword == null || keyword.IndexOf(m_demandFormatStr) != 0)
            {
                Debug.WriteLine("Warning: Ignore damaku with format error:" + keyword);
                return "";
            }

            return keyword.Remove(0, m_demandFormatStr.Length);
        }

        public void OnRcvDamaku(string userName, string keyword)
        {
            //收到弹幕

            //1、判断歌曲数目
            var isFull = m_demandMusicQue.isFull();
            if (isFull)
            {
                Debug.WriteLine("Warning: song reach max num");
                return;
            }

            
            //2、判断是不是弹幕点歌格式
            keyword = checkDemandMusicName(keyword);

            if (keyword.Length == 0)
            {
                Debug.WriteLine("Warning: no keyword:" + keyword);
                return;
            }

            var isExist = isKeywordExist(keyword);

            if (isExist)
            {
                Debug.WriteLine("Warning: keyword already exist:" + keyword);
                return;
            }

            Debug.WriteLine("On demand music:" + keyword);

            //add queue
            var demandInfo = new DemandInfo();
            demandInfo.Keyword = keyword;
            demandInfo.QueueId = genQueueId();
            demandInfo.UserName = userName;
            demandInfo.Status = DemandSongStatus.Queue;

            m_tmpDemandQue.Add(demandInfo);

            postDemandInfo(demandInfo);

            tryFigureTmpDemandQue();
        }

        private void postDemandInfo(DemandInfo info)
        {
            if (DemandInfoEvent != null)
            {
                DemandInfoEvent(info);
            }
        }

        private void tryFigureTmpDemandQue()
        {
            
            if (m_busyDemandQueueId != 0){return;} //处理中

            if (m_tmpDemandQue.Count == 0){return;} //没有可以处理的队列


            //永远处理临时点歌队列的第一个
            var queData = (DemandInfo)m_tmpDemandQue[0];
            m_tmpDemandQue.RemoveAt(0);

            var isFull = m_demandMusicQue.isFull();

            var isKeepOn = false;

            if (isFull) //点歌队列满了，继续处理下一个数据
            {
                isKeepOn = true;
                queData.Error = DemandSongError.Full;
                postDemandInfo(queData);
            }
            else
            {
                m_busyDemandQueueId = queData.QueueId;
                queData.Status = DemandSongStatus.Search; //搜索状态
                m_demandMusicQue.Add(queData);
                postDemandInfo(queData);
                m_searchHelper.SearchSong(queData.Keyword, queData.QueueId);

            }

            if (isKeepOn)
            {
                m_busyDemandQueueId = 0;
                tryFigureTmpDemandQue();
            }
        }

        private UInt32 genQueueId()
        {
            var queueId = m_queueIdOrder;
            m_queueIdOrder += 1;
            return queueId;
        }

        private void setMusicMode(MusicMode mode)
        {
            if (mode == m_curMusicMode)
            {
                return;
            }

            //旧的模式取消
            switch (m_curMusicMode)
            {
                case MusicMode.LocalMusic:
                    break;
                case MusicMode.DemandMusic:
                    break;
                default:
                    break;
            }

            //新的模式切换
            switch (mode)
            {
                case MusicMode.LocalMusic:

                    break;
                case MusicMode.DemandMusic:
                    break;
                default:
                    break;
            }
        }

        public bool AddMusicFolder(string folderName)
        {
            bool isOk = true;
            for (int i = 0; i < m_localMusicFolderNames.Count; i++)
            {
                string tmpFolderName = m_localMusicFolderNames[i] as string;

                if (tmpFolderName.CompareTo(folderName) == 0)
                {
                    isOk = false;
                    break;
                }
            }

            if (isOk)
            {
                m_localMusicFolderNames.Add(folderName);
                if (AddFolderEvent != null)
                {
                    AddFolderEvent(folderName);
                }

                refreshLocalMusicList(); //刷新音乐列表
            }

            return isOk;
        }

        public bool removeMusicFolder(string folderName)
        {
            m_localMusicFolderNames.Remove(folderName);
            if (RemoveFolderEvent != null)
            {
                RemoveFolderEvent(folderName);
            }

            refreshLocalMusicList(); //刷新音乐列表
            return true;
        }

        private void refreshLocalMusicList()
        {
            m_localMusicList.Clear();

            for (int i = 0; i < m_localMusicFolderNames.Count; i++)
            {
                string tmpFolderName = m_localMusicFolderNames[i] as string;
                DirectoryInfo dir = new DirectoryInfo(tmpFolderName);
                FileInfo[] inf = dir.GetFiles();
                foreach (FileInfo finf in inf)
                {
                    var fileExt = finf.Extension;

                    if (m_musicExtDic.ContainsKey(fileExt))
                    {
                        m_localMusicList.Add(finf.FullName);

                        Debug.WriteLine(finf.FullName);
                    }
                }
            }

            m_localMusicIdx = -1;
        }
    }
}
