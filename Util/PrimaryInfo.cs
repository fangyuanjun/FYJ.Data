using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYJ.Data.Util
{
    /// <summary>
    /// 主键信息
    /// </summary>
   public class PrimaryInfo
    {
       /// <summary>
       /// 主键名
       /// </summary>
       public string Name { get; set; }

       /// <summary>
       /// 表名
       /// </summary>
       public string TableName { get; set; }

       /// <summary>
       /// 列名
       /// </summary>
       public string ColmnName { get; set; }


       /// <summary>
       /// 拥有者,主要针对Oracle
       /// </summary>
       public string Owner { get; set; }
    }
}
