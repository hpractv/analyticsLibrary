using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace analyticsLibrary.dbObjects
{
    public class sql_string
    {
        public string name { get; set; }
        public string sql { get; set; }
        public string formattedSql(params object[] required) { return string.Format(sql, required); }
    }
}
