using System;
using System.Collections.Generic;

namespace analyticsLibrary.Core
{
    [Serializable]
    public class Table
    {
        public string schema { get; set; }
        public string name { get; set; }
        public IEnumerable<Column> columns { get; set; }
    }

}
