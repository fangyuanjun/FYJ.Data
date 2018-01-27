using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace FYJ.Data.Config
{
    public class DbHelperSection : ConfigurationSection
    {
       
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DbHelperCollection Items
        {
            get
            {
                return (DbHelperCollection)base[""];
            }
        }

    }
}
