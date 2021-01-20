using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace analyticsLibrary.library
{
    public class csvStream : IKeyIndex, IDisposable
    {
        private string _file { get; set; }
        private char _delimeter = ',';

        private bool _hasHeader;

        private StreamReader _stream;

        public csvStream(string file) :
            this(file, ',', true)
        { }

        public csvStream(string file, char delimiter) :
            this(file, delimiter, true)
        { }

        public csvStream(string file, bool hasHeader = true) :
            this(file, ',', hasHeader)
        { }

        public csvStream(string file, char delimeter, bool hasHeader = true)
        {
            this._delimeter = delimeter;
            this._file = file;
            this._hasHeader = hasHeader;
            this._stream = new StreamReader(this._file);
        }

        private indexObject<string>[] _header_lower_case;
        private indexObject<string>[] _header;

        private void buildHeader()
        {
            if (_header == null)
            {
                var headerStream = new StreamReader(this._file);
                var firstRow = headerStream.ReadLine().fromCsv(_delimeter);
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
                headerStream.Close();
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

        public bool endOfFile
        {
            get => _stream.EndOfStream;
        }

        private int _recordCount = 1;

        public int recordCount
        {
            get => _recordCount;
            private set => _recordCount = value;
        }

        public data<string> nextRecord
        {
            get
            {
                if (_header == null) buildHeader();
                if (!endOfFile)
                {
                    var data = new data<string>(this, _stream.ReadLine().fromCsv(this._delimeter));
                    if (recordCount++ == 1 && _hasHeader)
                        data = new data<string>(this, _stream.ReadLine().fromCsv(this._delimeter));

                    return data;
                }

                return default(data<string>);
            }
        }

        public void resetStream()
        {
            this.recordCount = 0;
            this._stream.Close();
            this._stream = new StreamReader(this._file);
        }

        public void Dispose()
        {
            this._stream.Close();
            this._stream.Dispose();
        }
    }
}