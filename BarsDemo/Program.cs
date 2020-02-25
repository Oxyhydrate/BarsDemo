using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BarsDemo.ConfigHelper;
using BarsDemo.Data;
using Dapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Npgsql;

namespace BarsDemo
{
    class Program
    {
        static ServersConfigSection cfg = (ServersConfigSection)ConfigurationManager.GetSection("serversSection");
        static string updatePeriod = System.Configuration.ConfigurationManager.AppSettings["UpdatePeriod"];
        static async Task Main(string[] args)
        {
            double timer = Double.Parse(updatePeriod);
            TimeSpan time = TimeSpan.FromMilliseconds(timer);
            Console.WriteLine($"Период обновления документа {time.Hours} ч, {time.Minutes} м, {time.Seconds} с");
            Console.WriteLine("Для выхода нажмите Ctrl+C");
            List<ServerElement> servers = new List<ServerElement>();
            foreach (var s in cfg.Servers)
            {
                ServerElement srv = (ServerElement)s;
                servers.Add(srv);
            }
            Parallel.ForEach<ServerElement>(servers, (server) =>

            {
                Console.WriteLine($"Server name {server.Name} HDD Size {server.HDDSize}");
                while (true)
                {
                    ProcessSrv(server);
                    Thread.Sleep((int)timer);
                }

            });
            
        }
        static async Task ProcessSrv(ServerElement server)
        {
            DataAcces da = new DataAcces(server.ConnectionStrings[0].ToString()); //todo try catch
            var dbs = await da.GetDbSize(); //получаем список ДБ на сервере
            GSheetsHelper gsh = new GSheetsHelper(server.Name, server.HDDSize, dbs);
            gsh.UpdateSheet();
            foreach (var db in dbs)
            {
                Console.WriteLine($"База {db.DatabaseName}, Размер {db.DBSizeGB.ToString("#.###")}");
            }
        }


    }
}
