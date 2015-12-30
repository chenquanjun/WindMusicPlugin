using System;

namespace Wayfarer.BroadCast.Common
{
    public delegate void BroadCastEventHandler(string info);
    public delegate void BroadCastDanmakuArgsEventHandler(BroadcastEventArgs e);

    public interface IBroadCast
    {
        event BroadCastEventHandler BroadCastEvent;
        event BroadCastDanmakuArgsEventHandler BroadCastDanmakuArgsEvent;
        void BroadCastingInfo(string info);
        void BroadCastingDanmakuArgsInfo(BroadcastEventArgs e);
    }
}
