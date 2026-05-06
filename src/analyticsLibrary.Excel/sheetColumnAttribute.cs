using System;

namespace analyticsLibrary.Excel
{
    public class sheetColumnAttribute : Attribute
    {
        private string[] _sheetColumnNames { get; set; }
        public string[] sheetColumnNames { get { return _sheetColumnNames; } }
        public sheetColumnAttribute(params string[] sheetColumnNames)
        {
            this._sheetColumnNames = sheetColumnNames;
        }
    }
}
