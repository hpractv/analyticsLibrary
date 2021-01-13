using System;
using System.Linq;

namespace analyticsLibrary.library
{
    public class record<t>
    {
        internal IKeyIndex parent { get; private set; }

        public record(IKeyIndex parent, t[] values)
        {
            this.parent = parent;
            this.values = values;
        }

        public t[] values { get; private set; }

        public t this[int index]
        {
            get
            {
                return (t)values[index];
            }
        }

        public t this[string key]
        {
            get
            {
                if (parent.keyExists(key, out int index))
                {
                    var v = values[index];
                    return (t)v;
                }
                else
                    throw new ApplicationException($"This key does not exists: {key}");
            }
        }

        public bool isNull(string key) => parent.keyExists(key, out var index);

        public t[] selectFields(params string[] names)
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