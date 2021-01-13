using System;
using System.Collections.Generic;

namespace analyticsLibrary.library
{
    [Serializable]
    public class table
    {
        public string schema { get; set; }
        public string name { get; set; }
        public IEnumerable<column> columns { get; set; }
    }

}
