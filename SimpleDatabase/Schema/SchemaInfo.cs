using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SimpleDatabase
{
    #region Base Classes
    public class SchemaDatabase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() { return Name; }
    }
    public class SchemaTable
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public override string ToString() { return Name; }
    }
    public class SchemaView
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public bool IsUpdatable { get; set; }
        public string Definition { get; set; }
        public override string ToString() { return Name; }
    }
    public class SchemaUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() { return Name; }
    }
    public class SchemaColumn
    {
        public string Owner { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int OrdinalPostion { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsNullable { get; set; }
        public object DefaultValue { get; set; }
        public override string ToString() { return ColumnName; }
    }
    public class SchemaRoutine
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public override string ToString() { return Name; }
    }
    public class SchemaIndex
    {
        public string IndexOwner { get; set; }
        public string IndexName { get; set; }
        public string TableOwner { get; set; }
        public string TableName { get; set; }
        public override string ToString() { return IndexName; }
    }
    public class SchemaIndexColumn
    {
        public string IndexOwner { get; set; }
        public string IndexName { get; set; }
        public string TableOwner { get; set; }
        public string TableName { get; set; }
        public int OrdinalPosition { get; set; }
        public string ColumnName { get; set; }
        public override string ToString() { return ColumnName; }
    }
    public class SchemaForeignKeyColumn
    {
        public string Owner { get; set; }
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
    }
    public class SchemaForeignKey
    {
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
        public List<string> FieldNames { get; set; }
        public string ReferencedTableName { get; set; }
        public List<string> ReferencedFieldNames { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(TableName);
            sb.Append("(");
            foreach (string fieldName in FieldNames)
            {
                sb.Append(fieldName);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            sb.Append(" Refer To ");

            sb.Append(ReferencedTableName);
            sb.Append("(");
            foreach (string fieldName in ReferencedFieldNames)
            {
                sb.Append(fieldName);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            return sb.ToString();
        }
    }
    #endregion Base Classes

    public class SchemaInfo
    {
        public Db Db { get; private set; }

        public SchemaInfo(Db db)
        {
            Db = db;
        }

        public virtual List<SchemaDatabase> GetDatabases() { return null; }
        public virtual List<SchemaTable> GetTables(string owner) { return null; }
        public virtual List<SchemaView> GetViews(string owner) { return null; }
        public virtual List<SchemaUser> GetUsers() { return null; }
        public virtual List<SchemaColumn> GetColumns(string owner, string tableName) { return null; }
        public virtual List<SchemaRoutine> GetProcedures(string owner) { return null; }
        public virtual List<SchemaRoutine> GetFunctions(string owner) { return null; }
        public virtual List<SchemaIndex> GetIndexes(string owner, string tableName) { return null; }
        public virtual List<SchemaIndexColumn> GetIndexColumns(string owner, string tableName, string indexName) { return null; }

        /// <summary>
        /// GetForeignKeyColumns is called to build foreign key collections (associations).
        /// You need to implement GetForeignKeyColumns function to make simple database works.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public virtual List<SchemaForeignKeyColumn> GetForeignKeyColumns(string owner, string tableName) { return null; }

        public virtual List<SchemaForeignKey> GetForeignKeys(string owner, string tableName)
        {
            List<SchemaForeignKeyColumn> fkcList = this.GetForeignKeyColumns(owner, tableName);
            List<SchemaForeignKey> list = new List<SchemaForeignKey>();

            if (fkcList != null && fkcList.Count > 0)
            {
                for (int i = 0; i < fkcList.Count; i++)
                {
                    SchemaForeignKeyColumn fkc = fkcList[i];
                    string constraintName = fkc.ConstraintName;
                    string tName = fkc.TableName;
                    List<string> fieldNames = new List<string>();
                    fieldNames.Add(fkc.ColumnName);
                    string refTName = fkc.ReferencedTableName;
                    List<string> refFieldNames = new List<string>();
                    refFieldNames.Add(fkc.ReferencedColumnName);
                    int currentOne = (tName + constraintName).GetHashCode();

                    while (i < fkcList.Count - 1)
                    {
                        SchemaForeignKeyColumn fkcNext = fkcList[i + 1];
                        string nextTName = fkcNext.TableName;
                        string nextConstraintName = fkcNext.ConstraintName;
                        int nextOne = (nextTName + nextConstraintName).GetHashCode();
                        if (currentOne == nextOne)
                        {
                            fieldNames.Add(fkcNext.ColumnName);
                            refFieldNames.Add(fkcNext.ReferencedColumnName);
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    SchemaForeignKey fk = new SchemaForeignKey();
                    fk.ConstraintName = constraintName;
                    fk.TableName = tName;
                    fk.FieldNames = fieldNames;
                    fk.ReferencedTableName = refTName;
                    fk.ReferencedFieldNames = refFieldNames;

                    list.Add(fk);
                }
            }
            return list;
        }

        #region overrides
        public override string ToString()
        {
            return Db.ConnectionString;
        }

        public override int GetHashCode()
        {
            return Db.ConnectionString.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Db.ConnectionString.Equals(obj);
        }
        #endregion overrides

    }
}
