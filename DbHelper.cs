using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using FYJ.Data.Util;

namespace FYJ.Data
{
    public class DbHelper : DbHelperAbstract
    {
        private DbTransaction tran;

        #region 构造函数
        public DbHelper()
        {

        }
        public DbHelper(string connectionName)
            : base(connectionName)
        {

        }
        public DbHelper(DbProviderFactory factory, string connectionString)
            : base(factory, connectionString)
        {

        }

        public DbHelper(string providerName, string connectionString)
            : base(providerName, connectionString)
        {

        }
        #endregion

        #region 构造连接字符串
        /// <summary>
        /// 构造连接字符串,静态方法
        /// </summary>
        /// <param name="dataType">数据库类型</param>
        /// <param name="host">主机地址</param>
        /// <param name="database">数据库名</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="port">[可选参数]端口号</param>
        /// <param name="encoding">[可选参数]编码</param>
        /// <returns></returns>
        public static string CreateConnectionString(DbHelperType dataType, string host, string database, string userName, string password, int? port = null, string encoding = null)
        {
            switch (dataType)
            {
                case DbHelperType.MySql:
                    //Server=mysql.kecq.com;UserName=990717_mysql;Password=605911fyj;Database=990717_mysql;Port=4306;CharSet=utf8;
                    //string mySqlString = "Host={0};UserName={1};Password={2};Database={3};Port={4};CharSet={5};Allow Zero Datetime=true";
                    string mySqlString = "Server={0};Uid={1};Pwd={2};Database={3};Port={4};{5};";
                    if (encoding != null)
                    {
                        encoding = "CharSet=" + encoding + ";";
                    }

                    return String.Format(mySqlString, host, userName, password, database, port == null ? 3306 : port, encoding);
                case DbHelperType.SqlServer:
                    return String.Format("Server={0};Database={1};Uid={2};Pwd={3}", host, database, userName, password);
                case DbHelperType.SqlLite:
                    return "data source=" + database;
                case DbHelperType.Oracle:
                    string oracleString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0}) (PORT={1})))(CONNECT_DATA=(SERVICE_NAME={2})));User Id={3}; Password={4}";
                    return String.Format(oracleString, host, port == null ? 1521 : port, database, userName, password);
                default:
                    break;
            }
            return null;
        }
        #endregion


        public override DbTransaction Tran
        {
            get { return tran; }
        }

        public override void BeginTran()
        {
            DbConnection conn = this.DbProviderFactoryInstance.CreateConnection();
            conn.ConnectionString = this.ConnectionString;
            conn.Open();
            this.tran = conn.BeginTransaction();
        }

        public override void Commit()
        {
            if (tran != null)
            {
                if (this.tran.Connection != null)
                {
                    this.tran.Commit();
                }

                if (this.tran.Connection != null)   //tran执行Commit 后的 Connection为空  所以保险起见再判断次
                {
                    this.tran.Connection.Close();
                }

                this.tran = null;
            }
        }

        public override void Rollback()
        {
            if (tran != null)
            {
                if (this.tran.Connection != null)
                {
                    this.tran.Rollback();
                }
                 
                if (this.tran.Connection != null)
                {
                    this.tran.Connection.Close();
                }

                this.tran = null;
            }
        }

        public override DataSet RunProcedure(string storedProcName, IDictionary<string, object> dic)
        {
            throw new NotImplementedException();
        }

        public override DataSet RunProcedure(IEnumerable<IDataParameter> parameters, string storedProcName)
        {
            DbConnection conn = null;
            if (this.tran != null)
            {
                conn = this.tran.Connection;
            }
            else
            {
                conn = this.DbProviderFactoryInstance.CreateConnection();
                conn.ConnectionString = this.ConnectionString;
                conn.Open();
            }

            conn.ConnectionString = this.ConnectionString;
            conn.Open();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = storedProcName;
            DataSet ds = new DataSet();
            DbDataAdapter adapter = this.DbProviderFactoryInstance.CreateDataAdapter();
            if (parameters != null)
            {
                foreach (DbParameter parameter in Helper.FixParametersPre(parameters, this.DbHelperTypeEnum))
                {
                    cmd.Parameters.Add(parameter);
                }
            }
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
            if (tran == null)
            {
                conn.Close();
            }
            return ds;
        }

        public override int ExecuteProcedure(string storedProcName, IDictionary<string, object> dic)
        {
            if (this.DbHelperTypeEnum == DbHelperType.SqlServer)
            {
                RunProcedure(storedProcName, dic);
                return Convert.ToInt32(dic["@RETURN_VALUE"]);
            }
            throw new NotImplementedException();
        }

        public override int ExecuteProcedure(IEnumerable<IDataParameter> parameters, string storedProcName)
        {
            throw new NotImplementedException();
        }

        protected override DataSet GetDataSet(IEnumerable<IDataParameter> parms, string sql)
        {
            DbConnection conn = this.DbProviderFactoryInstance.CreateConnection();
            conn.ConnectionString = this.ConnectionString;
            conn.Open();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            DbDataAdapter adapter = this.DbProviderFactoryInstance.CreateDataAdapter();
            if (parms != null)
            {
                foreach (IDataParameter parm in Helper.FixParametersPre(parms, this.DbHelperTypeEnum))
                {
                    cmd.Parameters.Add(parm);
                }
            }
            adapter.SelectCommand = cmd;
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            cmd.Parameters.Clear();
            conn.Close();

            return ds;
        }

        protected override int ExecuteSql(IEnumerable<System.Data.IDataParameter> parms, string sql)
        {
            DbConnection conn = null;
            try
            {
                if (tran != null)
                {
                    conn = tran.Connection;
                }
                else
                {
                    conn = this.DbProviderFactoryInstance.CreateConnection();
                    conn.ConnectionString = this.ConnectionString;
                    conn.Open();
                }

                DbCommand cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                if (parms != null)
                {
                    foreach (DbParameter parameter in parms)
                    {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        cmd.Parameters.Add(parameter);
                    }
                }
                cmd.CommandText = sql;
                int rows = cmd.ExecuteNonQuery();

                return rows;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (tran == null)
                {
                    if (conn != null)
                    {
                        conn.Close();
                    }
                }
            }
           
        }
    }
}
