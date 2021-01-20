using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace analyticsLibrary.library
{
    public class fixedWidthFile : IKeyIndex
    {
        protected string _file { get; set; }

        protected bool _hasHeader;
        protected int[] _headerPostiions;

        protected bool _hasFooter;
        protected int[] _footerPostions;

        protected int[] _positions;

        public fixedWidthFile(string file, int[] positions) :
            this(file, positions, true)
        { }

        public fixedWidthFile(string file, int[] positions, bool hasHeader = true)
        {
            this._positions = positions;
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
                var firstRow = stream.ReadLine().splitFromPositions(_positions);
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

        protected List<data<string>> _records;

        public IEnumerable<data<string>> records
        {
            get
            {
                if (_header == null) buildHeader();
                if (_records == null)
                {
                    _records = new List<data<string>>();

                    var stream = new StreamReader(this._file);
                    var addHeader = _hasHeader;
                    while (stream.Peek() > -1)
                    {
                        var record = new data<string>(this, stream.ReadLine().splitFromPositions(_positions));

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

        //public static void writeCsv(string outputFile, DataTable data)
        //    => writeCsv(outputFile, data, false);
        //public static void writeCsv(string outputFile, DataTable data, bool append)
        //    => writeCsv(outputFile, data, ',', append);

        //public static void writeCsv(string outputFile, DataTable data, char delimeter)
        //    => writeCsv(outputFile, data, delimeter, false);
        //public static void writeCsv(string outputFile, DataTable data, char delimeter, bool append)
        //    => writeCsv(outputFile,
        //        data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray(),
        //        data.Rows.Cast<DataRow>().Select(r => r.ItemArray), delimeter, append);

        //public static void writeCsv(string outputFile, IEnumerable<object[]> data)
        //    => writeCsv(outputFile, data, false);
        //public static void writeCsv(string outputFile, IEnumerable<object[]> data, bool append)
        //    => writeCsv(outputFile, data, ',', true, append);
        //public static void writeCsv(string outputFile, IEnumerable<string> header, IEnumerable<object[]> data)
        //    => writeCsv(outputFile, header, data, false);
        //public static void writeCsv(string outputFile, IEnumerable<string> header, IEnumerable<object[]> data, bool append)
        //    => writeCsv(outputFile, header, data, ',', append);

        //public static void writeCsv(string outputFile, IEnumerable<string> header, IEnumerable<object[]> data, char delimiter)
        //    => writeCsv(outputFile, header, data, delimiter, false);
        //public static void writeCsv(string outputFile, IEnumerable<string> header, IEnumerable<object[]> data, char delimiter, bool append)

        //{
        //    var file = new List<object[]>();
        //    file.Add(header.Cast<object>().ToArray());
        //    file.AddRange(data);
        //    writeCsv(outputFile, file, delimiter, true, append);
        //}

        //public static void writeCsv(string outputFile, IEnumerable<object[]> data, char delimeter, bool firstRowHeader)
        //    => writeCsv(outputFile, data, delimeter, firstRowHeader, false);
        //public static void writeCsv(string outputFile, IEnumerable<object[]> data, char delimeter, bool firstRowHeader, bool append)
        //{
        //    var file = new StreamWriter(outputFile, append);
        //    if (!firstRowHeader)
        //    {
        //        file.WriteLine(data.First().index().Select(i => $"Column{i.index + 1}").toCsv(delimeter));
        //    }
        //    data.forEach(r => file.WriteLine(r.toCsv(delimeter)));
        //    file.Close();
        //}
    }
}