using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FYJ.Data
{
    /// <summary>
    /// 获取各种数据类型抽象类
    /// </summary>
    /// <code>public abstract class GetDataAbstract</code>
    public abstract class GetDataAbstract
    {
        /// <summary>
        /// 获取GetDataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public abstract DataSet GetDataSet(string sql, params object[] parms);


        public virtual DataTable GetDataTable(string sql, params object[] parms)
        {
            DataSet ds = GetDataSet(sql, parms);
            if (ds != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            return null;
        }


        public virtual object GetObject(string sql, params object[] parms)
        {
            DataTable dt = this.GetDataTable(sql, parms);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0][0];
            }

            return null;
        }


        public virtual string GetString(string sql, params object[] parms)
        {
            object obj = this.GetObject(sql, parms);
            return obj == null ? null : obj.ToString();
        }

        public virtual int GetInt(string sql, params object[] parms)
        {
            return (int)GetLong(sql, parms);
        }

        /// <summary>
        /// 对于字符串或布尔 如果为true返回1 false返回0
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual long GetLong(string sql, params object[] parms)
        {
            String s = GetString(sql, parms);
            if (String.IsNullOrEmpty(s))
                return 0;
            if (s.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                return 1;
            if (s.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                return 0;
            return Convert.ToInt64(s);
        }

        /// <summary>
        /// 返回布尔型   大于0也返回true 否则返回false
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual bool GetBoolean(string sql, params object[] parms)
        {
            String s = GetString(sql, parms);
            if (String.IsNullOrEmpty(s))
                return false;
            bool result = false;
            if (Boolean.TryParse(s, out result))  //字符串是否可以转换成布尔型
                return result;
            int re = 0;
            if (Int32.TryParse(s, out re))  //字符串是否可以转换成整形
                return re > 0;

            return false;
        }

        public virtual double GetDouble(string sql, params object[] parms)
        {
            String s = GetString(sql, parms);
            if (String.IsNullOrEmpty(s))
                return 0;
            if (s.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                return 1;
            if (s.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                return 0;
            return Convert.ToDouble(s);
        }

        /// <summary>
        /// SQL语句查询的结果是否存在
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual bool Exists(string sql, params object[] parms)
        {
            DataTable dt = this.GetDataTable(sql, parms);

            return dt == null || dt.Rows.Count == 0 ? false : true;
        }
    }
}
