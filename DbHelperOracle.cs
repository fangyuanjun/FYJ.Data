using FYJ.Data.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FYJ.Data
{
    public class DbHelperOracle : DbHelper
    {
        #region 构造函数
        public DbHelperOracle()
        {

        }
        public DbHelperOracle(String connectionString)
            : base(System.Data.OracleClient.OracleClientFactory.Instance, connectionString)
        {

        }
        #endregion


        public List<TableInfo> GetTablesByOwner(string owner)
        {
            string sql = "";
            List<TableInfo> list = new List<Util.TableInfo>();
            if (String.IsNullOrEmpty(owner))
            {
                sql = "SELECT a.TABLE_NAME,a.num_rows,b.COMMENTS FROM user_Tables a left join USER_TAB_COMMENTS  b on a.TABLE_NAME = b.table_name  order by a.TABLE_NAME";
                DataTable dt = this.GetDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    TableInfo info = new Util.TableInfo();
                    info.Name = dr["TABLE_NAME"].ToString();
                    if (dr["num_rows"].ToString() != "")
                    {
                        info.RowCount = Convert.ToInt32(dr["num_rows"]);
                    }
                    info.Comment = dr["COMMENTS"].ToString();

                    list.Add(info);
                }
            }
            else
            {
                string str = "";
                foreach (string s in owner.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    str += "'" + s + "',";
                }
                str = str.TrimEnd(',');

               
                 sql = @"SELECT a.OWNER, a.TABLE_NAME, a.num_rows, b.COMMENTS
  FROM all_Tables a
  left join ALL_TAB_COMMENTS b
   on a.TABLE_NAME = b.TABLE_NAME
   and a.OWNER = b.OWNER
   where a.OWNER in({0}) order by a.TABLE_NAME";
                sql = String.Format(sql, str);
                DataTable dt = this.GetDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    TableInfo info = new Util.TableInfo();
                    info.Name = dr["OWNER"] +"." +dr["TABLE_NAME"].ToString();
                    if (dr["num_rows"].ToString() != "")
                    {
                        info.RowCount = Convert.ToInt32(dr["num_rows"]);
                    }
                    info.Comment = dr["COMMENTS"].ToString();

                    list.Add(info);
                }
            }
           

            return list;
        }

        /// <summary>
        /// 查询表  并将列Caption属性(标题)设为备注
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<ColumnInfo> GetDatableSchema(string tableName)
        {
            String sql = @"
                    SELECT B.TABLE_NAME,
                       A.COLUMN_ID,
                       B.COLUMN_NAME,
                       A.DATA_TYPE,
                       A.DATA_LENGTH,
                       A.DATA_PRECISION,
                       A.DATA_SCALE,
                       A.NULLABLE,
                       A.CHAR_LENGTH,
                       B.COMMENTS COLUMN_COMMENTS
                      FROM USER_TAB_COLUMNS A, USER_COL_COMMENTS B
                      WHERE A.TABLE_NAME = B.TABLE_NAME
                       AND A.COLUMN_NAME = B.COLUMN_NAME
                           AND A.TABLE_NAME='{0}'
                         order by  A.COLUMN_ID
                    ";

            if (tableName.Contains("."))
            {
                sql = @"
                        SELECT B.TABLE_NAME,
                           A.COLUMN_ID,
                           B.COLUMN_NAME,
                           A.DATA_TYPE,
                           A.DATA_LENGTH,
                           A.DATA_PRECISION,
                           A.DATA_SCALE,
                           A.NULLABLE,
                           A.CHAR_LENGTH,
                           B.COMMENTS COLUMN_COMMENTS
                          from ALL_TAB_COLUMNS A, ALL_COL_COMMENTS B
                         where A.TABLE_NAME = B.TABLE_NAME 
                           and A.COLUMN_NAME = B.COLUMN_NAME
                           AND a.OWNER=b.owner
                           AND a.OWNER='{0}'
                           AND A.TABLE_NAME='{1}'
                         order by A.COLUMN_ID
                        ";

                sql = String.Format(sql, tableName.Substring(0, tableName.IndexOf(".")), tableName.Substring(tableName.IndexOf(".") + 1));
            }
            else
            {
                sql = String.Format(sql, tableName);
            }

            DataTable dt = GetDataTable(sql);

            List<ColumnInfo> list = new List<ColumnInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                ColumnInfo col = new ColumnInfo();
                col.Name = dr["COLUMN_NAME"].ToString();
                col.Index = Convert.ToInt32(dr["COLUMN_ID"]);
                //col.IsIdentity = (dr["标识"].ToString() == "1");
                //col.IsPrimary = (dr["主键"].ToString() == "1");
                col.IsAllowNull = (dr["NULLABLE"].ToString() == "Y");
                col.Type = dr["DATA_TYPE"].ToString();
                //if (dr["主键"].ToString() == "1")
                //{
                //    col.IsUniqueness = true;
                //}

                col.Mark = dr["COLUMN_COMMENTS"].ToString();
                //col.DefaultValue = dr["默认值"].ToString();
                if (dr["DATA_TYPE"].ToString() == "NUMBER")
                {
                    if (dr["DATA_PRECISION"].ToString() != "")
                    {
                        col.Length = Convert.ToInt32(dr["DATA_PRECISION"]);
                        col.Radix = Convert.ToInt32(dr["DATA_SCALE"]);   //小数精度
                    }
                }
                else
                {
                    col.Length = Convert.ToInt32(dr["DATA_LENGTH"]);
                }
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

                c.IsIdentity = column.AutoIncrement;
                c.IsPrimary = DataTableHelper.IsPrimarykey(dt, column.ColumnName);
                c.DataType = column.DataType;
                //c.DefaultValue = column.DefaultValue;
            }
            return list;
        }//end


        public TableInfo GetTableInfo(string tableName)
        {
            if (tableName.Contains("."))
            {

                string sql = @"SELECT a.OWNER, a.TABLE_NAME, a.num_rows, b.COMMENTS
  FROM all_Tables a
  left join ALL_TAB_COMMENTS b
   on a.TABLE_NAME = b.TABLE_NAME
   and a.OWNER = b.OWNER
   where a.OWNER = '{0}'
   and a.TABLE_NAME = '{1}' ";
                string prefix = tableName.Substring(tableName.IndexOf("."));
                sql = String.Format(sql, prefix, tableName.Substring(tableName.IndexOf(".") + 1));
                DataTable dt = this.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    TableInfo info = new TableInfo();
                    info.Name = tableName;
                    info.Owner = prefix;
                    if (dt.Rows[0]["num_rows"].ToString() != "")
                    {
                        info.RowCount = Convert.ToInt32(dt.Rows[0]["num_rows"]);
                    }
                    info.Comment = dt.Rows[0]["COMMENTS"].ToString();
                    return info;
                }
            }
            else
            {
                string sql = @"SELECT a.TABLE_NAME,a.num_rows,b.COMMENTS
  FROM user_Tables a left join USER_TAB_COMMENTS  b on a.TABLE_NAME=b.table_name
 WHERE a.TABLE_NAME='{0}'";
                sql = String.Format(sql, tableName);
                DataTable dt = this.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    TableInfo info = new TableInfo();
                    info.Name = tableName;
                    info.Comment = dt.Rows[0]["COMMENTS"].ToString();
                    if (dt.Rows[0]["num_rows"].ToString() != "")
                    {
                        info.RowCount = Convert.ToInt32(dt.Rows[0]["num_rows"]);
                    }
                    return info;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<PrimaryInfo> GetPrimaryKeys(string tableName)
        {
            List<PrimaryInfo> list = new List<PrimaryInfo>();

            if (tableName.Contains("."))
            {
                string prefix = tableName.Substring(tableName.IndexOf("."));
                string sql = @"select a.constraint_name,  a.column_name 
 from all_cons_columns a, all_constraints b 
 where a.constraint_name = b.constraint_name 
 AND a.owner=b.owner
 and b.constraint_type = 'P'  AND a.table_name='{0}'
 AND a.owner='{1}'";
                sql = String.Format(sql, tableName.Substring(tableName.IndexOf(".")+1),prefix);
                DataTable dt = this.GetDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    PrimaryInfo info = new PrimaryInfo();
                    info.Name = dr["constraint_name"].ToString();
                    info.TableName = tableName;
                    info.ColmnName = dr["column_name"].ToString();
                    info.Owner = prefix;
                    list.Add(info);
                }
            }
            else
            {
                string sql = @"select a.constraint_name,  a.column_name 
 from user_cons_columns a, user_constraints b 
 where a.constraint_name = b.constraint_name 
 and b.constraint_type = 'P'  AND a.table_name='{0}'";
                sql = String.Format(sql, tableName);
                DataTable dt = this.GetDataTable(sql);
               foreach(DataRow dr in dt.Rows)
                {
                    PrimaryInfo info = new PrimaryInfo();
                    info.Name = dr["constraint_name"].ToString();
                    info.TableName = tableName;
                    info.ColmnName = dr["column_name"].ToString();

                    list.Add(info);
                }
            }

            return list;
        }

        public List<SequenceInfo> GetSequence(string owner=null)
        {
            List<SequenceInfo> list = new List<SequenceInfo>();
            DataTable dt = null;
            if (String.IsNullOrEmpty(owner))
            {
                dt = this.GetDataTable("SELECT * FROM user_sequences");
            }
            else
            {
                dt = this.GetDataTable("SELECT sequence_owner,sequence_name,MIN_VALUE,max_value,INCREMENT_BY,cycle_flag,ORDER_FLAG,cache_size,last_number FROM all_sequences   t where sequence_owner='"+owner+"'");
            }

            foreach (DataRow dr in dt.Rows)
            {
                SequenceInfo info = new SequenceInfo();
                info.Owner = owner;
                info.Cache = Convert.ToInt32(dr["cache_size"]);
                info.Cycle = (dr["cycle_flag"].ToString() == "Y" ? true : false);
                info.Increment = Convert.ToInt32(dr["INCREMENT_BY"]);
                info.MaxValue = Convert.ToDecimal(dr["max_value"]);
                info.MinValue = Convert.ToDecimal(dr["MIN_VALUE"]);
                info.Name = dr["sequence_name"].ToString();
                info.Start = Convert.ToDecimal(dr["last_number"]);

                list.Add(info);
            }

            return list;
        }

        /// <summary>
        /// 获取状态为OPEN的owner
        /// </summary>
        /// <returns></returns>
        public List<string> GetOpenOwners()
        {
            List<string> list = new List<string>();
            string sql = "SELECT * FROM Dba_Users WHERE account_status='OPEN' ORDER BY username";
            DataTable dt = this.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr["username"].ToString());
            }

            return list;
        }
    }
}
