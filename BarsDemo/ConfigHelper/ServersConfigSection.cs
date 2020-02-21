using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarsDemo.ConfigHelper
{
    public class ServersConfigSection : ConfigurationSection
    {
        // Список серверов
        [ConfigurationProperty("servers")]
        public ServersCollection Servers
        {
            get { return (ServersCollection)this["servers"]; }
        }
    }
}
