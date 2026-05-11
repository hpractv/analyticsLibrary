using System;

namespace analyticsLibrary.Core
{
    [Serializable]
    public class Column
    {
        public string parentTable { get; set; }
        public string name { get; set; }
        public dataTypeEnum dataType { get; set; }
        public int? length { get; set; }
        public bool nullable { get; set; }
    }
}
