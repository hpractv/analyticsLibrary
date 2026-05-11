using System;
using System.Linq;

namespace analyticsLibrary.Core
{
    public class Data<TValue>
    {
        internal IKeyIndex parent { get; private set; }

        public Data(IKeyIndex parent, TValue[] values)
        {
            this.parent = parent;
            this.values = values;
        }

        public TValue[] values { get; private set; }

        public TValue this[int index]
        {
            get
            {
                return (TValue)values[index];
            }
        }

        public TValue this[string key]
        {
            get
            {
                if (parent.keyExists(key, out int index))
                {
                    var v = values[index];
                    return (TValue)v;
                }
                else
                    throw new ApplicationException($"This key does not exists: {key}");
            }
        }

        public bool isNull(string key) => parent.keyExists(key, out var index);

        public TValue[] selectFields(params string[] names)
        {
            var indexes = names.Select(n =>
            {
                parent.keyExists(n, out var i);
                return i;
            });

            return indexes
                .Select(i => values[i])
                .ToArray();
        }
    }
}
