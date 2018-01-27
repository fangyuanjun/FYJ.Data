using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FYJ.Data.Util
{
   public static class DataSetHelper
   {
       #region 序列化和反序列化
       /// <summary>
       /// 序列化DataSet  并压缩 
       /// </summary>
       /// <param name="ds"></param>
       /// <returns></returns>
       public static string Serialize(this DataSet ds)
       {
           IFormatter formatter = new BinaryFormatter();
           MemoryStream stream_ = new MemoryStream();
           formatter.Serialize(stream_, ds);
           byte[] buffer_all = stream_.ToArray();
           stream_.Close();
           byte[] bytes_c = Compression(buffer_all, CompressionMode.Compress);//Compress
           return Convert.ToBase64String(bytes_c);
       }

       /// <summary>
       /// 反序列化DataSet
       /// </summary>
       /// <param name="src"></param>
       /// <returns></returns>
       public static DataSet Deserialize(string src)
       {
           MemoryStream stream = new MemoryStream();
           byte[] buffer = Convert.FromBase64String(src);
           byte[] bytes = Compression(buffer, CompressionMode.Decompress);
           stream = new MemoryStream(bytes);
           IFormatter formatter = new BinaryFormatter();
           DataSet ds = (DataSet)formatter.Deserialize(stream);
           stream.Close();
           return ds;
       }
       #endregion

       #region  压缩与解压缩
       private static byte[] Compression(byte[] data, CompressionMode mode)
       {
          
           //DeflateStream zip = null;
           GZipStream zip = null;
           try
           {
               if (mode == CompressionMode.Compress)
               {
                   MemoryStream ms = new MemoryStream();
                   zip = new GZipStream(ms, mode, true);
                   zip.Write(data, 0, data.Length);
                   zip.Close();
                   zip.Dispose();
                   byte[] bData = ms.ToArray();
                   ms.Close();
                   ms.Dispose();
                   return bData;
               }
               else
               {
                   byte[] bData;
                   MemoryStream ms = new MemoryStream();
                   ms.Write(data, 0, data.Length);
                   ms.Position = 0;
                   GZipStream stream = new GZipStream(ms, CompressionMode.Decompress, true);
                   byte[] buffer = new byte[1024];
                   MemoryStream temp = new MemoryStream();
                   int read = stream.Read(buffer, 0, buffer.Length);
                   while (read > 0)
                   {
                       temp.Write(buffer, 0, read);
                       read = stream.Read(buffer, 0, buffer.Length);
                   }

                   //必须把stream流关闭才能返回ms流数据,不然数据会不完整
                   stream.Close();
                   stream.Dispose();
                   ms.Close();
                   ms.Dispose();
                   bData = temp.ToArray();
                   temp.Close();
                   temp.Dispose();
                   return bData;
               }
           }
           catch
           {
               if (zip != null) zip.Close();
               return null;
           }
           finally
           {
               if (zip != null) zip.Close();
           }
       }
       #endregion

       #region  获取第一个表第一行第一列各种数据类型
       /// <summary>
       /// 返回第一个DataTable第一行第一列
       /// </summary>
       /// <param name="ds"></param>
       /// <returns></returns>
       public static object GetObject(this DataSet ds)
       {
           if (ds == null || ds.Tables.Count == 0)
           {
               return null;
           }

           if (ds.Tables[0] == null || ds.Tables[0].Rows.Count==0)
           {
               return null;
           }

           return ds.Tables[0].Rows[0][0];
       }

       public static string GetString(this DataSet ds)
       {
           object obj = GetObject(ds);
           return obj == null ? "" : obj.ToString();
       }

       /// <summary>
       /// 获取长整形  如果是bool型  true返回1  false返回0
       /// </summary>
       /// <param name="ds"></param>
       /// <returns></returns>
       public static long GetLong(this DataSet ds)
       {
           object obj = GetObject(ds);
           if (obj == null||obj.ToString().Trim()=="")
           {
               return 0;
           }
           long value=0;
           if(Int64.TryParse(obj.ToString(),out value))
           {
               return value;
           }

           bool b = false;
           if (Boolean.TryParse(obj.ToString(), out b))
           {
               return b?1:0;
           }

           throw new Exception("无法转换成数字");
       }

       public static int GetInt(this DataSet ds)
       {
           return (int)GetLong(ds);
       }

       public static bool GetBoolean(this DataSet ds)
       {
           return GetInt(ds) > 0 ? true : false;
       }

       public static double GetDouble(this DataSet ds)
       {
           object obj = GetObject(ds);
           if (obj == null || obj.ToString().Trim() == "")
           {
               return 0;
           }

           return Convert.ToDouble(obj);
       }

       public static DateTime GetDateTime(this DataSet ds)
       {
           return Convert.ToDateTime(GetObject(ds));
       }
       #endregion
    }
}
