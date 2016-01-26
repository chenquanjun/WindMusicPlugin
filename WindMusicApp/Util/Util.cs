using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace WindMusicApp
{
    class Util
    {
        private static readonly string magicEncryptString = "3go8&$8*3*3h0k(2)2";

        public static string GetEncryptStr(string id)
        {
            //三步，1、加密，2、md5，3、base64
            var encryptBytes = EncrytedId(id);
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytHash = md5.ComputeHash(encryptBytes);
            md5.Clear();
            var encode = Convert.ToBase64String(bytHash);
            encode = encode.Replace('/', '_');
            encode = encode.Replace('+', '-');
            return encode;
        }

        private static byte[] EncrytedId(string id)
        {
            byte[] byte1 = Encoding.ASCII.GetBytes(magicEncryptString);
            byte[] byte2 = Encoding.ASCII.GetBytes(id);
            int byte1Length  = byte1.Length;
            for (int i = 0; i < byte2.Length; i++)
            {
                var tmp = byte1[i % byte1Length];
                byte2[i] = (byte)(byte2[i] ^ tmp);
            }   
            return byte2;
        }

    }
}
