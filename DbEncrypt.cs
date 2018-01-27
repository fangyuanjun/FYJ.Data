using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace FYJ.Data
{
   public class DbEncrypt :IDbEncrypt
    {
       public static readonly DbEncrypt instance = new DbEncrypt();
        private void GeneralKeyIV(string keyStr, out byte[] key, out byte[] iv)
        {
            //RijndaelManaged rDel = new RijndaelManaged();
            byte[] bytes = Encoding.UTF8.GetBytes(keyStr);
            key = SHA256Managed.Create().ComputeHash(bytes);
            iv = MD5.Create().ComputeHash(bytes);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Encrypt(string text)
        {
            string sKey = "fangyuanjun";
            byte[] inputByteArray = Encoding.UTF8.GetBytes(text);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            byte[] _key;
            byte[] _iv;
            GeneralKeyIV(sKey, out _key, out _iv);
            aes.Key = _key;
            aes.IV = _iv;

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            string result = Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            cs.Dispose();

            return result;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="text">要解密的字符串</param>
        /// <returns></returns>
        public string Decrypt(string text)
        {
            string sKey = "fangyuanjun";
            byte[] inputByteArray = Convert.FromBase64String(text);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            byte[] _key;
            byte[] _iv;
            GeneralKeyIV(sKey, out _key, out _iv);
            aes.Key = _key;
            aes.IV = _iv;

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            cs.Close();
            string str = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();

            return str;
        }

    }
}
