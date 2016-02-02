using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WindMusicApp
{
   
    public enum MusicPlayState : byte
    {
        Invalid = 0,
        Stop = 1, //已经停止
        Pause = 2, //暂停
        Playing = 3, //播放中
        Resume = 4, //恢复播放
    }

    class MusicHelper
    {
        private int m_defaultVolume = 35;

        private AxWMPLib.AxWindowsMediaPlayer m_mediaPlayer = null;
        private System.Windows.Forms.Timer musicTimer = null;

        private UInt32 m_curPlayId;

        public event MusicEventMusicPlayStateHandler MusicPlayStateEvent;
        public event MusicEventMusicPlayDurationHandler MusicPlayDurationEvent;

        private void tryPostPlayState(MusicPlayState state, UInt32 playerId)
        {
            if (playerId != 0 && MusicPlayStateEvent != null)
            {
                MusicPlayStateEvent(playerId, state);
            }
        }

        private void tryPostPlayDuration(double curDur, double totalDur)
        {
            if (m_curPlayId != 0 && MusicPlayDurationEvent != null)
            {
                MusicPlayDurationEvent(m_curPlayId, curDur, totalDur);
            }
        }

        private void InitMusicPlayer()
        {
            m_mediaPlayer.settings.volume = m_defaultVolume; //读取配置表或者使用默认值35
            m_mediaPlayer.settings.autoStart = true;
            m_mediaPlayer.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(player_PlayStateChange);
            m_curPlayId = 0;
        }

        public void SetMusicPlayer(AxWMPLib.AxWindowsMediaPlayer player)
        {
            Debug.Assert(m_mediaPlayer == null, "should init player only one time");
            m_mediaPlayer = player;
            InitMusicPlayer();
        }

        public void SetTimer(System.Windows.Forms.Timer timer)
        {
            musicTimer = timer;
        }

        private void player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            WMPLib.WMPPlayState state = (WMPLib.WMPPlayState)e.newState;
            Debug.WriteLine("Play state:" + state);
            switch (state)
            {
                case WMPLib.WMPPlayState.wmppsBuffering:
                    break;
                case WMPLib.WMPPlayState.wmppsLast:
                    break;
                case WMPLib.WMPPlayState.wmppsMediaEnded:
                    break;
                case WMPLib.WMPPlayState.wmppsPaused:
                    break;
                case WMPLib.WMPPlayState.wmppsPlaying:
                    musicTimer.Start();
                    break;
                case WMPLib.WMPPlayState.wmppsReady:
                    break;
                case WMPLib.WMPPlayState.wmppsReconnecting:
                    break;
                case WMPLib.WMPPlayState.wmppsScanForward:
                    break;
                case WMPLib.WMPPlayState.wmppsScanReverse:
                    break;
                case WMPLib.WMPPlayState.wmppsStopped:
                    var playId = m_curPlayId;
                    m_curPlayId = 0;
                    musicTimer.Stop();
                    tryPostPlayState(MusicPlayState.Stop, playId); //停止
                    break;
                case WMPLib.WMPPlayState.wmppsTransitioning:
                    break;
                case WMPLib.WMPPlayState.wmppsUndefined:
                    break;
                case WMPLib.WMPPlayState.wmppsWaiting:
                    break;
                default:
                    break;
            }
        }

        public void TimerTicker(object sender, EventArgs e)
        {
            if (m_curPlayId != 0 && m_mediaPlayer.currentMedia != null)
            {
                double total = Math.Ceiling(m_mediaPlayer.currentMedia.duration);
                double cur = Math.Floor(m_mediaPlayer.Ctlcontrols.currentPosition);
                tryPostPlayDuration(cur, total);
            }
        }

        public void Play(UInt32 playId, string fileName)
        {
            //Debug.WriteLine("Play:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            tryPostPlayState(MusicPlayState.Stop, m_curPlayId); //尝试发送停止消息（切歌模式)
            m_curPlayId = playId;
            tryPostPlayState(MusicPlayState.Playing, playId); //播放中

            Thread thread = new Thread(new ParameterizedThreadStart(ThreadPlay));
            thread.Start((object)fileName);
        }

        private void ThreadPlay(object message)
        {
            //音乐控件在状态变化事件中改变状态会出错
            //所以此处用新线程播放
            string fileName = (string)message;
            m_mediaPlayer.URL = fileName;
            m_mediaPlayer.Ctlcontrols.play();
        }

        public void Resume()
        {
            tryPostPlayState(MusicPlayState.Resume, m_curPlayId); //播放中
            m_mediaPlayer.Ctlcontrols.play();
        }

        public void Pause()
        {
            tryPostPlayState(MusicPlayState.Pause, m_curPlayId); //暂停
            m_mediaPlayer.Ctlcontrols.pause();
        }

        public void Stop(UInt32 playId)
        {
            if (playId == m_curPlayId)
            {
                m_mediaPlayer.Ctlcontrols.stop(); //等待回调
            }
            
        }

        public double totalTime()
        {
            return Math.Ceiling(m_mediaPlayer.currentMedia.duration);
        }

        public double currentTime()
        {
            return Math.Floor(m_mediaPlayer.Ctlcontrols.currentPosition);
        }

        public string musicName()
        {
            return m_mediaPlayer.currentMedia.name;
        }

    }
}
