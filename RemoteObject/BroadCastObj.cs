using System;
using System.Runtime.Remoting.Messaging;
using Wayfarer.BroadCast.Common;
using System.Diagnostics;

namespace Wayfarer.BroadCast.RemoteObject
{
    /// <summary>
    /// Class1 的摘要说明。
    /// </summary>
    public class BroadCastObj : MarshalByRefObject, IBroadCast
    {
        public event BroadCastEventHandler BroadCastEvent;
        public event BroadCastDanmakuArgsEventHandler BroadCastDanmakuArgsEvent;

        #region IBroadCast 成员

        //[OneWay]
        public void BroadCastingInfo(string info)
        {
            if (BroadCastEvent != null)
            {
                BroadCastEventHandler tempEvent = null;

                int index = 1; //记录事件订阅者委托的索引，为方便标识，从1开始。
                foreach (Delegate del in BroadCastEvent.GetInvocationList())
                {
                    try
                    {
                        tempEvent = (BroadCastEventHandler)del;
                        tempEvent(info);
                    }
                    catch
                    {
                        Debug.WriteLine("事件订阅者" + index.ToString() + "发生错误,系统将取消事件订阅!");
                        BroadCastEvent -= tempEvent;
                    }
                    index++;
                }
            }
            else
            {
                Debug.WriteLine("事件未被订阅或订阅发生错误!");
            }
        }

        public void BroadCastingDanmakuArgsInfo(BroadcastEventArgs e)
        {
            if (BroadCastDanmakuArgsEvent != null)
            {
                BroadCastDanmakuArgsEventHandler tempEvent = null;

                int index = 1; //记录事件订阅者委托的索引，为方便标识，从1开始。
                foreach (Delegate del in BroadCastDanmakuArgsEvent.GetInvocationList())
                {
                    try
                    {
                        tempEvent = (BroadCastDanmakuArgsEventHandler)del;
                        tempEvent(e);
                    }
                    catch
                    {
                        Debug.WriteLine("事件订阅者" + index.ToString() + "发生错误,系统将取消事件订阅!");
                        BroadCastDanmakuArgsEvent -= tempEvent;
                    }
                    index++;
                }
            }
            else
            {
                Debug.WriteLine("事件未被订阅或订阅发生错误!");
            }
        }
        #endregion

        public override object InitializeLifetimeService()
        {
            return null;
        }

    }
}
