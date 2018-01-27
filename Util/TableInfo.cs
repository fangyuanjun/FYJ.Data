using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYJ.Data.Util
{
   public class TableInfo
    {
       /// <summary>
       /// 表名
       /// </summary>
       public string Name { get; set; }

       /// <summary>
       /// 表备注
       /// </summary>
       public string Comment { get; set; }

       /// <summary>
       /// 拥有者,主要针对Oracle
       /// </summary>
       public string Owner { get; set; }

        /// <summary>
        /// 行数
        /// </summary>
        public int RowCount { get; set; }
    }
}
