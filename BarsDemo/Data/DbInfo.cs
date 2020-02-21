using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarsDemo.Data
{
    public class DbInfo
    {
        public string DatabaseName { get; set; }
        public string DBSize { get; set; }
        public double DBSizeGB
        {
            get
            {
                return  Double.Parse(DBSize)/1024/1024/1024;
            }
            
        }

    }
}
