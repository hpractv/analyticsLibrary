using System;
using System.Reflection;
using System.Linq;

namespace analyticsLibrary.library
{
    public static class dataLibrary
    {
        public static PropertyInfo[] getColumnProperties(Type type)
        {
            var properties = type.GetProperties()
                .Where(EventTypeFilter)
                .Where(p => p.PropertyType.AssemblyQualifiedName != null && p.PropertyType.AssemblyQualifiedName.ToLower().StartsWith("system"))
                .ToArray();
            return properties;
        }

        public static bool EventTypeFilter(PropertyInfo p)
        {
            var allowedTypes = new string[] { "ValueType", "Object" };

            if (!allowedTypes.hasValue(p.PropertyType.BaseType?.Name) || (p.PropertyType.BaseType?.Name == "Object" && p.Name == "EntityKey"))
                return false;

            return true;
        }

        public static object GetPropertyValue(object o)
        {
            if (o == null)
                return DBNull.Value;
            return o;
        }
    }
}
