using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarsDemo.ConfigHelper;

namespace BarsDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var cfg = (ServersConfigSection)ConfigurationManager.GetSection("serversSection");
            if (cfg == null)
                return;
            foreach (var s in cfg.Servers)
            {
                ServerElement t = (ServerElement)s;
                Console.WriteLine($"Server name {t.Name} HDD Size {t.HDDSize} Connection strings {t.ConnectionStrings.Count}"); 
            }
            Console.WriteLine("any key to exit");
            Console.ReadLine();
        }
    }
}
