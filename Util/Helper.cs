using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace FYJ.Data.Util
{
    public class Helper
    {
        /// <summary>
        /// 修正参数前缀 oracle 为:
        /// </summary>
        /// <param name="cmdParms">参数数组</param>
        /// <returns></returns>
        public static IEnumerable<IDataParameter> FixParametersPre(IEnumerable<IDataParameter> cmdParms, DbHelperType dbHelperTypeEnum)
        {
            string parameterPre = "@"; //参数前缀
            if (dbHelperTypeEnum == DbHelperType.Oracle)
                parameterPre = ":";

            if (cmdParms != null)
            {
                foreach (IDataParameter parameter in cmdParms)
                {
                    string parameterName = parameter.ParameterName.Trim();
                    if (parameterName.StartsWith("@") || parameterName.StartsWith(":"))
                        parameterName = parameterName.Substring(1);   //移除开始符号  
                    parameterName = parameterPre + parameterName; //再加上开始符号
                    parameter.ParameterName = parameterName;
                }
            }

            return cmdParms;
        }

        /// <summary>
        /// 克隆IDataParameter
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static IEnumerable<IDataParameter> CloneParameters(DbProviderFactory factory, IEnumerable<IDataParameter> cmdParms)
        {
            List<IDataParameter> list = new List<IDataParameter>();
            foreach (IDataParameter parameter in cmdParms)
            {
                DbParameter newparameter = factory.CreateParameter();
                newparameter.ParameterName = parameter.ParameterName;
                newparameter.Value = parameter.Value;
                list.Add(newparameter);
            }

            return list;
        }

        public static List<String> GetOledbTables(string connectionString)
        {
            List<String> list = new List<string>();
            OleDbConnection OleDbCon = new OleDbConnection(connectionString);
            OleDbCon.Open();
            DataTable dt = OleDbCon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            foreach (DataRow dr in dt.Rows)
            {
                list.Add((String)dr["TABLE_NAME"]);
            }
            OleDbCon.Close();
            return list;
        }

        public static string GetAddSql(string tableName, string pkName, IEnumerable<IDataParameter> paras)
        {
            string sql = "insert into " + tableName + " (";
            string columns = "";
            string values = "";

            foreach (IDataParameter par in paras)
            {
                if (par.ParameterName.TrimEnd('@') == pkName) //如果是主键
                {
                    //并且值为空、空字符串、0 则不添加该项 让数据库自动插入
                    if (par.Value == null || par.Value.ToString() == "" || par.Value.ToString() == "0")
                    {
                        break;
                    }
                }
                columns += par.ParameterName.TrimStart('@') + ",";
                values += par.ParameterName + ",";
            }
            columns = columns.TrimEnd(',');
            values = values.TrimEnd(',');
            sql += columns + ") values (" + values + ")";

            return sql;
        }

        public static string GetUpdateSql(string tableName, string pkName, IEnumerable<IDataParameter> paras)
        {
            string sql = "update  " + tableName + " set ";

            foreach (IDataParameter par in paras)
            {
                if (par.ParameterName.TrimStart('@') != pkName)
                {
                    sql += par.ParameterName.TrimStart('@') + "=" + par.ParameterName + ",";
                }
            }
            sql = sql.TrimEnd(',');
            sql += " where " + pkName + "=@" + pkName;
            return sql;
        }

        //IDictionary<string, User> ilg = new ConcurrentDictionary<string, User>();
        //User u = new User { UserName="fangyuanjun"};
        //ilg.Add("user",u);

        //DataContractSerializer ds = new DataContractSerializer(typeof(IDictionary<string, User>));
        //var ms = new MemoryStream();
        //ds.WriteObject(ms, ilg);

        //ms.Position = 0;
        //var sr = new StreamReader(ms);
        //var str = sr.ReadToEnd();
        //Assert.AreSame(sr,str);
        //var buffer = System.Text.Encoding.UTF8.GetBytes(str);
        //var ms2 = new MemoryStream(buffer);
        //var v = ds.ReadObject(ms2) as IDictionary<string, User>;


        //[DataContract]
        //class User
        //{
        //    [DataMember]
        //    public string UserName
        //    {
        //        get;
        //        set;
        //    }
        //}


        /// <summary>
        /// 给以逗号分隔的字符或数字数组加上单引号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToStringWithSingleQuot(string str)
        {
            string result = "";
            foreach (string s in str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!s.StartsWith("'")) //如果不包含单引号
                {
                    result += "'" + s + "',";
                }
                else
                {
                    result += s + ",";
                }
            }
            result = result.TrimEnd(',');
            return result;
        }

        #region 过滤sql
        /// <summary>
        /// 过滤sql
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public static string SqlTextClear(string sqlText)
        {
            if (sqlText == null)
            {
                return null;
            }
            if (sqlText == "")
            {
                return "";
            }
            sqlText = sqlText.Replace(",", "");//去除,
            sqlText = sqlText.Replace("<", "");//去除<
            sqlText = sqlText.Replace(">", "");//去除>
            sqlText = sqlText.Replace("--", "");//去除--
            sqlText = sqlText.Replace("'", "");//去除'
            sqlText = sqlText.Replace("\"", "");//去除"
            sqlText = sqlText.Replace("=", "");//去除=
            sqlText = sqlText.Replace("%", "");//去除%
            sqlText = sqlText.Replace(" ", "");//去除空格
            return sqlText;
        }
        #endregion
    }
}
