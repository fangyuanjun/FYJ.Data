using System;
using System.Collections.Generic;
using System.Text;

namespace FYJ.Data
{
   public interface IDbEncrypt
    {
     
       /// <summary>
       /// 对称数据解密
       /// </summary>
       /// <param name="text">解密的字符串</param>
       /// <returns></returns>
       string Decrypt(string text);
    }
}
