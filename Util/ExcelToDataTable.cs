using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Diagnostics;

namespace FYJ.Data.Util
{
   public class ExcelToDataTable
    {
       
        /// <summary>
        /// 导入Excel数据到DataTable
        /// </summary>
        /// <param name="strFileName">带路径的文件名称</param>
        /// <param name="isHead">[可选参数]是否包含表头 默认true</param>
        /// <param name="iSheet">[可选参数]要导入的sheet 默认1</param>
        /// <returns>datatable</returns>
        public DataTable GetDataFromExcel(string strFileName, bool isHead=true, int iSheet=1)
        {
            if (strFileName.ToLower().EndsWith(".xls") ||
                strFileName.ToLower().EndsWith(".xlsx"))
            {
                DataTable dtReturn = new DataTable();
                string connectionString = strFileName.ToLower().EndsWith(".xls") ? "Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties='Excel 8.0;HDR={0};IMEX=1;';Data Source={1};" :
                    "Provider=Microsoft.ACE.OLEDB.12.0; Persist Security Info=False;Extended Properties='Excel 8.0;HDR={0};IMEX=1;';Data Source={1};";
                connectionString = String.Format(connectionString, isHead ? "YES" : "NO", strFileName);

                OleDbConnection connection = new OleDbConnection(connectionString);
                connection.Open();
                try
                {
                    string str = "Select * from [Sheet" + iSheet + "$]";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(str, connection);
                    adapter.Fill(dtReturn);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }


                return dtReturn;
            }
            else
            {
                throw new Exception("错误的格式！");
            }
        }


    }
}
