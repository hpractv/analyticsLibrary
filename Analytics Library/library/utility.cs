using System;
using System.IO;
using System.Reflection;

namespace analyticsLibrary.library
{
    internal static class utility
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
