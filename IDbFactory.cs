using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYJ.Data
{
    public interface IDbFactory
    {
        /// <summary>
        /// 根据配置名获取IDbHelper对象
        /// </summary>
        IDbHelper GetDbInstance(string name);


        /// <summary>
        /// 获取默认IDbHelper对象
        /// </summary>
        //IDbHelper DbInstance { get; }
    }
}
