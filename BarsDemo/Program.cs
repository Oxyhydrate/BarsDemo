using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarsDemo.ConfigHelper;
using BarsDemo.Data;
using Dapper;
using Npgsql;

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

                var connStr = t.ConnectionStrings[0];    
                DataAcces da = new DataAcces(connStr.ToString());
                foreach (var str in da.GetDbSize())
                {
                    Console.WriteLine($"База {str.DatabaseName}, Размер {str.DBSizeGB.ToString("#.##")}");
                }
                
                

            }

            Console.WriteLine("any key to exit");
            Console.ReadLine();
        }
    }
}
