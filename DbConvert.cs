using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FYJ.Data
{
   public static class DbConvert
    {
       public static DataTable IQueryableToDataTable(this IQueryable query)
       {
           DataTable dt = new DataTable();
           foreach (var v in query)
           {
               PropertyInfo[] properties = v.GetType().GetProperties();
               if (dt.Columns == null || dt.Columns.Count == 0)
               {
                   foreach (PropertyInfo info in properties)
                   {
                       Type colType = info.PropertyType;
                       if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                       {
                           colType = colType.GetGenericArguments()[0];
                       }

                       dt.Columns.Add(info.Name, colType);
                   }
               }

               List<object> list = new List<object>();
               foreach (PropertyInfo info in properties)
               {
                   object obj = info.GetValue(v, null);
                   list.Add(obj == null ? DBNull.Value : obj);
               }

               dt.Rows.Add(list.ToArray());
           }

           return dt;
       }
    }
}
