using System;
using System.Runtime.Remoting.Messaging;

namespace Wayfarer.BroadCast.Common
{
    /// <summary>
    /// EventClass 的摘要说明。
    /// </summary>
    public class EventWrapper : MarshalByRefObject
    {
        public event BroadCastEventHandler LocalBroadCastEvent;
        public event BroadCastDanmakuArgsEventHandler LocalBroadCastDanmakuArgsEvent;

        //[OneWay]
        public void BroadCasting(string message)
        {
            LocalBroadCastEvent(message);
        }

        public void BroadCastingDanmakuArgs(BroadcastEventArgs e)
        {
            LocalBroadCastDanmakuArgsEvent(e);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

    }
}
