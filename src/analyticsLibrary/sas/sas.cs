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
        public string path { get; }
        public string table { get; }

        public sas(string path)
        {
            this.path = $"{Path.GetDirectoryName(path)}\\";
            this.table = Path.GetFileNameWithoutExtension(path);
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
                        buildHeader(rs.Fields.Cast<Field>());
                    });

                return _header.Select(v => v.value);
            }
        }

        private void buildHeader(IEnumerable<Field> fields)
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
                    buildHeader(sr.Fields.Cast<Field>());
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

        private List<data<object>> _data;

        public IEnumerable<data<object>> records
        {
            get
            {
                if (_data == null)
                {
                    _data = new List<data<object>>();
                    execute((rs) =>
                    {
                        if (_header == null)
                            this.buildHeader(rs.Fields.Cast<Field>());

                        while (!rs.EOF)
                        {
                            _data.Add(new data<object>(this, rs.Fields.Cast<Field>().Select(f => f.Value).ToArray()));
                            rs.MoveNext();
                        }
                    });
                }
                return _data.ToArray();
            }
        }

        public void execute(Action<Recordset> recordProcess)
        {
            var connection = new Connection();
            connection.Mode = modGlobal.ConnectModeEnum.adModeRead;
            connection.Open($"Provider=sas.LocalProvider;Data Source={this.path};");

            var rs = new ADODB.Recordset();
            rs.Open(
                this.table,
                connection,
                modGlobal.CursorTypeEnum.adOpenForwardOnly,
                modGlobal.LockTypeEnum.adLockReadOnly,
                (int)modGlobal.CommandTypeEnum.adCmdTableDirect);
            recordProcess(rs);
            rs.Close();
            connection.Close();
        }
    }
}