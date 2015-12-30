using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

using Wayfarer.BroadCast.Common;

namespace WindMusicPlugin
{
    public class Wind_Music : BilibiliDM_PluginFramework.DMPlugin
    {
        private int PluginAuthRommId;


        //初始化
        public Wind_Music()
        {
            this.Connected += Wind_Music_Connected;
            this.Disconnected += Wind_Music_Disconnected;
            this.ReceivedDanmaku += Wind_Music_ReceivedDanmaku;
            this.ReceivedRoomCount += Wind_Music_ReceivedRoomCount;
            this.PluginAuth = "往事如风";
            this.PluginName = "如风音乐插件";
            this.PluginCont = "http://live.bilibili.com/46490";
            this.PluginVer = "v0.0.1";
            this.PlubinDesc = "一个简约而不简单的弹幕点歌音乐插件";
            PluginAuthRommId = 46490;

            //启动插件
            Start();
        }


        //房间人数
        private void Wind_Music_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
        {
            try
            {

            }
            catch (Exception pExc)
            {
                throw new Exception(pExc.Message);
            }
        }

        //收到弹幕
        private void Wind_Music_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
        {
            try
            {
                var pDanmuku = e.Danmaku;
      
                foreach (System.Reflection.PropertyInfo p in pDanmuku.GetType().GetProperties())
                {
                    Debug.WriteLine("Name:{0} Value:{1}", p.Name, p.GetValue(pDanmuku, null));
                }
            }
            catch (Exception pExc)
            {
                throw new Exception(pExc.Message);
            }
        }

        //断开直播间
        private void Wind_Music_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
        {
            try
            {

            }
            catch (Exception pExc)
            {
                throw new Exception(pExc.Message);
            }
        }

        //连接直播间
        private void Wind_Music_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
        {
            try
            {
                int roomId = e.roomid;
                if (roomId == this.PluginAuthRommId) {
                    this.AddDM("欢迎来到作者的直播间！");
                }
            }
            catch (Exception pExc)
            {
                throw new Exception(pExc.Message);
            }
        }

        //管理插件方法
        public override void Admin()
        {
            base.Admin();
            Debug.WriteLine("Plugin Admin");
        }

        //禁用插件方法 
        public override void Stop()
        {
            base.Stop();
            //請勿使用任何阻塞方法
            Debug.WriteLine("Plugin Stoped!");
        }

        //啟用插件方法
        public override void Start()
        {
            base.Start();
            //請勿使用任何阻塞方法
            Debug.WriteLine("Plugin Started!");
        }

        public override void DeInit()
        {
            base.DeInit();
            //請勿使用任何阻塞方法
            Debug.WriteLine("Plugin DeInit!");
        }
        
    }
}
