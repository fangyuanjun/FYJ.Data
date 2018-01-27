using FYJ.Data.Util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;

namespace FYJ.Data
{
    public class DbHelperSQL : DbHelper
    {
        #region 构造函数
        public DbHelperSQL()
        {

        }
        public DbHelperSQL(String connectionString)
            : base(System.Data.SqlClient.SqlClientFactory.Instance, connectionString)
        {

        }
        #endregion

        /// <summary>
        /// 取得数据库服务器列表
        /// </summary>
        /// <returns></returns>
        public static String[] GetSQLServerList()
        {
            //DataTable dataSources = SqlClientFactory.Instance.CreateDataSourceEnumerator().GetDataSources();
            //DataColumn column2 = dataSources.Columns["ServerName"];
            //DataColumn column = dataSources.Columns["InstanceName"];
            //DataRowCollection rows = dataSources.Rows;
            //string[] array = new string[rows.Count];

            //for (int i = 0; i < array.Length; i++)
            //{
            //    string str2 = rows[i][column2] as string;
            //    string str = rows[i][column] as string;
            //    if (((str == null) || (str.Length == 0)) || ("MSSQLSERVER" == str))
            //    {
            //        array[i] = str2;

            //    }
            //    else
            //    {
            //        array[i] = str2 + @"\" + str;
            //    }
            //}

            //Array.Sort<string>(array);

            //return array;

            RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
            String[] instances = (String[])rk.GetValue("InstalledInstances");
            if (instances != null && instances.Length > 0)
            {
                foreach (String element in instances)
                {
                    if (element == "MSSQLSERVER")
                        Console.WriteLine(System.Environment.MachineName);
                    else
                        Console.WriteLine(System.Environment.MachineName + @"\" + element);
                }
            }
            return instances;
        }

        /// <summary>
        /// 获取所有数据库名
        /// </summary>
        /// <returns></returns>
        public List<String> GetSQLServerDatabase()
        {
            List<String> list = new List<string>();
            string sql = "select name from master..sysdatabases";
            DataTable dt = this.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr[0].ToString());
            }
            return list;
        }

