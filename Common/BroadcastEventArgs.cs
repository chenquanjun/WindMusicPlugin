using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wayfarer.BroadCast.Common
{
    [Serializable]
    public class BroadcastEventArgs : EventArgs
    {

        public string RawData { set; get; }

        public BroadcastEventArgs(BilibiliDM_PluginFramework.ReceivedDanmakuArgs args)
        {
            var danmuku = args.Danmaku;
            RawData = danmuku.RawData;
        }

        public BilibiliDM_PluginFramework.ReceivedDanmakuArgs DanmakuArgs
        {
            get {
                var danmuku = new BilibiliDM_PluginFramework.DanmakuModel(RawData, 2);
                var arg = new BilibiliDM_PluginFramework.ReceivedDanmakuArgs();
                arg.Danmaku = danmuku;
                return arg;
           
            }
        }
    }
}
