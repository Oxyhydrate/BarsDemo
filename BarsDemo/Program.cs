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
        static void Main(string[] args)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(Double.Parse(updatePeriod));
            Console.WriteLine($"Период обновления документа {time.Hours} ч, {time.Minutes} м, {time.Seconds} с");
            Console.WriteLine("Для выхода нажмите Ctrl+C");
            foreach (var s in cfg.Servers)
            {
                ServerElement t = (ServerElement)s;
                Console.WriteLine($"Server name {t.Name} HDD Size {t.HDDSize}");
                DataAcces da = new DataAcces(t.ConnectionStrings[0].ToString()); //todo try catch
                var dbs = da.GetDbSize(); //получаем список ДБ на сервере
                GSheetsHelper gsh = new GSheetsHelper(t.Name, t.HDDSize, dbs);
                gsh.UpdateSheet();
                foreach (var db in dbs)
                {
                    Console.WriteLine($"База {db.DatabaseName}, Размер {db.DBSizeGB.ToString("#.###")}");
                }
            }
            Console.ReadLine();
        }



    }
}
