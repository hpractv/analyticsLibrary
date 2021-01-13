using System;

namespace analyticsLibrary.oracle
{
    public static class extensions
    {
        public static string toOracleDateString(this DateTime value)
        {
            return value.ToString("dd-MMM-yyyy").ToUpper();
        }
    }
}