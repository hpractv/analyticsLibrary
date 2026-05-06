using System;

namespace analyticsLibrary.Excel
{
    public class sheetColumnAttribute : Attribute
    {
        private string[] _sheetColumNames { get; set; }
        public string[] sheetColumnNames { get { return _sheetColumNames; } }
        public sheetColumnAttribute(params string[] sheetColumNames)
        {
            this._sheetColumNames = sheetColumNames;
        }
    }
}
