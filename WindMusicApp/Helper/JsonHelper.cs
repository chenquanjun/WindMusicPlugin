using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace WindMusicApp
{

    public class JsonHelper
    {

        public static bool isKeyExist(JObject jo, string key)
        {
            var property = jo.Property(key);
            if (property == null)
            {
                return false;
            }
            var value = property.Value;
            if (value.HasValues == false)
            {
                return false;
            }
            return true;
        }

        public static Song getSong(string jsonStr)
        {
            if (jsonStr == null || jsonStr.Length == 0)
            {
                return null;
            }

            Song song = null;
            try
            {
                JObject jo = JObject.Parse(jsonStr);
                var songDic = (JArray)jo["songs"];
                if (songDic.Count > 0)
                {
                    JObject tmpJSong = JObject.Parse(songDic[0].ToString());
                    song = new Song(tmpJSong);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
            return song;
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
                JObject jo = JObject.Parse(jsonStr);

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
