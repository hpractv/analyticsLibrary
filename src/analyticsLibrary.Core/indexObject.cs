using System;

namespace analyticsLibrary.Core
{
    public class indexObject<TValue> : indexObject<int, TValue>
    {
        public indexObject(int index, TValue value)
            : base(i => (int)i, index, value) { }
    }

    public class indexObject<TIndex, TValue>
    {
        public indexObject(Func<int, TIndex> indexCreate, int index, TValue value)
        {
            this.index = indexCreate(index);
            this.value = value;
        }

        public TIndex index { get; private set; }
        public TValue value { get; set; }
    }
}
