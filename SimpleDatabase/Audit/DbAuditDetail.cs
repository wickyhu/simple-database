using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using SimpleControls;

namespace SimpleDatabase
{
    [DbTable(AuditType = TableAuditTypes.None)]
    public class DbAuditDetail
    {
        public long Id { get; set; }
        public long AuditId { get; set; }

        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool Changed { get; set; } 

        public DbAuditDetail(long auditId)
        {
            AuditId = auditId;
        }

        static string GetValue(Db db, DbTable table, string fieldName, object value, int maxSize)
        {
            if (value.IsNull())
                return String.Empty;

            Type t = value.GetType();
            string valueStr;

            if (t.IsEnum) valueStr = Convert.ToInt32(value).ToString();
            else valueStr = value.ToString();

            if (table.ForeignKeys.Count > 0)
            {
                SchemaForeignKey fk = table.GetForeignKey(fieldName);
                if (fk != null && fk.FieldNames.Count == 1)
                {
                    DbTable fTable = db.GetDbTable(fk.ReferencedTableName);
                    if (fTable.ContainsField(DbFieldNames.Name))
                    {
                        string refFieldName = fk.ReferencedFieldNames[0];
                        string sql = String.Format("SELECT MIN({0}) FROM {1} WHERE {2}={3}",
                            DbFieldNames.Name, 
                            fk.ReferencedTableName,
                            refFieldName, 
                            db.GetParameterMarker(refFieldName));

                        using (DbCommand cm = db.CreateCommand())
                        {
                            cm.CommandText = sql;
                            cm.Parameters.Add(db.CreateParameter(fTable.Fields[refFieldName], value));
                            string nameStr = db.ExecScalar(cm)
                                    .IfNull(String.Empty);
                            valueStr = String.Format("{0} ({1})", valueStr, nameStr);
                        }
                    }
                }
            }

            if (valueStr.Length > maxSize)
                return valueStr.Substring(0, maxSize);
            return valueStr;
        }

        public static void Create(Db db, DbAudit a, DbAuditArgs arg)
        {
            TableAuditTypes auditType = db.GetTableAuditType(arg.Table.TableName);
            if(auditType == TableAuditTypes.Default) 
                auditType = Db.DefaultTableAuditType;
            switch (auditType)
            {
                case TableAuditTypes.SharedTable:
                    CreateAuditInShareTable(db, a, arg);
                    break;
                case TableAuditTypes.StandaloneTable:
                    CreateAuditStandaloneTable(db, a, arg);
                    break;
            }
        }

        static void CreateAuditInShareTable(Db db, DbAudit a, DbAuditArgs arg)
        {
            Dictionary<string, object> dic = (Dictionary<string, object>)arg.Data;
            DbTable table = db.GetDbTable(typeof(DbAuditDetail));

            //assume newvalue and oldvalue are same size
            int maxValueSize = table.Fields["NEWVALUE"].Length;

            DataRow oldRow = null;
            if (arg.Action == ExecTypes.Update)
            {
                oldRow = db.SelectRow(arg.Table, dic);
            }

            INextIdProvider nextId = db.NextIdProvider;
            foreach (KeyValuePair<string, object> kvp in dic)
            {
                DbAuditDetail ad = new DbAuditDetail(a.Id);
                ad.Id = nextId.Get();
                ad.FieldName = kvp.Key;

                string newValue = GetValue(db, arg.Table, kvp.Key, kvp.Value, maxValueSize);

                switch (arg.Action)
                {
                    case ExecTypes.Insert:
                        ad.NewValue = newValue;
                        ad.Changed = false;
                        break;
                    case ExecTypes.Update:
                        ad.NewValue = newValue;

                        if (oldRow != null)
                        {
                            string oldValue = GetValue(db, arg.Table, kvp.Key, oldRow[kvp.Key], maxValueSize);
                            ad.OldValue = oldValue;
                            ad.Changed = !DbHelper.IsEqual(kvp.Value, oldRow[kvp.Key]);
                        }
                        break;
                    case ExecTypes.Delete:
                        ad.OldValue = newValue;
                        ad.Changed = false;
                        break;
                }

                db.Insert<DbAuditDetail>(ad);
            }
        }

        static void CreateAuditStandaloneTable(Db db, DbAudit a, DbAuditArgs arg)
        {
            switch (arg.Action)
            {
                case ExecTypes.Insert:
                case ExecTypes.Update:
                case ExecTypes.Delete:
                    string historyTableName = String.Format("{0}_Audit", arg.Table.TableName);
                    DbTable historyTable = db.GetDbTable(historyTableName);
                    historyTable.DefaultAuditType = TableAuditTypes.None;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arg.Data;
                    dic.Add(DbFieldNames.AuditId, a.Id);
                    db.Insert(historyTable, dic);
                    break;
            }
        }

    }
}
