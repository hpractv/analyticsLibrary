using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace analyticsLibrary.cs
{
    public static class searching
    {
        private static bool stringSearch(object value1, object value2) => string.Compare(value1.ToString(), value2.ToString()) <= 0;

        private static bool intSearch(object value1, object value2) => (int)value1 <= (int)value2;

        private static bool doubleSearch(object value1, object value2) => (double)value1 <= (double)value2;

        private static bool floatSearch(object value1, object value2) => (float)value1 <= (float)value2;

        private static bool decimalSearch(object value1, object value2) => (decimal)value1 <= (decimal)value2;


        public static Func<object, object, bool> pickFunction<k>()
        {
            var kType = typeof(k);
            Func<object, object, bool> searchFunction;
            switch (kType)
            {
                case Type t when t == typeof(int):
                    searchFunction = intSearch;
                    break;

                case Type t when t == typeof(double):
                    searchFunction = doubleSearch;
                    break;

                case Type t when t == typeof(float):
                    searchFunction = floatSearch;
                    break;

                case Type t when t == typeof(decimal):
                    searchFunction = decimalSearch;
                    break;

                default:
                    searchFunction = stringSearch;
                    break;
            }
            return searchFunction;
        }

        //public static bool binarySearch<k>(this k item, k[] searchItems) => item.binarySearch(searchItems, null, false, null, out k[] sorted);
        //public static bool binarySearch<k>(this k item, k[] searchItems) => item.binarySearch(searchItems, null, false, null, out k[] sorted);

        //private static bool binarySearch<k>(this k item, k[] searchItems, Func<k, k, bool> searchFunc = null, bool doSort = false, Func<object, object, bool> sortFunc = null, out k[] sortedItems)
        //{
        //    var searchFunction = pickFunction<k>();
        //    var tempSearchItems = !doSort ? searchItems : searchItems.quickSort(doSort, sortFunc);
        //    sortedItems = tempSearchItems;
        //    return Array.BinarySearch(tempSearchItems, item) > -1;
        //}
    }
}
