using System;

namespace analyticsLibrary.Excel
{
    public class sheetColumnAttrubte : Attribute
    {
        private string[] _sheetColumNames { get; set; }
        public string[] sheetColumnNames { get { return _sheetColumNames; } }
        public sheetColumnAttrubte(params string[] sheetColumNames)
        {
            this._sheetColumNames = sheetColumNames;
        }
    }
}
