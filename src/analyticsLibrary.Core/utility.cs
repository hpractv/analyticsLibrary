using System;
using System.IO;
using System.Reflection;

namespace analyticsLibrary.Core
{
    internal static class Utility
    {
        internal static string assemblyDirectory
        {
            get
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"LINQPad Plugins\Framework 4.6");
                return path;
            }
        }
    }
}
