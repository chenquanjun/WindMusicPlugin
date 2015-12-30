using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace WindMusicApp
{
    public delegate void MusicEventAddFolderHandler(string folderName);
        
    class MusicPlayer
    {
        private int m_defaultVolume = 35;

        private AxWMPLib.AxWindowsMediaPlayer m_mediaPlayer = null;
        private System.Windows.Forms.Timer musicTimer = null;
        //private string[] musicFolderNames; 
        ArrayList m_musicFolderNames;
        public event MusicEventAddFolderHandler AddFolderEvent;

        private void InitMusicPlayer()
        {
            m_mediaPlayer.settings.volume = m_defaultVolume; //读取配置表或者使用默认值35
            m_mediaPlayer.settings.autoStart = false;

            m_musicFolderNames = new ArrayList();//读取保存的文件路径
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

        public void TimerTicker()
        {

        }

        public void play()
        {
            m_mediaPlayer.Ctlcontrols.play();
            musicTimer.Enabled = true;
        }

        public void pause()
        {
            m_mediaPlayer.Ctlcontrols.pause();
            musicTimer.Enabled = false;
        }

        public void stop()
        {
            m_mediaPlayer.Ctlcontrols.stop();
            musicTimer.Enabled = false;
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

        //1=停止，2=暂停，3=播放，6=正在缓冲，9=正在连接，10=准备就绪
        public int playState()
        {
            return (int)m_mediaPlayer.playState;
        }
        public void SetMusicFileName(string fileName)
        {
            m_mediaPlayer.URL = fileName;
        }


        public bool AddMusicFolder(string folderName)
        {
            bool isOk = true;
            for (int i = 0; i < m_musicFolderNames.Count; i++)
            {
                string tmpFolderName = m_musicFolderNames[i] as string;
                
                if (tmpFolderName.CompareTo(folderName) == 0) {
                    isOk = false;
                    break;
                }
            }

            if (isOk)
            {
                m_musicFolderNames.Add(folderName);
                if (AddFolderEvent != null)
                {
                    AddFolderEvent(folderName);
                }
            }

            return isOk;
        }
    }
}
