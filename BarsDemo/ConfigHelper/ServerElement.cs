using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarsDemo.ConfigHelper
{
    public class ServerElement : ConfigurationElement
    {
        // Название сервера
        [ConfigurationProperty("name")]
        public String Name
        {
            get { return (String)this["name"]; }
            set { this["name"] = value; }
        }
        
        // Размер HDD сервера в GB
        [ConfigurationProperty("hddSize")]
        public String HDDSize
        {
            get { return (String)this["hddSize"]; }
            set { this["hddSize"] = value; }
        }
        
        // Connection strings для сервера
        [ConfigurationProperty("connectionStrings")]
        public ConnectionStringSettingsCollection ConnectionStrings
        {
            get { return (ConnectionStringSettingsCollection) this["connectionStrings"]; }
        }
    }
}
