using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYJ.Data.Util
{
    /// <summary>
    /// 序列信息
    /// </summary>
    public class SequenceInfo
    {
        /// <summary>
        /// 序列名
        /// </summary>
        public string Name { get; set; }

        public decimal MinValue { get; set; }

        public decimal MaxValue { get; set; }

        /// <summary>
        /// 开始值
        /// </summary>
        public decimal Start { get; set; }

        public int Cache { get; set; }

        /// <summary>
        /// 增长值
        /// </summary>
        public int Increment { get; set; }

        public bool Cycle { get; set; }

        public string Owner { get; set; }

        public string OracleSQL
        {
            get
            {
                string sql = @"
create sequence {0}
minvalue {1}
maxvalue {2}
start with {3}
increment by {4}
cache {5}
{6}
";
                string n = String.IsNullOrEmpty(Owner) ? Name : Owner + "." + Name;
                sql = string.Format(sql, n, MinValue, MaxValue, Start, Increment, Cache, Cycle ? " cycle" : "");

                return sql;
            }
        }
    }
}
