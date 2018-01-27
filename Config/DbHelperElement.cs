using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace FYJ.Data.Config
{
    public class DbHelperElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
            set
            {
                this["name"] = value;
            }
        }

        //接口的实现  none,red,write
        [ConfigurationProperty("type", IsKey = false)]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
            set
            {
                this["type"] = value;
            }
        }

        [ConfigurationProperty("providerName", IsKey = false)]
        public string ProviderName
        {
            get
            {
                return this["providerName"] as string;
            }
            set
            {
                this["providerName"] = value;
            }
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        [ConfigurationProperty("connectionString", IsKey = false)]
        public string ConnectionString
        {
            get
            {
                return this["connectionString"] as string;
            }
            set
            {
                this["connectionString"] = value;
            }
        }

        /// <summary>
        /// 表名前缀
        /// </summary>
        [ConfigurationProperty("tbPre", IsKey = false)]
        public string TbPre
        {
            get
            {
                return this["tbPre"] as string;
            }
            set
            {
                this["tbPre"] = value;
            }
        }

        /// <summary>
        /// 视图名前缀
        /// </summary>
        [ConfigurationProperty("viewPre", IsKey = false)]
        public string ViewPre
        {
            get
            {
                return this["viewPre"] as string;
            }
            set
            {
                this["viewPre"] = value;
            }
        }

        /// <summary>
        /// 存储过程前缀
        /// </summary>
        [ConfigurationProperty("procPre", IsKey = false)]
        public string ProcPre
        {
            get
            {
                return this["procPre"] as string;
            }
            set
            {
                this["procPre"] = value;
            }
        }

        /// <summary>
        /// 是否加密
        /// </summary>
        [ConfigurationProperty("isEncrypt", IsKey = false)]
        public bool IsEncrypt
        {
            get
            {
                if (this["isEncrypt"] == null || this["isEncrypt"].ToString() == "")
                {
                    return false;
                }
                if ((this["isEncrypt"] as string) == "0")
                {
                    return false;
                }
                if ((this["isEncrypt"] as string) == "1")
                {
                    return true;
                }

                return Convert.ToBoolean(this["isEncrypt"]);
            }
            set
            {
                this["isEncrypt"] = value;
            }
        }

       
        /// <summary>
        /// 解密类
        /// </summary>
        [ConfigurationProperty("encryptType", IsKey = false)]
        public string EncryptType
        {
            get
            {
                if (String.IsNullOrEmpty(this["isEncrypt"] as string))
                {
                    return "FYJ.Data.DbEncrypt,FYJ.Data";
                }

                return this["encryptType"] as string;
            }
            set
            {
                this["encryptType"] = value;
            }
        }

        ///<summary>
        ///是否抛出SQL语句异常
        ///</summary>
        [ConfigurationProperty("isThrowSql", IsKey = false)]
        public bool IsThrowSql
        {
            get
            {
                if (this["isThrowSql"] == null || this["isThrowSql"].ToString() == "")
                {
                    return false;
                }
                if ((this["isThrowSql"] as string) == "0")
                {
                    return false;
                }
                if ((this["isThrowSql"] as string) == "1")
                {
                    return true;
                }

                return Convert.ToBoolean(this["isThrowSql"]);
            }
            set
            {
                this["isThrowSql"] = value;
            }
        }
    }
}
