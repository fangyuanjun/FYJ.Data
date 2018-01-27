using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Data.Common;
namespace FYJ.Data
{
    public enum DbHelperType : int
    {
        Other = 0,
        SqlServer = 1,
        Oracle = 2,
        OleDb = 3,
        MySql = 4,
        SqlLite = 5,
        Odbc = 6
    }

    /// <summary>
    /// 功能:数据接口
    /// 作者:fangyj
    /// 创建日期:2012-07-15
    /// 修改日期:2013-08-11
    /// </summary>
    /// <code>public interface IDbHelper</code>
    public interface IDbHelper
    {
        #region 执行
        /// <summary>
        /// 执行一条sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parms">sql参数,可选类型 IDataParameter, KeyValuePair<string, object>,ParameterEx,IEnumerable<IDataParameter>,IDictionary<string, object></param>
        /// <returns></returns>
        /// 
        int ExecuteSql(string sql, params object[] parms);

        /// <summary>
        /// 根据IDataParameter 自动生成sql语句并执行
        /// </summary>
        /// <param name="tableName">表明</param>
        /// <param name="pkName">主键名</param>
        /// <param name="iaAdd">是否插入语句，否则为修改</param>
        /// <param name="list"></param>
        /// <returns></returns>
        int ExecuteSql(string tableName, string pkName, bool iaAdd, IEnumerable<IDataParameter> parms);

        /// <summary>
        /// 执行存储过程 返回DataSet
        /// </summary>
        /// <param name="parms">存储过程参数</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <returns></returns>
        DataSet RunProcedure(IEnumerable<IDataParameter> parms, string storedProcName);

        /// <summary>
        /// 执行存储过程 返回DataSet
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parms">存储过程参数</param>
        /// <returns></returns>
        DataSet RunProcedure(string storedProcName, params IDataParameter[] parms);

        /// <summary>
        /// 执行存储过程  
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="dic">以存储过程参数名为key 参数值为value</param>
        /// <returns></returns>
        DataSet RunProcedure(string storedProcName, IDictionary<String, object> dic);

        /// <summary>
        /// 执行存储过程 返回整数值 @RETURN_VALUE 
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        int ExecuteProcedure(string storedProcName, IDictionary<String, object> dic);

        /// <summary>
        /// 执行存储过程 返回整数值 @RETURN_VALUE 
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="storedProcName"></param>
        /// <returns></returns>
        int ExecuteProcedure(IEnumerable<IDataParameter> parms, string storedProcName);

        /// <summary>
        /// 执行存储过程 返回整数值 @RETURN_VALUE 
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        int ExecuteProcedure(string storedProcName, params IDataParameter[] parms);
        #endregion

        #region 查询

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        DataSet GetDataSet(string sql, params object[] parms);

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
        DataTable GetDataTable(out int count, string tableName, string order, IEnumerable<IDataParameter> parms = null, string select = "*", string where = null, int currentPage = 1, int pageSize = 20);

        /// <summary>
        /// 获取DataTable 该值为DataSet的第一张表
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        DataTable GetDataTable(string sql, params object[] parms);

        /// <summary>
        /// 获取前max行数据  如果max小于1 则返回所有行
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="max"></param>
        /// <returns></returns>
        DataTable GetTopDataTable(string tableName, long max);

        /// <summary>
        /// 获取第一行第一列object值
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        object GetObject(string sql, params object[] parms);

        string GetString(string sql, params object[] parms);

        long GetLong(string sql, params object[] parms);

        double GetDouble(string sql, params object[] parms);

        int GetInt(string sql, params object[] parms);

        bool GetBoolean(string sql, params object[] parms);

        bool Exists(string sql, params object[] parms);

        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        bool ExistsTable(string tableName);

        /// <summary>
        /// 获取当前数据库所有表
        /// </summary>
        /// <returns></returns>
        List<string> GetTables();
        #endregion

        /// <summary>
        /// 创建DbParameter对象
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        DbParameter CreateParameter(string parameterName = null, object parameterValue = null, ParameterDirection? direction = null);

        #region  属性
        /// <summary>
        /// 获取DbProviderFactory对象
        /// </summary>
        /// <returns></returns>
        DbProviderFactory DbProviderFactoryInstance { get; set; }

        /// <summary>
        /// 获取或设置连接对象
        /// </summary>
        /// <returns></returns>
        String ConnectionString { get; set; }

        /// <summary>
        /// 获取数据类型枚举
        /// </summary>
        /// <returns></returns>
        DbHelperType DbHelperTypeEnum { get; }

        /// <summary>
        /// 事务对象
        /// </summary>
        DbTransaction Tran { get; }

        /// <summary>
        /// 参数前缀
        /// </summary>
        string ParameterPrefix { get; }
        #endregion
        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTran();

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚
        /// </summary>
        void Rollback();

        /// <summary>
        /// 测试数据库是否可以打开
        /// </summary>
        /// <returns></returns>
        bool TestCanConnectionOpen();
    }
}
