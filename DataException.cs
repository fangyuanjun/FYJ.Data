using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FYJ.Data
{
    public class DataException : ApplicationException, ISerializable
    {
       
        public DataException()
        {
        }
        public DataException(string message): base(message)
        {
        }
        public DataException(Exception inner, string sql) :base (sql,inner)
        {
           
        }
 
        // deserialization constructor
        //public DataException(SerializationInfo info,StreamingContext context) : base(info, context)
        //{
        //    sql = info.GetString("sql");
        //}

        // Called by the frameworks during serialization
        // to fetch the data from an object.
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);
        //    info.AddValue("sql", sql);
        //}

        // overridden Message property. This will give the
        // proper textual representation of the exception,
        // with the added field value.
        public override string Message
        {
            get
            {
                if (System.Configuration.ConfigurationManager.AppSettings["isThrowSql"] != null && System.Configuration.ConfigurationManager.AppSettings["isThrowSql"].ToLower() == "true")
                {
                    //return base.Message.Replace(sql,"");
                    return "ee";
                }
                else
                {
                    return base.Message;
                }
            }
        }

    }
}
