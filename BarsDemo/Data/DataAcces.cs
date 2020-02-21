using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace BarsDemo.Data
{  // в реальном приложении стоило бы вынести этот класс в отдельный проект библиотеки классов чтобы выделить Data Acces Layer
    public class DataAcces
    {
        private string _connStr;
        public DataAcces(string connStr)
        {
            _connStr = connStr;
        }
        public List<DbInfo> GetDbSize()
        {

            try
            {
                using (var conn = new NpgsqlConnection(_connStr))
                {
                    conn.Open();
                    var result = conn.Query<DbInfo>($"SELECT pg_database.datname as \"DatabaseName\", pg_database_size(pg_database.datname) as \"DBSize\" FROM pg_database ORDER by pg_database_size(pg_database.datname) DESC;");
                    return result.ToList();
                }
            }
            catch (Exception e)
            {
                return new List<DbInfo> { new DbInfo() { DatabaseName = e.Message, DBSize = "0" } };               
            }
            
        }

    }
}
