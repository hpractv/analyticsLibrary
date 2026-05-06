using System;

namespace analyticsLibrary.library
{
    [Serializable]
    public class column
    {
        public string parentTable { get; set; }
        public string name { get; set; }
        public dataTypeEnum dataType { get; set; }
        public int? length { get; set; }
        public bool nullable { get; set; }
    }
}
