using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace analyticsLibrary.library
{
    public class csv_base : IKeyIndex
    {
        protected string _file { get; set; }
        protected char _delimeter = ',';

        protected int[] _fieldMaxLengths;

        public int[] fieldMaxLengths
        {
            get
            {
                if (_fieldMaxLengths == null)
                {
                    _fieldMaxLengths = new int[header.Count()];

                    records.forEach(r =>
                    {
                        for (var i = 0; i < header.Count(); i++)
                        {
                            var valueLength = r.values.Length > i ? r.values[i].ToString().Length : 0;
                            lock (_fieldMaxLengths)
                            {
                                if (valueLength > _fieldMaxLengths[i])
                                    _fieldMaxLengths[i] = valueLength;
                            }
                        }
                    });
                }

                return _fieldMaxLengths;
            }
        }

        protected bool _hasHeader;

        public csv_base(string file) :
            this(file, ',', true)
        { }

        public csv_base(string file, char delimiter) :
            this(file, delimiter, true)
        { }

        public csv_base(string file, bool hasHeader = true) :
            this(file, ',', hasHeader)
        { }

        public csv_base(string file, char delimeter, bool hasHeader = true)
        {
            this._delimeter = delimeter;
            this._file = file;
            this._hasHeader = hasHeader;
        }

        protected indexObject<string>[] _header_lower_case;
        protected indexObject<string>[] _header;

        protected void buildHeader()
        {
            if (_header == null)
            {
                var stream = new StreamReader(this._file);
                var firstRow = stream.ReadLine().fromCsv(_delimeter);
                if (_hasHeader)
                {
                    _header = firstRow.index();
                    for (int i = 0; i < _header.Length; i++)
                        _header[i].value = _header[i].value ?? $"Column{_header[i].index + 1}";
                }
                else
                {
                    var record = firstRow
                        .index();

                    var format = record.Length < 10 ?
                            "0" :
                        record.Length < 100 ?
                            "00" :
                        record.Length < 1000 ?
                            "000" :
                            //else
                            "0";

                    _header = record
                        .Select(r => $"Column{(r.index + 1).ToString(format)}")
                        .index();
                }
                stream.Close();
                _header_lower_case = _header.Select(h => h.value.ToLower()).index();
            }
        }

        public IEnumerable<string> header
        {
            get
            {
                buildHeader();
                return _header.Select(h => h.value);
            }
        }

        public IEnumerable<int> lineCounts
        {
            get => records.Select(r => r.values.Length).Distinct();
        }

        public bool keyExists(string key) => keyExists(key, out var index);

        public bool keyExists(string key, out int index)
        {
            buildHeader();
            var field = _header_lower_case.SingleOrDefault(v => v.value == key.ToLower());
            if (field != null)
            {
                index = field.index;
                return true;
            }
            index = -1;
            return false;
        }

        protected List<record<string>> _records;

        public IEnumerable<record<string>> records
        {
            get
            {
                if (_header == null) buildHeader();
                if (_records == null)
                {
                    _records = new List<record<string>>();

                    var stream = new StreamReader(this._file);
                    var addHeader = _hasHeader;
                    while (stream.Peek() > -1)
                    {
                        var record = new record<string>(this, stream.ReadLine().fromCsv(_delimeter));

                        if (addHeader)
                        {
                            addHeader = false;
                            continue;
                        }
                        else
                        {
                            _records.Add(record);
                        }
                    }
                }
                return _records.ToArray();
            }
        }
    }
}