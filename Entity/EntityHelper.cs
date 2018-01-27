using FYJ.Data;
using FYJ.Data.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace FYJ.Data.Entity
{

    public delegate object NewIDHandler();
    /// <summary>
    /// 提供实体通用的基本操作方法
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class EntityHelper<T>
        where T : new()
    {
        public string TableName
        {
            get;
            set;
        }
        public string Primary
        {
            get;
            set;
        }

        private NewIDHandler newID;

        public EntityHelper(string tableName, string primary, NewIDHandler newID)
        {
            this.TableName = tableName;
            this.Primary = primary;
            this.newID = newID;
        }

        #region private
        /// <summary>
        /// 获取一个实体不为null属性的IDataParameter参数
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        private List<IDataParameter> GetIDataParameters(T model, IDbHelper dbHelper)
        {
            PropertyInfo[] pis = model.GetType().GetProperties();

            List<IDataParameter> parames = new List<IDataParameter>();

            foreach (PropertyInfo pi in pis)
            {
                string parameterName = pi.Name;

                object obj = pi.GetValue(model, null);
                if (obj != null)    //如果属性的值不为null  才将其加入到参数中
                {
                    DbParameter parameter = dbHelper.CreateParameter();
                    parameter.ParameterName = dbHelper.ParameterPrefix + parameterName;
                    if (obj == null)
                    {
                        parameter.Value = DBNull.Value;
                    }
                    else
                    {
                        parameter.Value = obj;
                    }
                    parames.Add(parameter);
                }
            }
            return parames;
        }

        #endregion

        #region 查询数据

        /// <summary>
        /// 获取一条实体数据
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        public T GetEntity(string id, IDbHelper dbHelper)
        {
            if (id == null || id == "")
            {
                throw new Exception("参数id不能为空");
            }
            T model = default(T);
            string sql = "SELECT * FROM " + this.TableName + " WHERE " + this.Primary + "=" + dbHelper.ParameterPrefix + this.Primary;
            DataTable dt = dbHelper.GetDataTable(sql, dbHelper.CreateParameter(dbHelper.ParameterPrefix + this.Primary, id));
            if (dt.Rows.Count == 1)
            {
                model = ObjectHelper.DataTableToSingleModel<T>(dt);
            }
            return model;
        }


        /// <summary>
        /// 获取一条实体数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        public static T GetEntity(string tableName, string key, string value, IDbHelper dbHelper)
        {
            T model = default(T);
            string sql = "SELECT * FROM " + tableName + " WHERE " + key + "=" + dbHelper.ParameterPrefix + key;
            DataTable dt = dbHelper.GetDataTable(sql, dbHelper.CreateParameter(dbHelper.ParameterPrefix + key, value));
            if (dt.Rows.Count == 1)
            {
                model = ObjectHelper.DataTableToSingleModel<T>(dt);
            }
            return model;
        }
        #endregion

        #region  新增

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int Insert(T entity, string tableName, string primaryKey, bool isAddPrimaryKey, IDbHelper dbHelper)
        {
            PropertyInfo[] pis = entity.GetType().GetProperties();

            String sql = "insert into {0} ({1}) values ({2})";
            String col = "";
            String val = "";
            List<DbParameter> parames = new List<DbParameter>();

            foreach (PropertyInfo pi in pis)
            {
                object[] att = pi.GetCustomAttributes(typeof (IgnoreAttribute),false);
                if (att != null && att.Length > 0)
                {
                    continue;
                }

                if (pi.PropertyType.IsGenericType)
                {
                    continue;
                }

                if (!pi.PropertyType.FullName.StartsWith("System."))
                {
                    continue;
                }

                string parameterName = pi.Name;
                DbParameter parameter = dbHelper.CreateParameter();
                object obj = pi.GetValue(entity, null);

                //如果不增加主键 则排除
                if (!isAddPrimaryKey)
                {
                    if (parameterName.Equals(primaryKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                }

                col += parameterName + ",";
                val += dbHelper.ParameterPrefix + parameterName + ",";
                parameter.ParameterName = dbHelper.ParameterPrefix + parameterName;
                parameter.Value = ConvertDbObject(obj);
                parames.Add(parameter);
            }
            col = col.TrimEnd(',');
            val = val.TrimEnd(',');
            sql = String.Format(sql, tableName, col, val);

            return dbHelper.ExecuteSql(sql, parames.ToArray());
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        public int Insert(T entity, IDbHelper dbHelper)
        {
            PropertyInfo[] pis = entity.GetType().GetProperties();

            String sql = "insert into {0} ({1}) values ({2})";
            String col = "";
            String val = "";
            List<DbParameter> parames = new List<DbParameter>();

            foreach (PropertyInfo pi in pis)
            {
                object[] att = pi.GetCustomAttributes(typeof(IgnoreAttribute), false);
                if (att != null && att.Length > 0)
                {
                    continue;
                }

                if (pi.PropertyType.IsGenericType && 
                    (!pi.PropertyType.FullName.StartsWith("System.Nullable"))
                    )
                {
                    continue;
                }

                if (!pi.PropertyType.FullName.StartsWith("System."))
                {
                    continue;
                }
                string parameterName = pi.Name;
                DbParameter parameter = dbHelper.CreateParameter();
                object obj = pi.GetValue(entity, null);

                //如果是主键
                if (parameterName.Equals(this.Primary, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (obj == null || obj.ToString() == "" || obj.ToString() == "0")
                    {
                        continue;
                    }
                }

                col += parameterName + ",";
                val += dbHelper.ParameterPrefix + parameterName + ",";
                parameter.ParameterName = dbHelper.ParameterPrefix + parameterName;
                parameter.Value = ConvertDbObject(obj);
                parames.Add(parameter);
            }
            col = col.TrimEnd(',');
            val = val.TrimEnd(',');
            sql = String.Format(sql, this.TableName, col, val);

            return dbHelper.ExecuteSql(sql, parames.ToArray());
        }
        #endregion

        #region  修改

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="entity">新实体</param>
        /// <param name="dbHelper">要更新的列</param>
        /// <returns></returns>
        public int Update(T entity, IDbHelper dbHelper)
        {
            String sql = "update {0} set {1} where " + this.Primary + "=" + dbHelper.ParameterPrefix + this.Primary;
            String col = "";
            List<DbParameter> parames = new List<DbParameter>();
            object id = 0;
            String pkName = null;
            PropertyInfo[] pis = entity.GetType().GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                object[] att = pi.GetCustomAttributes(typeof(IgnoreAttribute), false);
                if (att != null && att.Length > 0)
                {
                    continue;
                }

                if (pi.PropertyType.IsGenericType)
                {
                    continue;
                }

                if (!pi.PropertyType.FullName.StartsWith("System."))
                {
                    continue;
                }
                string parameterName = pi.Name;
                object obj = pi.GetValue(entity, null);  //获取属性值
                //如果不是主键
                if (!this.Primary.Equals(parameterName, StringComparison.CurrentCultureIgnoreCase))
                {
                    DbParameter parameter = dbHelper.CreateParameter();
                    col += parameterName + "=" + dbHelper.ParameterPrefix + parameterName + ",";
                    parameter.ParameterName = dbHelper.ParameterPrefix + parameterName;
                    parameter.Value = ConvertDbObject(obj); ;
                    parames.Add(parameter);
                }
                else
                {
                    DbParameter parameter = dbHelper.CreateParameter();
                    parameter.ParameterName = dbHelper.ParameterPrefix + parameterName;
                    parameter.Value = obj;
                    parames.Add(parameter);
                }

            }
            col = col.TrimEnd(',');
            sql = String.Format(sql, this.TableName, col, pkName, id);

            return dbHelper.ExecuteSql(sql, parames.ToArray());
        }

        private static bool IsPrimary(string key, params string[] primaryKey)
        {
            foreach (string p in primaryKey)
            {
                if ((p.Equals(key, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static object ConvertDbObject(object obj)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }
            else
            {
                if (obj.GetType() == typeof(DateTime))
                {
                    if (Convert.ToDateTime(obj) < new DateTime(1753, 1, 1) || Convert.ToDateTime(obj) > new DateTime(9999, 12, 31))
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return obj;
                    }
                }
                else
                {
                    return obj;
                }
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="entity">新实体</param>
        /// <param name="dbHelper">要更新的列</param>
        /// <returns></returns>
        public static int Update(T entity, string tableName, IDbHelper dbHelper, params string[] primaryKey)
        {

            String col = "";
            List<DbParameter> parames = new List<DbParameter>();
            PropertyInfo[] pis = entity.GetType().GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                object[] att = pi.GetCustomAttributes(typeof(IgnoreAttribute), false);
                if (att != null && att.Length > 0)
                {
                    continue;
                }

                if (pi.PropertyType.IsGenericType)
                {
                    continue;
                }

                if (!pi.PropertyType.FullName.StartsWith("System."))
                {
                    continue;
                }
                string parameterName = pi.Name;
                object obj = pi.GetValue(entity, null);  //获取属性值
                //如果不是主键
                if (!IsPrimary(parameterName, primaryKey))
                {
                    DbParameter parameter = dbHelper.CreateParameter();
                    col += parameterName + "=" + dbHelper.ParameterPrefix + parameterName + ",";
                    parameter.ParameterName = dbHelper.ParameterPrefix + parameterName;
                    parameter.Value = ConvertDbObject(obj);
                    parames.Add(parameter);
                }
                else
                {
                    DbParameter parameter = dbHelper.CreateParameter();
                    parameter.ParameterName = dbHelper.ParameterPrefix + parameterName;
                    parameter.Value = obj;
                    parames.Add(parameter);
                }

            }
            col = col.TrimEnd(',');

            string tmp = "";
            foreach (string p in primaryKey)
            {
                tmp += " and " + p + "=@" + p;
            }

            string sql = "update " + tableName + " set " + col + " where 1=1 " + tmp;

            return dbHelper.ExecuteSql(sql, parames.ToArray());
        }

        /// <summary>
        /// 修改某个字段值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        public int Update(string id, string fieldName, object value, IDbHelper dbHelper)
        {
            string tmp = "";
            foreach (string s in id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = s.Trim('\'');
                tmp += "'" + t + "',";
            }
            tmp = tmp.TrimEnd(',');

            String sql = "UPDATE  " + this.TableName + " SET " + fieldName + "=" + dbHelper.ParameterPrefix + fieldName + " where " + this.Primary + " in (" + tmp + ")";

            return dbHelper.ExecuteSql(sql, dbHelper.CreateParameter(dbHelper.ParameterPrefix + fieldName, value));
        }

        #endregion

        #region 删除
        /// <summary>
        /// 根据id删除一条或多条数据
        /// </summary>
        /// <param name="accessId"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        public int Delete(string id, IDbHelper dbHelper)
        {
            string tmp = "";
            foreach (string s in id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = s.Trim('\'');
                tmp += "'" + t + "',";
            }
            tmp = tmp.TrimEnd(',');
            string sql = "delete from " + TableName + " where " + this.Primary + " in (" + tmp + ")";
            int result = dbHelper.ExecuteSql(sql);

            return result;
        }

        #endregion


    }
}
