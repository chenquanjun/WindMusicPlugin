using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WindMusicApp
{

    public enum DemandSongStatus : byte
    {
        Invalid = 0, //无
        Queue = 1, //等待
        Search = 2, //搜索
        GetDetail = 3, //获取信息
        Download = 4, //下载
        WaitPlay = 5, //等待播放
        Playing = 6, //播放中
        PlayEnd = 7, //播放结束
    }

    public enum DemandSongError : byte
    {
        Invalid = 0,
        Full = 1, //歌单满了
        Search = 2, //搜索失败
        GetDetail = 3, //获取失败
        Download = 4, //下载失败
        Cancel = 5, //用户取消
        Repeat = 6, //重复
    }

    public class DemandInfo
    {
        public DemandInfo()
        {
            Error = DemandSongError.Invalid;
        }

        public string UserName { get; set; }

        public string Keyword { get; set; }

        public UInt32 QueueId { get; set; }

        public Song SongInfo { get; set; }

        public DemandSongStatus Status { get; set; }

        public DemandSongError Error { get; set; }

        public bool isError()
        {
            return Error != DemandSongError.Invalid;
        }

        //public DemandInfo Clone()
        //{
        //    Type t = GetType();
        //    PropertyInfo[] properties = t.GetProperties();
        //    Object p = t.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, this, null);
        //    foreach (PropertyInfo pi in properties)
        //    {
        //        if (pi.CanWrite)
        //        {
        //            object value = pi.GetValue(this, null);
        //            pi.SetValue(p, value, null);
        //        }
        //    }
        //    return (DemandInfo)p;
        //}
    }

}
