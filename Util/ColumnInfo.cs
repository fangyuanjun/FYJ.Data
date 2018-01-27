using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYJ.Data.Util
{
    public class ColumnInfo
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 列序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 类型   数据库中的类型 如nvarchar
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 存储在DataTable的DataColumn   列中的数据的类型
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// 是否自动递增
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 是否唯一
        /// </summary>
        public bool IsUniqueness { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// 长度   -1 表示 max
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool IsAllowNull { get; set; }

        /// <summary>
        /// 小数位数
        /// </summary>
        public int Radix { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>

        public string DefaultValue { get; set; }

        public override bool Equals(object obj)
        {
            ColumnInfo dest = obj as ColumnInfo;
            if (dest != null)
            {
                if (dest.Type == this.Type
                    && dest.Length == this.Length
                    && dest.IsAllowNull == this.IsAllowNull
                    && dest.IsIdentity == this.IsIdentity)
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string str = Type;
            if (IsAllowNull)
            {
                str += ",可为空";
            }
            else
            {
                str += ",不能为空";
            }

            if (IsIdentity)
            {
                str += ",自动递增";
            }
            else
            {
                str += ",不自增";
            }

            str += ",长度" + Length;

            if (Radix > 0)
            {
                str += ",小数位数" + Radix;
            }

            return str;
        }
    }
}
