using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindMusicApp
{
    [System.Flags]
    public enum QualityType
    {
        Invalid = 0,
        H = 1,  //001
        M = 2,  //010
        L = 4,  //100
        All = 7, //111
    }
    public class SongQuality
    {
        public UInt32 Id { get; set; }
        public UInt32 Size { get; set; }
        public string Extension { get; set; }
        public int Bitrate { get; set; }

        public string DfsId { get; set; }

        public SongQuality(JObject jo)
        {
            UInt32 id;
            UInt32.TryParse(jo["id"].ToString(), out id);
            Id = id;

            UInt32 size;
            UInt32.TryParse(jo["size"].ToString(), out size);
            Size = size;

            int bitrate;
            int.TryParse(jo["bitrate"].ToString(), out bitrate);
            Bitrate = bitrate;

            Extension = jo["extension"].ToString();
            DfsId = jo["dfsId"].ToString();
        }

        public static QualityType isQualityExist(JObject jo)
        {
            var h = JsonHelper.isKeyExist(jo, "hMusic") ? QualityType.H : QualityType.Invalid;
            var m = JsonHelper.isKeyExist(jo, "mMusic") ? QualityType.M : QualityType.Invalid;
            var l = JsonHelper.isKeyExist(jo, "lMusic") ? QualityType.L : QualityType.Invalid;
            return h | m | l;
        }
    }

    public class Song
    {
        public Song(JObject jo)
        {
            UInt32 id;
            UInt32.TryParse(jo["id"].ToString(), out id);
            Id = id;

            int duration;
            Int32.TryParse(jo["duration"].ToString(), out duration);
            Duration = duration;

            Name = jo["name"].ToString();

            var isQualityExist = SongQuality.isQualityExist(jo);

            if (isQualityExist != QualityType.Invalid)
            {
                //判断音质优先级
                SongQuality quality = null;
                if ((isQualityExist & QualityType.H) == QualityType.H)
                {
                    var jQuality = (JObject)jo["hMusic"];
                    quality = new SongQuality(jQuality);
                }

                if (quality == null & (isQualityExist & QualityType.M) == QualityType.M)
                {
                    var jQuality = (JObject)jo["mMusic"];
                    quality = new SongQuality(jQuality);
                }

                if (quality == null & (isQualityExist & QualityType.L) == QualityType.L)
                {
                    var jQuality = (JObject)jo["lMusic"];
                    quality = new SongQuality(jQuality);
                }

                Quality = quality;
            }

        }
        public UInt32 Id { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }

        public SongQuality Quality { get; set; }
    } 
}
