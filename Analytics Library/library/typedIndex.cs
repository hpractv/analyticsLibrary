using analyticsLibrary.cs;
using analyticsLibrary.library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace analyticsLibrary.library
{
    public class typedIndex<k, v>
    {
        private k[] _index = new k[0];
        private v[] _values = new v[0];

        public typedIndex(k[] keys, v[] values)
        {
            var keysLength = keys.Length;
            var valuesLength = values.Length;
            if (keysLength != valuesLength) throw new ApplicationException("Key count not equal to the values count.");
            
            var tempIndex = keys.index()
                .quickSort((v1, v2) => string.Compare(((indexObject<int, k>)v1).value.ToString(), ((indexObject<int, k>)v2).value.ToString()) <= 0);

            _index = tempIndex.Select(ki => ki.value).ToArray();
            _values = new v[keysLength];
            
            for (int i = 0; i < keysLength; i++)
            {
                var indexItem = tempIndex[i];
                _values[i] = values[indexItem.index];
            }
        }

        public bool containsKey(k key) => containsKey(key, out int index);

        public bool containsKey(k key, out int index)
        {
            if (key != null)
                index = Array.BinarySearch(_index, key);
            else
                index = -1;

            return index >= 0;
        }

        public v this[k key]
        {
            get
            {
                if (!containsKey(key, out int index)) return default(v); // throw new ApplicationException("Key does not exist.");
                return _values[index];
            }
        }

        public v this[int index]
        {
            get => _values[index];
        }

        public int count
        {
            get => _index.Length;
        }
    }
}