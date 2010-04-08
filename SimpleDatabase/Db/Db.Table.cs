using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SimpleDatabase
{
    public partial class Db
    {
        public int Delete(DbTable table, Dictionary<string, object> row)
        {
            int r = 0;
            using (DbCommand cm = this.CreateCommand())
            {
                StringBuilder s = new StringBuilder(String.Format("DELETE FROM {0} WHERE ", table.TableName));

                BuildWhere(table, table.PrimaryKeys, row, s, cm);

                string sql = s.ToString();
                cm.CommandText = sql;

                //OnExecuting(ExecTypes.Delete, cm);

                r = cm.ExecuteNonQuery();

                //OnExecuted(ExecTypes.Delete, cm, r);

                DoAudit(new DbAuditArgs(ExecTypes.Delete, table, cm.CommandText, row));

            }
            return r;
        }

        public int Update(DbTable table, Dictionary<string, object> row)
        {
            int r = 0;

            using (DbCommand cm = this.CreateCommand())
            {
                

                if (table.ContainsField(DbFieldNames.UpdatedOn))
                {
                    if (row.ContainsKey(DbFieldNames.UpdatedOn))
                        row[DbFieldNames.UpdatedOn] = DateTime.Now;
                    else
                        row.Add(DbFieldNames.UpdatedOn, DateTime.Now);
                }

                if (UserInfoProvider != null && table.ContainsField(DbFieldNames.UpdatedBy))
                {
                    if (row.ContainsKey(DbFieldNames.UpdatedBy))
                        row[DbFieldNames.UpdatedBy] = UserInfoProvider.UserName;
                    else
                        row.Add(DbFieldNames.UpdatedBy, UserInfoProvider.UserName);
                }

                StringBuilder s = new StringBuilder(String.Format("UPDATE {0} SET ", table.TableName));
                int count = table.Fields.Count;
                for (int i = 0; i < count; i++)
                {
                    DbField dbf = table.Fields[i];
                    if (dbf.IsAutoIncrement) continue;
                    if (dbf.IsKey) continue;

                    string fieldName = dbf.FieldName;
                    if (!row.ContainsKey(fieldName)) continue;

                    if (fieldName.Equals(DbFieldNames.CreatedBy, StringComparison.OrdinalIgnoreCase)) continue;
                    if (fieldName.Equals(DbFieldNames.CreatedOn, StringComparison.OrdinalIgnoreCase)) continue;

                    object o = row[fieldName];

                    s.AppendFormat("{0}={1},", fieldName, GetParameterMarker(fieldName));
                    DbParameter p = CreateParameter(dbf, o);
                    cm.Parameters.Add(p);

                }
                if (s.Length > 0)
                {
                    s.Remove(s.Length - 1, 1);
                }
                s.Append(" WHERE ");

                // Note: if key value changed, update record count will be 0
                BuildWhere(table, table.PrimaryKeys, row, s, cm);

                string sql = s.ToString();
                cm.CommandText = sql;

                //OnExecuting(ExecTypes.Update, cm);

                DoAudit(new DbAuditArgs(ExecTypes.Update, table, cm.CommandText, row));

                r = cm.ExecuteNonQuery();

                //OnExecuted(ExecTypes.Update, cm, r);
            }
            return r;
        }

        public int Insert(DbTable table, Dictionary<string, object> row)
        {
            int r = 0;

            using (DbCommand cm = this.CreateCommand())
            {
                //InsertStatementParameter isp = null;
                //if (useCache && Cache.InsertStatementParameters.TryGetValue(table.TableName, out isp))
                //{
                //    cm.CommandText = isp.Sql;
                //    foreach (DbField f in isp.Fields)
                //    {
                //        DbParameter p = CreateParameter(f, row[f.FieldName]);
                //        cm.Parameters.Add(p);
                //    }
                //}
                //else
                {
                    //if (useCache)
                    //{
                    //    isp = new InsertStatementParameter();
                    //}
                    StringBuilder f = new StringBuilder();
                    StringBuilder v = new StringBuilder();

                    if (table.ContainsField(DbFieldNames.CreatedOn))
                    {
                        if (row.ContainsKey(DbFieldNames.CreatedOn))
                            row[DbFieldNames.CreatedOn] = DateTime.Now;
                        else
                            row.Add(DbFieldNames.CreatedOn, DateTime.Now);
                    }
                    if (UserInfoProvider != null && table.ContainsField(DbFieldNames.CreatedBy))
                    {
                        if (row.ContainsKey(DbFieldNames.CreatedBy))
                            row[DbFieldNames.CreatedBy] = UserInfoProvider.UserName;
                        else
                            row.Add(DbFieldNames.CreatedBy, UserInfoProvider.UserName);
                    }
                    if (NextIdProvider != null && table.ContainsField(DbFieldNames.Id))
                    {
                        if (row.ContainsKey(DbFieldNames.Id))
                        {
                            object id = row[DbFieldNames.Id];
                            if (IsNewId(id))
                                row[DbFieldNames.Id] = NextIdProvider.Get();
                        }
                        else
                        {
                            row.Add(DbFieldNames.Id, NextIdProvider.Get());
                        }
                    }

                    int count = table.Fields.Count - 1;
                    for (int i = 0; i <= count; i++)
                    {
                        DbField dbf = table.Fields[i];

                        if (dbf.IsAutoIncrement) continue;
                        string fieldName = dbf.FieldName;

                        if (!row.ContainsKey(fieldName)) continue;

                        if (fieldName.Equals(DbFieldNames.UpdatedBy, StringComparison.OrdinalIgnoreCase)) continue;
                        if (fieldName.Equals(DbFieldNames.UpdatedOn, StringComparison.OrdinalIgnoreCase)) continue;

                        f.Append(fieldName);
                        f.Append(",");

                        object o = row[fieldName];
                        v.Append(GetParameterMarker(fieldName));                        
                        DbParameter p = CreateParameter(dbf, o);
                        cm.Parameters.Add(p);
                        v.Append(",");

                        //if (useCache)
                        //{
                        //    isp.Fields.Add(dbf);
                        //}

                    }
                    if (f.Length > 0)
                    {
                        f.Remove(f.Length - 1, 1);
                        v.Remove(v.Length - 1, 1);
                    }

                    string sql = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", table.TableName, f.ToString(), v.ToString());
                    cm.CommandText = sql;

                    //if (useCache)
                    //{                        
                    //    isp.Sql = sql;
                    //    Cache.InsertStatementParameters.Add(table.TableName, isp);
                    //}
                }

                //OnExecuting(ExecTypes.Insert, cm);

                r = cm.ExecuteNonQuery();

                //OnExecuted(ExecTypes.Insert, cm, r);

                DoAudit(new DbAuditArgs(ExecTypes.Insert, table, cm.CommandText, row));
            }
            return r;
        }

        #region Utils

        internal DataRow SelectRow(DbTable table, Dictionary<string, object> keys)
        {
            using(DbCommand cm = this.CreateCommand()) 
            {
                StringBuilder s = new StringBuilder(String.Format("SELECT * FROM {0} WHERE ", table.TableName));
                BuildWhere(table, table.PrimaryKeys, keys, s, cm);

                string sql = s.ToString();
                
                cm.CommandText = sql;

                DataTable dt = ExecQuery(cm);

                DataRow dr = null;
                if (dt.Rows.Count > 0)
                    dr = dt.Rows[0];

                return dr;
            }      
        }

        protected void BuildWhere(DbTable table, List<DbField> fields, Dictionary<string, object> values, StringBuilder sql, DbCommand cm)
        {
            if (fields == null || fields.Count < 1)
            {
                throw new MissingPrimaryKeyException();
            }

            int count = fields.Count - 1;
            for (int i = 0; i <= count; i++)
            {
                string fieldName = fields[i].FieldName;
                if (!values.ContainsKey(fieldName))
                {
                    throw new MissingPrimaryKeyValueException(fieldName);
                }

                DbField dbf = table.Fields[fieldName];

                object o = values[fieldName];
                string whereFieldName = String.Format("wh{0}", fieldName);
                sql.AppendFormat("{0}={1}", fieldName, GetParameterMarker(whereFieldName));
                DbParameter p = CreateParameter(dbf, o);
                p.ParameterName = GetParameterMarker(whereFieldName);
                cm.Parameters.Add(p);

                if (i < count)
                {
                    sql.Append(" AND ");
                }
            }// end for
        }

        public void SetTableAuditType(string tableName, TableAuditTypes auditType)
        {
            TableAuditTypesList[tableName] = auditType;
        }

        public TableAuditTypes GetTableAuditType(string tableName)
        {
            if (TableAuditTypesList.ContainsKey(tableName))
                return TableAuditTypesList[tableName];
            DbTable table = GetDbTable(tableName);
            if (table != null) 
                return table.DefaultAuditType;
            return TableAuditTypes.Default;
        }
        public bool IsAuditEnabled(string tableName)
        {
            TableAuditTypes auditType = GetTableAuditType(tableName);
            bool enabled = false;
            switch (auditType)
            {
                case TableAuditTypes.SharedTable:
                case TableAuditTypes.StandaloneTable:
                    enabled = true;
                    break;
                case TableAuditTypes.Default:
                    switch (Db.DefaultTableAuditType)
                    {
                        case TableAuditTypes.SharedTable:
                        case TableAuditTypes.StandaloneTable:
                            enabled = true;
                            break;
                    }
                    break;
            }
            return enabled;
        }

        protected void DoAudit(DbAuditArgs arg)
        {
            DbAudit.Create(this, arg);
        }
        #endregion Utils
    }//end of class
}
