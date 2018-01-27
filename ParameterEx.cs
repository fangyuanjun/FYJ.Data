using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYJ.Data
{
    public class ParameterEx
    {
        private string key;
        private object value;
        
        public ParameterEx(string key,string value)
        {
            this.key = key;
            this.value = value;
        } 

        public string Key
        {
            get { return key; }
        }

        public object Value
        {
            get { return value; }
        }
    }
}
