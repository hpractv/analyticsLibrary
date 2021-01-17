using ADODB;
using analyticsLibrary.library;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace analyticsLibrary.sas
{
    public class sas : IKeyIndex
    {
        private string _path = null;
        public string path { get => _path; }

        private string _table = null;
        public string table { get => _table; }

        public sas(string path)
        {
            _path = $"{Path.GetDirectoryName(path)}\\";
            _table = Path.GetFileNameWithoutExtension(path);
        }

        private indexObject<string>[] _header_lower_case;
        private indexObject<string>[] _header;

        public IEnumerable<string> header
        {
            get
            {
                if (_header == null)
                    execute(rs =>
                    {
                        builHeader(rs.Fields.Cast<Field>());
                    });

                return _header.Select(v => v.value);
            }
        }

        private void builHeader(IEnumerable<Field> fields)
        {
            _header = fields.Select(f => f.Name).index();
            _header_lower_case = fields.Select(f => f.Name.ToLower()).index();
        }

        public bool keyExists(string key) => keyExists(key, out var index);

        public bool keyExists(string key, out int index)
        {
            if (_header == null)
                execute(sr =>
                {
                    builHeader(sr.Fields.Cast<Field>());
                });

            var field = _header_lower_case.SingleOrDefault(v => v.value == key.ToLower());
            if (field != null)
            {
                index = field.index;
                return true;
            }
            index = -1;
            return false;
        }

        private List<data<object>> _records;

        public IEnumerable<data<object>> records
        {
            get
            {
                if (_records == null)
                {
                    _records = new List<data<object>>();
                    execute((rs) =>
                    {
                        if (_header == null)
                            this.builHeader(rs.Fields.Cast<Field>());

                        while (!rs.EOF)
                        {
                            _records.Add(new record<object>(this, rs.Fields.Cast<Field>().Select(f => f.Value).ToArray()));
                            rs.MoveNext();
                        }
                    });
                }
                return _records.ToArray();
            }
        }

        public void execute(Action<Recordset> recordProcess)
        {
            var connection = new Connection();
            connection.Mode = ConnectModeEnum.adModeRead;
            connection.Open($"Provider=sas.LocalProvider;Data Source={_path};");

            var rs = new ADODB.Recordset();
            rs.LockType = LockTypeEnum.adLockReadOnly;
            rs.Open(_table, connection, CursorTypeEnum.adOpenForwardOnly, LockTypeEnum.adLockReadOnly, (int)CommandTypeEnum.adCmdTableDirect);
            recordProcess(rs);
            rs.Close();
            connection.Close();
        }
    }
}