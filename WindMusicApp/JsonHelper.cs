using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace WindMusicApp
{
    [System.Flags]
    public enum QualityType
    {
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

        public string Dfsid { get; set; }

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
            Dfsid = jo["dfsid"].ToString();
        }

        public static QualityType isQualityExist(JObject jo)
        {
            var h = JsonHelper.isKeyExist(jo, "hMusic") ? QualityType.H : ~QualityType.H;
            var m = JsonHelper.isKeyExist(jo, "mMusic") ? QualityType.M : ~QualityType.M;
            var l = JsonHelper.isKeyExist(jo, "lMusic") ? QualityType.L : ~QualityType.L;
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

            if (isQualityExist != 0)
            {
                //判断音质优先级
                SongQuality quality = null;
                if ((isQualityExist & QualityType.H) == QualityType.H)
                {
                    quality = new SongQuality(jo);
                }

                if (quality != null & (isQualityExist & QualityType.M) == QualityType.M)
                {
                    quality = new SongQuality(jo);
                }

                if (quality != null & (isQualityExist & QualityType.L) == QualityType.L)
                {
                    quality = new SongQuality(jo);
                }

                Quality = quality;
            }
  
        }
        public UInt32 Id { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }

        public SongQuality Quality { get; set; }
    } 
    public class JsonHelper
    {

        public static bool isKeyExist(JObject jo, string key)
        {
            var value = jo.Property(key);
            if (value == null)
            {
                return false;
            }
            if (value.ToString() == "")
            {
                return false;
            }
            return true;
        } 

        public static List<Song> getSongList(string jsonStr)
        {
            if (jsonStr == null || jsonStr.Length == 0)
            {
                return null;
            }

            List<Song>songList = new List<Song>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(jsonStr);

                var resultDic = (JObject)jo["result"];
                var code = jo["code"].ToString();
                var songCount = 0;
                var countStr = resultDic["songCount"].ToString();
                Int32.TryParse(countStr, out songCount);

                if (code.CompareTo("200") == 0 && songCount > 0)
                {
                    var songDic = (JArray)resultDic["songs"];

                    for (int i = 0; i < songDic.Count; ++i)  //遍历JArray
                    {

                        JObject tmpJSong = JObject.Parse(songDic[i].ToString());
                        var song = new Song(tmpJSong);
                        songList.Add(song);
                    }
                }

            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            return songList;
        }

    }
}
