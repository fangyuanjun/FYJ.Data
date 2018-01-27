using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace FYJ.Data.Util
{
   public class DataTableToExcel
    {
        public void SaveExcel(DataTable dt, string Filter, string FileName, string SheetName)
        {

            string ConnStr;
            ConnStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"" + FileName + "\";Extended Properties=\"Excel 8.0;HDR=YES\"";

            OleDbConnection conn_excel = new OleDbConnection();
            conn_excel.ConnectionString = ConnStr;

            OleDbCommand cmd_excel = new OleDbCommand();

            string sql;
            sql = SqlCreate(dt, SheetName);

            conn_excel.Open();
            cmd_excel.Connection = conn_excel;
            cmd_excel.CommandText = sql;
            cmd_excel.ExecuteNonQuery();

            conn_excel.Close();

            OleDbDataAdapter da_excel = new OleDbDataAdapter("Select * From [" + SheetName + "$]", conn_excel);
            DataTable dt_excel = new DataTable();
            da_excel.Fill(dt_excel);

            da_excel.InsertCommand = SqlInsert(SheetName, dt, conn_excel);

            DataRow dr_excel;
            string ColumnName;

            foreach (DataRow dr in dt.Select(Filter))
            {
                dr_excel = dt_excel.NewRow();

                foreach (DataColumn dc in dt.Columns)
                {
                    ColumnName = dc.ColumnName;
                    if (!String.IsNullOrEmpty(dc.Caption))
                        ColumnName = dc.Caption;
                    dr_excel[ColumnName] = dr[ColumnName];

                }
                dt_excel.Rows.Add(dr_excel);

            }

            da_excel.Update(dt_excel);
            conn_excel.Close();

        }

        private void CheckColumn(DataTable dt, DataTable dt_v)
        {
            foreach (DataRow dr in dt_v.Select())
            {
                if (!dt.Columns.Contains(dr["列名"].ToString()))
                {
                    dr.Delete();
                }
            }
            dt_v.AcceptChanges();
        }

        private string GetDataType(Type i)
        {
            string s;

            switch (i.Name)
            {
                case "String":
                    s = "Char";
                    break;
                case "Int32":
                    s = "Int";
                    break;
                case "Int64":
                    s = "Int";
                    break;
                case "Int16":
                    s = "Int";
                    break;
                case "Double":
                    s = "Double";
                    break;
                case "Decimal":
                    s = "Double";
                    break;
                default:
                    s = "Char";
                    break;

            }
            return s;
        }

        private OleDbType StringToOleDbType(Type i)
        {
            OleDbType s;

            switch (i.Name)
            {
                case "String":
                    s = OleDbType.Char;
                    break;
                case "Int32":
                    s = OleDbType.Integer;
                    break;
                case "Int64":
                    s = OleDbType.Integer;
                    break;
                case "Int16":
                    s = OleDbType.Integer;
                    break;
                case "Double":
                    s = OleDbType.Double;
                    break;
                case "Decimal":
                    s = OleDbType.Decimal;
                    break;
                default:
                    s = OleDbType.Char;
                    break;

            }
            return s;

        }


        private string SqlCreate(DataTable dt, string SheetName)
        {
            string sql;

            sql = "CREATE TABLE " + SheetName + " (";

            foreach (DataColumn dc in dt.Columns)
            {
                sql += "[" + dc.ColumnName + "] " + GetDataType(dc.DataType) + " ,";
            }

            //sql = "CREATE TABLE [" + SheetName + "] (";

            //foreach (C1.Win.C1TrueDBGrid.C1DataColumn dc in grid.Columns)
            //{
            //    sql += "[" + dc.Caption + "] " + GetDataType(dc.DataType) + ",";
            //}
            //sql = sql.Substring(0, sql.Length - 1);
            //sql += ")";

            sql = sql.Substring(0, sql.Length - 1);
            sql += ")";

            return sql;
        }


        // 生成 InsertCommand 并设置参数
        private OleDbCommand SqlInsert(string SheetName, DataTable dt, OleDbConnection conn_excel)
        {
            OleDbCommand i;
            string sql;

            sql = "INSERT INTO [" + SheetName + "$] (";
            foreach (DataColumn dc in dt.Columns)
            {
                sql += "[" + dc.ColumnName + "] ";
                sql += ",";
            }
            sql = sql.Substring(0, sql.Length - 1);
            sql += ") VALUES (";
            foreach (DataColumn dc in dt.Columns)
            {
                sql += "?,";
            }
            sql = sql.Substring(0, sql.Length - 1);
            sql += ")";

            i = new OleDbCommand(sql, conn_excel);

            foreach (DataColumn dc in dt.Columns)
            {
                i.Parameters.Add("@" + dc.Caption, StringToOleDbType(dc.DataType), 0, dc.Caption);
            }

            return i;
        }

    }
}
