using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace analyticsLibrary.library
{
    public class csv : IKeyIndex
    {
        private string _file { get; set; }
        private char _delimeter = ',';

        private int[] _fieldMaxLengths;

        public int[] fieldMaxLengths
        {
            get
            {
                if (_fieldMaxLengths == null)
                {
                    _fieldMaxLengths = new int[header.Count()];

                    data.forEach(r =>
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

        private bool _hasHeader;

        public csv(string file) :
            this(file, ',', true)
        { }

        public csv(string file, char delimiter) :
            this(file, delimiter, true)
        { }

        public csv(string file, bool hasHeader = true) :
            this(file, ',', hasHeader)
        { }

        public csv(string file, char delimeter, bool hasHeader = true)
        {
            this._delimeter = delimeter;
            this._file = file;
            this._hasHeader = hasHeader;
        }

        private indexObject<string>[] _header_lower_case;
        private indexObject<string>[] _header;

        private void buildHeader()
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
                    var data = firstRow
                        .index();

                    var format = data.Length < 10 ?
                            "0" :
                        data.Length < 100 ?
                            "00" :
                        data.Length < 1000 ?
                            "000" :
                            //else
                            "0";

                    _header = data
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
            get => data.Select(r => r.values.Length).Distinct();
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

        private List<data<string>> _data;

        public IEnumerable<data<string>> data
        {
            get
            {
                if (_header == null) buildHeader();
                if (_data == null)
                {
                    _data = new List<data<string>>();

                    var stream = new StreamReader(this._file);
                    var addHeader = _hasHeader;
                    while (stream.Peek() > -1)
                    {
                        var record = new data<string>(this, stream.ReadLine().fromCsv(_delimeter));

                        if (addHeader)
                        {
                            addHeader = false;
                            continue;
                        }
                        else
                        {
                            _data.Add(record);
                        }
                    }
                }
                return _data.ToArray();
            }
        }
    }
}