
﻿using analyticsLibrary.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace analyticsLibrary.Core
{
    public class typedIndex<TKey, TValue>
    {
        private TKey[] _index = new TKey[0];
        private TValue[] _values = new TValue[0];

        public typedIndex(TKey[] keys, TValue[] values)
        {
            var keysLength = keys.Length;
            var valuesLength = values.Length;
            if (keysLength != valuesLength) throw new ApplicationException("Key count not equal to the values count.");
            
            var tempIndex = keys.index();
            Array.Sort(tempIndex, (a, b) => string.Compare(a.value.ToString(), b.value.ToString()));

            _index = tempIndex.Select(ki => ki.value).ToArray();
            _values = new TValue[keysLength];
            
            for (int i = 0; i < keysLength; i++)
            {
                var indexItem = tempIndex[i];
                _values[i] = values[indexItem.index];
            }
        }

        public bool containsKey(TKey key) => containsKey(key, out int index);

        public bool containsKey(TKey key, out int index)
        {
            if (key != null)
                index = Array.BinarySearch(_index, key);
            else
                index = -1;

            return index >= 0;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!containsKey(key, out int index)) return default(TValue); // throw new ApplicationException("Key does not exist.");
                return _values[index];
            }
        }

        public TValue this[int index]
        {
            get => _values[index];
        }

        public int count
        {
            get => _index.Length;
        }
    }
}
