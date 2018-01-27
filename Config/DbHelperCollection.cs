using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Collections;

namespace FYJ.Data.Config
{
    public class DbHelperCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbHelperElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DbHelperElement)element).Name;

        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "DbHelperSetting";
            }
        }


        public DbHelperElement this[int index]
        {
            get
            {
                return (DbHelperElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

    }
}
