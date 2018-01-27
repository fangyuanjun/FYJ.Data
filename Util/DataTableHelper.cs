using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FYJ.Data.Util
{
   public class DataTableHelper
    {
       

      
        /// <summary>
        /// 是否主键
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool IsPrimarykey(DataTable dt, string columnName)
        {
            DataColumn[] columns = dt.PrimaryKey;
            if (columns != null && columns.Length > 0)
            {
                foreach (DataColumn c in columns)
                {
                    if (c.ColumnName == columnName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
