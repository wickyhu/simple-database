using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    public partial class DbTable
    {
        #region Properties

        public string TableName { get; private set; }
        public DbFieldCollection Fields { get; private set; }

        public List<DbField> PrimaryKeys { get; private set; }
        public List<SchemaForeignKey> ForeignKeys { get; private set; }

        public TableAuditTypes DefaultAuditType { get; set; }

        #endregion Properties

        private DbTable()
        {
        }

        public static DbTable Create(Db db, string tableName)
        {
            DbTable t = new DbTable();
            t.TableName = tableName;

            DataTable dt = db.GetSchemaTable(tableName);

            if (dt == null || dt.Rows.Count <= 0)
                return t;


            t.Fields = DbFieldCollection.Create(dt);

            t.PrimaryKeys = new List<DbField>();
            foreach (DbField f in t.Fields)
            {
                if (f.IsKey)
                {
                    t.PrimaryKeys.Add(f);
                }
            }

            t.ForeignKeys = db.SchemaInfo.GetForeignKeys(null, tableName);

            #region AuditType
            switch (Db.AuditType)
            {
                case DbAuditTypes.Explicit:
                    t.DefaultAuditType = TableAuditTypes.None;
                    break;
                case DbAuditTypes.Implicit:
                    t.DefaultAuditType = TableAuditTypes.Default;
                    break;
                case DbAuditTypes.None:
                    t.DefaultAuditType = TableAuditTypes.None;
                    break;
            }
            #endregion AuditType
            return t;
        }

        public override bool Equals(object obj)
        {
            if (obj is DbTable)
            {
                return this.TableName.Equals(((DbTable)obj).TableName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return TableName.GetHashCode();
        }

        public override string ToString()
        {
            return TableName;
        }
        
        public int GetFieldIndex(string fieldName)
        {
            return Fields.IndexOf(fieldName);
        }

        public bool ContainsField(string fieldName)
        {
            return GetFieldIndex(fieldName) >= 0;
        }

        public bool IsPrimaryKey(string fieldName)
        {
            foreach (DbField f in PrimaryKeys)
            {
                if (fieldName.Equals(f.FieldName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public SchemaForeignKey GetForeignKey(string fieldName)
        {
            foreach (SchemaForeignKey fk in ForeignKeys)
            {
                foreach (string s in fk.FieldNames)
                {
                    if (fieldName.Equals(s, StringComparison.OrdinalIgnoreCase))
                        return fk;
                }
            }
            return null;
        }

    }
}

