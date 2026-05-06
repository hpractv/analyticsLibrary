using System;

namespace analyticsLibrary.library
{
    public static class dataTypeHelper
    {
        public static dataTypeEnum dataTypeFromString(string type)
        {
            var returnType = dataTypeEnum.unknown;
            switch (type.ToLower())
            {
                case "bigint":
                    returnType = dataTypeEnum.bigintType;
                    break;

                case "bit":
                    returnType = dataTypeEnum.bitType;
                    break;

                case "char":
                    returnType = dataTypeEnum.charType;
                    break;

                case "date":
                    returnType = dataTypeEnum.dateType;
                    break;

                case "datetime":
                    returnType = dataTypeEnum.datetimeType;
                    break;

                case "decimal":
                    returnType = dataTypeEnum.decimalType;
                    break;

                case "float":
                    returnType = dataTypeEnum.floatType;
                    break;

                case "int":
                    returnType = dataTypeEnum.intType;
                    break;

                case "money":
                    returnType = dataTypeEnum.moneyType;
                    break;

                case "long":
                case "number":
                case "numeric":
                    returnType = dataTypeEnum.numericType;
                    break;

                case "nvarchar":
                case "nvarchar2":
                    returnType = dataTypeEnum.nvarcharType;
                    break;

                case "smalldatetime":
                    returnType = dataTypeEnum.smalldatetimeType;
                    break;

                case "timestamp":
                case "timestamp(0)":
                case "timestamp(3)":
                case "timestamp(6)":
                case "timestamp(9)":
                    returnType = dataTypeEnum.timestampType;
                    break;

                case "tinyint":
                    returnType = dataTypeEnum.tinyintType;
                    break;

                case "varbinary":
                    returnType = dataTypeEnum.varbinaryType;
                    break;

                case "varchar":
                case "varchar2":
                    returnType = dataTypeEnum.varcharType;
                    break;
            }
            return returnType;
        }
    }
}
