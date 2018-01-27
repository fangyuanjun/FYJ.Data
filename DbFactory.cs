using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Linq;

namespace FYJ.Data
{
    public class DbHelperItem
    {
        public string configName;
        public string providerName;
        public string connectionString;

        public IDbHelper db;
    }
    /// <summary>
    /// 工厂类，无法继承该类
    /// </summary>
    public sealed class DbFactory :IDbFactory
    {
        private static List<DbHelperItem> items = new List<DbHelperItem>();
        /// <summary>
        /// 根据配置节点主键名构造IDbHelper对象
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static IDbHelper CreateIDbHelper(string configName)
        {
            var section = ConfigurationManager.GetSection("DbHelperSettings") as FYJ.Data.Config.DbHelperSection;
            if(section==null)
            {
                return new DbHelper(configName);
            }

            var items = section.Items;

            foreach (FYJ.Data.Config.DbHelperElement item in items)
            {
                if (item.Name == configName)
                {
                    Type t = Type.GetType(item.Type);
                    IDbHelper db = (IDbHelper)Activator.CreateInstance(t);

                    DbProviderFactory factory = null;
                    string connectionString = item.ConnectionString;
                    if (item.IsEncrypt == true)
                    {
                        IDbEncrypt encrypt = (IDbEncrypt)Activator.CreateInstance(Type.GetType(item.EncryptType));
                        connectionString = encrypt.Decrypt(connectionString);
                    }
                    if (item.ProviderName.StartsWith("MySql.Data", StringComparison.CurrentCultureIgnoreCase))
                    {
                        object obj = Assembly.Load("MySql.Data").CreateInstance("MySql.Data.MySqlClient.MySqlClientFactory");
                        factory = (DbProviderFactory)obj;
                    }
                    else if (item.ProviderName.StartsWith("System.Data.SQLite", StringComparison.CurrentCultureIgnoreCase))
                    {
                        object obj = Assembly.Load("System.Data.SQLite").CreateInstance("System.Data.SQLite.SQLiteFactory");
                        factory = (DbProviderFactory)obj;
                        String dbFileName = Regex.Match(connectionString, "data\\s*source\\s*=\\s*(.*)").Groups[1].Value;
                        if (!File.Exists(dbFileName))
                        {
                            connectionString = "data source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName);
                        }
                    }
                    else
                    {
                        factory = DbProviderFactories.GetFactory(item.ProviderName);
                    }

                    db.DbProviderFactoryInstance = factory;
                    db.ConnectionString = connectionString;
                    return db;
                }
            }

            throw new Exception("名为" + configName + "的数据配置不存在");
        }

        public static IDbHelper CreateIDbHelper(DbHelperType dbType, string connectionString)
        {
            if (dbType == DbHelperType.SqlServer)
            {
                return new DbHelperSQL(connectionString);
            }
            if (dbType == DbHelperType.MySql)
            {
                string dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "MySql.Data.dll");
                object obj = Assembly.LoadFrom(dllPath).CreateInstance("MySql.Data.MySqlClient.MySqlClientFactory");
                DbProviderFactory factory = (DbProviderFactory)obj;
                return new DbHelper(factory, connectionString);
            }
            if (dbType == DbHelperType.Odbc)
            {
                return new DbHelper(System.Data.Odbc.OdbcFactory.Instance, connectionString);
            }
            if (dbType == DbHelperType.OleDb)
            {
                return new DbHelper(System.Data.OleDb.OleDbFactory.Instance, connectionString);
            }
            if (dbType == DbHelperType.Oracle)
            {
                return new DbHelperOracle(connectionString);
            }
            if (dbType == DbHelperType.SqlLite)
            {
                if (IntPtr.Size == 4)
                {
                    // 32-bit
                }
                else if (IntPtr.Size == 8)
                {
                    // 64-bit
                }
                object obj = Assembly.Load("System.Data.SQLite").CreateInstance("System.Data.SQLite.SQLiteFactory");
                DbProviderFactory factory = (DbProviderFactory)obj;
                return new DbHelper(factory, connectionString);
            }

            return null;
        }

        public static IDbHelper CreateIDbHelper(DbProviderFactory factory, string connectionString)
        {
            return new DbHelper(factory, connectionString);
        }

        public static IDbHelper CreateIDbHelper(string providerName, string connectionString)
        {
            return new DbHelper(providerName, connectionString);
        }

        public static IDbHelper GetIDbHelper(string providerName, string connectionString)
        {
            lock (items)
            {
                if (items.Where(c => c.providerName == providerName && c.connectionString == connectionString).Count() == 0)
                {
                    items.Add(new DbHelperItem { providerName = providerName, connectionString = connectionString, db = CreateIDbHelper(providerName, connectionString) });
                }
            }

            IDbHelper db = items.Where(c => c.providerName == providerName && c.connectionString == connectionString).First().db;

            return db;
        }
        /// <summary>
        /// 根据配置节点主键名获取IDbHelper对象
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static IDbHelper GetIDbHelper(string configName)
        {
            lock (items)
            {
                if (items.Where(c => c.configName == configName).Count() == 0)
                {
                    items.Add(new DbHelperItem { configName = configName, db = CreateIDbHelper(configName) });
                }
            }

            IDbHelper db = items.Where(c => c.configName == configName).First().db;
            //连接释放的时候可能将ConnectionString清除了 所以再重新设置
            //var items = (ConfigurationManager.GetSection("DbHelperSettings") as FYJ.Data.Config.DbHelperSection).Items;
            //foreach (FYJ.Data.Config.DbHelperElement item in items)
            //{
            //    if (item.Name == configName)
            //    {
            //        if (String.IsNullOrEmpty(db.DbConnectionInstance.ConnectionString) || db.DbConnectionInstance.ConnectionString!=item.ConnectionString)
            //        {
            //            db.CloseConnection();
            //            db.DbConnectionInstance.ConnectionString = item.ConnectionString;
            //        }
            //    }
            //}

            return db;
        }

        public IDbHelper GetDbInstance(string name)
        {
            return DbFactory.GetIDbHelper(name);
        }
    }
}
