using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Text;
using FYJ.Data.Util;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace FYJ.Data
{
    /// <summary>
    /// 功能:数据库抽象类
    /// 作者:fangyj
    /// 创建日期:
    /// 修改日期:
    /// </summary>
    public abstract class DbHelperAbstract : GetDataAbstract, IDbHelper
    {
        private DbProviderFactory factory;
        private string connectionString;
        #region 属性
        public DbProviderFactory DbProviderFactoryInstance
        {
            get
            {
                return factory;
            }
            set
            {
                this.factory = value;
            }
        }

        public virtual String ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                this.connectionString = value;
            }
        }

        public DbHelperType DbHelperTypeEnum
        {
            get
            {
                if (factory.GetType() == typeof(System.Data.SqlClient.SqlClientFactory))
                {
                    return DbHelperType.SqlServer;
                }
                else if (factory.GetType() == typeof(System.Data.OleDb.OleDbFactory))
                {
                    return DbHelperType.OleDb;
                }
                else if (factory.GetType() == typeof(System.Data.Odbc.OdbcFactory))
                {
                    return DbHelperType.Odbc;
                }
                else if (factory.GetType().FullName.Contains("MySql.Data"))
                {
                    return DbHelperType.MySql;
                }
                else if (factory.GetType().FullName.Contains("Oracle"))
                {
                    return DbHelperType.Oracle;
                }
                else if (factory.GetType().FullName.Contains("SQLite"))
                {
                    return DbHelperType.SqlLite;
                }

                return DbHelperType.Other;
            }
        }

        public abstract DbTransaction Tran { get; }

        #endregion

        #region 构造函数
        protected DbHelperAbstract()
        {

        }
        protected DbHelperAbstract(string connectionName)
        {
            this.factory = GetDbProviderFactory(System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ProviderName);
            this.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }
        protected DbHelperAbstract(DbProviderFactory factory, string connectionString)
        {
            this.factory = factory;
            this.ConnectionString = connectionString;
        }

        protected DbHelperAbstract(string providerName, string connectionString)
        {
            this.factory = GetDbProviderFactory(providerName);
            this.connectionString = connectionString;
        }
        #endregion

        #region 执行
        protected abstract int ExecuteSql(IEnumerable<IDataParameter> parms, string sql);

        public abstract DataSet RunProcedure(IEnumerable<IDataParameter> parms, string storedProcName);

        public abstract DataSet RunProcedure(string storedProcName, IDictionary<string, object> dic);

        public abstract int ExecuteProcedure(IEnumerable<IDataParameter> parms, string storedProcName);

        public abstract int ExecuteProcedure(string storedProcName, IDictionary<string, object> dic);


        public int ExecuteSql(string sql, params object[] parms)
        {
            IEnumerable<IDataParameter> list = ParamsToIDataParameters(sql, parms);

            return ExecuteSql(list, sql);
        }

        public int ExecuteSql(string sql, IDictionary<string, object> parms)
        {
            List<IDataParameter> list = new List<IDataParameter>();
            foreach (var item in parms)
            {
                string key = item.Key.Trim();
                if (key.StartsWith(this.ParameterPrefix))
                {
                    key = key.Substring(this.ParameterPrefix.Length);
                }
                list.Add(this.CreateParameter(this.ParameterPrefix + key, item.Value));
            }

            return ExecuteSql(list, sql);
        }

        public int ExecuteSql(string tableName, string pkName, bool iaAdd, IEnumerable<IDataParameter> parms)
        {
            string sql = "";
            if (iaAdd)
            {
                sql = Helper.GetAddSql(tableName, pkName, parms);
            }
            else
            {
                sql = Helper.GetUpdateSql(tableName, pkName, parms);
            }

            return this.ExecuteSql(parms, sql);
        }

        public DataSet RunProcedure(string storedProcName, params IDataParameter[] parms)
        {
            IEnumerable<IDataParameter> par = parms;
            return this.RunProcedure(par, storedProcName);
        }

        public int ExecuteProcedure(string storedProcName, params IDataParameter[] parms)
        {
            IEnumerable<IDataParameter> par = parms;
            return this.ExecuteProcedure(par, storedProcName);
        }
        #endregion

        #region 查询
        protected abstract DataSet GetDataSet(IEnumerable<IDataParameter> parms, string sql);

        public override DataSet GetDataSet(string sql, params object[] parms)
        {
            IEnumerable<IDataParameter> list = ParamsToIDataParameters(sql, parms);
            return GetDataSet(list, sql);
        }

        /// <summary>
        /// 根据表名查询该表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public virtual bool ExistsTable(string tableName)
        {
            try
            {
                DataTable dt = GetTopDataTable(tableName, 1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取查询的数据列表  IDataParameter参数前缀@ ：或者不加都会自动修正
        /// count传递null表示不返回总数   
        /// </summary>
        /// <param name="count">是否返回总记录条数 </param>
        /// <param name="tableName">表或视图名 </param>
        /// <param name="order">排序  格式要求ADD_DATE DESC,UPDATE_DATE ASC</param>
        /// <param name="parms">[参数列表] 默认空</param>
        /// <param name="select">[要访问的列] 默认*</param>
        /// <param name="where">[查询条件] 默认空</param>
        /// <param name="currentPage">[当前页] 默认1</param>
        /// <param name="pageSize">[每页显示条数] 默认20</param>
        /// <returns></returns>
        /// <author>fangyj 2012-07-15</author>
        public virtual DataTable GetDataTable(out int count, string tableName, string order, IEnumerable<IDataParameter> parms = null, string select = "*", string where = null, int currentPage = 1, int pageSize = 20)
        {
            //如果当前页数传递0 则默认为第一页
            if (currentPage == 0)
            {
                currentPage = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 10;
            }

            //如果条件不为空 但是where参数没有包含where 则自动加上
            if ((!String.IsNullOrEmpty(where)) && (!where.TrimStart().StartsWith("where", StringComparison.CurrentCultureIgnoreCase)))
                where = " where " + where;

            //如果排序不为空 但是order参数没有包含order by则自动加上
            if (order != null && order.Trim() != "")
            {
                if (!order.TrimStart().StartsWith("order", StringComparison.CurrentCultureIgnoreCase))
                    order = "order by " + order;
            }
            else
            {
                throw new Exception("排序时必须指定排序列名");
            }

            //构造sql语句
            String sql = String.Format("select {0} from {1} {2} {3}", select, tableName, where, order);

            //sql server   oledb  access 分页
            if ((this.DbHelperTypeEnum == DbHelperType.SqlServer)
                || (this.DbHelperTypeEnum == DbHelperType.OleDb))
            {
                if (String.IsNullOrEmpty(order))
                {
                    throw new Exception("SqlServer 分页时order 不能为空");
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("SELECT * FROM");
                sb.AppendLine("(");
                sb.AppendLine("SELECT row_number() over(" + order + ") AS rownum," + select + " FROM " + tableName + " " + where);
                sb.AppendLine(") AS table1 ");
                sb.AppendLine(" WHERE (rownum  BETWEEN " + ((currentPage - 1) * pageSize + 1) + " AND " + (currentPage * pageSize) + " )" + order);

                sql = sb.ToString();
            }
            //sqllite mysql分页
            if ((DbHelperTypeEnum == DbHelperType.SqlLite)
                || (DbHelperTypeEnum == DbHelperType.MySql))
            {
                sql = String.Format("select {0} from {1} {2} {3} limit {4},{5}  ", select, tableName, where, order, pageSize * (currentPage - 1), pageSize);
            }
            //oracle分页
            if (DbHelperTypeEnum == DbHelperType.Oracle)
            {
                sql = String.Format("select * from (select a.*,ROWNUM RN FROM (SELECT {0} FROM {1} {2} {3} ) A  WHERE RN BETWEEN {4} AND {5})", select, tableName, where, order, pageSize * (currentPage - 1), pageSize * currentPage);
            }

            DataSet ds = new DataSet();
            String countSql = String.Format("select count(*) from  {0} {1}", tableName, where);
            sql += ";\n" + countSql;
            //查询
            ds = GetDataSet(sql, parms);
            //总条数
            count = Convert.ToInt32(ds.Tables[1].Rows[0][0]);

            return ds.Tables[0];
        }

        public virtual List<string> GetTables()
        {
            List<String> list = new List<string>();
            if (this.DbHelperTypeEnum == DbHelperType.SqlServer)
            {
                //select * from sysobjects where [type]='U'
                string sql = "select * from information_schema.tables  where table_type='base table'";
                DataTable dt = this.GetDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr["TABLE_NAME"].ToString());
                }
            }

            if (this.DbHelperTypeEnum == DbHelperType.MySql)
            {
                string sql = "SHOW TABLES";     //SHOW TABLES FROM database, SHOW COLUMNS FROM TABLE
                DataTable dt = this.GetDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr[0].ToString());
                }
            }

            if (this.DbHelperTypeEnum == DbHelperType.Oracle)
            {
                string sql = "select TABLE_NAME from user_Tables   ORDER BY TABLE_NAME";     
                DataTable dt = this.GetDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr[0].ToString());
                }
            }

            return list;
        }

        /// <summary>
        /// 获取前max行数据  如果max小于1 则返回所有行
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="max"></param>
        /// <returns></returns>
        /// <author>fangyj 2012-09-27</author>
        public virtual DataTable GetTopDataTable(string tableName, long max)
        {
            DataTable dt = null;
            if (max < 1)
            {
                dt = GetDataTable("select * from " + tableName);
            }
            else
            {
                //sql server   oledb  access 分页
                if ((this.DbHelperTypeEnum == DbHelperType.SqlServer)
                    || (this.DbHelperTypeEnum == DbHelperType.OleDb))
                {
                    dt = GetDataTable("select top " + max + " * from " + tableName);
                }
                //sqllite mysql分页
                if ((this.DbHelperTypeEnum == DbHelperType.SqlLite)
                    || (this.DbHelperTypeEnum == DbHelperType.MySql))
                {
                    dt = GetDataTable("select  * from " + tableName + " limit 0," + max);
                }
                //oracle分页
                if (this.DbHelperTypeEnum == DbHelperType.Oracle)
                {
                    dt = GetDataTable(String.Format("select * from (select a.*,ROWNUM DBHELPER_RN FROM (SELECT * FROM {0}  ) A)  WHERE DBHELPER_RN BETWEEN {1} AND {2}", tableName, 0, max));
                }
            }
            dt.TableName = tableName;

            return dt;

        }
        #endregion


        public  List<ColumnInfo> GetDatableSchema(DataTable dt)
        {
            List<ColumnInfo> list = new List<ColumnInfo>();
            foreach (DataColumn column in dt.Columns)
            {
                ColumnInfo c = new ColumnInfo();
                c.Name = column.ColumnName;
                c.IsAllowNull = column.AllowDBNull;
                c.IsIdentity = column.AutoIncrement;
                c.IsPrimary = DataTableHelper.IsPrimarykey(dt, column.ColumnName);

                list.Add(c);
            }

            return list;
        }

        /// <summary>
        /// 构造参数
        /// </summary>
        /// <param name="parameterName">[可选参数]参数名</param>
        /// <param name="parameterValue">[可选参数]参数值</param>
        /// <param name="direction">[可选参数]参数类型</param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName = null, object parameterValue = null, ParameterDirection? direction = null)
        {
            DbParameter paramter = this.factory.CreateParameter();
            if (parameterName != null)
            {
                paramter.ParameterName = parameterName;
            }
            if (parameterValue != null)
            {
                paramter.Value = parameterValue;
            }
            if (direction != null)
            {
                paramter.Direction = direction.Value;
            }
            return paramter;
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public abstract void BeginTran();

        /// <summary>
        /// 提交事务
        /// </summary>
        public abstract void Commit();

        /// <summary>
        /// 回滚
        /// </summary>
        public abstract void Rollback();

        public virtual bool TestCanConnectionOpen()
        {
            try
            {
                DbConnection conn = factory.CreateConnection();
                conn.ConnectionString = this.connectionString;
                conn.Open();
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 参数前缀
        /// </summary>
        public string ParameterPrefix
        {
            get
            {
                if (DbHelperTypeEnum == DbHelperType.Oracle)
                {
                    return ":";
                }
                else
                {
                    return "@";
                }
            }
        }

        #region 私有方法

        private DbProviderFactory GetDbProviderFactory(string providerName)
        {
            DbProviderFactory factory = null;
            if (providerName.StartsWith("MySql.Data", StringComparison.CurrentCultureIgnoreCase))
            {
                factory = (DbProviderFactory)Assembly.Load("MySql.Data").CreateInstance("MySql.Data.MySqlClient.MySqlClientFactory");
            }
            else if (providerName.StartsWith("System.Data.SQLite", StringComparison.CurrentCultureIgnoreCase))
            {
                factory = (DbProviderFactory)Assembly.Load("System.Data.SQLite").CreateInstance("System.Data.SQLite.SQLiteFactory");
                string path = Regex.Match(connectionString, @"data\s*source\s*=\s*(.*)").Groups[1].Value;
                if (!File.Exists(path))
                {
                    connectionString = "data source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                }
            }
            else
            {
                factory = DbProviderFactories.GetFactory(providerName);
            }

            return factory;
        }

        /// <summary>
        /// 将参数转为IDataParameter 集合 自动识别类型
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        /// 2015-8-13
        private IEnumerable<IDataParameter> ParamsToIDataParameters(string sql, params object[] parms)
        {
            if (parms == null || parms.Length == 0)
            {
                return null;
            }

            List<IDataParameter> list = new List<IDataParameter>();

            //是否值类型或者字符串类型
            bool IsValueTypeOrString = false;

            foreach (object obj in parms)
            {
                if (obj is IDataParameter)
                {
                    list.Add(obj as IDataParameter);
                }

                else if (obj is KeyValuePair<string, object>)
                {
                    var item = (KeyValuePair<string, object>)obj;
                    string key = item.Key.Trim();
                    if (key.StartsWith(this.ParameterPrefix))
                    {
                        key = key.Substring(this.ParameterPrefix.Length);
                    }
                    list.Add(this.CreateParameter(this.ParameterPrefix + key, item.Value));
                }

                else if (obj is ParameterEx)
                {
                    var item = obj as ParameterEx;
                    string key = item.Key.Trim();
                    if (key.StartsWith(this.ParameterPrefix))
                    {
                        key = key.Substring(this.ParameterPrefix.Length);
                    }
                    list.Add(this.CreateParameter(this.ParameterPrefix + key, item.Value));
                }

                //集合类型只取第一个
                else if (obj is IEnumerable<IDataParameter>)
                {
                    return (obj as IEnumerable<IDataParameter>);
                }

                //集合类型只取第一个
                else if (obj is IDictionary<string, object>)
                {
                    foreach (var item in (obj as IDictionary<string, object>))
                    {
                        string key = item.Key.Trim();
                        if (key.StartsWith(this.ParameterPrefix))
                        {
                            key = key.Substring(this.ParameterPrefix.Length);
                        }
                        list.Add(this.CreateParameter(this.ParameterPrefix + key, item.Value));
                    }

                    break;
                }
                else if (obj != null)
                {
                    //如果第一个是值类型或者字符串类型则执行正则匹配
                    if ((obj is string) || obj.GetType().IsValueType)
                    {
                        IsValueTypeOrString = true;
                        break;
                    }
                    else
                    {
                        throw new Exception("不支持的参数类型");
                    }
                }
            }

            if (IsValueTypeOrString)
            {
                sql = sql + " ";
                MatchCollection matchs = Regex.Matches(sql, this.ParameterPrefix + "(.*?)[\\s|,]+");
                List<string> keyList = new List<string>();
                for (int i = 0; i < matchs.Count; i++)
                {
                    string key = matchs[i].Groups[1].Value;
                    if (!keyList.Contains(key))
                    {
                        keyList.Add(key);
                    }
                }

                for (int i = 0; i < keyList.Count; i++)
                {
                    list.Add(this.CreateParameter(this.ParameterPrefix + keyList[i], parms[i]));
                }
            }

            return list;
        }
        #endregion
    }
}