using System;
using System.Security.Cryptography;
using System.Text;

namespace DCache.Utils
{
    public static class General
    {
        public static UInt64 GetHash(string key)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(key);
            bytes = md5.ComputeHash(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
