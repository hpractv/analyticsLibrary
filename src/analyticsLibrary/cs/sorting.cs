using System;
using System.Collections.Generic;
using System.Linq;

namespace analyticsLibrary.cs
{
    public static class sorting
    {
        private static bool stringLess(object value1, object value2) => string.Compare(value1.ToString(), value2.ToString()) <= 0;

        private static bool stringMore(object value1, object value2) => string.Compare(value1.ToString(), value2.ToString()) >= 0;

        private static bool intLess(object value1, object value2) => (int)value1 <= (int)value2;

        private static bool intMore(object value1, object value2) => (int)value1 >= (int)value2;

        private static bool doubleLess(object value1, object value2) => (double)value1 <= (double)value2;

        private static bool doubleMore(object value1, object value2) => (double)value1 >= (double)value2;

        private static bool floatLess(object value1, object value2) => (float)value1 <= (float)value2;

        private static bool floatMore(object value1, object value2) => (float)value1 >= (float)value2;

        private static bool decimalLess(object value1, object value2) => (decimal)value1 <= (decimal)value2;

        private static bool decimalMore(object value1, object value2) => (decimal)value1 >= (decimal)value2;

        public static Func<object, object, bool> pickFunction<k>(bool descending = false)
        {
            var kType = typeof(k);
            Func<object, object, bool> checkFunction;
            switch (kType)
            {
                case Type t when t == typeof(int):
                    if (!descending) checkFunction = intLess;
                    else checkFunction = intMore;
                    break;

                case Type t when t == typeof(double):
                    if (!descending) checkFunction = doubleLess;
                    else checkFunction = doubleMore;
                    break;

                case Type t when t == typeof(float):
                    if (!descending) checkFunction = floatLess;
                    else checkFunction = floatMore;
                    break;

                case Type t when t == typeof(decimal):
                    if (!descending) checkFunction = decimalLess;
                    else checkFunction = decimalMore;
                    break;

                default:
                    if (!descending) checkFunction = stringLess;
                    else checkFunction = stringMore;
                    break;
            }
            return checkFunction;
        }

        public static k[] mergeSort<k>(this k[] sortItems, Func<object, object, bool> compare)
            => sortItems.mergeSort(false, compare);

        public static k[] mergeSort<k>(this k[] sortItems, bool descending = false)
            => sortItems.mergeSort(descending, null);

        internal static k[] mergeSort<k>(this k[] sortItems, bool descending = false, Func<object, object, bool> compare = null)
        {
            var checkFunction = compare ?? pickFunction<k>(descending);

            return doSort(sortItems);

            k[] doSort(k[] subSortItems)
            {
                if (subSortItems.Length == 1)
                {
                    return subSortItems;
                }
                else
                {
                    var mid = subSortItems.Length / 2;
                    var sortedA = doSort(subSortItems.Take(mid).ToArray());
                    var sortedB = doSort(subSortItems.Skip(mid).ToArray());
                    var sorting = new List<k>();

                    int iA = 0, iB = 0;

                    var sA = sortedA[iA];
                    var sB = sortedB[iB];

                    while (iA + iB < sortedA.Length + sortedB.Length)
                    {
                        if (iA < sortedA.Length && iB < sortedB.Length)
                        {
                            if (checkFunction(sA, sB))
                            {
                                sorting.Add(sA);
                                if (++iA < sortedA.Length) sA = sortedA[iA];
                            }
                            else
                            {
                                sorting.Add(sB);
                                if (++iB < sortedB.Length) sB = sortedB[iB];
                            }
                        }
                        else if (iA < sortedA.Length)
                        {
                            sorting.Add(sA);
                            if (++iA < sortedA.Length) sA = sortedA[iA];
                        }
                        else if (iB < sortedB.Length)
                        {
                            sorting.Add(sB);
                            if (++iB < sortedB.Length) sB = sortedB[iB];
                        }
                    }

                    return sorting.ToArray();
                }
            }
        }

        public static k[] quickSort<k>(this k[] sortItems, Func<object, object, bool> compare)
            => sortItems.quickSort(false, compare);

        public static k[] quickSort<k>(this k[] sortItems, bool descending = false)
            => sortItems.quickSort(descending, null);

        internal static k[] quickSort<k>(this k[] sortItems, bool descending = false, Func<object, object, bool> compare = null)
        {
            var checkFunction = compare ?? pickFunction<k>(descending);

            var i = 0;
            var j = sortItems.Length - 1;
            quickSort(sortItems, i, j);

            return sortItems;

            void quickSort(k[] sorting, int si, int sj)
            {
                if (si < sj)
                {
                    int pos = partition(sorting, si, sj);
                    quickSort(sorting, si, pos - 1);
                    quickSort(sorting, pos + 1, sj);
                }
            }

            int partition(k[] arr, int si, int sj)
            {
                k pivot = arr[sj];
                int small = si - 1;

                for (int l = si; l < sj; l++)
                {
                    if (checkFunction(arr[l], pivot))
                    {
                        small++;
                        swap(arr, l, small);
                    }
                }

                swap(arr, sj, small + 1);
                return small + 1;
            }

            void swap(k[] arr, int l, int ss)
            {
                k temp;
                temp = arr[l];
                arr[l] = arr[ss];
                arr[ss] = temp;
            }
        }
    }
}