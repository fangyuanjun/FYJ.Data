using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using System.Text.RegularExpressions;
using System.Reflection;
using FYJ.Data.Util;

namespace FYJ.Data
{
    public class DbHelperMySql : DbHelper
    {
        #region 构造函数
        public DbHelperMySql()
        {

        }

        public DbHelperMySql(String connectionString)
        {
            object obj = Assembly.Load("MySql.Data").CreateInstance("MySql.Data.MySqlClient.MySqlClientFactory");
            DbProviderFactory factory = (DbProviderFactory)obj;
            this.ConnectionString = connectionString;
            this.DbProviderFactoryInstance = factory;
        }

        #endregion

        /// <summary>
        /// 获取所有数据库名
        /// </summary>
        /// <returns></returns>
        public List<String> GetDatabases()
        {
            List<String> list = new List<string>();
            string sql = "SHOW DATABASES";
            DataTable dt = this.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr[0].ToString());
            }
            return list;
        }


        /// <summary>
        /// 表是否存在  重写
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override bool ExistsTable(string tableName)
        {
            foreach (string s in GetTables())
            {
                if (s.Equals(tableName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 查询表  并将列Caption属性(标题)设为备注
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<ColumnInfo> GetDatableSchema(string tableName)
        {
            //  show  COLUMNS from blog_tb_tag;
            //  show full COLUMNS from blog_tb_tag;
            //  show full fields from blog_tb_tag

            string sql = @" show full COLUMNS from " + tableName;

            DataTable dt = GetDataTable(sql);

            List<ColumnInfo> list = new List<ColumnInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                ColumnInfo col = new ColumnInfo();
                col.Name = dr["Field"].ToString();
                col.IsPrimary = (dr["Key"].ToString() == "PRI");
                col.IsAllowNull = (dr["Null"].ToString() == "YES");

                if (dr["Key"].ToString() == "PRI")
                {
                    col.IsUniqueness = true;
                }

                if (Regex.IsMatch(dr["Type"].ToString(), "(\\d+)"))
                {
                    col.Type = dr["Type"].ToString().Substring(0, dr["Type"].ToString().IndexOf("("));
                    col.Length = Convert.ToInt32(Regex.Match(dr["Type"].ToString(), "(\\d+)").Groups[1].Value);
                }
                else
                {
                    col.Type = dr["Type"].ToString();
                }

                col.DefaultValue = dr["Default"].ToString();
                col.Mark = dr["Comment"].ToString();
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

                if (c.Type == "bit")
                {
                    c.DataType = typeof(bool);
                }
            }

            return list;
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

            adapter.InsertCommand.CommandText = adapter.InsertCommand.CommandText;

            using (TransactionScope ts = new TransactionScope())
            {
                int result = adapter.Update(dt);
                ts.Complete();
                dt.AcceptChanges();
                return result;
            }
        }

    }

}
