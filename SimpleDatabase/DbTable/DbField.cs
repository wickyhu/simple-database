using System;
using System.Data;
using SimpleControls;
using IQToolkit.Data.Common;

namespace SimpleDatabase
{
    //[Serializable]
    public class DbField : QueryType
    {
        public string FieldName { get; private set; }
        public object DefaultValue { get; private set; }
        public int FieldOrdinal { get; private set; }

        public int ProviderType { get; private set; }
        public bool IsLong { get; private set; }
        public bool IsReadOnly { get; private set; }
        public bool IsRowVersion { get; private set; }
        public bool IsUnique { get; private set; }
        public bool IsKey { get; private set; }
        public bool IsAutoIncrement { get; private set; }
        public bool IsHidden { get; private set; }
        public string BaseSchemaName { get; private set; }
        public string BaseCatalogName { get; private set; }
        public string BaseTableName { get; private set; }
        public string BaseFieldName { get; private set; }

        #region QueryType
        int _length;
        public override int Length { get { return _length; } }
        short _precision;
        public override short Precision { get { return _precision; } }
        short _scale;
        public override short Scale { get { return _scale; } }
        bool _notNull;
        public override bool NotNull { get { return _notNull; } }
        #endregion QueryType

        public DbType DbType { get; private set; }
        public string DataType { get; private set; }
        private void DetectDbType()
        {

            DbType dbType = DbType.Object;
            switch (DataType)
            {
                case "System.String":
                    dbType = DbType.String;
                    break;
                case "System.DateTime":
                    dbType = DbType.DateTime;
                    break;
                case "System.Decimal":
                    dbType = DbType.Decimal;
                    break;
                case "System.Int16":
                    dbType = DbType.Int16;
                    break;
                case "System.Int32":
                    dbType = DbType.Int32;
                    break;
                case "System.Int64":
                    dbType = DbType.Int64;
                    break;
                case "System.UInt16":
                    dbType = DbType.UInt16;
                    break;
                case "System.UInt32":
                    dbType = DbType.UInt32;
                    break;
                case "System.UInt64":
                    dbType = DbType.UInt64;
                    break;
                case "System.Single":
                    dbType = DbType.Single;
                    break;
                case "System.Double":
                    dbType = DbType.Double;
                    break;
                case "System.SByte":
                    dbType = DbType.SByte;
                    break;
                case "System.Byte":
                    dbType = DbType.Byte;
                    break;
                case "System.Guid":
                    dbType = DbType.Guid;
                    break;
                case "System.Byte[]":
                    dbType = DbType.Binary;
                    break;
                default:
                    break;
            }
            DbType = dbType;
        }

        private DbField()
        {
        }

        private static object GetValue(DataRow dr, string fieldName)
        {
            if (dr.Table.Columns.IndexOf(fieldName) < 0)
                return null;
            return dr[fieldName];
        }

        public static DbField Create(DataRow dr)
        {
            DbField f = new DbField();

            f.FieldName = GetValue(dr, "ColumnName").IfNull(String.Empty);
            f.DataType = GetValue(dr, "DataType").IfNull("System.String");
            f.DetectDbType();

            f.ProviderType = GetValue(dr, "ProviderType").IfNull(-1);
            f.DefaultValue = GetValue(dr, "DefaultValue");
            f.FieldOrdinal = GetValue(dr, "ColumnOrdinal").IfNull(0);

            f._length = GetValue(dr, "ColumnSize").IfNull(0);
            f._precision = (short)GetValue(dr, "NumericPrecision").IfNull(0);
            f._scale = (short)GetValue(dr, "NumericScale").IfNull(0);
            f._notNull = !(GetValue(dr, "AllowDBNull").IfNull(true));

            f.IsLong = GetValue(dr, "IsLong").IfNull(false);
            f.IsReadOnly = GetValue(dr, "IsReadOnly").IfNull(false);
            f.IsRowVersion = GetValue(dr, "IsRowVersion").IfNull(false);
            f.IsUnique = GetValue(dr, "IsUnique").IfNull(false);
            f.IsKey = GetValue(dr, "IsKey").IfNull(false);
            f.IsAutoIncrement = GetValue(dr, "IsAutoIncrement").IfNull(false);

            f.BaseSchemaName = GetValue(dr, "BaseSchemaName").IfNull(String.Empty);
            f.BaseCatalogName = GetValue(dr, "BaseCatalogName").IfNull(String.Empty);
            f.BaseTableName = GetValue(dr, "BaseTableName").IfNull(String.Empty);
            f.BaseFieldName = GetValue(dr, "BaseColumnName").IfNull(String.Empty);

            return f;
        }

        public override string ToString()
        {
            return this.FieldName;
        }

        public override int GetHashCode()
        {
            return this.FieldName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is DbField)
            {
                return this.FieldName.Equals(((DbField)obj).FieldName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }//end of class
}

