using System;
using System.Linq;
using analyticsLibrary.Algorithms;
using Xunit;

namespace analyticsLibrary.Algorithms.Tests
{
    public class MergeSortTests
    {
        [Fact]
        public void MergeSort_Integers_SortsAscending()
        {
            var input = new int[] { 5, 3, 8, 1, 9, 2, 7, 4, 6 };
            var sorted = input.mergeSort();
            Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, sorted);
        }

        [Fact]
        public void MergeSort_Integers_SortsDescending()
        {
            var input = new int[] { 3, 1, 4, 1, 5 };
            var sorted = input.mergeSort(descending: true);
            Assert.Equal(new int[] { 5, 4, 3, 1, 1 }, sorted);
        }

        [Fact]
        public void MergeSort_Strings_SortsAlphabetically()
        {
            var input = new string[] { "banana", "apple", "cherry", "date" };
            var sorted = input.mergeSort();
            Assert.Equal(new string[] { "apple", "banana", "cherry", "date" }, sorted);
        }

        [Fact]
        public void MergeSort_SingleElement_ReturnsSame()
        {
            var input = new int[] { 42 };
            Assert.Equal(new int[] { 42 }, input.mergeSort());
        }

        [Fact]
        public void MergeSort_Doubles_SortsAscending()
        {
            var input = new double[] { 3.14, 1.41, 2.71 };
            var sorted = input.mergeSort();
            Assert.Equal(new double[] { 1.41, 2.71, 3.14 }, sorted);
        }
    }

    public class QuickSortTests
    {
        [Fact]
        public void QuickSort_Integers_SortsAscending()
        {
            var input = new int[] { 9, 7, 5, 3, 1, 2, 4, 6, 8 };
            var sorted = input.quickSort();
            Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, sorted);
        }

        [Fact]
        public void QuickSort_Integers_SortsDescending()
        {
            var input = new int[] { 1, 3, 2, 5, 4 };
            var sorted = input.quickSort(descending: true);
            Assert.Equal(new int[] { 5, 4, 3, 2, 1 }, sorted);
        }

        [Fact]
        public void QuickSort_Strings_SortsAlphabetically()
        {
            var input = new string[] { "zebra", "ant", "mango" };
            var sorted = input.quickSort();
            Assert.Equal(new string[] { "ant", "mango", "zebra" }, sorted);
        }

        [Fact]
        public void QuickSort_AlreadySorted_StaysOrdered()
        {
            var input = new int[] { 1, 2, 3, 4, 5 };
            Assert.Equal(input, input.quickSort());
        }
    }

    public class SortingPickFunctionTests
    {
        [Fact]
        public void PickFunction_Int_ReturnsLessOrEqualForAscending()
        {
            var fn = Sorting.pickFunction<int>(descending: false);
            Assert.True(fn(1, 2));
            Assert.True(fn(2, 2));
            Assert.False(fn(3, 2));
        }

        [Fact]
        public void PickFunction_Double_ReturnsExpectedComparison()
        {
            var fn = Sorting.pickFunction<double>(descending: false);
            Assert.True(fn(1.5, 2.5));
            Assert.False(fn(2.5, 1.5));
        }

        [Fact]
        public void PickFunction_String_ComparesLexicographically()
        {
            var fn = Sorting.pickFunction<string>(descending: false);
            Assert.True(fn("apple", "banana"));
            Assert.False(fn("zebra", "ant"));
        }
    }
}