        /// <summary>
        /// 判断是否存在某表的某个字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列名称</param>
        /// <returns>是否存在</returns>
        public bool ColumnExists(string tableName, string columnName)
        {
            string sql = "select count(1) from syscolumns where [id]=object_id('" + tableName + "') and [name]='" + columnName + "'";
            object res = GetObject(sql);
            if (res == null)
            {
                return false;
            }
            return Convert.ToInt32(res) > 0;
        }
        public int GetMaxID(string FieldName, string TableName)
        {
            string sql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetObject(sql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }

        /// <summary>
        /// 表是否存在  重写
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override bool ExistsTable(string tableName)
        {
            string sql = "select count(*) from sysobjects where id = object_id('[" + tableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";
            //string sql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID('[" + TableName + "]') AND type in ('U')";
            object obj = GetObject(sql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 查询表  并将列Caption属性(标题)设为备注
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<ColumnInfo> GetDatableSchema(string tableName)
        {
            // select * from syscolumns where [id]=object_id('news_tb_comment')
            String sql = @"
                    SELECT  
                    表名               =   CASE   WHEN   A.COLORDER=1   THEN   D.NAME   ELSE   ' '   END,
                    表备注           =   CASE   WHEN   A.COLORDER=1   THEN   ISNULL(F.VALUE, ' ')   ELSE   ' '   END,
                    列序号           =   A.COLORDER,
                    列名称           =   A.NAME,
                    标识               =   CASE   WHEN   COLUMNPROPERTY(   A.ID,A.NAME, 'ISIDENTITY ')=1   THEN   '1 'ELSE   '0'   END,
                    主键               =   CASE   WHEN   EXISTS(SELECT   1   FROM   SYSOBJECTS   WHERE   XTYPE= 'PK '   AND   PARENT_OBJ=A.ID   AND   NAME   IN   (
                    SELECT   NAME   FROM   SYSINDEXES   WHERE   INDID   IN(
                    SELECT   INDID   FROM   SYSINDEXKEYS   WHERE   ID   =   A.ID   AND   COLID=A.COLID)))   THEN   '1'   ELSE   '0'   END,
                    类型               =   B.NAME,
                    字节               =   A.LENGTH,
                    长度               =   COLUMNPROPERTY(A.ID,A.NAME, 'PRECISION '),
                    小数位          =   ISNULL(COLUMNPROPERTY(A.ID,A.NAME, 'SCALE '),0),
                    允许空           =   A.ISNULLABLE,
                    默认值           =   ISNULL(E.TEXT, ' '),
                    列备注       =   ISNULL(G.[VALUE], ' ')
                    FROM  
                    SYSCOLUMNS   A
                    LEFT   JOIN  
                    SYSTYPES   B  
                    ON  
                    A.XUSERTYPE=B.XUSERTYPE
                    INNER   JOIN  
                    SYSOBJECTS   D  
                    ON  
                    A.ID=D.ID     AND   D.XTYPE= 'U '   AND     D.NAME <> 'DTPROPERTIES '
                    LEFT   JOIN  
                    SYSCOMMENTS   E  
                    ON  
                    A.CDEFAULT=E.ID
                    LEFT   JOIN  
                    sys.extended_properties   G  
                    ON  
                    A.ID=G.major_id   AND   A.COLID=G.minor_id    
                    LEFT   JOIN  
                    sys.extended_properties   F  
                    ON  
                    D.ID=F.major_id   AND   F.minor_id=0
                    where  D.NAME='{0}'   --查询这个表
                    ORDER   BY  
                    A.ID,A.COLORDER 
              
                    ";

            sql = String.Format(sql, tableName);
            DataTable dt = GetDataTable(sql);

            List<ColumnInfo> list = new List<ColumnInfo>();
            foreach(DataRow dr in dt.Rows)
            {
                ColumnInfo col = new ColumnInfo();
                col.Name = dr["列名称"].ToString();
                col.Index = Convert.ToInt32(dr["列序号"]);
                col.IsIdentity=(dr["标识"].ToString()=="1");
                col.IsPrimary = (dr["主键"].ToString() == "1");
                col.IsAllowNull = (dr["允许空"].ToString() == "1");
                col.Type = dr["类型"].ToString();
                if (dr["主键"].ToString() == "1")
                {
                    col.IsUniqueness = true;
                }

                if (dr["小数位"].ToString() != "")
                {
                    col.Radix = Convert.ToInt32(dr["小数位"]);
                }

                if(dr["长度"].ToString()!="")
                {
                    col.Length = Convert.ToInt32(dr["长度"]);
                }
                col.Mark = dr["列备注"].ToString();
                col.DefaultValue = dr["默认值"].ToString();

                list.Add(col);
            }

            dt = GetTopDataTable(tableName, 1);
            foreach (DataColumn column in dt.Columns)
            {
                ColumnInfo c = list.Where(x => x.Name == column.ColumnName).FirstOrDefault();
                if (c == null)
                {
                    continue;
                }
                c.Name = column.ColumnName;
                c.IsAllowNull = column.AllowDBNull;
                c.IsIdentity = column.AutoIncrement;
                c.IsPrimary = DataTableHelper.IsPrimarykey(dt, column.ColumnName);
                c.DataType = column.DataType;
            }

            return list;
        }

        private void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, IDataParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (SqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 构建没有主键的表Adapter  暂时没有update delete
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DbDataAdapter CreateAdapter(DataTable dt)
        {
            DbDataAdapter adapter = this.DbProviderFactoryInstance.CreateDataAdapter();
            DataTableMapping tableMapping = new DataTableMapping();
            tableMapping.SourceTable = "Table";
            tableMapping.DataSetTable = dt.TableName;
            DbCommand insertCommand = this.DbProviderFactoryInstance.CreateCommand();
            DbCommand updateCommand = this.DbProviderFactoryInstance.CreateCommand();
            DbCommand deleteCommand = this.DbProviderFactoryInstance.CreateCommand();
            insertCommand.CommandType = CommandType.Text;
            string parameterPre = this.DbHelperTypeEnum == DbHelperType.Oracle ? ":" : "@";  //参数前缀
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";  //更新语句
            string temp4 = ""; //更新语句
            foreach (DataColumn column in dt.Columns)
            {
                tableMapping.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                temp1 += "[" + column.ColumnName + "],";
                temp2 += parameterPre + column.ColumnName + ",";

                if (column.AllowDBNull == false && column.Unique)
                {
                    temp4 = "and " + column.ColumnName + "=" + parameterPre + column.ColumnName + ",";
                }
                else
                {
                    if (column.AutoIncrement == false) //如果该行不自动递增
                    {
                        temp3 = column.ColumnName + "=" + parameterPre + column.ColumnName + ",";
                    }
                }
                DbParameter para = this.DbProviderFactoryInstance.CreateParameter();
                para.ParameterName = parameterPre + column.ColumnName;
                para.SourceColumn = column.ColumnName;
                para.SourceVersion = DataRowVersion.Current;
                insertCommand.Parameters.Add(para);

                DbParameter para2 = this.DbProviderFactoryInstance.CreateParameter();
                para2.ParameterName = parameterPre + column.ColumnName;
                para2.SourceColumn = column.ColumnName;
                para2.SourceVersion = DataRowVersion.Current;
                updateCommand.Parameters.Add(para2);
                // insertCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PFP", System.Data.SqlDbType.VarChar, 20, System.Data.ParameterDirection.Input, 0, 0, "PFP", System.Data.DataRowVersion.Current, false, null, "", "", ""));
            }
            temp1 = temp1.TrimEnd(',');
            temp2 = temp2.TrimEnd(',');
            temp3 = temp3.TrimEnd(',');
            temp4 = temp4.TrimEnd(',');
            //构造插入语句
            insertCommand.CommandText = " insert into [" + dt.TableName + "] (" + temp1 + ") values (" + temp2 + ")";
            updateCommand.CommandText = " update [" + dt.TableName + "] set " + temp3 + " where 1=1 " + temp4;
            adapter.TableMappings.Add(tableMapping);
            adapter.DeleteCommand = deleteCommand;
            adapter.InsertCommand = insertCommand;
            adapter.UpdateCommand = updateCommand;

            return adapter;
        }

        public int Update(DataTable dt)
        {
            if (String.IsNullOrEmpty(dt.TableName))
            {
                throw new Exception("没有表名");
            }
            //貌似能自动打开释放连接

            DbDataAdapter adapter = null;
            DataTable tempDt = GetDataTable(dt.TableName, 1); //为了获取表的列数
            if (dt.PrimaryKey.Length == 0 || (tempDt.Columns.Count != dt.Columns.Count))  //如果没有主键或者传入的表列不完整则调用自定义的构造 
            {
                adapter = CreateAdapter(dt);
            }
            else
            {
                DbCommand cmd = this.DbProviderFactoryInstance.CreateCommand();
                cmd.CommandText = "select * from " + dt.TableName;
                adapter = this.DbProviderFactoryInstance.CreateDataAdapter();
                adapter.SelectCommand = cmd;
                DbCommandBuilder commandBuilder = this.DbProviderFactoryInstance.CreateCommandBuilder();
                commandBuilder.DataAdapter = adapter;
                adapter.DeleteCommand = commandBuilder.GetDeleteCommand(true);
                adapter.InsertCommand = commandBuilder.GetInsertCommand(true);
                adapter.UpdateCommand = commandBuilder.GetUpdateCommand(true);

            }

            string temp = "";
            foreach (DataColumn column in dt.Columns)   //查找是否有自动递增的自动 如果有并且数据库类型是sqlserver那么 加入运行插入自动递增字段
            {
                if (column.AutoIncrement)
                {
                    temp += "SET IDENTITY_INSERT  [" + dt.TableName + "] ON;";
                    break;
                }
            }

            adapter.InsertCommand.CommandText = temp + adapter.InsertCommand.CommandText;

            using (TransactionScope ts = new TransactionScope())
            {
                int result = adapter.Update(dt);
                ts.Complete();
                dt.AcceptChanges();
                return result;
            }

        }
        /// <summary>
        /// 执行存储过程  
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="dic">以存储过程参数名为key 参数值为value</param>
        /// <returns></returns>
        public override DataSet RunProcedure(string storedProcName, IDictionary<String, object> dic)
        {
            using (SqlConnection connection = (SqlConnection)this.DbProviderFactoryInstance.CreateConnection())
            {
                connection.ConnectionString = this.ConnectionString;
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = storedProcName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Transaction = this.Tran as SqlTransaction;

                connection.Open();
                SqlCommandBuilder.DeriveParameters(cmd); //获取存储过程信息

                foreach (SqlParameter para in cmd.Parameters)
                {
                    //如果不是输出并且不是返回
                    if (para.Direction != ParameterDirection.Output && para.Direction != ParameterDirection.ReturnValue)
                    {
                        //如果存储过程获取的参数名包含在传入的参数中  忽略大小写和@
                        if (dic.ContainsKey(para.ParameterName) || dic.ContainsKey(para.ParameterName.Replace("@", "")))
                        {
                            //获取存储过程参数值 忽略@
                            object value = dic.ContainsKey(para.ParameterName) ? dic[para.ParameterName] : dic[para.ParameterName.Replace("@", "")];
                            if (para.DbType == DbType.String)
                            {
                                para.Value = value == null ? "" : value;
                            }
                            else
                                para.Value = value;
                        }
                        else
                        {
                            if (para.DbType == DbType.Int32 || para.DbType == DbType.Double || para.DbType == DbType.Decimal)
                            {
                                para.Value = 0;
                            }
                            if (para.DbType == DbType.String)
                            {
                                para.Value = "";
                            }
                            if (para.DbType == DbType.DateTime)
                            {
                                para.Value = DateTime.Now;
                            }
                        }
                    }
                }
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = cmd;
                //sqlDA.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                sqlDA.Fill(ds);


                foreach (SqlParameter para in cmd.Parameters)
                {
                    if (para.Direction != ParameterDirection.Input)
                    {
                        if (dic.ContainsKey(para.ParameterName) || dic.ContainsKey(para.ParameterName.TrimStart(new char[] { '@' })))
                        {
                            dic[para.ParameterName] = para.Value;
                        }
                        else
                        {
                            dic.Add(para.ParameterName, para.Value);
                        }
                    }
                }
                return ds;
            }
        }

        /// <summary>
        /// 执行存储过程 返回整数值 @RETURN_VALUE
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public override int ExecuteProcedure(string storedProcName, IDictionary<String, object> dic)
        {
            RunProcedure(storedProcName, dic);
            return Convert.ToInt32(dic["@RETURN_VALUE"]);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int BulkInsert(DataTable dt)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                string sql = " BULK INSERT " + dt.TableName + "   FROM 'd:/1.txt' with (fieldterminator ='\t')";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string temp = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        string s = dt.Rows[i][j] + "";
                        if (dt.Columns[j].DataType == typeof(bool))
                        {
                            if (s != "")
                            {
                                s = Convert.ToBoolean(dt.Rows[i][j]) ? "1" : "0";
                            }
                        }

                        if (j == dt.Columns.Count - 1)
                        {
                            temp += s.Replace("\t", "");
                        }
                        else
                        {
                            temp += s.Replace("\t", "") + "\t";
                        }
                    }
                    temp = temp + Environment.NewLine;
                    sb.Append(temp);
                }

                StreamWriter write = new StreamWriter("d:\\1.txt", false, Encoding.Default, 40960);
                write.Write(sb.ToString());
                write.Close();


                return ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                File.Delete("d:\\1.txt");
            }
        }

        #region  分页
        /// <summary>
        /// MS SQLSERVER 分页SQL语句生成器，同样适用于ACCESS数据库(edit:2008.3.29)
        /// </summary>
        /// <param name="strSQLInfo">原始SQL语句</param>
        /// <param name="strWhere">在分页前要替换的字符串，用于分页前的筛选</param>
        /// <param name="PageSize">页大小</param>
        /// <param name="PageNumber">页码</param>
        /// <param name="AllCount">记录总数</param>
        /// <returns>生成SQL分页语句</returns>
        private static string MakePageSQLStringByMSSQL(string strSQLInfo, string strWhere, int PageSize, int PageNumber, int AllCount)
        {
            //分页位置分析#region 分页位置分析
            string strSQLType = string.Empty;
            if (AllCount != 0)
            {
                if (PageNumber == 1) //首页
                {
                    strSQLType = "First";
                }
                else if (PageSize * PageNumber > AllCount) //最后的页 @@LeftSize
                {
                    PageSize = AllCount - PageSize * (PageNumber - 1);
                    strSQLType = "Last";
                }
                else //中间页
                {
                    strSQLType = "Mid";
                }
            }
            else if (AllCount < 0) //特殊处理
            {
                strSQLType = "First";
            }
            else
            {
                strSQLType = "Count";
            }



            //SQL 复杂度分析 开始
            bool SqlFlag = true;//简单SQL标记
            string TestSQL = strSQLInfo.ToUpper();
            int n = TestSQL.IndexOf("SELECT ", 0);
            n = TestSQL.IndexOf("SELECT ", n + 7);
            if (n == -1)
            {
                //可能是简单的查询，再次处理
                n = TestSQL.IndexOf(" JOIN ", n + 7);
                if (n != -1) SqlFlag = false;
                else
                {
                    //判断From 谓词情况
                    n = TestSQL.IndexOf("FROM ", 9);
                    if (n == -1) return "";
                    //计算 WHERE 谓词的位置
                    int m = TestSQL.IndexOf("WHERE ", n + 5);
                    // 如果没有WHERE 谓词
                    if (m == -1) m = TestSQL.IndexOf("ORDER BY ", n + 5);
                    //如果没有ORDER BY 谓词，那么无法排序，退出；
                    if (m == -1)
                        throw new Exception("查询语句分析：当前没有为分页查询指定排序字段！请适当修改SQL语句。 " + strSQLInfo);
                    string strTableName = TestSQL.Substring(n, m - n);
                    //表名中有 , 号表示是多表查询
                    if (strTableName.IndexOf(",") != -1)
                        SqlFlag = false;
                }
            }
            else
            {
                //有子查询；
                SqlFlag = false;
            }
            //SQL 复杂度分析 结束

            // 排序语法分析#region 排序语法分析
            //排序语法分析 开始
            int iOrderAt = strSQLInfo.ToLower().LastIndexOf("order by ");
            //如果没有ORDER BY 谓词，那么无法排序分页，退出；
            if (iOrderAt == -1)
                throw new Exception("查询语句分析：当前没有为分页查询指定排序字段！请适当修改SQL语句。 " + strSQLInfo);

            string strOrder = strSQLInfo.Substring(iOrderAt + 9);
            strSQLInfo = strSQLInfo.Substring(0, iOrderAt);
            string[] strArrOrder = strOrder.Split(new char[] { ',' });
            for (int i = 0; i < strArrOrder.Length; i++)
            {
                string[] strArrTemp = (strArrOrder[i].Trim() + " ").Split(new char[] { ' ' });
                //压缩多余空格
                for (int j = 1; j < strArrTemp.Length; j++)
                {
                    if (strArrTemp[j].Trim() == "")
                    {
                        continue;
                    }
                    else
                    {
                        strArrTemp[1] = strArrTemp[j];
                        if (j > 1) strArrTemp[j] = "";
                        break;
                    }
                }
                //判断字段的排序类型
                switch (strArrTemp[1].Trim().ToUpper())
                {
                    case "DESC":
                        strArrTemp[1] = "ASC";
                        break;
                    case "ASC":
                        strArrTemp[1] = "DESC";
                        break;
                    default:
                        //未指定排序类型，默认为降序
                        strArrTemp[1] = "DESC";
                        break;
                }
                //消除排序字段对象限定符
                if (strArrTemp[0].IndexOf(".") != -1)
                    strArrTemp[0] = strArrTemp[0].Substring(strArrTemp[0].IndexOf(".") + 1);
                strArrOrder[i] = string.Join(" ", strArrTemp);

            }
            //生成反向排序语句
            string strNewOrder = string.Join(",", strArrOrder).Trim();
            strOrder = strNewOrder.Replace("ASC", "ASC0").Replace("DESC", "ASC").Replace("ASC0", "DESC");
            //排序语法分析结束


            string SQL = string.Empty;
            if (!SqlFlag)
            {
                //复杂查询处理
                switch (strSQLType.ToUpper())
                {
                    case "FIRST":
                        SQL = "Select Top @@PageSize * FROM ( " + strSQLInfo +
                            " ) P_T0 @@Where ORDER BY " + strOrder;
                        break;
                    case "MID":
                        SQL = @"SELECT Top @@PageSize * FROM
                         (SELECT Top @@PageSize * FROM
                           (
                             SELECT Top @@Page_Size_Number * FROM (";
                        SQL += " " + strSQLInfo + " ) P_T0 @@Where ORDER BY " + strOrder + " ";
                        SQL += @") P_T1
            ORDER BY " + strNewOrder + ") P_T2  " +
                            "ORDER BY " + strOrder;
                        break;
                    case "LAST":
                        SQL = @"SELECT * FROM (     
                          Select Top @@LeftSize * FROM (" + " " + strSQLInfo + " ";
                        SQL += " ) P_T0 @@Where ORDER BY " + strNewOrder + " " +
                            " ) P_T1 ORDER BY " + strOrder;
                        break;
                    case "COUNT":
                        SQL = "Select COUNT(*) FROM ( " + strSQLInfo + " ) P_Count @@Where";
                        break;
                    default:
                        SQL = strSQLInfo + strOrder;//还原
                        break;
                }

            }
            else
            {
                //简单查询处理
                switch (strSQLType.ToUpper())
                {
                    case "FIRST":
                        SQL = strSQLInfo.ToUpper().Replace("SELECT ", "SELECT TOP @@PageSize ");
                        SQL += "  @@Where ORDER BY " + strOrder;
                        break;
                    case "MID":
                        string strRep = @"SELECT Top @@PageSize * FROM
                         (SELECT Top @@PageSize * FROM
                           (
                             SELECT Top @@Page_Size_Number  ";
                        SQL = strSQLInfo.ToUpper().Replace("SELECT ", strRep);
                        SQL += "  @@Where ORDER BY " + strOrder;
                        SQL += "  ) P_T0 ORDER BY " + strNewOrder + " " +
                            " ) P_T1 ORDER BY " + strOrder;
                        break;
                    case "LAST":
                        string strRep2 = @"SELECT * FROM (     
                          Select Top @@LeftSize ";
                        SQL = strSQLInfo.ToUpper().Replace("SELECT ", strRep2);
                        SQL += " @@Where ORDER BY " + strNewOrder + " " +
                            " ) P_T1 ORDER BY " + strOrder;
                        break;
                    case "COUNT":
                        SQL = "Select COUNT(*) FROM ( " + strSQLInfo + " @@Where) P_Count ";//edit
                        break;
                    default:
                        SQL = strSQLInfo + strOrder;//还原
                        break;
                }
            }

            //执行分页参数替换
            SQL = SQL.Replace("@@PageSize", PageSize.ToString())
                .Replace("@@Page_Size_Number", Convert.ToString(PageSize * PageNumber))
                .Replace("@@LeftSize", PageSize.ToString());//
            //.Replace ("@@Where",strWhere);
            //针对用户的额外条件处理：
            if (strWhere != "" && strWhere.ToUpper().Trim().StartsWith("WHERE "))
            {
                throw new Exception("分页额外查询条件不能带Where谓词！");
            }
            if (!SqlFlag)
            {
                if (strWhere != "") strWhere = " Where " + strWhere;
                SQL = SQL.Replace("@@Where", strWhere);
            }
            else
            {
                if (strWhere != "") strWhere = " And (" + strWhere + ")";
                SQL = SQL.Replace("@@Where", strWhere);
            }
            return SQL;

        }
        #endregion
    }

}
