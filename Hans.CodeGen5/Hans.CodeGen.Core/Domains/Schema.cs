using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//private static OracleDbType GetOracleDbType(object o) 
//{
//    if (o is string) return OracleDbType.Varchar2;
//    if (o is DateTime) return OracleDbType.Date;
//    if (o is Int64) return OracleDbType.Int64;
//    if (o is Int32) return OracleDbType.Int32;
//    if (o is Int16) return OracleDbType.Int16;
//    if (o is byte) return OracleDbType.Byte;
//    if (o is decimal) return OracleDbType.Decimal;
//    if (o is float) return OracleDbType.Single;
//    if (o is double) return OracleDbType.Double;
//    if (o is byte[]) return OracleDbType.Blob;

//    return OracleDbType.Varchar2;
//}
namespace Hans.CodeGen.Core.Domains
{
    public class Schema
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string DataType { get; set; }
        public string MaxLength { get; set; }
        //public string OrdinalPosition { get; set; }
        public string IsNullable { get; set; }
        public string ConstraintType { get; set; }
        public string ColumnType { get; set; }
        public string MsSqlDataType()
        {
            var type = string.Empty;

            switch (RemoveNullable())
            {
                case Commons.MsSqlDataType.UniqueIdentifier:
                    type = Commons.MsSqlDataType.Guid;
                    break;
                case Commons.MsSqlDataType.BigInt:
                    type = Commons.MsSqlDataType.Long;
                    break;
                case Commons.MsSqlDataType.Bit:
                    type = Commons.MsSqlDataType.Bool;
                    break;
                case Commons.MsSqlDataType.Char:
                case Commons.MsSqlDataType.NChar:
                    type = Commons.MsSqlDataType.Char;
                    break;
                case Commons.MsSqlDataType.Money:
                    type = Commons.MsSqlDataType.Decimal;
                    break;
                case Commons.MsSqlDataType.Decimal:
                    type = Commons.MsSqlDataType.Decimal;
                    break;
                case Commons.MsSqlDataType.Float:
                    type = Commons.MsSqlDataType.Float;
                    break;
                case Commons.MsSqlDataType.Image:
                case Commons.MsSqlDataType.TimeStamp:
                case Commons.MsSqlDataType.VarBinary:
                    type = Commons.MsSqlDataType.Byte;
                    break;
                case Commons.MsSqlDataType.Int:
                case Commons.MsSqlDataType.TinyInt:
                case Commons.MsSqlDataType.SmallInt:
                    type = Commons.MsSqlDataType.Int;
                    break;
                case Commons.MsSqlDataType.DateTime:
                case Commons.MsSqlDataType.SmallDateTime:
                    type = Commons.MsSqlDataType.DateAndTime;
                    break;
                case Commons.MsSqlDataType.NText:
                case Commons.MsSqlDataType.Text:
                case Commons.MsSqlDataType.NVarChar:
                case Commons.MsSqlDataType.VarChar:
                    type = Commons.MsSqlDataType.String;
                    break;
            }

            return type;
        }

        public Type OracleDataType()
        {
            var dataType = RemoveNullable();

            if (dataType == "DATE" || dataType == "datetime" || dataType == "TIMESTAMP" ||
                dataType == "TIMESTAMP WITH TIME ZONE" || dataType == "TIMESTAMP WITH LOCAL TIME ZONE" ||
                dataType == "smalldatetime")
            {
                return typeof (DateTime);
            }
            if (dataType == "NUMBER" || dataType == "nchar" || dataType == "LONG" || dataType == "bigint")
            {
                return typeof (long);
            }
            if (dataType == "int" || dataType == "INTERVAL YEAR TO MONTH" || dataType == "BINARY_INTEGER")
            {
                return typeof (int);
            }
            if (dataType == "BINARY_DOUBLE" || dataType == "float")
            {
                return typeof (double);
            }
            if (dataType == "BINARY_FLOAT" || dataType == "FLOAT")
            {
                return typeof (float);
            }
            if (dataType == "BLOB" || dataType == "BFILE *" || dataType == "LONG RAW" || dataType == "binary" ||
                dataType == "image" || dataType == "timestamp" || dataType == "varbinary")
            {
                return typeof (byte[]);
            }
            if (dataType == "INTERVAL DAY TO SECOND")
            {
                return typeof (TimeSpan);
            }
            if (dataType == "bit")
            {
                return typeof (Boolean);
            }
            if (dataType == "decimal" || dataType == "money" || dataType == "smallmoney")
            {
                return typeof (decimal);
            }
            if (dataType == "real")
            {
                return typeof (Single);
            }
            if (dataType == "smallint")
            {
                return typeof (Int16);
            }
            if (dataType == "uniqueidentifier")
            {
                return typeof (Guid);
            }
            if (dataType == "tinyint")
            {
                return typeof (Byte);
            }

            // CHAR, CLOB, NCLOB, NCHAR, XMLType, VARCHAR2, nchar, ntext
            return typeof (string);
        }

        public string SetToNullable()
        {
            return IsNullable.ToLower() == Commons.MsSqlDataType.Yes ? 
                Commons.MsSqlDataType.True : 
                Commons.MsSqlDataType.False;
        }

        public string RemoveNullable()
        {
            return DataType.Contains("?") ? 
                DataType.Remove(DataType.IndexOf("?")) : 
                DataType;
        }
    }
}
