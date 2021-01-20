using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace analyticsLibrary.library
{
    public class loopDateRange
    {
        public enum intervalEnum { hour, day, month, year };

        public intervalEnum interval { get; private set; }
        public DateTime start { get; private set; }
        public DateTime end { get; private set; }

        public loopDateRange(DateTime start, DateTime end, intervalEnum interval = intervalEnum.month)
        {
            this.start = start;
            this.end = end;
            this.interval = interval;
        }
    }
}