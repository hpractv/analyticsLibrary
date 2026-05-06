using System;

namespace analyticsLibrary.library
{
    public class indexObject<v> : indexObject<int, v>
    {
        public indexObject(int index, v value)
            : base(i => (int)i, index, value) { }
    }

    public class indexObject<t, v>
    {
        public indexObject(Func<int, t> indexCreate, int index, v value)
        {
            this.index = indexCreate(index);
            this.value = value;
        }

        public t index { get; private set; }
        public v value { get; set; }
    }
}